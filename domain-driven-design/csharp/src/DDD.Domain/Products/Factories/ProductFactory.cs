namespace DDD.Domain.Products.Factories;

public class ProductFactory
{
    public static Product CreateProduct(
        string name,
        string description,
        string sku,
        decimal price,
        string currency,
        int stockQuantity,
        bool isActive)
    {
        var productId = ProductId.CreateNew();
        var productSku = SKU.Create(sku);
        var productPrice = Price.Create(price, currency);
        
        return new Product(
            productId,
            name,
            description,
            productSku,
            productPrice,
            stockQuantity,
            isActive);
    }
    
    public static Product CreateDiscountedProduct(
        string name,
        string description,
        string sku,
        decimal originalPrice,
        decimal discountPercent,
        string currency,
        int stockQuantity,
        bool isActive)
    {
        var productId = ProductId.CreateNew();
        var productSku = SKU.Create(sku);
        
        var discountedAmount = originalPrice * (1 - discountPercent / 100);
        var productPrice = Price.Create(Math.Round(discountedAmount, 2), currency);
        
        return new Product(
            productId,
            name,
            description,
            productSku,
            productPrice,
            stockQuantity,
            isActive);
    }
}