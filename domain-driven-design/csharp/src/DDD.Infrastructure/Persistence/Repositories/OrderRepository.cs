namespace DDD.Infrastructure.Persistence.Repositories;

using DDD.Domain.Customers;
using DDD.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _dbContext;
    
    public OrderRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<Order?> GetByIdAsync(OrderId id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
            .Include(o => o.OrderLines)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }
    
    public async Task<IEnumerable<Order>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
            .Include(o => o.OrderLines)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<IEnumerable<Order>> GetByCustomerIdAsync(
        CustomerId customerId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
            .Include(o => o.OrderLines)
            .Where(o => o.CustomerId == customerId)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<IEnumerable<Order>> GetByStatusAsync(
        OrderStatus status,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
            .Include(o => o.OrderLines)
            .Where(o => o.Status == status)
            .ToListAsync(cancellationToken);
    }
    
    public async Task AddAsync(Order entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.Orders.AddAsync(entity, cancellationToken);
    }
    
    public Task UpdateAsync(Order entity, CancellationToken cancellationToken = default)
    {
        _dbContext.Orders.Update(entity);
        return Task.CompletedTask;
    }
    
    public Task DeleteAsync(Order entity, CancellationToken cancellationToken = default)
    {
        _dbContext.Orders.Remove(entity);
        return Task.CompletedTask;
    }
}