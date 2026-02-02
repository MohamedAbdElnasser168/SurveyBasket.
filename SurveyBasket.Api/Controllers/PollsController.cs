
using Mapster;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SurveyBasket.Api.Mapping;

namespace SurveyBasket.Api.Controllers

{
    [Route("api/[controller]")]
    [ApiController]
    public class PollsController(IPollService pollService) : ControllerBase
    {

        private readonly IPollService _pollService = pollService;




        [HttpGet("GetAll")]
        public IActionResult GetAll()
        {
            var polls = _pollService.GetAll();
            var response = polls.Adapt<List<PollResponse>>();
            return Ok(response);
        }



        // mapped id from route to method parameter

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var poll = _pollService.GetPollById(id);
            if (poll == null)
            {
                return NotFound();
            }

            //var config = new TypeAdapterConfig();
            //config.NewConfig<Poll, PollResponse>()
            //.Map(dest => dest.Description, src => src.Description);


            var response = poll.Adapt<PollResponse>();

            return Ok(response);


        }

        
        
        

        [HttpPost("")]
        public IActionResult Add([FromBody]CreatePollRequest request, [FromServices]IValidator<CreatePollRequest> validator)
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
            var newPoll = _pollService.Add(request.Adapt<Poll>());
            return CreatedAtAction(nameof(Get), new { id = newPoll.Id }, newPoll.Adapt<PollResponse>());
        
        }




        [HttpPut("{id}")]
        public IActionResult Update([FromRoute]int id, [FromBody]CreatePollRequest request)
        {
            var isUpdated= _pollService.Update(id, request.Adapt<Poll>());
            
            if (!isUpdated)
            {
                return NotFound();
            }

            return NoContent();

        }

        [HttpDelete("{id}")]
        public IActionResult Delete([FromRoute]int id)
        {
            var IsDeleted = _pollService.Delete(id);

            if (!IsDeleted)
            {
                return NotFound();
            }


            return NoContent();
        }

    }
}
