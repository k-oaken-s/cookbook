namespace DDD.Infrastructure.Persistence.Configurations;

using DDD.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);
        
        builder
            .Property(p => p.Id)
            .HasConversion(
                id => id.Value,
                value => ProductId.Create(value));
                
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(p => p.Description)
            .HasMaxLength(2000);
            
        builder.OwnsOne(p => p.Sku, sku =>
        {
            sku.Property(s => s.Value)
                .HasColumnName("SKU")
                .IsRequired()
                .HasMaxLength(50);
        });
        
        builder.OwnsOne(p => p.Price, price =>
        {
            price.Property(p => p.Amount)
                .HasColumnName("Price_Amount")
                .HasColumnType("decimal(18,2)")
                .IsRequired();
                
            price.Property(p => p.Currency)
                .HasColumnName("Price_Currency")
                .HasMaxLength(3)
                .IsRequired();
        });
        
        builder.Property(p => p.StockQuantity)
            .IsRequired();
            
        builder.Property(p => p.IsActive)
            .IsRequired();
            
        builder
            .HasMany(p => p.Categories)
            .WithMany()
            .UsingEntity(j => j.ToTable("ProductCategories"));
            
        builder.Ignore(p => p.DomainEvents);
    }
}