using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Driver;
using Core.Enum;
using Xunit;

namespace UnitTests;

public class MemoryCacheTests
{
    public MemoryCache _memoryCache;

    public MemoryCacheTests()
    {
        _memoryCache = new MemoryCache( 50, MaxMemoryPolicy.TTL);
    }

    [Theory]
    [InlineData(MaxMemoryPolicy.LRU)]
    [InlineData(MaxMemoryPolicy.TTL)]
    [InlineData(MaxMemoryPolicy.RANDOM)]
    public void TestWhenTheMemoryIsFull_EliminatedSuccess(MaxMemoryPolicy maxMemoryPolicy)
    {
        var memoryCache = new MemoryCache(50, maxMemoryPolicy);
        for (var i = 0; i < 50; i++)
        {
            Thread.Sleep(TimeSpan.FromSeconds(0.1));
            memoryCache.Set($"{i}", $"{i}");
            for (var j = 0; j < i; j++)
            {
                memoryCache.Get($"{i}");
            }
        }
        Assert.Equal(memoryCache.GetBuckets().Count, 50);
        memoryCache.Set("100", "100");
        Assert.Equal(memoryCache.GetBuckets().Count, 50 - (50 / 10) + 1);
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
    public async void TestMemoryCacheCanDeleteByLastPattern(string key, string value, string result)
    {
        await _memoryCache.Set(key, value);
        await _memoryCache.Delete("anson*");
        var s = await _memoryCache.Get(key);
        Assert.Equal(s, result);
    }
    
    [Theory]
    [InlineData("1111Joe", "18", "")]
    [InlineData("2222Joe", "19", "")]
    public async void TestMemoryCacheCanDeleteByFirstPattern(string key, string value, string result)
    {
        await _memoryCache.Set(key, value);
        await _memoryCache.Delete("*Joe");
        var s = await _memoryCache.Get(key);
        Assert.Equal(s, result);
    }
    
    [Theory]
    [InlineData("anson10", "18", "", 1)]
    [InlineData("anson11", "19", "", 1)]
    public async void TestMemoryCacheCanExpire(string key, string value, string result, long expire)
    {
        await _memoryCache.Set(key, value, expire);

        await Task.Run(() => Thread.Sleep(TimeSpan.FromSeconds(2)));
        
        var s = await _memoryCache.Get(key);
        Assert.Equal(s, result);
    }
}