namespace RedisClientsWatcher.Loggers;

public class LogTemplate
{
    public int BusyIoThreads { get; set; }

    public int MinIoThreads { get; set; }

    public int MaxIoThreads { get; set; }

    public int BusyWorkerThreads { get; set; }

    public int MinWorkerThreads { get; set; }

    public int MaxWorkerThreads { get; set; }
}