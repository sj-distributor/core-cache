using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using Core.Entity;
using Core.Enum;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Driver;
public class MultiBucketsMemoryCache : ICacheClient
{
    private readonly uint _buckets;
    private readonly MaxMemoryPolicy _maxMemoryPolicy;
    private readonly uint _bucketMaxCapacity;
    private static int _cleanupRange;

    private Dictionary<uint, ConcurrentDictionary<string, CacheItem>> _map = new();

    public MultiBucketsMemoryCache(uint buckets = 5, uint bucketMaxCapacity = 500000,
        MaxMemoryPolicy maxMemoryPolicy = MaxMemoryPolicy.LRU, int cleanUpPercentage = 10)
    {
        if (buckets > 128)
        {
            throw new Exception("buckets must less than 128");
        }

        _buckets = buckets;
        _bucketMaxCapacity = bucketMaxCapacity;
        _maxMemoryPolicy = maxMemoryPolicy;
        _cleanupRange = (int)(bucketMaxCapacity - (bucketMaxCapacity / cleanUpPercentage));
        InitBucket(_map, _buckets);
    }

    public ValueTask Set(string key, string value, long expire = 0)
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
            CreatedAt = DateTime.Now.Ticks,
            ExpireAt = expire > 0 ? DateTime.Now.AddSeconds(expire).Ticks : DateTime.Now.AddYears(1).Ticks
        });

        return ValueTask.CompletedTask;
    }

    public ValueTask<string> Get(string key)
    {
        var bucket = GetBucket(HashKey(key));

        if (!bucket.TryGetValue(key, out var cacheItem)) return ValueTask.FromResult("");
        if (cacheItem.ExpireAt < DateTime.Now.Ticks)
        {
            bucket.Remove(key,  out  _);
            return ValueTask.FromResult("");
        }

        ++cacheItem.Hits;
        return ValueTask.FromResult(cacheItem.Value);
    }

    public ValueTask Delete(string key)
    {
        if (key.Contains('*'))
        {
            if (key.First() == '*')
            {
                key = key.Substring(1, key.Length - 1);
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
        return BitConverter.ToUInt32(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(key)), 0) % _buckets;
    }
}

public static class MultiBucketsCoreCacheExtension
{
    public static void AddMultiBucketsCoreCache(
        this IServiceCollection services,
        uint buckets = 5,
        uint maxCapacity = 500000,
        MaxMemoryPolicy maxMemoryPolicy = MaxMemoryPolicy.LRU,
        int cleanUpPercentage = 10)
    {
        services.AddSingleton<ICacheClient>(
            new MultiBucketsMemoryCache(buckets, maxCapacity, maxMemoryPolicy, cleanUpPercentage));
    }
}