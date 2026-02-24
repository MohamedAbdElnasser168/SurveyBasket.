using Microsoft.AspNetCore.Authorization;
using SurveyBasket.Api.Contracts.Polls;

namespace SurveyBasket.Api.Controllers

{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PollsController(IPollService pollService) : ControllerBase
    {

        private readonly IPollService _pollService = pollService;




        // Get All Polls

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
        {
            var result = await _pollService.GetAllAsync(cancellationToken); 
            // adapt the list of polls to a list of PollResponse objects using Mapster
            //var response = polls.Adapt<List<PollResponse>>();

            return result.IsSuccess
                ? Ok(result.Value)
                : result.ToProblem(StatusCodes.Status404NotFound);

        }



        //mapped id from route to method parameter

        // Get Poll By Id
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id, CancellationToken cancellationToken = default)
        {
            var result = await _pollService.GetPollByIdAsync(id, cancellationToken);

            return result.IsSuccess
                ? Ok(result.Value)
                : result.ToProblem(StatusCodes.Status404NotFound);

        }




        //ADD New Poll
        // 
        [HttpPost("")]
        public async Task<IActionResult> Add([FromBody] PollRequest request,
            CancellationToken cancellationToken = default)
        {
            #region Code Before Using Fluent Validation Sharp Package
            //var validationResult = validator.Validate(request);

            //if (!validationResult.IsValid)// لو في ايرورز يعني
            //{
            //    var modelState = new ModelStateDictionary();
            //    validationResult.Errors.ForEach(x => modelState.AddModelError(x.PropertyName,x.ErrorMessage));

            //    return ValidationProblem(modelState);
            //}
            #endregion



            var result = await _pollService.AddAsync(request, cancellationToken);
            return result.IsSuccess
               ? CreatedAtAction(nameof(Get), new { id = result.Value.Id }, result.Value)
               : result.ToProblem(StatusCodes.Status404NotFound);


        }



        // Update Poll

        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] PollRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _pollService.UpdateAsync(id, request, cancellationToken);

            return result.IsSuccess
                 ? NoContent()
                 // 409 Conflict if the poll with the specified id  has  conflict with existing data (e.g., duplicate title)
                 : result.ToProblem(StatusCodes.Status409Conflict);

        }



        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken = default)
        {
            var result = await _pollService.DeleteAsync(id, cancellationToken);

            return result.IsSuccess
               ? NoContent()
               // 404 Not Found if the poll with the specified id does not exist
               : result.ToProblem(StatusCodes.Status404NotFound);

        }



        [HttpPut("{id}/togglepublish")]
        public async Task<IActionResult> togglepublish(int id, CancellationToken cancellationtoken = default)
        {
            var result = await _pollService.TogglePublishStatusAsync(id, cancellationtoken);
            return result.IsSuccess
               ? NoContent()
               : result.ToProblem(StatusCodes.Status404NotFound);

        }

    }
}
