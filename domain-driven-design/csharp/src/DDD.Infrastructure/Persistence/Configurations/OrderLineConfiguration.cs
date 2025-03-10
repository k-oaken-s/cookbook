namespace DDD.Infrastructure.Persistence.Configurations;

using DDD.Domain.Orders;
using DDD.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class OrderLineConfiguration : IEntityTypeConfiguration<OrderLine>
{
    public void Configure(EntityTypeBuilder<OrderLine> builder)
    {
        builder.HasKey(ol => ol.Id);
        
        builder
            .Property(ol => ol.ProductId)
            .HasConversion(
                id => id.Value,
                value => ProductId.Create(value))
            .IsRequired();
                
        builder.Property(ol => ol.ProductName)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(ol => ol.Quantity)
            .IsRequired();
            
        builder.OwnsOne(ol => ol.UnitPrice, price =>
        {
            price.Property(p => p.Amount)
                .HasColumnName("UnitPrice_Amount")
                .HasColumnType("decimal(18,2)")
                .IsRequired();
                
            price.Property(p => p.Currency)
                .HasColumnName("UnitPrice_Currency")
                .HasMaxLength(3)
                .IsRequired();
        });
    }
}