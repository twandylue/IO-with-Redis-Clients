using StackExchange.Redis;

namespace RedisClientsWatcher.Data;

public class StackExchangeRedisClient : IRedisClient
{
    private readonly IDatabase _db;
    private readonly ILogger<StackExchangeRedisClient> _logger;

    public StackExchangeRedisClient(ILogger<StackExchangeRedisClient> logger)
    {
        this._logger = logger;
        var redis = ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING"));
        this._db = redis.GetDatabase();
    }

    public async Task<string> Get(string key)
    {
        this._logger.LogInformation("Getting value for key: {Key}", key);

        return await this._db.StringGetAsync(key);
    }

    public async Task Set(string key, string value)
    {
        this._logger.LogInformation("Setting value for key: {Key}, value: {Value}", key, value);

        await this._db.StringSetAsync(key, value);
    }

    public async Task<string> SetIdempotentValue(string key, string value)
    {
        this._logger.LogInformation("Set idempotent value for key: {Key}, value: {Value}", key, value);

        if (await this._db.KeyExistsAsync(key))
        {
            this._logger.LogInformation("Key: {Key} is already in the cache", key);
            var valueInCache = await this._db.StringGetAsync(key);

            return valueInCache;
        }

        this._logger.LogInformation("Setting value for key: {Key}, value: {Value}", key, value);
        await this._db.StringSetAsync(key, value);

        return value;
    }
}