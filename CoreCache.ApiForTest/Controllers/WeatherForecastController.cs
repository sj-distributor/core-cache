using Core.Attributes;
using CoreCache.ApiForTest.Entity;
using Microsoft.AspNetCore.Mvc;

namespace CoreCache.ApiForTest.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    [Route("/"), HttpGet]
    [Caching(typeof(Cacheable), "anything", "QueryId:{id}", TimeSpan.TicksPerSecond * 2)]
    public IEnumerable<WeatherForecast> Get([FromQuery] string id)
    {
        return GetData();
    }
    
    [Route("/evict-and-cache"), HttpGet]
    [Caching(typeof(Cacheable), "anson", "QueryId:{id}")]
    [Evicting(typeof(CacheEvict), new []{"anything"}, "QueryId:{id}")]
    public IEnumerable<WeatherForecast> Get2([FromQuery] string id)
    {
        Console.WriteLine("action...");
        return GetData();
    }

    [Route("/"), HttpPost]
    [Evicting(typeof(CacheEvict), new []{"anything"}, "QueryId:{id}")]
    public ResponseResult<IEnumerable<WeatherForecast>> Post([FromQuery] string id)
    {
        return ResponseResult<IEnumerable<WeatherForecast>>.SuccessResult(GetData());
    }
    
    [Route("/users"), HttpPost]
    [Caching(typeof(Cacheable), "anything", "user:1:id")]
    public ResponseResult<IEnumerable<WeatherForecast>> GetUsers([FromBody] List<User> users)
    {
        return ResponseResult<IEnumerable<WeatherForecast>>.SuccessResult(GetData());
    }

    
    
    

    private WeatherForecast[] GetData()
    {
        var summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = summaries[Random.Shared.Next(summaries.Length)]
            })
            .ToArray();
    }
}