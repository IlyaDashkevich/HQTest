namespace HQTestClient.Domain.Models;

public class Balance 
{
    public decimal TotalUSDT { get; set; }
    
    public decimal TotalBTC { get; set; }
    
    public decimal AmountBTC { get; set; } = 1;
    
    public decimal TotalXRP { get; set; }
    
    public decimal AmountXRP { get; set; } = 15000;
    public decimal TotalXMR { get; set; }
    
    public decimal AmountXMR { get; set; } = 50;
    
    public decimal TotalDASH { get; set; }
    
    public decimal AmountDASH { get; set; } = 30;
}