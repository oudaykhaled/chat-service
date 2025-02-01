using ChatService.Domain.Response;
using Microsoft.AspNetCore.Mvc;

namespace ChatService.MockModeration.Controllers
{
    [Route("api/moderations")]
    [ApiController]
    public class ModerationController : ControllerBase
    {
        [HttpPost("pre")]
        public async Task<IActionResult> Pre([FromBody] MessageResponse request)
        {
            Console.WriteLine($"Received a pre moderation {request.Text}.");
            Random random = new Random();
            await Task.Delay(random.Next(100, 1001));
            return Ok();
        }

        [HttpPost("post")]
        public async Task<IActionResult> Post([FromBody] MessageResponse request)
        {
            Console.WriteLine($"Received a pro moderation {request.Text}.");
            Random random = new Random();
            await Task.Delay(random.Next(100, 1001));
            return Ok();
        }

        [HttpPost("dispatch")]
        public async Task<IActionResult> Dispatch([FromBody] MessageResponse request)
        {
            Console.WriteLine($"Received a dispatch moderation {request.Text}.");
            Random random = new Random();
            await Task.Delay(random.Next(100, 1001));
            return Ok(request);
        }
    }
}
