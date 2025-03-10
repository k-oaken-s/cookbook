namespace DDD.Infrastructure.Persistence.Repositories;

using DDD.Domain.Customers;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class CustomerRepository : ICustomerRepository
{
    private readonly AppDbContext _dbContext;
    
    public CustomerRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<Customer?> GetByIdAsync(CustomerId id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Customers.FindAsync(new object[] { id }, cancellationToken);
    }
    
    public async Task<IEnumerable<Customer>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Customers.ToListAsync(cancellationToken);
    }
    
    public async Task<Customer?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Customers
            .FirstOrDefaultAsync(c => c.Email.Value == email.Value, cancellationToken);
    }
    
    public async Task AddAsync(Customer entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.Customers.AddAsync(entity, cancellationToken);
    }
    
    public Task UpdateAsync(Customer entity, CancellationToken cancellationToken = default)
    {
        _dbContext.Customers.Update(entity);
        return Task.CompletedTask;
    }
    
    public Task DeleteAsync(Customer entity, CancellationToken cancellationToken = default)
    {
        _dbContext.Customers.Remove(entity);
        return Task.CompletedTask;
    }
}