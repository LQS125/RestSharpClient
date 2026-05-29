using ApiClient;

var builder = WebApplication.CreateBuilder(args);

// 注册 TokenProvider,来注册tokenprovider实例来获取token。AddSingleton<IApiTokenProvider>:整个应用程序生命周期，只创建一个IApiTokenProvider实例，所以地方共用
builder.Services.AddSingleton<IApiTokenProvider>(
    _ => new StaticApiTokenProvider(builder.Configuration["ApiClient:Token"]));
// 给 MyApiClient 类 自动分配一个 HttpClient
builder.Services.AddHttpClient<MyApiClient>(client =>
{
    client.BaseAddress = new Uri("https://api.example.com/");
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();


string[] summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

var api = app.MapGroup("/api");
api.MapGet("weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.MapDefaultEndpoints();

app.UseFileServer();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

