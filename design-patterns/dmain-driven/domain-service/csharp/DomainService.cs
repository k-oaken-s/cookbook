// domain-driven/domain-service/csharp/DomainService.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DesignPatternsCookbook.DomainDriven.DomainService
{
    #region ドメインモデル

    // 値オブジェクト：お金
    public class Money
    {
        public decimal Amount { get; }
        public string Currency { get; }

        private Money() { } // ORM用

        public Money(decimal amount, string currency)
        {
            Amount = amount;
            Currency = currency;
        }

        public static Money FromUsd(decimal amount) => new Money(amount, "USD");
        public static Money FromEur(decimal amount) => new Money(amount, "EUR");
        public static Money Zero(string currency) => new Money(0, currency);

        public Money Add(Money other)
        {
            if (Currency != other.Currency)
                throw new InvalidOperationException("Cannot add money with different currencies");

            return new Money(Amount + other.Amount, Currency);
        }

        public Money Subtract(Money other)
        {
            if (Currency != other.Currency)
                throw new InvalidOperationException("Cannot subtract money with different currencies");

            return new Money(Amount - other.Amount, Currency);
        }

        public Money Multiply(decimal factor)
        {
            return new Money(Amount * factor, Currency);
        }

        public override string ToString() => $"{Amount} {Currency}";
    }

    // エンティティ：製品
    public class Product
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public Money Price { get; private set; }
        public int StockQuantity { get; private set; }
        public ProductCategory Category { get; private set; }
        public bool IsActive { get; private set; }

        protected Product() { } // ORM用

        public Product(string name, Money price, int stockQuantity, ProductCategory category)
        {
            Id = Guid.NewGuid();
            Name = name;
            Price = price;
            StockQuantity = stockQuantity;
            Category = category;
            IsActive = true;
        }

        public void DecreaseStock(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be positive");

            if (StockQuantity < quantity)
                throw new InvalidOperationException("Not enough stock available");

            StockQuantity -= quantity;
        }

        public void IncreaseStock(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be positive");

            StockQuantity += quantity;
        }

        public void Deactivate()
        {
            IsActive = false;
        }

        public void Activate()
        {
            IsActive = true;
        }

        public void UpdatePrice(Money newPrice)
        {
            if (newPrice.Amount < 0)
                throw new ArgumentException("Price cannot be negative");

            Price = newPrice;
        }
    }

    public enum ProductCategory
    {
        Electronics,
        Clothing,
        Books,
        Food,
        Toys
    }

    // エンティティ：顧客
    public class Customer
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public CustomerType Type { get; private set; }
        public int LoyaltyPoints { get; private set; }
        public DateTime RegistrationDate { get; private set; }

        protected Customer() { } // ORM用

        public Customer(string name, CustomerType type)
        {
            Id = Guid.NewGuid();
            Name = name;
            Type = type;
            LoyaltyPoints = 0;
            RegistrationDate = DateTime.UtcNow;
        }

        public void AddLoyaltyPoints(int points)
        {
            if (points <= 0)
                throw new ArgumentException("Points must be positive");

            LoyaltyPoints += points;
        }

        public void UpgradeCustomerType()
        {
            if (Type == CustomerType.Regular && LoyaltyPoints >= 1000)
                Type = CustomerType.Silver;
            else if (Type == CustomerType.Silver && LoyaltyPoints >= 5000)
                Type = CustomerType.Gold;
            else if (Type == CustomerType.Gold && LoyaltyPoints >= 10000)
                Type = CustomerType.Platinum;
        }
    }

    public enum CustomerType
    {
        Regular,
        Silver,
        Gold,
        Platinum
    }

    // エンティティ：注文
    public class Order
    {
        public Guid Id { get; private set; }
        public Guid CustomerId { get; private set; }
        public List<OrderItem> Items { get; private set; } = new List<OrderItem>();
        public OrderStatus Status { get; private set; }
        public Money TotalAmount { get; private set; }
        public Money DiscountAmount { get; private set; }
        public Money FinalAmount { get; private set; }
        public DateTime OrderDate { get; private set; }

        protected Order() { } // ORM用

        public Order(Guid customerId)
        {
            Id = Guid.NewGuid();
            CustomerId = customerId;
            Status = OrderStatus.Created;
            TotalAmount = Money.Zero("USD");
            DiscountAmount = Money.Zero("USD");
            FinalAmount = Money.Zero("USD");
            OrderDate = DateTime.UtcNow;
        }

        public void AddItem(Product product, int quantity)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (quantity <= 0)
                throw new ArgumentException("Quantity must be positive");

            if (Status != OrderStatus.Created)
                throw new InvalidOperationException("Cannot add items to an order that is not in Created status");

            var existingItem = Items.FirstOrDefault(i => i.ProductId == product.Id);
            if (existingItem != null)
            {
                existingItem.IncreaseQuantity(quantity);
            }
            else
            {
                var orderItem = new OrderItem(Id, product.Id, product.Name, quantity, product.Price);
                Items.Add(orderItem);
            }

            RecalculateTotals();
        }

        public void RemoveItem(Guid productId)
        {
            if (Status != OrderStatus.Created)
                throw new InvalidOperationException("Cannot remove items from an order that is not in Created status");

            var item = Items.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                Items.Remove(item);
                RecalculateTotals();
            }
        }

        public void ApplyDiscount(Money discount)
        {
            if (discount.Amount < 0)
                throw new ArgumentException("Discount cannot be negative");

            if (discount.Amount > TotalAmount.Amount)
                throw new InvalidOperationException("Discount cannot be greater than total amount");

            DiscountAmount = discount;
            RecalculateTotals();
        }

        public void Confirm()
        {
            if (Status != OrderStatus.Created)
                throw new InvalidOperationException("Can only confirm orders in Created status");

            if (!Items.Any())
                throw new InvalidOperationException("Cannot confirm an empty order");

            Status = OrderStatus.Confirmed;
        }

        public void Ship()
        {
            if (Status != OrderStatus.Confirmed)
                throw new InvalidOperationException("Can only ship orders in Confirmed status");

            Status = OrderStatus.Shipped;
        }

        public void Deliver()
        {
            if (Status != OrderStatus.Shipped)
                throw new InvalidOperationException("Can only deliver orders in Shipped status");

            Status = OrderStatus.Delivered;
        }

        public void Cancel()
        {
            if (Status != OrderStatus.Created && Status != OrderStatus.Confirmed)
                throw new InvalidOperationException("Can only cancel orders in Created or Confirmed status");

            Status = OrderStatus.Cancelled;
        }

        private void RecalculateTotals()
        {
            decimal total = 0;
            foreach (var item in Items)
            {
                total += item.Quantity * item.UnitPrice.Amount;
            }

            TotalAmount = new Money(total, "USD");
            FinalAmount = TotalAmount.Subtract(DiscountAmount);
        }
    }

    public class OrderItem
    {
        public Guid Id { get; private set; }
        public Guid OrderId { get; private set; }
        public Guid ProductId { get; private set; }
        public string ProductName { get; private set; }
        public int Quantity { get; private set; }
        public Money UnitPrice { get; private set; }

        protected OrderItem() { } // ORM用

        public OrderItem(Guid orderId, Guid productId, string productName, int quantity, Money unitPrice)
        {
            Id = Guid.NewGuid();
            OrderId = orderId;
            ProductId = productId;
            ProductName = productName;
            Quantity = quantity;
            UnitPrice = unitPrice;
        }

        public void IncreaseQuantity(int additionalQuantity)
        {
            if (additionalQuantity <= 0)
                throw new ArgumentException("Quantity must be positive");

            Quantity += additionalQuantity;
        }
    }

    public enum OrderStatus
    {
        Created,
        Confirmed,
        Shipped,
        Delivered,
        Cancelled
    }

    #endregion

    #region ドメインサービス

    // ドメインサービス: 割引サービス
    public interface IDiscountService
    {
        Money CalculateDiscount(Order order, Customer customer);
    }

    public class DiscountService : IDiscountService
    {
        // 顧客ランクに基づく割引率
        private static readonly Dictionary<CustomerType, decimal> CustomerDiscountRates = new Dictionary<CustomerType, decimal>
        {
            { CustomerType.Regular, 0.00m },
            { CustomerType.Silver, 0.05m },
            { CustomerType.Gold, 0.10m },
            { CustomerType.Platinum, 0.15m }
        };

        // 製品カテゴリに基づく割引率
        private static readonly Dictionary<ProductCategory, decimal> CategoryDiscountRates = new Dictionary<ProductCategory, decimal>
        {
            { ProductCategory.Electronics, 0.03m },
            { ProductCategory.Clothing, 0.05m },
            { ProductCategory.Books, 0.07m },
            { ProductCategory.Food, 0.02m },
            { ProductCategory.Toys, 0.04m }
        };

        private readonly IProductRepository _productRepository;

        public DiscountService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public Money CalculateDiscount(Order order, Customer customer)
        {
            if (order == null) throw new ArgumentNullException(nameof(order));
            if (customer == null) throw new ArgumentNullException(nameof(customer));

            // 顧客ランクに基づく割引
            decimal customerDiscountRate = CustomerDiscountRates[customer.Type];
            
            // 注文合計に基づく割引
            decimal orderTotalDiscount = 0;
            if (order.TotalAmount.Amount > 1000)
                orderTotalDiscount = 0.05m;
            else if (order.TotalAmount.Amount > 500)
                orderTotalDiscount = 0.03m;
            
            // 製品カテゴリに基づく割引（最も高い割引率を適用）
            decimal categoryDiscount = 0;
            foreach (var item in order.Items)
            {
                var product = _productRepository.GetByIdAsync(item.ProductId).Result;
                if (product != null && CategoryDiscountRates.TryGetValue(product.Category, out decimal discount))
                {
                    categoryDiscount = Math.Max(categoryDiscount, discount);
                }
            }
            
            // シーズン割引
            decimal seasonalDiscount = IsHolidaySeason() ? 0.02m : 0;
            
            // ロイヤリティポイント使用割引 (例: 100ポイントごとに1ドル)
            decimal loyaltyDiscount = 0;
            if (customer.LoyaltyPoints >= 100)
            {
                int pointsToUse = Math.Min(customer.LoyaltyPoints, 1000); // 最大1000ポイントまで使用可能
                loyaltyDiscount = pointsToUse / 100;
            }
            
            // 総割引率の計算（最大30%まで）
            decimal totalDiscountRate = Math.Min(customerDiscountRate + orderTotalDiscount + categoryDiscount + seasonalDiscount, 0.30m);
            
            // 金額ベースの割引
            var percentageDiscount = order.TotalAmount.Multiply(totalDiscountRate);
            
            // ポイントによる追加割引
            var pointsDiscount = Money.FromUsd(loyaltyDiscount);
            
            // 合計割引額を返す
            return percentageDiscount.Add(pointsDiscount);
        }

        private bool IsHolidaySeason()
        {
            var today = DateTime.Today;
            
            // 11月15日〜1月15日をホリデーシーズンと定義
            if ((today.Month == 11 && today.Day >= 15) || 
                today.Month == 12 || 
                (today.Month == 1 && today.Day <= 15))
                return true;
                
            return false;
        }
    }

    // ドメインサービス: 在庫確認サービス
    public interface IInventoryService
    {
        bool IsProductAvailable(Guid productId, int requestedQuantity);
        Task<bool> CanFulfillOrderAsync(Order order);
        Task ReserveStockForOrderAsync(Order order);
    }

    public class InventoryService : IInventoryService
    {
        private readonly IProductRepository _productRepository;

        public InventoryService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public bool IsProductAvailable(Guid productId, int requestedQuantity)
        {
            if (requestedQuantity <= 0)
                throw new ArgumentException("Requested quantity must be positive");

            var product = _productRepository.GetByIdAsync(productId).Result;
            if (product == null)
                return false;

            return product.IsActive && product.StockQuantity >= requestedQuantity;
        }

        public async Task<bool> CanFulfillOrderAsync(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            foreach (var item in order.Items)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product == null || !product.IsActive || product.StockQuantity < item.Quantity)
                    return false;
            }

            return true;
        }

        public async Task ReserveStockForOrderAsync(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            if (!await CanFulfillOrderAsync(order))
                throw new InvalidOperationException("Cannot fulfill order due to inventory constraints");

            foreach (var item in order.Items)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                product.DecreaseStock(item.Quantity);
                await _productRepository.UpdateAsync(product);
            }
        }
    }

    // ドメインサービス: 注文検証サービス
    public interface IOrderValidationService
    {
        bool ValidateOrder(Order order, Customer customer);
        IEnumerable<string> GetValidationErrors(Order order, Customer customer);
    }

    public class OrderValidationService : IOrderValidationService
    {
        private readonly IInventoryService _inventoryService;

        public OrderValidationService(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        public bool ValidateOrder(Order order, Customer customer)
        {
            return !GetValidationErrors(order, customer).Any();
        }

        public IEnumerable<string> GetValidationErrors(Order order, Customer customer)
        {
            if (order == null)
                yield return "Order cannot be null";

            if (customer == null)
                yield return "Customer cannot be null";

            if (order != null)
            {
                if (!order.Items.Any())
                    yield return "Order must have at least one item";

                if (order.CustomerId != customer?.Id)
                    yield return "Order customer ID does not match the provided customer";

                if (order.TotalAmount.Amount <= 0)
                    yield return "Order total amount must be positive";

                foreach (var item in order.Items)
                {
                    if (!_inventoryService.IsProductAvailable(item.ProductId, item.Quantity))
                        yield return $"Product {item.ProductName} is not available in the requested quantity";
                }
            }
        }
    }
    
    #endregion

    #region リポジトリインターフェース

    // リポジトリインターフェース
    public interface IProductRepository
    {
        Task<Product> GetByIdAsync(Guid id);
        Task UpdateAsync(Product product);
    }

    public interface ICustomerRepository
    {
        Task<Customer> GetByIdAsync(Guid id);
    }

    public interface IOrderRepository
    {
        Task<Order> GetByIdAsync(Guid id);
        Task AddAsync(Order order);
        Task UpdateAsync(Order order);
    }

    #endregion

    #region アプリケーションサービス

    // アプリケーションサービス
    public class OrderApplicationService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IProductRepository _productRepository;
        private readonly IDiscountService _discountService;
        private readonly IInventoryService _inventoryService;
        private readonly IOrderValidationService _orderValidationService;

        public OrderApplicationService(
            IOrderRepository orderRepository,
            ICustomerRepository customerRepository,
            IProductRepository productRepository,
            IDiscountService discountService,
            IInventoryService inventoryService,
            IOrderValidationService orderValidationService)
        {
            _orderRepository = orderRepository;
            _customerRepository = customerRepository;
            _productRepository = productRepository;
            _discountService = discountService;
            _inventoryService = inventoryService;
            _orderValidationService = orderValidationService;
        }

        public async Task<Guid> CreateOrderAsync(Guid customerId, List<OrderItemDto> items)
        {
            // 顧客の取得と検証
            var customer = await _customerRepository.GetByIdAsync(customerId);
            if (customer == null)
                throw new ArgumentException($"Customer with ID {customerId} not found");

            // 注文の作成
            var order = new Order(customerId);

            // 注文項目の追加
            foreach (var itemDto in items)
            {
                var product = await _productRepository.GetByIdAsync(itemDto.ProductId);
                if (product == null)
                    throw new ArgumentException($"Product with ID {itemDto.ProductId} not found");

                order.AddItem(product, itemDto.Quantity);
            }

            // 注文の検証
            if (!_orderValidationService.ValidateOrder(order, customer))
            {
                var errors = _orderValidationService.GetValidationErrors(order, customer);
                throw new InvalidOperationException($"Invalid order: {string.Join(", ", errors)}");
            }

            // 割引の計算と適用
            var discount = _discountService.CalculateDiscount(order, customer);
            order.ApplyDiscount(discount);

            // 注文の保存
            await _orderRepository.AddAsync(order);

            // ロイヤリティポイントの追加（例: 購入金額100あたり1ポイント）
            int pointsToAdd = (int)Math.Floor(order.FinalAmount.Amount / 100);
            if (pointsToAdd > 0)
            {
                customer.AddLoyaltyPoints(pointsToAdd);
                customer.UpgradeCustomerType(); // ポイントに基づいて顧客タイプをアップグレード
            }

            return order.Id;
        }

        public async Task ConfirmOrderAsync(Guid orderId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
                throw new ArgumentException($"Order with ID {orderId} not found");

            var customer = await _customerRepository.GetByIdAsync(order.CustomerId);
            if (customer == null)
                throw new InvalidOperationException("Customer not found");

            // 在庫のチェック
            if (!await _inventoryService.CanFulfillOrderAsync(order))
                throw new InvalidOperationException("Cannot fulfill order due to inventory constraints");

            // 注文の確認
            order.Confirm();

            // 在庫の予約
            await _inventoryService.ReserveStockForOrderAsync(order);

            // 変更の保存
            await _orderRepository.UpdateAsync(order);
        }
    }

    public class OrderItemDto
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }

    #endregion
}