using System.Collections.Concurrent;
using Core.Entity;
using Core.Enum;

namespace Core.Driver;

 public class MemoryCache : ICacheClient
    {
        private readonly int _maxCapacity;
        private readonly MaxMemoryPolicy _maxMemoryPolicy;
        private static int _cleanupRange;

        private static ConcurrentDictionary<string, CacheItem> _dist;

        public MemoryCache(int maxCapacity = 5000000, MaxMemoryPolicy maxMemoryPolicy = MaxMemoryPolicy.LRU, int cleanUpPercentage = 10)
        {
            _maxCapacity = maxCapacity;
            _maxMemoryPolicy = maxMemoryPolicy;
            _cleanupRange = _maxCapacity - (_maxCapacity / cleanUpPercentage);
            _dist = new ConcurrentDictionary<string, CacheItem>(Environment.ProcessorCount * 2, _maxCapacity);
        }

        public ValueTask Set(string key, string value, long _ = 0)
        {
            if (_dist.ContainsKey(key)) return ValueTask.CompletedTask;
            if (_dist.Count >= _maxCapacity)
            {
                ReleaseCached();
            }
            
            _dist.TryAdd(key, new CacheItem
            {
                Value = value,
                CreatedAt = DateTime.Now,
            });

            return ValueTask.CompletedTask;
        }

        public ValueTask<string> Get(string key)
        {
            if (!_dist.TryGetValue(key, out var cacheItem)) return ValueTask.FromResult("");

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

                _dist.Keys.Where(x => x.Contains(key)).ToList().ForEach(k => _dist.TryRemove(k, out var _));
            }
            else
            {
                _dist.TryRemove(key, out var _);
            }

            return ValueTask.CompletedTask;
        }

        private void ReleaseCached()
        {
            if (_dist.Count < _maxCapacity) return;

            var bucketCount = _dist.Count;

            if (_maxMemoryPolicy == MaxMemoryPolicy.RANDOM)
            {
                foreach (var key in _dist.Keys.Take(new Range(_cleanupRange, bucketCount)))
                {
                    _dist.Remove(key, out var _);
                }
            }
            else
            {
                IOrderedEnumerable<KeyValuePair<string, CacheItem>> keyValuePairs;
                if (_maxMemoryPolicy == MaxMemoryPolicy.LRU)
                {
                    keyValuePairs = _dist.OrderByDescending(
                        x => x.Value.Hits
                    );
                }
                else
                {
                    keyValuePairs = _dist.OrderByDescending(
                        x => x.Value.CreatedAt
                    );
                }

                foreach (var keyValuePair in keyValuePairs.Take(new Range(_cleanupRange, bucketCount)))
                {
                    _dist.Remove(keyValuePair.Key, out var _);
                }
            }
        }

        public ConcurrentDictionary<string, CacheItem> GetBuckets()
        {
            return _dist;
        }
    }