using AgentApi.Infrastructure;
using AgentApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace AgentApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KBController : ControllerBase
    {
        [ApiKey]
        [HttpGet]
        public ActionResult<IEnumerable<KBModel>> Get(string expertId)
        {
            if (!IsValidUser(expertId)) return Unauthorized();

            var kbList = new List<KBModel>
            {
                new KBModel
                {
                    KbName = "Convention collective abc <test> l'école",
                    KbId = "1",
                    OpenQuestions = 10,
                    LastQuestionReceived = DateTime.Now
                },
                new KBModel
                {
                    KbName = "Convention collective def <test> l'école",
                    KbId = "1",
                    OpenQuestions = 10,
                    LastQuestionReceived = DateTime.Now
                }
            };

            return Ok(kbList);
        }

        [ApiKey]
        [HttpGet("{kbId}")]
        public ActionResult<KBModel> GetKb(string kbId, string expertId)
        {
            if (!IsValidUser(expertId)) return Unauthorized();

            return Ok(new KBModel
            {
                KbName = "Convention collective abc <test> l'école",
                KbId = "1",
                OpenQuestions = 10,
                LastQuestionReceived = DateTime.Now
            });
        }

        [ApiKey]
        [HttpGet("{kbId}/questions")]
        public ActionResult<IEnumerable<QuestionModel>> GetKbQuestions(string kbId, string expertId)
        {
            if (!IsValidUser(expertId)) return Unauthorized();
            var questions = new List<QuestionModel>();

            for (int i = 0; i < 10; i++)
            {
                questions.Add(new QuestionModel
                {
                    QuestionId = i.ToString(),
                    Question = $"Sample question {i}",
                    Demandeur = $"User{i}@abc.com",
                    Status = "pending",
                    DateDemande = DateTime.Now.AddDays(-i)
                });
            }

            return Ok(questions);
        }

        [ApiKey]
        [HttpGet("{kbId}/questions/{questionId}")]
        public ActionResult<QuestionModel> GetKbQuestion(string kbId, string questionId, string expertId)
        {
            if (!IsValidUser(expertId)) return Unauthorized();

            return Ok(new QuestionModel
            {
                QuestionId = questionId,
                Question = $"Sample question {questionId}",
                Demandeur = $"User{questionId}@abc.com",
                Status = "pending",
                DateDemande = DateTime.Now.AddDays(-1)
            });
        }

        private bool IsValidUser(string expertId)
        {
            return expertId == "joel@jqdev.onmicrosoft.com";
        }
    }
}