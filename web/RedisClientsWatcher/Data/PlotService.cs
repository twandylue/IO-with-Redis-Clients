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
        var r = File.ReadLines(fileName);
        var data = r
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
}
