using System;
using System.Threading;
using Core.Driver;
using NSubstitute;
using Xunit;

namespace UnitTests;

public class MemoryCacheTests
{

    public MemoryCache _memoryCache;
    
    public MemoryCacheTests()
    {
        _memoryCache = Substitute.For<MemoryCache>();
    }

    [Theory]
    [InlineData("anson", "18", "18")]
    [InlineData("anson1", "19", "19")]
    public void TestMemoryCacheCanSet(string key, string value, string result)
    {
        _memoryCache.Set(key, value);
        var s = _memoryCache.Get(key);
        Assert.Equal(s, result);
    }
    
    [Theory]
    [InlineData("key", "18", "", TimeSpan.TicksPerSecond * 1)]
    [InlineData("key1", "19", "", TimeSpan.TicksPerSecond * 1)]
    public void TestMemoryCacheCanSetTimeout(string key, string value, string result, long expire = 0)
    {
        _memoryCache.Set(key, value, expire);
        Thread.Sleep(TimeSpan.FromSeconds(1.5));
        var s = _memoryCache.Get(key);
        Assert.Equal(s, result);
    }
    
    [Theory]
    [InlineData("anson", "18", "")]
    [InlineData("anson1", "19", "")]
    public void TestMemoryCacheCanDelete(string key, string value, string result)
    {
        _memoryCache.Set(key, value);
        _memoryCache.Delete(key);
        var s = _memoryCache.Get(key);
        Assert.Equal(s, result);
    }
    
    [Theory]
    [InlineData("anson1111", "18", "")]
    [InlineData("anson2222", "19", "")]
    public void TestMemoryCacheCanDeleteByPattern(string key, string value, string result)
    {
        _memoryCache.Set(key, value);
        _memoryCache.Delete("anson*");
        var s = _memoryCache.Get(key);
        Assert.Equal(s, result);
    }
}