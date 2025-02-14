namespace HQTestClient.Infrastructure.Connectors;

public class BinanceConnector : ITestConnector
{
    private readonly BinanceRestClient _restClient;
    private readonly BinanceWebSocketClient _webSocketClient;
    private readonly ConcurrentDictionary<string, bool> _subscriptions = new();
    private readonly IHubContext<CandleHub> _candleHubContext;
    private readonly IHubContext<TradeHub> _tradeHubContext;

    public BinanceConnector(string? apiKey, string? apiSecret, WebSocketApi websocket,  IHubContext<CandleHub> candleHubContext, IHubContext<TradeHub> tradeHubContext, HttpClient httpClient)
    {
        _restClient = new BinanceRestClient(apiKey, apiSecret, httpClient);
        _webSocketClient = new BinanceWebSocketClient(apiKey, apiSecret, websocket);
        _candleHubContext = candleHubContext;
        _tradeHubContext = tradeHubContext; 
    }

    public async Task<IEnumerable<Trade>> GetNewTradesAsync(string pair, int maxCount)
    {
        var json = await _restClient.GetTradesAsync(pair, maxCount);
        var trades = JArray.Parse(json);
        var tradeList = new List<Trade>();

        foreach (var trade in trades)
        {
            var newTrade = new Trade
            {
                Pair = trade["symbol"].ToString(),
                Id = trade["id"].ToString(),
                Price = decimal.Parse(trade["price"]?.ToString() ?? string.Empty, CultureInfo.InvariantCulture), 
                Amount = decimal.Parse(trade["qty"].ToString(), CultureInfo.InvariantCulture),
                Time = DateTimeOffset.FromUnixTimeMilliseconds((long)trade["time"]),
                Side = trade["isBuyer"].Value<bool>() ? "buy" : "sell"
            };
            
            tradeList.Add(newTrade);
        }

        return tradeList; 
    }
    
    public async Task<IEnumerable<Candle>> GetCandleSeriesAsync(string pair, int periodInSec, DateTimeOffset? from,
        DateTimeOffset? to, int? count)
    {
        var interval = ConvertPeriodToInterval(periodInSec);
        if (interval == null) throw new ArgumentException($"Invalid interval: {periodInSec}");
        
        long? newFrom = from?.ToUnixTimeMilliseconds();
        long? newTo = to?.ToUnixTimeMilliseconds();
        int? newCount = count.HasValue ? (int?)count.Value : null;
        
        
        var json = await _restClient.GetCandleSeriesAsync(pair, interval.Value, newFrom, newTo, newCount);
        var candlesList = new List<Candle>();
        var candles = JArray.Parse(json);
        
        foreach (var candle in candles)
        {
            var newCandle = new Candle
            {
                Pair = pair,
                OpenTime = DateTimeOffset.FromUnixTimeMilliseconds((long)(candle[0] ?? throw new InvalidOperationException())),
                OpenPrice = decimal.Parse(candle[1]?.ToString() ?? string.Empty, CultureInfo.InvariantCulture),
                HighPrice = decimal.Parse(candle[2]?.ToString() ?? string.Empty, CultureInfo.InvariantCulture),
                LowPrice = decimal.Parse(candle[3]?.ToString() ?? string.Empty, CultureInfo.InvariantCulture),
                ClosePrice = decimal.Parse(candle[4]?.ToString() ?? string.Empty, CultureInfo.InvariantCulture),
                TotalPrice = decimal.Parse(candle[5]?.ToString() ?? string.Empty, CultureInfo.InvariantCulture),
                TotalVolume = decimal.Parse(candle[6]?.ToString() ?? string.Empty, CultureInfo.InvariantCulture)
            };
            
            candlesList.Add(newCandle);
        }
        
        return candlesList;
    }

    public async Task<IEnumerable<Symbol>> GetNewSymbolPriceTicker(string symbols)
    {
        var json = await _restClient.GetSymbolPriceTicker(symbols);
        var prices = JArray.Parse(json);
        var pricesList = new List<Symbol>();

        foreach (var price in prices)
        {
            var newSymbol = new Symbol
            {
                Name = price["symbol"]?.ToString(),
                Price = decimal.Parse(price["price"]?.ToString() ?? string.Empty, CultureInfo.InvariantCulture)
            };
            
            pricesList.Add(newSymbol);
        }
        
        return pricesList;
    }

    public event Action<Trade>? NewBuyTrade;
    public event Action<Trade>? NewSellTrade;

