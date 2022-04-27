using System.Collections.Generic;
using Core.Utils;
using Xunit;

namespace UnitTests;

public class KeyGenerateTests
{
    [Fact]
    public void TestKeyGenerate()
    {
        #region Test Data
        var list = new List<List<object>>()
        {
            new()
            {
                "anything", "QueryId:{id}",
                new Dictionary<string, object>() { { "id", 123 } },
                @"anything:QueryId:123",
            },
            new()
            {
                "user", "{obj:id}",
                new Dictionary<string, object>() { { "obj", new { Id = 123 }} },
                @"user:123",
            },
            new()
            {
                "user", "{obj:1:id}",
                new Dictionary<string, object>() { { "obj", new List<object>(){ new { Id = 123 }, new { Id = 234 } } }},
                @"user:234",
            },
        };

        #endregion

        list.ForEach(x =>
        {
            var key = KeyGenerateHelper.GetKey(x[0] as string, x[1] as string, x[2] as Dictionary<string, object>);
            Assert.Equal(key, x[3]);
        });

    }
}