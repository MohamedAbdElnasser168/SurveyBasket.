using SurveyBasket.Api.Contracts.Polls;
using SurveyBasket.Api.Contracts.Questions;

namespace SurveyBasket.Api.Controllers
{
    // route for the questions controller, it will be api/poll/{pollId}/questions because the questions are related to the poll and we need to
    // specify the pollId in the route to be able to perform operations on the questions of a specific poll and that restful way to design the api because the questions
    // are a sub-resource of the poll and we need to specify the pollId in the route to be able to perform operations on the questions of a specific poll
    [Route("api/polls/{pollId}/[controller]")]
    [ApiController]
    [Authorize]
    public class QuestionsController(IQuestionService questionService) : ControllerBase
    {
        private readonly IQuestionService _questionService = questionService;



        [HttpGet]
        public async Task<IActionResult> GetAll([FromRoute] int pollId, CancellationToken cancellationToken)
        {
            var result = await _questionService.GetAllAsync(pollId, cancellationToken);
            return result.IsSuccess
                ? Ok(result.Value)
                : result.ToProblem();
        }



        [HttpGet("{questionId}")]
        public IActionResult Get([FromRoute] int pollId, [FromRoute] int questionId)
        {
            var result = _questionService.GetByIdAsync(pollId, questionId).Result;
            return result.IsSuccess
                ? Ok(result.Value)
                : result.ToProblem();

        }



        [HttpPost("")]
        public async Task<IActionResult> Add([FromRoute] int pollId, [FromBody] QuestionRequest request, CancellationToken cancellationToken)
        {
            var result = await _questionService.AddAsync(pollId, request, cancellationToken);

            return result.IsSuccess
                ? CreatedAtAction(nameof(Get), new { pollId, id = result.Value.Id }, result.Value)
                : result.ToProblem();

            
        }



        // route will be api/poll/{pollId}/questions/{id}

        [HttpPut("{questionId}")]
        public async Task<IActionResult> Update([FromRoute] int pollId, [FromRoute] int questionId, [FromBody] QuestionRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _questionService.UpdateAsync(pollId,questionId, request, cancellationToken);

            return result.IsSuccess
                 ? NoContent()
                 : result.ToProblem();
           
        }




        [HttpPut("{questionId}/ToggeleStatus")]
        public async Task<IActionResult> ToggeleIsActive([FromRoute] int pollId, [FromRoute] int questionId)
        {
            var result = await _questionService.ToggeleIsActive(pollId, questionId);

            return result.IsSuccess
                ? NoContent()
                : result.ToProblem();
        }







    }
}
