using RedisClientsWatcher.Controllers;
using RedisClientsWatcher.Data;
using RedisClientsWatcher.Loggers;
using Serilog;
using Serilog.Filters;
using Serilog.Formatting.Json;

int minWorkerThreads = int.TryParse(Environment.GetEnvironmentVariable("MIN_WORKER_THREADS"), out minWorkerThreads) ? minWorkerThreads : 1;
int minIOThreads = int.TryParse(Environment.GetEnvironmentVariable("MIN_IO_THREADS"), out minIOThreads) ? minIOThreads : 1;
ThreadPool.SetMinThreads(minWorkerThreads, minIOThreads);

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
             .WriteTo.Logger(lc => lc
                                 .Filter.ByIncludingOnly(Matching.FromSource<ThreadPoolLogger>())
                                 .WriteTo.File(new JsonFormatter(), "./logs/ThreadPool/ThreadPool-.jsonl", rollingInterval: RollingInterval.Day)
             )
             .WriteTo.Logger(lc => lc
                                 .MinimumLevel.Error()
                                 .Filter.ByIncludingOnly(Matching.FromSource<StackExchangeRedisClient>())
                                 .WriteTo.File(new JsonFormatter(), "./logs/RedisClient/RedisClient-.jsonl", rollingInterval: RollingInterval.Day)
                                 )
             .WriteTo.Logger(lc => lc
                                 .Filter.ByIncludingOnly(Matching.FromSource<DataController>())
                                 .WriteTo.File(new JsonFormatter(), "./logs/DataController/DataController-.jsonl", rollingInterval: RollingInterval.Day)
                                 )
             .CreateLogger();

var loggerFactory = LoggerFactory.Create(logBuilder =>
{
    logBuilder.AddSerilog(dispose: true);
});
var log = loggerFactory.CreateLogger<ThreadPoolLogger>();

var _ = new ThreadPoolLogger(new TimeSpan(0, 0, 1), log);

builder.WebHost.ConfigureLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddSerilog();
});

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<StackExchangeRedisClient>();
builder.Services.AddSingleton<PlotService>();
builder.Services.AddSingleton<IRedisClient, StackExchangeRedisClient>();
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");

    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting()
   .UseEndpoints(endpoints =>
   {
       endpoints.MapControllers();
       endpoints.MapHealthChecks("/health");
       endpoints.MapHealthChecks("/_hc");
   });

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();