using System.Collections.Concurrent;

namespace CoreCache.Cache;

public class MemoryCache : ICacheClient
{
    private static readonly ConcurrentDictionary<string, string> _dict = new ConcurrentDictionary<string, string>();

    public void Set(string key, string value, long duration = 0)
    {
        if (!_dict.ContainsKey(key))
        {
            _dict.TryAdd(key, value);

            if (duration > 0)
            {
                Console.WriteLine($"ActionStart:  {DateTime.Now.Second}");
                new Timer(_ =>
                {
                    Delete(key);
                    Console.WriteLine($"DoAction:  {DateTime.Now.Second}");
                }, null, duration, -1);
            }
        }
    }

    public string Get(string key)
    {
        var tryGetValue = _dict.TryGetValue(key, out var value);
        return tryGetValue ? value : "";
    }

    public void Delete(string key)
    {
        if (_dict.ContainsKey(key))
        {
            _dict.TryRemove(key, out _);
        }
    }
}