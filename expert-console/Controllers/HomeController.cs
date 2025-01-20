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
        var userPrincipalName = await GetUserPrincipalName();

        _logger.LogInformation("Getting KB from Agent API");
        try
        {
            var kbs = await _kbService.GetKBAsync(userPrincipalName);
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
        var userPrincipalName = await GetUserPrincipalName();

        _logger.LogInformation("Getting KB from Agent API");
        var kb = await _kbService.GetKBAsync(kbId, userPrincipalName);
        ViewData["CurrentKnowledgeBase"] = kb;

        _logger.LogInformation("Getting opened questions Agent API");
        var questions = await _kbService.GetKBQuestionsAsync(kbId, userPrincipalName);

        return View(questions);
    }

    public async Task<IActionResult> QuestionDetail(string kbId, string questionId)
    {
        var userPrincipalName = await GetUserPrincipalName();

        _logger.LogInformation("Getting KB from Agent API");
        var kb = await _kbService.GetKBAsync(kbId, userPrincipalName);
        ViewData["CurrentKnowledgeBase"] = kb;

        _logger.LogInformation("Getting question from Agent API");
        var kbs = await _kbService.GetKBQuestionsAsync(kbId, userPrincipalName);
        var question = kbs.FirstOrDefault(q => q.QuestionId == questionId);

        return View(question);
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

    private async Task<string> GetUserPrincipalName()
    {
        _logger.LogInformation("Getting logged user info from Graph");
        var userInfo = await _graphClient.Me.GetAsync();
        if (userInfo == null)
        {
            throw new Exception("No user found");
        }
        if (userInfo.UserPrincipalName == null)
        {
            throw new Exception("No UPN found");
        }
        ViewData["UserDisplayName"] = userInfo.DisplayName;
        return userInfo.UserPrincipalName;
    }
}
