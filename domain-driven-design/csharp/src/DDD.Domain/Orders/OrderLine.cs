namespace DDD.Domain.Orders;

using DDD.Domain.Common;
using DDD.Domain.Products;

public class OrderLine : Entity<Guid>
{
    public ProductId ProductId { get; private set; }
    public string ProductName { get; private set; }
    public int Quantity { get; private set; }
    public Money UnitPrice { get; private set; }
    
    public OrderLine(Guid id, ProductId productId, string productName, int quantity, Money unitPrice)
    {
        Id = id;
        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }
    
    private OrderLine() { } // For EF Core
    
    public Money GetTotal()
    {
        return UnitPrice.MultiplyBy(Quantity);
    }
    
    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(newQuantity));
            
        Quantity = newQuantity;
    }
}
