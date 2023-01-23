using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AspNetLoggingMiddleware.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Havazás", "ÁÉÍÓÚ", "Harmatos", "Hideg", "Esős", "Fagy", "Borult", "Hűvös", "Ködös", "Ónos-szitálás"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();
            var data = Enumerable.Range(1, 10000).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToList();

            Console.WriteLine("Get:" + data.Count.ToString());

            return data;
        }
        
        [HttpPost]
        public void Post(List<WeatherForecast> data)
        {
            Console.WriteLine("Post:" + data.Count.ToString());
        }

    }
}
