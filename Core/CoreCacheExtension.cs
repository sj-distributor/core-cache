using Core.Driver;
using Core.Entity;
using Core.Enum;
using Microsoft.Extensions.DependencyInjection;

namespace Core;

public static class CoreCacheExtension
{
    public static void AddCoreCache(
        this IServiceCollection services,
        uint buckets = 5,
        uint bucketMaxCapacity = 500000,
        MaxMemoryPolicy maxMemoryPolicy = MaxMemoryPolicy.LRU,
        int cleanUpPercentage = 10
    )
    {
        services.AddSingleton<ICacheClient>(
            new MemoryCache(
                buckets,
                bucketMaxCapacity,
                maxMemoryPolicy,
                cleanUpPercentage
            )
        );
    }
}