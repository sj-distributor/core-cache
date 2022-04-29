using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using Core.Entity;

namespace Core.Driver;

public class MemoryCache : ICacheClient
{
    private readonly uint _buckets;

    private Dictionary<uint, ConcurrentDictionary<string, CacheItem>> _map = new();

    public MemoryCache(uint buckets = 5)
    {
        if (buckets > 128)
        {
            throw new Exception("buckets must less than 128");
        }

        _buckets = buckets;
        InitBucket(_map, _buckets);
    }

    public void Set(string key, string value, long expire = 0)
    {
        var bucket = GetBucket(HashKey(key));

        if (!bucket.ContainsKey(key))
        {
            if (expire > 0)
            {
                bucket.TryAdd(key, new CacheItem()
                {
                    Value = value,
                    Timeout = DateTime.Now.AddMilliseconds(expire / 10000)
                });
            }
            else
            {
                bucket.TryAdd(key, new CacheItem()
                {
                    Value = value,
                    Timeout = null
                });
            }
        }
    }

    private void EvictLoop(Dictionary<uint, ConcurrentDictionary<string, CacheItem>> map)
    {
        foreach (var bucketId in map.Keys)
        {
            var bucket = GetBucket(bucketId);
            new Timer((bk) =>
            {
                DeleteTimeout(bk as ConcurrentDictionary<string, CacheItem>);
            }, bucket, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        }
    }

    public string Get(string key)
    {
        var bucket = GetBucket(HashKey(key));

        if (!bucket.TryGetValue(key, out var cacheItem)) return "";
        if (!(DateTime.Now > cacheItem.Timeout) || cacheItem.Timeout == null) return cacheItem.Value;
        bucket.TryRemove(key, out var _);
        return "";
    }

    public void Delete(string key)
    {
        if (key.Contains('*'))
        {
            if (key.First() == '*')
            {
                key = key.Substring(1, key.Length);
            }
            else if (key.Last() == '*')
            {
                key = key[..^1];
            }

            foreach (var bucket in _map.Keys.Select(bucketId => GetBucket(bucketId)))
            {
                bucket.Keys.Where(x => x.Contains(key)).ToList().ForEach(k => bucket.TryRemove(k, out var _));
            }
        }
        else
        {
            var bucket = GetBucket(HashKey(key));
            bucket.Keys.Where(x => x == key).ToList().ForEach(k => bucket.TryRemove(k, out var _));
        }
    }

    private void DeleteTimeout(ConcurrentDictionary<string, CacheItem> bucket)
    {
        var now = DateTime.Now;
        foreach (var key in bucket.Keys)
        {
            if (!bucket.TryGetValue(key, out var value)) continue;
            if (!(now > value.Timeout)) continue;
            bucket.TryRemove(key, out var _);
        }
    }

    private void InitBucket(Dictionary<uint, ConcurrentDictionary<string, CacheItem>> map, uint buckets)
    {
        for (uint i = 0; i < buckets; i++)
        {
            map.Add(i, new ConcurrentDictionary<string, CacheItem>());
        }

        EvictLoop(map);
    }

    private ConcurrentDictionary<string, CacheItem> GetBucket(uint bucketId)
    {
        if (_map.TryGetValue(bucketId, out var bucket))
        {
            return bucket;
        }

        throw new Exception($"Not Found Bucket: {bucketId}");
    }

    private uint HashKey(string key)
    {
        byte[] encoded = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(key));
        var value = BitConverter.ToUInt32(encoded, 0) % _buckets;
        return value;
    }
}