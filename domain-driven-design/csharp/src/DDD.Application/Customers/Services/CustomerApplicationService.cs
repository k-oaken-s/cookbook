namespace DDD.Application.Customers.Services;

using DDD.Domain.Customers;
using DDD.Domain.Customers.Services;
using DDD.Domain.Orders;
using DDD.Infrastructure.CrossCuttingConcerns.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class CustomerApplicationService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly CustomerService _customerService;
    private readonly IUnitOfWork _unitOfWork;
    
    public CustomerApplicationService(
        ICustomerRepository customerRepository,
        IOrderRepository orderRepository,
        CustomerService customerService,
        IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _orderRepository = orderRepository;
        _customerService = customerService;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync(CancellationToken cancellationToken = default)
    {
        var customers = await _customerRepository.GetAllAsync(cancellationToken);
        
        return customers.Select(c => new CustomerDto(
            c.Id.Value,
            c.FirstName,
            c.LastName,
            c.Email.Value,
            c.DefaultShippingAddress != null
                ? new AddressDto(
                    c.DefaultShippingAddress.Street,
                    c.DefaultShippingAddress.City,
                    c.DefaultShippingAddress.State,
                    c.DefaultShippingAddress.Country,
                    c.DefaultShippingAddress.ZipCode)
                : null));
    }
    
    public async Task<CustomerDto?> GetCustomerByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customerId = CustomerId.Create(id);
        var customer = await _customerRepository.GetByIdAsync(customerId, cancellationToken);
        
        if (customer == null)
            return null;
            
        return new CustomerDto(
            customer.Id.Value,
            customer.FirstName,
            customer.LastName,
            customer.Email.Value,
            customer.DefaultShippingAddress != null
                ? new AddressDto(
                    customer.DefaultShippingAddress.Street,
                    customer.DefaultShippingAddress.City,
                    customer.DefaultShippingAddress.State,
                    customer.DefaultShippingAddress.Country,
                    customer.DefaultShippingAddress.ZipCode)
                : null);
    }
    
    public async Task<CustomerDetailsDto?> GetCustomerDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customerId = CustomerId.Create(id);
        var customer = await _customerRepository.GetByIdAsync(customerId, cancellationToken);
        
        if (customer == null)
            return null;
            
        var orders = await _orderRepository.GetByCustomerIdAsync(customerId, cancellationToken);
        
        return new CustomerDetailsDto(
            customer.Id.Value,
            customer.FirstName,
            customer.LastName,
            customer.Email.Value,
            customer.DefaultShippingAddress != null
                ? new AddressDto(
                    customer.DefaultShippingAddress.Street,
                    customer.DefaultShippingAddress.City,
                    customer.DefaultShippingAddress.State,
                    customer.DefaultShippingAddress.Country,
                    customer.DefaultShippingAddress.ZipCode)
                : null,
            orders.Select(o => new CustomerOrderDto(
                o.Id.Value,
                o.OrderDate,
                o.Status.Name,
                o.GetTotal().Amount)));
    }
    
    public async Task<Guid> RegisterCustomerAsync(
        string firstName,
        string lastName,
        string email,
        AddressDto? defaultShippingAddress = null,
        CancellationToken cancellationToken = default)
    {
        // 住所の変換
        Address? shippingAddress = null;
        if (defaultShippingAddress != null)
        {
            shippingAddress = Address.Create(
                defaultShippingAddress.Street,
                defaultShippingAddress.City,
                defaultShippingAddress.State,
                defaultShippingAddress.Country,
                defaultShippingAddress.ZipCode);
        }
        
        // トランザクションで顧客を登録
        var customer = await _unitOfWork.ExecuteWithTransactionAsync(async () =>
        {
            var newCustomer = await _customerService.RegisterCustomerAsync(
                firstName,
                lastName,
                email,
                shippingAddress,
                cancellationToken);
                
            if (newCustomer == null)
                throw new InvalidOperationException("Failed to register customer");
                
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            return newCustomer;
        }, cancellationToken);
        
        if (customer == null)
            throw new InvalidOperationException("Failed to register customer");
            
        return customer.Id.Value;
    }
    
    public async Task<bool> UpdateCustomerInfoAsync(
        Guid id,
        string firstName,
        string lastName,
        string email,
        CancellationToken cancellationToken = default)
    {
        var customerId = CustomerId.Create(id);
        var customer = await _customerRepository.GetByIdAsync(customerId, cancellationToken);
        
        if (customer == null)
            return false;
            
        var emailObj = Email.Create(email);
        
        // メールアドレスが変更される場合は一意性を検証
        if (customer.Email.Value != emailObj.Value)
        {
            if (!await _customerService.IsEmailUniqueAsync(emailObj, cancellationToken))
            {
                throw new InvalidOperationException("Email is already in use");
            }
        }
        
        return await _unitOfWork.ExecuteWithTransactionAsync(async () =>
        {
            customer.UpdatePersonalInfo(firstName, lastName, emailObj);
            await _customerRepository.UpdateAsync(customer, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            return true;
        }, cancellationToken) ?? false;
    }
    
    public async Task<bool> UpdateShippingAddressAsync(
        Guid id,
        AddressDto addressDto,
        CancellationToken cancellationToken = default)
    {
        var customerId = CustomerId.Create(id);
        var customer = await _customerRepository.GetByIdAsync(customerId, cancellationToken);
        
        if (customer == null)
            return false;
            
        var address = Address.Create(
            addressDto.Street,
            addressDto.City,
            addressDto.State,
            addressDto.Country,
            addressDto.ZipCode);
            
        return await _unitOfWork.ExecuteWithTransactionAsync(async () =>
        {
            customer.UpdateDefaultShippingAddress(address);
            await _customerRepository.UpdateAsync(customer, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            return true;
        }, cancellationToken) ?? false;
    }
}