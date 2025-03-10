namespace DDD.Domain.Orders.Factories;

using DDD.Domain.Customers;

public class OrderFactory
{
    public static Order CreateOrder(CustomerId customerId, Address? shippingAddress = null)
    {
        var orderId = OrderId.CreateNew();
        
        return new Order(orderId, customerId, shippingAddress);
    }
    
    public static Order CreateOrderWithDefaultShipping(Customer customer)
    {
        if (customer.DefaultShippingAddress == null)
            throw new InvalidOperationException("Customer does not have a default shipping address");
            
        return CreateOrder(customer.Id, customer.DefaultShippingAddress);
    }
}