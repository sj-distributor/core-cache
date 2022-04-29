using Core.Driver;
using Core.Entity;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class CachingAttribute : Attribute, IFilterFactory
{
    private readonly Type _type;
    private readonly string _name;
    private readonly string _key;
    private readonly long _expire;
    private readonly string _contentType;

    public CachingAttribute(Type type, string name, string key, long expire = 0, string contentType = "" )
    {
        _type = type;
        _name = name;
        _key = key;
        _expire = expire;
        _contentType = contentType;
    }

    public bool IsReusable { get; }

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        var cacheClient = serviceProvider.GetService<ICacheClient>();
        if (cacheClient == null) throw new Exception($"Error: Type {_type.FullName} Must Be Found In Di Container");
        var instance = ActivatorUtilities.CreateInstance(
            serviceProvider,
            _type,
            new CacheableSettings() { Name = _name, Key = _key, Expire = _expire, ContentType = _contentType},
            cacheClient);
        return (IFilterMetadata)instance;
    }
}