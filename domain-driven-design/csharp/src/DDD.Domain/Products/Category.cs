namespace DDD.Domain.Products;

using DDD.Domain.Common;

public class Category : Entity<Guid>
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    
    public Category(Guid id, string name, string description)
    {
        Id = id;
        Name = name;
        Description = description;
    }
    
    private Category() { } // For EF Core
    
    public void Update(string name, string description)
    {
        Name = name;
        Description = description;
    }
}