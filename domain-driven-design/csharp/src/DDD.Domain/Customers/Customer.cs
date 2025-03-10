namespace DDD.Domain.Customers;

using DDD.Domain.Common;
using System.Collections.Generic;

public class Customer : Entity<CustomerId>, IAggregateRoot
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public Email Email { get; private set; }
    public Address? DefaultShippingAddress { get; private set; }
    
    public Customer(CustomerId id, string firstName, string lastName, Email email, Address? defaultShippingAddress = null)
    {
        Id = id;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        DefaultShippingAddress = defaultShippingAddress;
    }
    
    private Customer() { } // For EF Core
    
    public string FullName => $"{FirstName} {LastName}";
    
    public void UpdatePersonalInfo(string firstName, string lastName, Email email)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
    }
    
    public void UpdateDefaultShippingAddress(Address address)
    {
        DefaultShippingAddress = address;
    }
}