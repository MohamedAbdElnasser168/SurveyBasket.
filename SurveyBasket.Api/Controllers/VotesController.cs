using SurveyBasket.Api.Contracts.Votes;
using SurveyBasket.Api.Services;
using System.Security.Claims;

namespace SurveyBasket.Api.Controllers
{
    [Route("api/polls/{pollId}/vote")]
    [ApiController]
    [Authorize]

    public class VotesController(IQuestionService questionService,IVoteService voteService) : ControllerBase
    {
        private readonly IVoteService _voteService = voteService;
        private readonly IQuestionService _questionService = questionService;




        [HttpGet]
        public async Task<IActionResult> Start([FromRoute] int pollId, CancellationToken cancellationToken)
        {

            
            var result = await _questionService.GetAvailableAsync(pollId, User.GetUserId()!, cancellationToken);

            return result.IsSuccess
                ? Ok(result.Value)
                : result.ToProblem();
        }


        [HttpPost]
        public async Task<IActionResult> Vote([FromRoute] int pollId, [FromBody]VoteRequest request ,CancellationToken cancellationToken)
        {
            var result = await _voteService.AddAsync(pollId, User.GetUserId()!, request, cancellationToken);

            return result.IsSuccess
                ? Ok()
                : result.ToProblem();
        }


    }
}
 