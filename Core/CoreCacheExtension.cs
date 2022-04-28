using Core.Driver;
using Core.Entity;
using Microsoft.Extensions.DependencyInjection;

namespace Core;

public static class CoreCacheExtension
{
    public static void AddCoreCache(this IServiceCollection services)
    {
        services.AddSingleton<ICacheClient>(new MemoryCache());
    }
}