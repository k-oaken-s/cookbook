namespace DDD.Domain.Products;

using DDD.Domain.Common;
using DDD.Domain.Products.Events;
using System;

public class Product : Entity<ProductId>, IAggregateRoot
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public SKU Sku { get; private set; }
    public Price Price { get; private set; }
    public int StockQuantity { get; private set; }
    public bool IsActive { get; private set; }
    
    // ファーストクラスコレクション
    private readonly List<Category> _categories = new();
    public IReadOnlyCollection<Category> Categories => _categories.AsReadOnly();

    public Product(
        ProductId id,
        string name,
        string description,
        SKU sku,
        Price price,
        int stockQuantity,
        bool isActive,
        IEnumerable<Category>? categories = null)
    {
        Id = id;
        Name = name;
        Description = description;
        Sku = sku;
        Price = price;
        StockQuantity = stockQuantity;
        IsActive = isActive;
        
        if (categories != null)
        {
            _categories.AddRange(categories);
        }
        
        AddDomainEvent(new ProductCreatedEvent(id));
    }
    
    private Product() { }  // For EF Core
    
    public void UpdateDetails(string name, string description, Price price)
    {
        Name = name;
        Description = description;
        Price = price;
    }
    
    public void AddToStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));
            
        StockQuantity += quantity;
    }
    
    public bool RemoveFromStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));
            
        if (StockQuantity < quantity)
            return false;
            
        StockQuantity -= quantity;
        
        if (StockQuantity == 0)
        {
            AddDomainEvent(new ProductOutOfStockEvent(Id));
        }
        
        return true;
    }
    
    public void Activate()
    {
        IsActive = true;
    }
    
    public void Deactivate()
    {
        IsActive = false;
    }
    
    public void AddCategory(Category category)
    {
        if (!_categories.Contains(category))
        {
            _categories.Add(category);
        }
    }
    
    public void RemoveCategory(Category category)
    {
        _categories.Remove(category);
    }
}