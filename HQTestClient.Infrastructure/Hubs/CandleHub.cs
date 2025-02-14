namespace HQTestClient.Infrastructure.Hubs;

public class CandleHub : Hub
{
    private readonly ITestConnector _connector;
    
    public CandleHub(ITestConnector connector)
    {
        _connector = connector;
    }

    public async Task TakeCandle(string pair, int periodInSec, string from , string to, int count)
    {
        if (!DateTimeOffset.TryParse(from, out DateTimeOffset fromDate))
        {
            throw new ArgumentException($"Invalid 'from' datetime format: {from}", nameof(from));
        }

        if (!DateTimeOffset.TryParse(to, out DateTimeOffset toDate))
        {
            throw new ArgumentException($"Invalid 'to' datetime format: {to}", nameof(to));
        }
        
        await _connector.SubscribeCandles(pair, periodInSec, count: count);
    }
    
    public async Task SendCandle(Candle candle)
    {
        await Clients.All.SendAsync("ReceiveCandle", candle);
    }
}