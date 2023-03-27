// ref: https://github.com/JonCole/SampleCode/blob/master/ThreadPoolMonitor/ThreadPoolLogger.cs

using System.Text.Json;

namespace RedisClientsWatcher.Loggers;

class ThreadPoolLogger : IDisposable
{
    private TimeSpan _logFrequency;
    private bool _disposed;
    private ILogger<ThreadPoolLogger> _logger;

    public ThreadPoolLogger(TimeSpan logFrequency, ILogger<ThreadPoolLogger> logger)
    {
        if (logFrequency <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException("logFrequency");
        }

        this._logFrequency = logFrequency;
        this._logger = logger;
        this.StartLogging();
    }

    private async void StartLogging()
    {
        try
        {
            while (!this._disposed)
            {
                await Task.Delay(this._logFrequency);

                var stats = GetThreadPoolStats();

                this.LogUsage(stats);
            }
        }
        catch (Exception)
        {
            // ignored
        }
    }

    protected virtual void LogUsage(ThreadPoolUsageStats stats)
    {
        var message = string.Format("[{0}] IOCP:(Busy={1},Min={2},Max={3}), WORKER:(Busy={4},Min={5},Max={6}), Local CPU: {7}",
                                    DateTimeOffset.UtcNow.ToString("u"),
                                    stats.BusyIoThreads, stats.MinIoThreads, stats.MaxIoThreads,
                                    stats.BusyWorkerThreads, stats.MinWorkerThreads, stats.MaxWorkerThreads,
                                    PerfCounterHelper.GetSystemCPU()
                                    );

        Console.WriteLine(message);
        // this._logger.LogInformation(message);
        this._logger.LogInformation(JsonSerializer.Serialize(stats.ConvertToLogTemplate()));
    }

    /// <summary>
    /// Returns the current thread pool usage statistics for the CURRENT AppDomain/Process
    /// </summary>
    public static ThreadPoolUsageStats GetThreadPoolStats()
    {
        //BusyThreads =  TP.GetMaxThreads() –TP.GetAvailable();
        //If BusyThreads >= TP.GetMinThreads(), then threadpool growth throttling is possible.

        int maxIoThreads, maxWorkerThreads;
        ThreadPool.GetMaxThreads(out maxWorkerThreads, out maxIoThreads);

        int freeIoThreads, freeWorkerThreads;
        ThreadPool.GetAvailableThreads(out freeWorkerThreads, out freeIoThreads);

        int minIoThreads, minWorkerThreads;
        ThreadPool.GetMinThreads(out minWorkerThreads, out minIoThreads);

        int busyIoThreads = maxIoThreads - freeIoThreads;
        int busyWorkerThreads = maxWorkerThreads - freeWorkerThreads;

        return new ThreadPoolUsageStats
        {
            BusyIoThreads = busyIoThreads,
            MinIoThreads = minIoThreads,
            MaxIoThreads = maxIoThreads,
            BusyWorkerThreads = busyWorkerThreads,
            MinWorkerThreads = minWorkerThreads,
            MaxWorkerThreads = maxWorkerThreads,
        };
    }

    public void Dispose()
    {
        this._disposed = true;
    }
}

public struct ThreadPoolUsageStats
{
    public int BusyIoThreads { get; set; }

    public int MinIoThreads { get; set; }

    public int MaxIoThreads { get; set; }

    public int BusyWorkerThreads { get; set; }

    public int MinWorkerThreads { get; set; }

    public int MaxWorkerThreads { get; set; }
}

public static class ThreadPoolUsageStatsExtensions
{
    public static LogTemplate ConvertToLogTemplate(this ThreadPoolUsageStats stats)
    {
        return new LogTemplate
        {
            BusyIoThreads = stats.BusyIoThreads,
            MinIoThreads = stats.MinIoThreads,
            MaxIoThreads = stats.MaxIoThreads,
            BusyWorkerThreads = stats.BusyWorkerThreads,
            MinWorkerThreads = stats.MinWorkerThreads,
            MaxWorkerThreads = stats.MaxWorkerThreads,
        };
    }
}