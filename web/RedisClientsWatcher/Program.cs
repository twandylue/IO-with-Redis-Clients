using RedisClientsWatcher.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddSingleton<StackExchangeRedisClient>();
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