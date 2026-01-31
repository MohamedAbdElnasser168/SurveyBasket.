
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
            return Ok(_pollService.GetAll());
        }

        // mapped id from route to method parameter

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var poll = _pollService.GetPollById(id);
            return poll is null ? NotFound() : Ok(poll);
        }

        [HttpPost("")]
        public IActionResult Add(Poll request)
        {
            var newPoll = _pollService.Add(request);
            return CreatedAtAction(nameof(Get), new { id = newPoll.Id }, newPoll);
        }


        [HttpPut("{id}")]
        public IActionResult Update([FromRoute]int id, [FromBody]Poll request)
        {
            var isUpdated= _pollService.Update(id, request);
            
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
