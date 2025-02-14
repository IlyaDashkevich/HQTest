namespace HQTestClient.Infrastructure.Clients;

public class BinanceWebSocketClient
{
    private readonly string? _apiKey;
    private readonly string? _apiSecret;
    private readonly WebSocketApi _websocket;
    
    public BinanceWebSocketClient(string? apiKey, string? apiSecret, WebSocketApi websocket)
    {
        _apiKey = apiKey;
        _apiSecret = apiSecret;
        _websocket = new WebSocketApi("wss://ws-api.binance.com:443/ws-api/v3", _apiKey, new BinanceHmac(_apiSecret));
    }
    
    public async Task SubscribeCandles(string pair, Interval periodInSec, long? from, long? to, int? count, Func<string, Task> onDataReceived)
    {
        _websocket.OnMessageReceived(async (data) =>
        {
            await onDataReceived(data);
        }, CancellationToken.None);

        await _websocket.ConnectAsync(CancellationToken.None);
        await _websocket.Market.KlinesAsync(symbol: pair, interval: periodInSec,  from, to, count, cancellationToken: CancellationToken.None);
    }
    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        await _websocket.DisconnectAsync(cancellationToken);
    }
    
    public async Task SubscribeTrades(string pair, int maxCount, Func<string, Task> onDataReceived)
    {
        _websocket.OnMessageReceived(async (data) =>
        {
            await onDataReceived(data);
        }, CancellationToken.None);

        await _websocket.ConnectAsync(CancellationToken.None);
        await _websocket.AccountTrade.AccountTradeListAsync(symbol: pair, limit:maxCount, cancellationToken: CancellationToken.None);
    }
}