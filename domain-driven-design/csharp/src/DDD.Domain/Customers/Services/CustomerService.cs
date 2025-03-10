namespace DDD.Domain.Customers.Services;

public class CustomerService
{
    private readonly ICustomerRepository _customerRepository;
    
    public CustomerService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }
    
    public async Task<bool> IsEmailUniqueAsync(Email email, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByEmailAsync(email, cancellationToken);
        return customer == null;
    }
    
    public async Task<Customer?> RegisterCustomerAsync(
        string firstName,
        string lastName,
        string email,
        Address? defaultShippingAddress = null,
        CancellationToken cancellationToken = default)
    {
        var customerEmail = Email.Create(email);
        
        // Eメールの一意性を検証
        if (!await IsEmailUniqueAsync(customerEmail, cancellationToken))
            throw new InvalidOperationException("Email is already in use");
            
        var customerId = CustomerId.CreateNew();
        var customer = new Customer(customerId, firstName, lastName, customerEmail, defaultShippingAddress);
        
        await _customerRepository.AddAsync(customer, cancellationToken);
        
        return customer;
    }
}