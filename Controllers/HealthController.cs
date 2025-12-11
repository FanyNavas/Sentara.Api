using Microsoft.AspNetCore.Mvc;

namespace Sentara.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // -> /api/health
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { ok = true });
        }
    }
}

