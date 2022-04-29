# NetCoreCache

[![Build Status](https://github.com/sj-distributor/core-cache//actions/workflows/build.yml/badge.svg?branch=master)](https://github.com/sj-distributor/core-cache/actions?query=branch%3Amaster)
[![codecov](https://codecov.io/gh/sj-distributor/core-cache/branch/master/graph/badge.svg?token=ELTCS7STTN)](https://codecov.io/gh/sj-distributor/core-cache)
[![NuGet version (NetCoreCache)](https://img.shields.io/nuget/v/NetCoreCache.svg?style=flat-square)](https://www.nuget.org/packages/NetCoreCache/)
![](https://img.shields.io/badge/license-MIT-green)

## 🔥Core Api And MVC Cache🔥

* Easy use of caching with dotnet core
* Fast, concurrent, evicting in-memory cache written to keep big number of entries without impact on performance.
* The cache consists of many buckets, each with its own lock. This helps scaling the performance on multi-core CPUs, since multiple CPUs may concurrently access distinct buckets.

## 🤟Install 
```
PM     : Install-Package NetCoreCache
Net CLI: dotnet add package NetCoreCache
```

## 🚀Quick start

```
// Program.cs
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCoreCache();


// UserController.cs
[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    [Route("/"), HttpGet]
    [Caching(typeof(Cacheable), "user", "{id}")]
    public User Get([FromQuery] string id)
    {
        return DataUtils.GetData();
    }
}
```

## 🪣About BigCache ( Multi Buckets )
```
Services.AddCoreCache(5); // Default 5 buckets 
Services.AddCoreCache(128); // Maximum number of 128 buckets
```

## Cache automatic eviction

```
// UserController.cs
[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    [Route("/"), HttpGet]
    [Caching(typeof(Cacheable), "user", "{id}", TimeSpan.TicksPerSecond * 2)] // Cache expires after two seconds
    public User Get([FromQuery] string id)
    {
        return DataUtils.GetData();
    }
}

```

## Active cache eviction

```
// UserController.cs
[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    [Route("/"), HttpGet]
    [Caching(typeof(Cacheable), "user", "{id}", TimeSpan.TicksPerHour * 2)] // Cache expires after two hours
    public User Get([FromQuery] string id)
    {
        return DataUtils.GetData();
    }
 
 
    // Requesting this API will trigger the above API cache eviction 👆🏻👆🏻👆🏻
    [Route("/"), HttpPost]
    [Evicting(typeof(CacheEvict), new []{"user"}, "{user:id}")]
    public User Update([FromBody] User user)
    {
        return User;
    }   
}

```

## Match both uses

```
    [Route("/evict-and-cache"), HttpGet]
    [Caching(typeof(Cacheable), "anson", "QueryId:{id}")]
    [Evicting(typeof(CacheEvict), new[] { "anything" }, "QueryId:{id}")]
    public IEnumerable<WeatherForecast> Get([FromQuery] string id)
    {
        return DataUtils.GetData();
    }
```

## Variable explanation

```
// foo:bar:1 -> "item1"
{
   "foo": {
      "bar": [
         "item1",
         "qux"
     ]
   }
}

// foo:bar:0:url -> "test.weather.com"
{
   "foo": {
      "bar": [
         {
            "url": "test.weather.com",
            "key": "DEV1234567"
         }
     ]
   }
}
```
