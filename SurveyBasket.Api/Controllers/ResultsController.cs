using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SurveyBasket.Api.Controllers
{
    [Route("api/polls/{pollId}/[controller]")]
    [ApiController]
    [Authorize]
    public class ResultsController(IResultService resultService) : ControllerBase
    {
        private readonly IResultService _resultService = resultService;

        [HttpGet("row-data")]
        public async Task<IActionResult> PollVotes([FromRoute] int pollId,CancellationToken cancellationToken)
        {
            var result = await _resultService.GetPollVotesAsync(pollId,cancellationToken);
            return result.IsSuccess 
                ? Ok(result.Value) 
                : result.ToProblem();
        }


        [HttpGet("data-PerDay")]
        public async Task<IActionResult> PollVotesPerDay([FromRoute] int pollId, CancellationToken cancellationToken)
        {
            var result = await _resultService.GetVotesPerDayAsync(pollId, cancellationToken);
            return result.IsSuccess
                ? Ok(result.Value)
                : result.ToProblem();
        }


        [HttpGet("data-PerQuestion")]
        public async Task<IActionResult> PollVotesPerQuestion([FromRoute] int pollId, CancellationToken cancellationToken)
        {
            var result = await _resultService.GetVotesPerQuestionAsync(pollId, cancellationToken);
            return result.IsSuccess
                ? Ok(result.Value)
                : result.ToProblem();
        }
    }
}
