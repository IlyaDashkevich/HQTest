using HQTestClient.Domain.Models;

namespace HQTestClient.Web.Pages;

public class TradeModel : PageModel
{
    private readonly ITestConnector _connector;
    
    public TradeModel(ITestConnector connector)
    {
        _connector = connector;
    }
    public List<Trade> AccountTrades { get; set; } = new();

    [BindProperty]
    public string Pair { get; set; } = "DOGEUSDT";
    
    [BindProperty]
    public int MaxCount { get; set; } = 10;

    public async Task<IActionResult> OnPostGetTradesAsync()
    {
        AccountTrades = (await _connector.GetNewTradesAsync(Pair, MaxCount)).ToList();
        return Page();
    }
}