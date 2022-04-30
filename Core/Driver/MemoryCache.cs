using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using Core.Entity;
using Core.Enum;

namespace Core.Driver;

public class MemoryCache : ICacheClient
{
    private readonly uint _buckets;
    private readonly MaxMemoryPolicy _maxMemoryPolicy;
    private readonly uint _bucketMaxCapacity;
    private static int _cleanupRange;

    private Dictionary<uint, ConcurrentDictionary<string, CacheItem>> _map = new();

    public MemoryCache(uint buckets = 5, uint bucketMaxCapacity = 500000,
        MaxMemoryPolicy maxMemoryPolicy = MaxMemoryPolicy.LRU, int cleanUpPercentage = 10)
    {
        if (buckets > 128)
        {
            throw new Exception("buckets must less than 128");
        }

        _buckets = buckets;
        _bucketMaxCapacity = bucketMaxCapacity;
        _maxMemoryPolicy = maxMemoryPolicy;
        _cleanupRange = (int) (bucketMaxCapacity - (bucketMaxCapacity / cleanUpPercentage));
        InitBucket(_map, _buckets);
    }

    public ValueTask Set(string key, string value, long _ = 0)
    {
        var bucket = GetBucket(HashKey(key));

        if (bucket.ContainsKey(key)) return ValueTask.CompletedTask;
        if (bucket.Count >= _bucketMaxCapacity)
        {
            ReleaseCached(bucket);
        }

        bucket.TryAdd(key, new CacheItem
        {
            Value = value,
            CreatedAt = DateTime.Now,
        });

        return ValueTask.CompletedTask;
    }

    public ValueTask<string> Get(string key)
    {
        var bucket = GetBucket(HashKey(key));

        if (!bucket.TryGetValue(key, out var cacheItem)) return ValueTask.FromResult("");

        ++cacheItem.Hits;
        return ValueTask.FromResult(cacheItem.Value);
    }

    public ValueTask Delete(string key)
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
            GetBucket(HashKey(key)).TryRemove(key, out var _);
        }

        return ValueTask.CompletedTask;
    }

    private void InitBucket(Dictionary<uint, ConcurrentDictionary<string, CacheItem>> map, uint buckets)
    {
        for (uint i = 0; i < buckets; i++)
        {
            map.Add(i, new ConcurrentDictionary<string, CacheItem>());
        }
    }

    private ConcurrentDictionary<string, CacheItem> GetBucket(uint bucketId)
    {
        if (_map.TryGetValue(bucketId, out var bucket))
        {
            return bucket;
        }

        throw new Exception($"Not Found Bucket: {bucketId}");
    }

    private void ReleaseCached(ConcurrentDictionary<string, CacheItem> bucket)
    {
        var bucketCount = bucket.Count;

        if (_maxMemoryPolicy == MaxMemoryPolicy.RANDOM)
        {
            foreach (var key in bucket.Keys.Take(new Range(_cleanupRange, bucketCount)))
            {
                bucket.Remove(key, out var _);
            }
        }
        else
        {
            IOrderedEnumerable<KeyValuePair<string, CacheItem>> keyValuePairs;
            if (_maxMemoryPolicy == MaxMemoryPolicy.LRU)
            {
                keyValuePairs = bucket.OrderByDescending(
                    x => x.Value.Hits
                );
            }
            else
            {
                keyValuePairs = bucket.OrderByDescending(
                    x => x.Value.CreatedAt
                );
            }

            foreach (var keyValuePair in keyValuePairs.Take(new Range(_cleanupRange, bucketCount)))
            {
                bucket.Remove(keyValuePair.Key, out var _);
            }
        }
    }

    public Dictionary<uint, ConcurrentDictionary<string, CacheItem>> GetBuckets()
    {
        return _map;
    }

    private uint HashKey(string key)
    {
        byte[] encoded = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(key));
        var value = BitConverter.ToUInt32(encoded, 0) % _buckets;
        return value;
    }
}