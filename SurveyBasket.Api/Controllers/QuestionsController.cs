using Microsoft.AspNetCore.OutputCaching;
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
        // cache the response for 60 seconds to improve performance and reduce the load on the server
        // ResponseCashe works only with endpoints that returns 200 status code and it will cache the response for 60 seconds 
        // client controls the cache and it will be stored in the client cache and it will be sent to the server with the request and the server will check if the cache is still valid and if it is valid it will return the cached response otherwise it will return a new response and update the cache in the client
        //[ResponseCache(Duration =60)]

        // //////////////////////////////

        //// OutputCache works with all status codes and it will cache the response for 60 seconds and
        ///it will be stored in the server cache and it will be sent to the client with the response and 
        ///the client will store it in the client cache and it will be sent to the server with the request and
        ///the server will check if the cache is still valid and if it is valid it will return
        ///the cached response otherwise it will return a new response and update the cache in the server
        //[OutputCache(Duration =60)]
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
