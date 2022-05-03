using Core.Driver;
using Core.Enum;
using Microsoft.Extensions.DependencyInjection;

namespace Core;

public static class CoreCacheExtension
{
    public static void AddCoreCache(
        this IServiceCollection services, int maxCapacity = 1000000,
        MaxMemoryPolicy maxMemoryPolicy = MaxMemoryPolicy.LRU, int cleanUpPercentage = 10
    )
    {
        services.AddSingleton<ICacheClient>(new MemoryCache(maxCapacity, maxMemoryPolicy, cleanUpPercentage));
    }
}