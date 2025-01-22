using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ExpertConsole.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Graph;
using ExpertConsole.Services;

namespace ExpertConsole.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly GraphServiceClient _graphClient;
    private readonly KBService _kbService;

    public HomeController(ILogger<HomeController> logger, GraphServiceClient graphClient, KBService kbService)
    {
        _logger = logger;
        _graphClient = graphClient;
        _kbService = kbService;
    }
    
    public async Task<IActionResult> Index()
    {
        var userInfo = await GetUserInfo();

        _logger.LogInformation("Getting KB from Agent API");
        try
        {
            var kbs = await _kbService.GetKBAsync(userInfo.UserPrincipalName);
            return View(kbs);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "Unauthorized to access API, return empty list");
            return View(new List<KBViewModel>());
        }
    }

    public async Task<IActionResult> Questions(string kbId)
    {
        var userInfo = await GetUserInfo();

        _logger.LogInformation("Getting KB from Agent API");
        var kb = await _kbService.GetKBAsync(kbId, userInfo.UserPrincipalName);
        ViewData["CurrentKnowledgeBase"] = kb;

        _logger.LogInformation("Getting opened questions Agent API");
        var questions = await _kbService.GetKBQuestionsAsync(kbId, userInfo.UserPrincipalName);

        return View(questions);
    }

    public async Task<IActionResult> QuestionDetail(string kbId, string questionId)
    {
        var userInfo = await GetUserInfo();

        _logger.LogInformation("Getting KB from Agent API");
        var kb = await _kbService.GetKBAsync(kbId, userInfo.UserPrincipalName);
        ViewData["CurrentKnowledgeBase"] = kb;

        _logger.LogInformation("Getting question from Agent API");
        var kbs = await _kbService.GetKBQuestionsAsync(kbId, userInfo.UserPrincipalName);
        var question = kbs.FirstOrDefault(q => q.QuestionId == questionId);
        if (question == null)
        {
            throw new KeyNotFoundException($"Question with ID {questionId} not found in knowledge base with ID {kbId}.");
        }

       _logger.LogInformation("Getting history for requester from Agent API");
        var history = await _kbService.GetUserQuestionsHistoryAsync(question.Demandeur, kbId, userInfo.UserPrincipalName);
        ViewData["UserHistory"] = history;

        return View(question);
    }

    public async Task<IActionResult> AddQnAPair(string kbId, string questionId, string question, string reponse)
    {
        var userInfo = await GetUserInfo();

        _logger.LogInformation("Getting KB from Agent API");
        var kb = await _kbService.GetKBAsync(kbId, userInfo.UserPrincipalName);
        ViewData["CurrentKnowledgeBase"] = kb;

        _logger.LogInformation("Answering question in Agent API");
        await _kbService.AddQnAPairAsync(kbId, questionId, question, reponse, userInfo.UserPrincipalName);

        return RedirectToAction("Questions", new { kbId });
    }

    public IActionResult Conditions()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private async Task<UserViewModel> GetUserInfo()
    {
        _logger.LogInformation("Getting logged user info from Graph");
        var graphUserInfo = await _graphClient.Me.GetAsync();
        if (graphUserInfo == null)
        {
            throw new Exception("No user found");
        }
        if (graphUserInfo.UserPrincipalName == null)
        {
            throw new Exception("No UPN found");
        }
        if (graphUserInfo.DisplayName == null)
        {
            throw new Exception("No display name found");
        }
        if (graphUserInfo.Id == null)
        {
            throw new Exception("No user ID found");
        }

        var userInfo = new UserViewModel
        {
            UserId = graphUserInfo.Id,
            UserPrincipalName = graphUserInfo.UserPrincipalName,
            DisplayName = graphUserInfo.DisplayName
        };

        // Add user DisplayName to ViewData to be used in all views
        ViewData["UserDisplayName"] = userInfo.DisplayName;
        return userInfo;
    }
}
