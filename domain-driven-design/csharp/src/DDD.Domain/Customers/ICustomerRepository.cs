namespace DDD.Domain.Customers;

using DDD.Domain.Common;

public interface ICustomerRepository : IRepository<Customer, CustomerId>
{
    Task<Customer?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);
}