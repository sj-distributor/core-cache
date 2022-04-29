using System;
using System.Threading.Tasks;
using Core.Driver;
using Xunit;

namespace UnitTests;

[Collection("Sequential")]
public class MemoryCacheTests
{
    public MemoryCache _memoryCache;

    public MemoryCacheTests()
    {
        _memoryCache = new MemoryCache();
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
    [InlineData("key1", "18", "", TimeSpan.TicksPerSecond * 1)]
    [InlineData("key2", "19", "", TimeSpan.TicksPerSecond * 1)]
    public async void TestMemoryCacheCanSetTimeout(string key, string value, string result, long expire = 0)
    {
       await _memoryCache.Set(key, value, expire);

        await Task.Delay(TimeSpan.FromSeconds(1.5));

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