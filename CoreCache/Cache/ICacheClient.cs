namespace CoreCache.Cache;

public interface ICacheClient
{
    void Set(string key, string value, long duration);

    string Get(string key);

    void Delete(string key);
}