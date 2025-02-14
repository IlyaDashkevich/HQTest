using HQTestClient.Domain.Models;

namespace HQTestClient.Web.Pages;

public class BalanceModel : PageModel
{
    private readonly IBalanceService _balanceService;
    
    public Balance Balance { get; set; } = new ();

    public BalanceModel(IBalanceService balanceService)
    {
        _balanceService = balanceService;
    }

    public async Task OnGetAsync()
    {
        Balance = await _balanceService.GetBalanceAsync("BTCUSDT,XRPUSDT,XMRUSDT,DASHUSDT"); 
    }
}