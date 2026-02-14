
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SurveyBasket.Api.Contracts.Polls;
using SurveyBasket.Api.Mapping;
using System.Threading.Tasks;

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
            var polls = await _pollService.GetAllAsync(cancellationToken);
            var response = polls.Adapt<List<PollResponse>>();
            return Ok(response);
        }



        //mapped id from route to method parameter

        // Get Poll By Id
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id, CancellationToken cancellationToken = default)
        {
            var poll = await _pollService.GetPollByIdAsync(id, cancellationToken);
            if (poll == null)
            {
                return NotFound();
            }

            var response = poll.Adapt<PollResponse>();
            return Ok(response);
        }




        /// ADD New Poll
        /// 
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

            //  request is valid
            var newPoll = await _pollService.AddAsync(request.Adapt<Poll>(), cancellationToken);
            return CreatedAtAction(nameof(Get), new { id = newPoll.Id }, newPoll.Adapt<PollResponse>());

        }



        // Update Poll

        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] PollRequest request, CancellationToken cancellationToken = default)
        {
            var isUpdated = await _pollService.UpdateAsync(id, request.Adapt<Poll>(), cancellationToken);

            if (!isUpdated)
            {
                return NotFound();
            }
            return NoContent();

        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken = default)
        {
            var IsDeleted = await _pollService.DeleteAsync(id, cancellationToken);

            if (!IsDeleted)
            {
                return NotFound();
            }

            return NoContent();
        }



        [HttpPut("{id}/togglePublish")]
        public async Task<IActionResult> TogglePublish(int id,CancellationToken cancellationToken =default)
        { 
            var isUpdated= await _pollService.TogglePublishStatusAsync(id,cancellationToken);
            return isUpdated ? NoContent() : NotFound();
        }

    }
    }
