using HQTestClient.Domain.Models;

namespace HQTestClient.Web.Pages;

public class CandleModel : PageModel
{
    private readonly ITestConnector _connector;

    public CandleModel(ITestConnector connector)
    {
        _connector = connector;
    }

    public List<Candle> Candles { get; set; } = new();

    [BindProperty] 
    public string Pair { get; set; } = "BTCUSDT";

    [BindProperty] 
    public int Period { get; set; } = 60;
    
    [BindProperty] 
    public DateTimeOffset From { get; set; } = DateTimeOffset.Now.AddHours(-1);

    [BindProperty] 
    public DateTimeOffset To { get; set; } = DateTimeOffset.Now; 

    [BindProperty] 
    public int Count { get; set; } = 3;

    public async Task<IActionResult> OnPostGetCandlesAsync()
    {
        if (From == To) return BadRequest();
        
        Candles = (await _connector.GetCandleSeriesAsync(Pair, Period, From, To, Count)).ToList();
        return Page();
    }
}