namespace Core.Entity;

public class CacheItem
{
    public string Value { get; set; }

    public DateTime CreatedAt { get; set; }

    public int Hits = 0;
}