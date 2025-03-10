namespace DDD.Infrastructure.Persistence.Configurations;

using DDD.Domain.Customers;
using DDD.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);
        
        builder
            .Property(o => o.Id)
            .HasConversion(
                id => id.Value,
                value => OrderId.Create(value));
                
        builder
            .Property(o => o.CustomerId)
            .HasConversion(
                id => id.Value,
                value => CustomerId.Create(value))
            .IsRequired();
            
        builder.Property(o => o.OrderDate)
            .IsRequired();
            
        builder
            .Property(o => o.Status)
            .HasConversion(
                s => s.Id,
                id => OrderStatus.FromId<OrderStatus>(id))
            .IsRequired();
            
        builder.OwnsOne(o => o.ShippingAddress, address =>
        {
            address.Property(a => a.Street)
                .HasColumnName("ShippingAddress_Street")
                .HasMaxLength(200);
                
            address.Property(a => a.City)
                .HasColumnName("ShippingAddress_City")
                .HasMaxLength(100);
                
            address.Property(a => a.State)
                .HasColumnName("ShippingAddress_State")
                .HasMaxLength(50);
                
            address.Property(a => a.Country)
                .HasColumnName("ShippingAddress_Country")
                .HasMaxLength(50);
                
            address.Property(a => a.ZipCode)
                .HasColumnName("ShippingAddress_ZipCode")
                .HasMaxLength(20);
        });
        
        builder
            .HasMany(o => o.OrderLines)
            .WithOne()
            .HasForeignKey("OrderId")
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.Ignore(o => o.DomainEvents);
    }
}