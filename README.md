# NetCoreCache

[![Build Status](https://github.com/sj-distributor/core-cache//actions/workflows/build.yml/badge.svg?branch=master)](https://github.com/sj-distributor/core-cache/actions?query=branch%3Amaster)
[![codecov](https://codecov.io/gh/sj-distributor/core-cache/branch/master/graph/badge.svg?token=ELTCS7STTN)](https://codecov.io/gh/sj-distributor/core-cache)
[![NuGet version (NetCoreCache)](https://img.shields.io/nuget/v/NetCoreCache.svg?style=flat-square)](https://www.nuget.org/packages/NetCoreCache/)
![](https://img.shields.io/badge/license-MIT-green)

## 🔥Core Api And MVC Cache🔥

* Easy use of caching with dotnet core
* Fast, concurrent, evicting in-memory cache written to keep big number of entries without impact on performance.

## ⏱ About Cache expiration
* There are three ways of cache eviction, `LRU` and `TTL` and `Random`

## 🪣 About BigCache ( Multi Buckets )
* Fast. Performance scales on multi-core CPUs.
* The cache consists of many buckets, each with its own lock. This helps scaling the performance on multi-core CPUs,
  since multiple CPUs may concurrently access distinct buckets.
* `NetCoreCache` automatically evicts old entries when reaching the maximum cache size set on its creation.

## 🤟Install

```
PM     : Install-Package NetCoreCache
Net CLI: dotnet add package NetCoreCache
```

## 🚀 Quick start

```c#
// Program.cs
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCoreCache();


// UserController.cs
[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    [Route("/"), HttpGet]
    [Caching(typeof(Cacheable), "user", "{id}", 2)] // Cache expires after two seconds
    public User Get([FromQuery] string id)
    {
        return DataUtils.GetData();
    }
}
```

## Active cache eviction

```c#
// UserController.cs
[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    [Route("/"), HttpGet]
    [Caching(typeof(Cacheable), "user", "{id}")] // Long term cache
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

## 🎬 ️️Match both uses

```c#
// **** ‼️ If the cache is hit, 'Evicting' will only be executed once ‼️ ****

[Route("/evict-and-cache"), HttpGet]
[Caching(typeof(Cacheable), "anson", "QueryId:{id}")]
[Evicting(typeof(CacheEvict), new[] { "anything" }, "QueryId:{id}")]
public IEnumerable<WeatherForecast> Get([FromQuery] string id)
{
    return DataUtils.GetData();
}



// **** ‼️ Evicting will always execute ‼️ ****

[Route("/evict-and-cache"), HttpGet]
[Evicting(typeof(CacheEvict), new[] { "anything" }, "QueryId:{id}")]
[Caching(typeof(Cacheable), "anson", "QueryId:{id}")]
public IEnumerable<WeatherForecast> Get([FromQuery] string id)
{
    return DataUtils.GetData();
}
```

## 🎃 Parameter Description

```c#
// MemoryCache
public static void AddCoreCache(
    this IServiceCollection services,                
    uint bucketMaxCapacity = 1000000,
    MaxMemoryPolicy maxMemoryPolicy = MaxMemoryPolicy.LRU,
    int cleanUpPercentage = 10
)
{
   services.AddSingleton<ICacheClient>(new MemoryCache(bucketMaxCapacity, maxMemoryPolicy, cleanUpPercentage));
}

// MulitBucketsMemoryCache
public static void AddMultiBucketsCoreCache(
        this IServiceCollection services,
        uint buckets = 5,
        uint maxCapacity = 500000,
        MaxMemoryPolicy maxMemoryPolicy = MaxMemoryPolicy.LRU,
        int cleanUpPercentage = 10)
    {
        services.AddSingleton<ICacheClient>(
            new MultiBucketsMemoryCache(buckets, maxCapacity, maxMemoryPolicy, cleanUpPercentage));
    }
```

|                          Parameter                           | Type |       Default       | Require | Explain                                                                                                                                     |
|:------------------------------------------------------------:|:----:|:-------------------:|:-------:|---------------------------------------------------------------------------------------------------------------------------------------------|
| `buckets` | uint | 5 | false | The number of containers to store the cache, up to 128                                                                                      |
|                     `bucketMaxCapacity`                      | uint |       1000000       |  false  | (MemoryCache) Initialize capacity <br/>  <br/> (MulitBucketsMemroyCache) The capacity of each barrel, it is recommended that 500,000 ~ 1,000,000 |
|                      `maxMemoryPolicy`                       | MaxMemoryPolicy | MaxMemoryPolicy.LRU |  false  | LRU = Least Recently Used , TTL = Time To Live, Or RANDOM                                                                                   |
|                     `cleanUpPercentage`                      | int |         10          |  false  | After the capacity is removed, the percentage deleted                                                                                       |  

## Variable explanation

```js
// foo:bar:1 -> "item1"
{
    "foo"
:
    {
        "bar"
    :
        [
            "item1",
            "qux"
        ]
    }
}

// foo:bar:0:url -> "test.weather.com"
{
    "foo"
:
    {
        "bar"
    :
        [
            {
                "url": "test.weather.com",
                "key": "DEV1234567"
            }
        ]
    }
}
```
