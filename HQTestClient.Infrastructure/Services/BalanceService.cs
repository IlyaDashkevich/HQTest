namespace HQTestClient.Infrastructure.Services;
[AutoInterface]

public class BalanceService : IBalanceService
{
    private readonly ITestConnector _testConnector;
    private readonly Balance _balance;

    public BalanceService(ITestConnector testConnector, Balance balance)
    {
        _testConnector = testConnector;
        _balance = balance;
    }

    public async Task<Balance> GetBalanceAsync(string symbols)
    {
        var prices = await _testConnector.GetNewSymbolPriceTicker(symbols);

        foreach (var price in prices)
        {
            var symbol = price.Name;

            switch (symbol)
            {
                case "BTCUSDT":
                    _balance.TotalUSDT += _balance.AmountBTC * price.Price;
                    break;
                case "XRPUSDT":
                    _balance.TotalUSDT += _balance.AmountXRP * price.Price;
                    break;
                case "XMRUSDT":
                    _balance.TotalUSDT += _balance.AmountXMR * price.Price;
                    break;
                case "DASHUSDT":
                    _balance.TotalUSDT += _balance.AmountDASH * price.Price;
                    break;
            }
        }
        var balance = new Balance
        {
            TotalUSDT = _balance.TotalUSDT,
            TotalBTC = _balance.TotalUSDT / prices.FirstOrDefault(p => p.Name == "BTCUSDT")?.Price ?? 0,
            TotalXRP = _balance.TotalUSDT / prices.FirstOrDefault(p => p.Name == "XRPUSDT")?.Price ?? 0,
            TotalXMR = _balance.TotalUSDT / prices.FirstOrDefault(p => p.Name == "XMRUSDT")?.Price ?? 0,
            TotalDASH = _balance.TotalUSDT / prices.FirstOrDefault(p => p.Name == "DASHUSDT")?.Price ?? 0
        };
        return balance;
    }
}