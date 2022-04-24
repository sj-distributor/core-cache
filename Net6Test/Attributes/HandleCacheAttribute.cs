using Microsoft.AspNetCore.Mvc.Filters;
using Net6Test.Cache;
using Net6Test.Tools;

namespace Net6Test.Attributes;
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class HandleCacheAttribute : Attribute, IFilterFactory, IFilterMetadata
{
    private readonly Type _type;
    private readonly string _name;
    private readonly string _key;

    public HandleCacheAttribute(Type type, string name, string key)
    {
        _type = type;
        _name = name;
        _key = key;
    }

    public bool IsReusable { get; }

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        var cacheClient = serviceProvider.GetService<ICacheClient>();
        
        if (cacheClient == null) throw new Exception($"Error: Type {_type.FullName} Must Be Found In Di Container");
        
        //通过反射实例化该过滤器的类
        var objFactory = ActivatorUtilities.CreateInstance(serviceProvider, _type,
            new object[] { _name, _key, cacheClient });
        return (IFilterMetadata)objFactory;

    }
}