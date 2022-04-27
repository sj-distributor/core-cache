namespace Core.Driver;

public class RedisCache : ICacheClient
{
    
    
    
    public void Set(string key, string value, long expire)
    {
        throw new NotImplementedException();
    }

    public string Get(string key)
    {
        throw new NotImplementedException();
    }

    public void Delete(string key)
    {
        throw new NotImplementedException();
    }
}