using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using CoreCache.Entity;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace CoreCache;

public class Test
{
    private readonly ITestOutputHelper _testOutputHelper;

    public Test(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void Test1()
    {
        var user = new User()
        {
            Id = "id",
            Name = "Anson",
        };

        var userString = JsonConvert.SerializeObject(user);

        var configurationBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(userString)));

        IConfiguration config = configurationBuilder.Build();
        
        
        _testOutputHelper.WriteLine(config["Name"]);
        
        
        
        var dict = new Dictionary<string, string[]>();
        var str = "{user.Name}{id}";
        var reg = new Regex("\\{([^\\}]*)\\}");
        var matches = reg.Matches(str);

        foreach (Match match in matches)
        {
            if (match.Value.Contains('.'))
            {
                var split = match.Value.Split('.');
            }
        }
    }

    public static Dictionary<string, string> ObjectToMap(object obj)
    {
        Dictionary<string, string> map = new Dictionary<string, string>();

        Type t = obj.GetType(); // 获取对象对应的类， 对应的类型

        var pi = t.GetProperties(BindingFlags.Public | BindingFlags.Instance); // 获取当前type公共属性

        foreach (var p in pi)
        {
            var m = p.GetGetMethod();

            if (m == null || !m.IsPublic) continue;
            // 进行判NULL处理
            if (m.Invoke(obj, new object[] { }) != null)
            {
                map.Add(p.Name, m.Invoke(obj, new object[] { }).ToString()); // 向字典添加元素
            }
        }

        return map;
    }
}