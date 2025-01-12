using LSC.SmartCertify.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LSC.SmartCertify.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class WeatherForecastController : ControllerBase
    {
        private readonly SmartCertifyContext smartCertifyContext;

        public WeatherForecastController(SmartCertifyContext smartCertifyContext)
        {
            this.smartCertifyContext = smartCertifyContext;
        }

        [HttpGet("get")]
        public IActionResult Get()
        {
            var response = smartCertifyContext.UserProfiles.ToList();
            return Ok(new { data = response, message = "Hello World" });
        }
    }
}
