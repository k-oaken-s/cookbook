namespace DDD.API.Controllers;

public class UpdateStockRequest
{
    public int Quantity { get; set; }
    public bool IsAddition { get; set; }
}