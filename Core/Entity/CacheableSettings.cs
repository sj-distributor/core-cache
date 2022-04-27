namespace Core.Entity;

public class CacheableSettings
{
    public string Name { get; set; }
    public string Key { get; set; }
    
    public long Expire = 0;
}

public class CacheEvictSettings
{
    public string[] Name { get; set; }
    public string Key { get; set; }
}