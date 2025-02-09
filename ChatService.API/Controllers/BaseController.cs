using Microsoft.AspNetCore.Mvc;

namespace ChatService.API.Controllers
{
    [Route("api/[controller]/[Action]")]
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
    }
}
