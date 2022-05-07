namespace Core.Entity;

public class CacheItem
{
    public string Value { get; set; }

    public long CreatedAt { get; set; }

    public long ExpireAt { get; set; }

    public int Hits = 0;
}