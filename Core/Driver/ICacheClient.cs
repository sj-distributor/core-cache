namespace Core.Driver;

public interface ICacheClient
{
    void Set(string key, string value, long expire);

    string Get(string key);

    void Delete(string key);
}