namespace DDD.Domain.Orders;

using DDD.Domain.Common;

public class OrderStatus : Enumeration
{
    public static OrderStatus Pending = new(1, nameof(Pending));
    public static OrderStatus Processing = new(2, nameof(Processing));
    public static OrderStatus Shipped = new(3, nameof(Shipped));
    public static OrderStatus Delivered = new(4, nameof(Delivered));
    public static OrderStatus Cancelled = new(5, nameof(Cancelled));
    
    public OrderStatus(int id, string name) : base(id, name)
    {
    }
}