namespace HQTestClient.Infrastructure.Clients;

public class BinanceRestClient
{
    private readonly string? _apiKey;
    private readonly string? _apiSecret;
    private readonly HttpClient _httpClient;

    public BinanceRestClient(string? apiKey, string? apiSecret, HttpClient httpClient)
    {
        _apiKey = apiKey;
        _apiSecret = apiSecret;
        _httpClient = httpClient;
    }
    
    public async Task<string> GetTradesAsync(string symbol, int maxCount)
    {
        var spotAccountTrade = new SpotAccountTrade(_httpClient, apiKey: _apiKey, apiSecret: _apiSecret);

        var result = await spotAccountTrade.AccountTradeList(symbol, limit: maxCount);

        return result;
    }

    public async Task<string> GetCandleSeriesAsync(string pair, Interval periodInSec, long? from, long? to, int? count)
    {
        var market = new Market(_httpClient);
        
        var result = await market.KlineCandlestickData(pair, periodInSec,from,to, limit: count);
        
        return result; 
    }
    
    public async Task<string> GetSymbolPriceTicker(string symbols)
    {
        var market = new Market(_httpClient);
        
        var symbolArray = symbols.Split(',');
        var jsonSymbols = JsonConvert.SerializeObject(symbolArray);
        
        var result = await market.SymbolPriceTicker(symbols: jsonSymbols);

        return result;
    }
}