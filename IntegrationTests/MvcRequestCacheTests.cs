using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using CoreCache.ApiForTest.Entity;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace IntegrationTests;

[Collection("Sequential")]
public class MvcRequestCacheTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _httpClient;

    public MvcRequestCacheTests(WebApplicationFactory<Program> factory)
    {
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async void MvcRequestCanCache()
    {
        var resp1 = await _httpClient.GetAsync("/home/index?id=1");
        var result1 = await resp1.Content.ReadAsStringAsync();

        var resp2 = await _httpClient.GetAsync("/home/index?id=1");
        var result2 = await resp2.Content.ReadAsStringAsync();

        Assert.Equal(result1, result2);
    }

    [Fact]
    public async void MvcRequestCanTriggerEvict()
    {
        var resp1 = await _httpClient.GetAsync("/home/index?id=3");
        var result1 = await resp1.Content.ReadAsStringAsync();

        await _httpClient.GetAsync("/home/tow?id=3");

        var resp3 = await _httpClient.GetAsync("/home/index?id=3");
        var result3 = await resp3.Content.ReadAsStringAsync();

        Assert.NotEqual(result1, result3);
    }
}