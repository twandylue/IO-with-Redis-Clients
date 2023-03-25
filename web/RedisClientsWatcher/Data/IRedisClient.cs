namespace RedisClientsWatcher.Data;

public interface IRedisClient
{
    /// <summary>
    /// Get the value of the key
    /// </summary>
    /// <returns></returns>
    Task<string> Get(string key);

    /// <summary>
    /// Set the value of the key
    /// </summary>
    /// <returns></returns>
    Task Set(string key, string value);

    /// <summary>
    /// Set the idempotent value of the key
    /// </summary>
    /// <returns></returns>
    Task<string> SetIdempotentValue(string key, string value);
}