    public async Task SubscribeTrades(string pair, int maxCount = 100)
    {
        if (!_subscriptions.TryAdd(pair, true)) return;

        await _webSocketClient.SubscribeTrades(pair, maxCount, async (data) =>
        {
            var trades = JArray.Parse(JObject.Parse(data)["result"]?.ToString() ?? string.Empty);

            foreach (var trade in trades)
            {
                var newTrade = new Trade
                {
                    Pair = pair,
                    Id = trade["id"].ToString(),
                    Price = decimal.Parse(trade["price"]?.ToString() ?? string.Empty, CultureInfo.InvariantCulture),
                    Amount = decimal.Parse(trade["qty"]?.ToString() ?? string.Empty, CultureInfo.InvariantCulture),
                    Time = DateTimeOffset.FromUnixTimeMilliseconds(
                        (long)(trade["time"] ?? throw new InvalidOperationException())),
                    Side = trade["isBuyer"].Value<bool>() ? "buy" : "sell"
                };

                if (newTrade.Side == "buy") NewBuyTrade?.Invoke(newTrade);
                else NewSellTrade?.Invoke(newTrade);

                await _tradeHubContext.Clients.All.SendAsync("ReceiveTrade", newTrade);
            }
        });
    }

    public async Task UnsubscribeTrades(string pair)
    {
        if (_subscriptions.TryRemove(pair, out _))
        {
            await _webSocketClient.DisconnectAsync(CancellationToken.None);
        }
    }

    public event Action<Candle>? CandleSeriesProcessing;

    public async Task SubscribeCandles(string pair, int periodInSec, DateTimeOffset? from, DateTimeOffset? to, long? count)
    {
        var interval = ConvertPeriodToInterval(periodInSec);
        if (interval == null) throw new ArgumentException($"Invalid interval: {periodInSec}");
        
        long? newFrom = from?.ToUnixTimeMilliseconds();
        long? newTo = to?.ToUnixTimeMilliseconds();
        int? newCount = count.HasValue ? (int?)count.Value : null;
        
        if (!_subscriptions.TryAdd(pair, true)) return; 
        
        await _webSocketClient.SubscribeCandles(pair, interval.Value, newFrom, newTo, newCount, async (data) =>
        {
            var candlesList = new List<Candle>();
            if (candlesList == null) throw new ArgumentNullException(nameof(candlesList));

            var candles = JArray.Parse(JObject.Parse(data)["result"]?.ToString() ?? string.Empty);
            foreach (var candle in candles)
            {
                var newCandle = new Candle
                {
                    Pair = pair,
                    OpenTime = DateTimeOffset.FromUnixTimeMilliseconds((long)(candle[0] ?? throw new InvalidOperationException())),
                    OpenPrice = decimal.Parse(candle[1]?.ToString() ?? string.Empty, CultureInfo.InvariantCulture),
                    HighPrice = decimal.Parse(candle[2]?.ToString() ?? string.Empty, CultureInfo.InvariantCulture),
                    LowPrice = decimal.Parse(candle[3]?.ToString() ?? string.Empty, CultureInfo.InvariantCulture),
                    ClosePrice = decimal.Parse(candle[4]?.ToString() ?? string.Empty, CultureInfo.InvariantCulture),
                    TotalPrice = decimal.Parse(candle[5]?.ToString() ?? string.Empty, CultureInfo.InvariantCulture),
                    TotalVolume = decimal.Parse(candle[6]?.ToString() ?? string.Empty, CultureInfo.InvariantCulture)
                };
                
                CandleSeriesProcessing?.Invoke(newCandle);
                candlesList.Add(newCandle);
                
                await _candleHubContext.Clients.All.SendAsync("ReceiveCandle", newCandle);
            }
        });
    }

    public async Task UnsubscribeCandles(string pair)
    {
        if (_subscriptions.TryRemove(pair, out _))
        {
            await _webSocketClient.DisconnectAsync(CancellationToken.None);
        }
    }
    
    private Interval? ConvertPeriodToInterval(int periodInSec)
    {
        return periodInSec switch
        {
            1 => Interval.ONE_SECOND,
            60 => Interval.ONE_MINUTE,
            180 => Interval.THREE_MINUTE,
            300 => Interval.FIVE_MINUTE,
            900 => Interval.FIFTEEN_MINUTE,
            1800 => Interval.THIRTY_MINUTE,
            3600 => Interval.ONE_HOUR,
            7200 => Interval.TWO_HOUR,
            14400 => Interval.FOUR_HOUR,
            21600 => Interval.SIX_HOUR,
            43200 => Interval.TWELVE_HOUR,
            86400 => Interval.ONE_DAY,
            _ => null
        };
    }
}