using AgentApi.Infrastructure;
using AgentApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace AgentApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        [ApiKey]
        [HttpGet("{requesterId}/history")]
        public ActionResult<IEnumerable<QuestionModel>> GetUserQuestionsHistory(string requesterId, string kbId, string expertId)
        {
            if (!IsValidUser(expertId)) return Unauthorized();
            var questions = new List<QuestionModel>();

            for (int i = 0; i < 5; i++)
            {
                questions.Add(new QuestionModel
                {
                    QuestionId = i.ToString(),
                    Question = $"Sample question {i} répondue du kb {kbId}",
                    Demandeur = requesterId,
                    Status = "done",
                    DateDemande = DateTime.Now.AddDays(-i),
                    Reponse = $"Sample response to question {i}",
                    DateReponse = DateTime.Now.AddDays(-i).AddHours(4)
                });
            }

            return Ok(questions);
        }

        private bool IsValidUser(string userName)
        {
            return userName == "joel@jqdev.onmicrosoft.com";
        }
    }
}