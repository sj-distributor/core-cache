namespace Net6Test.Cache;

public interface ICacheClient
{
    void Set(string key, string value, int duration);

    string Get(string key);

    void Delete(string key);
}