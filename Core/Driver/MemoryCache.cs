using System.Collections.Concurrent;

namespace Core.Driver;

public class MemoryCache : ICacheClient
{
    private static readonly ConcurrentDictionary<string, string> _dict = new();

    public void Set(string key, string value, long expire = 0)
    {
        if (!_dict.ContainsKey(key))
        {
            _dict.TryAdd(key, value);

            if (expire > 0)
            {
                new Timer(_ =>
                {
                    Delete(key);
                    Console.WriteLine($"DoEvicted: {key}");
                }, null, expire / 10000, -1);
            }
        }
    }

    public string Get(string key)
    {
        var tryGetValue = _dict.TryGetValue(key, out var value);
        return (tryGetValue ? value : "")!;
    }

    public void Delete(string key)
    {
        if (key.First() == '*')
        {
            key = key.Substring(1, key.Length);
        }
        else if (key.Last() == '*')
        {
            key = key.Substring(0, key.Length - 1);
        }

        _dict.Keys.Where(x => x.Contains(key)).ToList().ForEach(k => { _dict.TryRemove(k, out _); });
    }
}