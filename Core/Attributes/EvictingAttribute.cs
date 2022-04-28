using Core.Driver;
using Core.Entity;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Attributes;

public class EvictingAttribute : Attribute, IFilterFactory
{
    private readonly Type _type;
    private readonly string[] _names;
    private readonly string _key;

    public EvictingAttribute(Type type, string[] names, string key)
    {
        _type = type;
        _names = names;
        _key = key;
    }

    public bool IsReusable { get; }


    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        var cacheClient = serviceProvider.GetService<ICacheClient>();

        if (cacheClient == null)
            throw new Exception($"Error: Type {_type.FullName} Must Be Found In Di Container");

        var instance = ActivatorUtilities.CreateInstance(
            serviceProvider,
            _type,
            new CacheEvictSettings() { Name = _names, Key = _key},
            cacheClient);
        return (IFilterMetadata)instance;
    }
}