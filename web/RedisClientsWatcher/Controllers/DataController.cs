using Microsoft.AspNetCore.Mvc;
using RedisClientsWatcher.Controllers.Models;
using RedisClientsWatcher.Data;

namespace RedisClientsWatcher.Controllers;

[ApiController]
public class DataController : ControllerBase
{
    private readonly IRedisClient _redisClient;
    private readonly ILogger<DataController> _logger;

    public DataController(IRedisClient redisClient, ILogger<DataController> logger)
    {
        this._redisClient = redisClient;
        this._logger = logger;
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
        try
        {
            this._logger.LogInformation("Set idempotent value for key: {Key}, value: {Value} at {Stage}", parameterModel.Key, parameterModel.Value, "Entry");
            var valueInCache = await this._redisClient.SetIdempotentValue(parameterModel.Key, parameterModel.Value);

            // throw new Exception("test");

            this._logger.LogInformation("Set idempotent value for key: {Key}, value: {Value} at {Stage}", parameterModel.Key, parameterModel.Value, "Exit");

            return this.Ok(valueInCache);
        }
        catch (Exception e)
        {
            this._logger.LogError(e, "Error while setting idempotent value");

            return this.BadRequest("Error while setting idempotent value");
        }
    }
}