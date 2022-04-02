using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreApiTest.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AspNetCoreApiTest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public partial class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IDemoService _demoService;

        public WeatherForecastController(ILogger<WeatherForecastController> logger,IDemoService demoService)
        {
            _logger = logger;
            _demoService=demoService.WithLogging(logger);

            var d = new A();
            _logger.LogInformation("aaaa {d}",d);

        }

        [NonAction]
        [LoggerMessage(
            EventId = 10,
            Level = LogLevel.Information,
            Message = "Welcome to {City} {Province}!")]
        public partial void LogMethodSupportsPascalCasingOfNames(
            string city, string province);

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();
            LogMethodSupportsPascalCasingOfNames("123", "12312");
            _demoService.Demo(new A());
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}
