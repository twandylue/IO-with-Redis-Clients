using System.Text.Json;
using RedisClientsWatcher.Loggers;

namespace RedisClientsWatcher.Data;

public class PlotService
{
    /// <summary>
    /// Read the data(log) from the file and parse it to LogTemplate
    /// </summary>
    /// <param name="fileName">"./logs/threadPool/*.jsonl"</param>
    /// <returns></returns>
    public static IList<LogTemplate> ParseLog(string fileName)
    {
        var r = File.ReadLines(fileName);
        var data = r
                    .Select(line =>
                    {
                        var e = JsonSerializer.Deserialize<JsonElement>(line);
                        return JsonSerializer.Deserialize<LogTemplate>(e.GetProperty("MessageTemplate").ToString());
                    })
                    .Where(d => d != null).ToList();

        return data!;
    }
}
