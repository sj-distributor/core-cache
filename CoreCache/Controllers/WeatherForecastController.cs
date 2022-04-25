using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using CoreCache.Attributes;
using CoreCache.Entity;
using Type = System.Type;

namespace CoreCache.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }
    
    [Route("/"), HttpGet]
    [HandleCache(typeof(Cacheable), "user", "PostId:{user:id} QueryId: {id}", TimeSpan.TicksPerSecond * 10 )]
    public IEnumerable<WeatherForecast> Get([FromBody]User user, [FromQuery]string id)
    {
        Console.WriteLine($"Req start:  {DateTime.Now.Millisecond}");

        var result = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        Console.WriteLine($"Req end:  {DateTime.Now.Millisecond}");
        return result;
    }

}