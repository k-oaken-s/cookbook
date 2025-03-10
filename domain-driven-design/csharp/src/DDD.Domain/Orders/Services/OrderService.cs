namespace DDD.Domain.Orders.Services;

using DDD.Domain.Customers;
using DDD.Domain.Products;
using DDD.Domain.Products.Services;
using DDD.Domain.Orders.Factories;
using System.Threading;
using System.Threading.Tasks;

public class OrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IProductRepository _productRepository;
    private readonly ProductService _productService;
    
    public OrderService(
        IOrderRepository orderRepository,
        ICustomerRepository customerRepository,
        IProductRepository productRepository,
        ProductService productService)
    {
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
        _productRepository = productRepository;
        _productService = productService;
    }
    
    public async Task<Order?> CreateOrderAsync(
        CustomerId customerId,
        Dictionary<ProductId, int> orderItems,
        Address? shippingAddress = null,
        CancellationToken cancellationToken = default)
    {
        // 顧客を検証
        var customer = await _customerRepository.GetByIdAsync(customerId, cancellationToken);
        if (customer == null)
            return null;
            
        // 出荷先住所を取得
        var address = shippingAddress ?? customer.DefaultShippingAddress;
        
        // オーダーを作成
        var order = OrderFactory.CreateOrder(customerId, address);
        
        // 注文アイテムを追加
        foreach (var item in orderItems)
        {
            var productId = item.Key;
            var quantity = item.Value;
            
            // 在庫の確認
            if (!await _productService.CanRemoveFromStockAsync(productId, quantity, cancellationToken))
            {
                // 在庫不足の場合、注文を棄却
                throw new InvalidOperationException($"Insufficient stock for product: {productId}");
            }
            
            // 商品情報の取得
            var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
            if (product == null)
                throw new InvalidOperationException($"Product not found: {productId}");
                
            // 注文に商品を追加
            order.AddOrderLine(
                productId,
                product.Name,
                quantity,
                Money.Create(product.Price.Amount, product.Price.Currency));
                
            // 在庫から商品を減らす
            product.RemoveFromStock(quantity);
            await _productRepository.UpdateAsync(product, cancellationToken);
        }
        
        // 注文を保存
        await _orderRepository.AddAsync(order, cancellationToken);
        
        return order;
    }
    
    public async Task<bool> CancelOrderAsync(OrderId orderId, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order == null)
            return false;
            
        // 注文のキャンセル
        order.Cancel();
        
        // キャンセルした注文の商品在庫を戻す
        foreach (var orderLine in order.OrderLines)
        {
            var product = await _productRepository.GetByIdAsync(orderLine.ProductId, cancellationToken);
            if (product != null)
            {
                product.AddToStock(orderLine.Quantity);
                await _productRepository.UpdateAsync(product, cancellationToken);
            }
        }
        
        // 変更を保存
        await _orderRepository.UpdateAsync(order, cancellationToken);
        
        return true;
    }
}