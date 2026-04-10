using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace aspnet_mongo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        [HttpPost("login")]
        public void Login([FromBody] string value)
        {

        }

        [HttpGet]
        public async Task<IActionResult> GetUserInfoAsync()
        {
            await Task.Delay(500);

            return Ok();
        }
    }
}
