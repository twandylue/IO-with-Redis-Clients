using Microsoft.AspNetCore.Mvc;
using RedisClientsWatcher.Controllers.Models;
using RedisClientsWatcher.Data;

namespace RedisClientsWatcher.Controllers;

[ApiController]
public class DataController : ControllerBase
{
    private readonly IRedisClient _redisClient;

    public DataController(IRedisClient redisClient)
    {
        this._redisClient = redisClient;
    }

    [HttpGet("~/api/v1/data/get/{key}")]
    public async Task<IActionResult> Get(string key)
    {
        var value = await this._redisClient.Get(key);

        return this.Ok(value);
    }

    [HttpPost("~/api/v1/data")]
    public async Task<IActionResult> Set([FromBody] SetValueParameterModel parameterModel)
    {
        await this._redisClient.Set(parameterModel.Key, parameterModel.Value);

        return this.Ok();
    }

    [HttpPost("~/api/v1/data/idempotent-value")]
    public async Task<IActionResult> SetIdempotentValue([FromBody] SetValueParameterModel parameterModel)
    {
        var valueInCache = await this._redisClient.SetIdempotentValue(parameterModel.Key, parameterModel.Value);

        return this.Ok(valueInCache);
    }
}