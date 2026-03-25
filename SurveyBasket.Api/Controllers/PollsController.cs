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
            return Ok(await _pollService.GetAllAsync(cancellationToken));  
        }

        [HttpGet("current")]
        public async Task<IActionResult> GetCurrent(CancellationToken cancellationToken = default)
        {   
            return Ok(await _pollService.GetCurrentAsync(cancellationToken));
        }


        //mapped id from route to method parameter

        // Get Poll By Id
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id, CancellationToken cancellationToken = default)
        {
            var result = await _pollService.GetPollByIdAsync(id, cancellationToken);

            return result.IsSuccess
                ? Ok(result.Value)
                : result.ToProblem();

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
               : result.ToProblem();


        }



        // Update Poll

        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] PollRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _pollService.UpdateAsync(id, request, cancellationToken);

            return result.IsSuccess
                 ? NoContent()
                 // 409 Conflict if the poll with the specified id  has  conflict with existing data (e.g., duplicate title)
                 : result.ToProblem();

        }



        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken = default)
        {
            var result = await _pollService.DeleteAsync(id, cancellationToken);

            return result.IsSuccess
               ? NoContent()
               // 404 Not Found if the poll with the specified id does not exist
               : result.ToProblem();

        }



        [HttpPut("{id}/togglepublish")]
        public async Task<IActionResult> togglepublish(int id, CancellationToken cancellationtoken = default)
        {
            var result = await _pollService.TogglePublishStatusAsync(id, cancellationtoken);
            return result.IsSuccess
               ? NoContent()
               : result.ToProblem();

        }

    }
}
