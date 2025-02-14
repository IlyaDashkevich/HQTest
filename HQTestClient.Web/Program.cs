using HQTestClient.Domain.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddSignalR(e =>
    {
        e.EnableDetailedErrors = true;

    });

builder.Services.AddSingleton<WebSocketApi>(provider => new WebSocketApi());

builder.Services.AddHttpClient<BinanceRestClient>();
builder.Services.AddHttpClient<BinanceWebSocketClient>();
builder.Services.AddScoped<ITestConnector,BinanceConnector>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var apiKey = configuration["Binance:ApiKey"];
    var apiSecret = configuration["Binance:ApiSecret"];
    var websocket = provider.GetRequiredService<WebSocketApi>();
    var candleHubContext = provider.GetRequiredService<IHubContext<CandleHub>>();
    var tradeHubContext = provider.GetRequiredService<IHubContext<TradeHub>>();
    var httpClient = provider.GetRequiredService<HttpClient>();

    return new BinanceConnector(apiKey, apiSecret, websocket, candleHubContext, tradeHubContext, httpClient);
});

builder.Services.AddScoped<Balance>();
builder.Services.AddScoped<IBalanceService, BalanceService>(); 

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.MapHub<TradeHub>("/tradeHub");
app.MapHub<CandleHub>("/candleHub");

app.MapRazorPages();

app.Run();