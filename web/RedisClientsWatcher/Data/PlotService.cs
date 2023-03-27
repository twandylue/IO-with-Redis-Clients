using System.Text.Json;
using RedisClientsWatcher.Data.Models;
using RedisClientsWatcher.Loggers;

namespace RedisClientsWatcher.Data;

public class PlotService
{
    /// <summary>
    /// Read the data(log) from the file and parse it to ThreadPoolLogModel.
    /// </summary>
    /// <param name="fileName">"./logs/ThreadPool/*.jsonl"</param>
    /// <returns></returns>
    public static IList<ThreadPoolPlotModel> ParseThreadPoolLog(string fileName)
    {
        using var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var sr = new StreamReader(fs);
        var lines = new List<string>();
        while (sr.ReadLine() is { } line)
        {
            lines.Add(line);
        }

        var data = lines
                    .Where(line =>
                    {
                        var e = JsonSerializer.Deserialize<JsonElement>(line);
                        var currentTime = DateTime.UtcNow;
                        var time = DateTimeOffset.Parse(e.GetProperty("Timestamp").ToString());
                        return time < currentTime;
                    })
                    .Select(line =>
                    {
                        var e = JsonSerializer.Deserialize<JsonElement>(line);
                        var temp = JsonSerializer.Deserialize<LogTemplate>(e.GetProperty("MessageTemplate").ToString());
                        if (temp == null)
                        {
                            return null;
                        }

                        var results = new ThreadPoolPlotModel
                        {
                            BusyIoThreads = temp.BusyIoThreads,
                            MinIoThreads = temp.MinIoThreads,
                            MaxIoThreads = temp.MaxIoThreads,
                            BusyWorkerThreads = temp.BusyWorkerThreads,
                            MinWorkerThreads = temp.MinWorkerThreads,
                            MaxWorkerThreads = temp.MaxWorkerThreads,
                            Time = e.GetProperty("Timestamp").ToString()
                        };

                        return results;
                    })
                    .Where(x => x != null)
                    .ToList();

        return data!;
    }

    public static IList<DataControllerExceptionPlotModel> ParseDataControllerExceptionLog(string filePath)
    {
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var sr = new StreamReader(fs);
        var lines = new List<string>();
        while (sr.ReadLine() is { } line)
        {
            lines.Add(line);
        }

        var count = 0;
        var data = lines
            .Where(line =>
            {
                var e = JsonSerializer.Deserialize<JsonElement>(line);
                var currentTime = DateTime.UtcNow;
                var time = DateTimeOffset.Parse(e.GetProperty("Timestamp").ToString());
                return time < currentTime;
            })
            .Where(line =>
            {
                var e = JsonSerializer.Deserialize<JsonElement>(line);
                return e.TryGetProperty("Exception", out var _);
            })
            .Select(line =>
            {
                var e = JsonSerializer.Deserialize<JsonElement>(line);
                count++;

                return new DataControllerExceptionPlotModel
                {
                    Count = count,
                    Msg = e.GetProperty("Exception").ToString(),
                    Time = e.GetProperty("Timestamp").ToString()
                };
            })
            .ToList();

        return data;
    }
}
