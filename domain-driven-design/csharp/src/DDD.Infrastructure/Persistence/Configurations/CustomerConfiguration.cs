namespace DDD.Infrastructure.Persistence.Configurations;

using DDD.Domain.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasKey(c => c.Id);
        
        builder
            .Property(c => c.Id)
            .HasConversion(
                id => id.Value,
                value => CustomerId.Create(value));
                
        builder.Property(c => c.FirstName)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(c => c.LastName)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.OwnsOne(c => c.Email, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("Email")
                .IsRequired()
                .HasMaxLength(320);
                
            email.HasIndex(e => e.Value)
                .IsUnique();
        });
        
        builder.OwnsOne(c => c.DefaultShippingAddress, address =>
        {
            address.Property(a => a.Street)
                .HasColumnName("DefaultShippingAddress_Street")
                .HasMaxLength(200);
                
            address.Property(a => a.City)
                .HasColumnName("DefaultShippingAddress_City")
                .HasMaxLength(100);
                
            address.Property(a => a.State)
                .HasColumnName("DefaultShippingAddress_State")
                .HasMaxLength(50);
                
            address.Property(a => a.Country)
                .HasColumnName("DefaultShippingAddress_Country")
                .HasMaxLength(50);
                
            address.Property(a => a.ZipCode)
                .HasColumnName("DefaultShippingAddress_ZipCode")
                .HasMaxLength(20);
        });
        
        builder.Ignore(c => c.DomainEvents);
    }
}