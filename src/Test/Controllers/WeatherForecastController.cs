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
            "Ávbnfgd", "Épqrst", "Őijkl", "Őefgh", "Űabcd", "ŐASDFGH","áŐ76633225499","ü9HTUIDnbv","ÉüŰhfsf4422"
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
            var data = Enumerable.Range(1, 100000).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 85),
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
