namespace DDD.API.Extensions;

using DDD.Application.Products.Commands.CreateProduct;
using DDD.Domain.Customers;
using DDD.Domain.Customers.Services;
using DDD.Domain.Orders;
using DDD.Domain.Orders.Services;
using DDD.Domain.Products;
using DDD.Domain.Products.Services;
using DDD.Infrastructure.CrossCuttingConcerns.Transactions;
using DDD.Infrastructure.DomainEvents;
using DDD.Infrastructure.Persistence;
using DDD.Infrastructure.Persistence.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // DBコンテキスト
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));
                
        // リポジトリ
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        
        // UnitOfWork
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        // ドメインイベントディスパッチャー
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        
        return services;
    }
    
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // MediatR
        services.AddMediatR(cfg => 
            cfg.RegisterServicesFromAssembly(typeof(CreateProductCommand).Assembly));
            
        return services;
    }
    
    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        // ドメインサービス
        services.AddScoped<ProductService>();
        services.AddScoped<OrderService>();
        services.AddScoped<CustomerService>();
        
        return services;
    }
}