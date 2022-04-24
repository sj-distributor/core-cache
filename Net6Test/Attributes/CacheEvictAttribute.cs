using Microsoft.AspNetCore.Mvc.Filters;
using Net6Test.Cache;

namespace Net6Test.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class CacheEvict : Attribute, IAsyncActionFilter
{
    private readonly string[] _name;
    private readonly string _key;
    private readonly ICacheClient _cacheClient;

    public CacheEvict(string[] name, string key,  ICacheClient cacheClient) 
    {
        _name = name;
        _key = key;
        _cacheClient = cacheClient;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // do logic generate key and do cache evict
        var executedContext = await next();
        
    }
}