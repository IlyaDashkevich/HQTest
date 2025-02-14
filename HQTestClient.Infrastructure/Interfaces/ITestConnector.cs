namespace HQTestClient.Infrastructure.Interfaces;

public interface ITestConnector
    {
        #region Rest

        public Task<IEnumerable<Trade>> GetNewTradesAsync(string pair, int maxCount);
        public Task<IEnumerable<Candle>> GetCandleSeriesAsync(string pair, int periodInSec, DateTimeOffset? from, DateTimeOffset? to = null, int? count = 0);
        public Task<IEnumerable<Symbol>> GetNewSymbolPriceTicker(string symbol);

        #endregion

        #region Socket


        event Action<Trade> NewBuyTrade;
        event Action<Trade> NewSellTrade;
        Task SubscribeTrades(string pair, int maxCount = 100);
        Task UnsubscribeTrades(string pair);
        
        event Action<Candle> CandleSeriesProcessing;
        Task SubscribeCandles(string pair, int periodInSec, DateTimeOffset? from = null, DateTimeOffset? to = null, long? count = 0);
        Task UnsubscribeCandles(string pair);

        #endregion

    }
