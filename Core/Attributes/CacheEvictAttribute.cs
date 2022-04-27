using Core.Driver;
using Core.Entity;
using Core.Utils;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Core.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class CacheEvict : Attribute, IAsyncActionFilter
{
    private readonly string[] _names;
    private readonly string _key;
    private readonly ICacheClient _cacheClient;

    public CacheEvict(CacheEvictSettings cacheEvictSettings, ICacheClient cacheClient)
    {
        _names = cacheEvictSettings.Name;
        _key = cacheEvictSettings.Key;
        _cacheClient = cacheClient;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        await next();
        
        _names.ToList().ForEach(name =>
        {
            _cacheClient.Delete(KeyGenerateHelper.GetKey(name, _key, context.ActionArguments));
        });
        
    }
}