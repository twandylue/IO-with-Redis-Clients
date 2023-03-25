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
        this._logger.LogInformation("Setting value for key: {Key}", key);

        await this._db.StringSetAsync(key, value);
    }
}