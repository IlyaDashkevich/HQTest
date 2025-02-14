namespace HQTestClient.Infrastructure.Hubs;

public class TradeHub : Hub
{
    private readonly ITestConnector _connector;

    public TradeHub(ITestConnector connector)
    {
        _connector = connector;
    }

    public async Task TakeTrade(string pair, int maxCount)
    {
        await _connector.SubscribeTrades(pair, maxCount);
    }

    public async Task SendTrade(Trade trade)
    {
        await Clients.All.SendAsync("ReceiveTrade", trade);
    }
}