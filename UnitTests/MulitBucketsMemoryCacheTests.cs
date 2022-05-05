using System;
using System.Threading;
using Core.Driver;
using Core.Enum;
using Xunit;

namespace UnitTests;

public class MultiBucketsMemoryCacheTests
{
    private MultiBucketsMemoryCache _memoryCache;

    public MultiBucketsMemoryCacheTests()
    {
        _memoryCache = new MultiBucketsMemoryCache();
    }
    
    [Theory]
    [InlineData(MaxMemoryPolicy.LRU)]
    [InlineData(MaxMemoryPolicy.TTL)]
    [InlineData(MaxMemoryPolicy.RANDOM)]
    public void TestWhenTheMemoryIsFull_EliminatedSuccess(MaxMemoryPolicy maxMemoryPolicy)
    {
        var memoryCache = new MultiBucketsMemoryCache(1, 50);
        for (var i = 0; i < 50; i++)
        {
            Thread.Sleep(TimeSpan.FromSeconds(0.1));
            memoryCache.Set($"{i}", $"{i}");
            for (var j = 0; j < i; j++)
            {
                memoryCache.Get($"{i}");
            }
        }

        memoryCache.GetBuckets().TryGetValue(0, out var bucket);
        

        Assert.Equal(50, bucket.Count);
        memoryCache.Set("100", "100");
        Assert.Equal(bucket.Count, 50 - (50 / 10) + 1);
    }

    [Theory]
    [InlineData("anson", "18", "18")]
    [InlineData("anson1", "19", "19")]
    public async void TestMemoryCacheCanSet(string key, string value, string result)
    {
        await _memoryCache.Set(key, value);
        var s = await _memoryCache.Get(key);
        Assert.Equal(s, result);
    }

    [Theory]
    [InlineData("anson", "18", "")]
    [InlineData("anson1", "19", "")]
    public async void TestMemoryCacheCanDelete(string key, string value, string result)
    {
        await _memoryCache.Set(key, value);
        await _memoryCache.Delete(key);
        var s = await _memoryCache.Get(key);
        Assert.Equal(s, result);
    }

    [Theory]
    [InlineData("anson1111", "18", "")]
    [InlineData("anson2222", "19", "")]
    public async void TestMemoryCacheCanDeleteByPattern(string key, string value, string result)
    {
        await _memoryCache.Set(key, value);
        await _memoryCache.Delete("anson*");
        var s = await _memoryCache.Get(key);
        Assert.Equal(s, result);
    }
}