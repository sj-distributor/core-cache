namespace Core.Driver;

public interface ICacheClient
{
    ValueTask Set(string key, string value, long expire = 0);

    ValueTask<string> Get(string key);

    ValueTask Delete(string key);
}