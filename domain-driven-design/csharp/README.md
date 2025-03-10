# ドメイン駆動設計（Domain-Driven Design：DDD）

ドメイン駆動設計（DDD）は、複雑なビジネスドメインを持つソフトウェアを開発するためのアプローチです。DDDはエリック・エヴァンスによって提唱され、ソフトウェア開発の中心にドメインとドメインロジックを置くことを強調しています。

## 主要な概念

### 戦略的設計（Strategic Design）

#### 境界づけられたコンテキスト（Bounded Context）
システム内の概念や用語が特定の意味を持つ境界を定義します。異なるコンテキスト間では、同じ用語でも異なる意味や実装を持つことがあります。この例では、OnlineShopContextの中でのみ製品や注文の概念を扱っています。

#### ユビキタス言語（Ubiquitous Language）
特定の境界づけられたコンテキスト内で、開発者とドメインエキスパートが共通して使用する言語です。例えば、「商品」「注文」「顧客」などの用語は、コードと会話の両方で同じ意味で使用されています。

### 戦術的設計（Tactical Design）

#### 値オブジェクト（Value Object）
同一性ではなく属性によって定義されるオブジェクトです。不変性を持ち、副作用のない操作を提供します。

```csharp
// 値オブジェクトの例
public class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }
    
    // 値オブジェクトは不変
    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }
    
    public static Money Create(decimal amount, string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency cannot be empty", nameof(currency));
            
        if (currency.Length != 3)
            throw new ArgumentException("Currency code must be 3 characters", nameof(currency));
            
        return new Money(amount, currency);
    }
    
    // 同一性は属性によって決定される
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}
```

#### エンティティ（Entity）
同一性（ID）によって定義されるオブジェクトです。時間の経過とともに変化する可能性があります。

```csharp
// エンティティの例
public class Product : Entity<ProductId>, IAggregateRoot
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public SKU Sku { get; private set; }
    public Price Price { get; private set; }
    public int StockQuantity { get; private set; }
    public bool IsActive { get; private set; }
    
    // エンティティはIDで識別される
    public Product(
        ProductId id,
        string name,
        string description,
        SKU sku,
        Price price,
        int stockQuantity,
        bool isActive)
    {
        Id = id;
        Name = name;
        Description = description;
        Sku = sku;
        Price = price;
        StockQuantity = stockQuantity;
        IsActive = isActive;
        
        AddDomainEvent(new ProductCreatedEvent(id));
    }
    
    // ビジネスルールを含むメソッド
    public bool RemoveFromStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));
            
        if (StockQuantity < quantity)
            return false;
            
        StockQuantity -= quantity;
        
        if (StockQuantity == 0)
        {
            AddDomainEvent(new ProductOutOfStockEvent(Id));
        }
        
        return true;
    }
}
```

#### 集約（Aggregate）
関連するエンティティと値オブジェクトのクラスターで、一貫性の境界を形成します。一つの集約ルート（Aggregate Root）を通じてのみアクセスされます。

```csharp
// 集約とルートの例
public class Order : Entity<OrderId>, IAggregateRoot
{
    public CustomerId CustomerId { get; private set; }
    public DateTime OrderDate { get; private set; }
    public OrderStatus Status { get; private set; }
    public Address? ShippingAddress { get; private set; }
    
    // 集約内の関連エンティティ
    private readonly List<OrderLine> _orderLines = new();
    public IReadOnlyCollection<OrderLine> OrderLines => _orderLines.AsReadOnly();
    
    // ビジネスロジックは集約ルート内でカプセル化される
    public void AddOrderLine(ProductId productId, string productName, int quantity, Money unitPrice)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Cannot modify order once it has been processed");
            
        var existingLine = _orderLines.FirstOrDefault(ol => ol.ProductId == productId);
        if (existingLine != null)
        {
            existingLine.UpdateQuantity(existingLine.Quantity + quantity);
        }
        else
        {
            var orderLine = new OrderLine(Guid.NewGuid(), productId, productName, quantity, unitPrice);
            _orderLines.Add(orderLine);
        }
    }
}
```

#### ドメインイベント（Domain Events）
ドメイン内で発生した重要な出来事を表すオブジェクトです。副作用やクロスカッティングコンサーンの処理に使用されます。

```csharp
// ドメインイベントの例
public class ProductOutOfStockEvent : IDomainEvent
{
    public ProductId ProductId { get; }
    public DateTime OccurredOn { get; }
    
    public ProductOutOfStockEvent(ProductId productId)
    {
        ProductId = productId;
        OccurredOn = DateTime.UtcNow;
    }
}

// ドメインイベントを発行するエンティティ
public abstract class Entity<TId>
{
    public TId Id { get; protected set; } = default!;
    private readonly List<IDomainEvent> _domainEvents = new();
    
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    
    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
}
```

#### リポジトリ（Repository）
集約の永続化と取得を担当します。データストアの詳細をドメインモデルから隠蔽します。

```csharp
// リポジトリインターフェースの例
public interface IProductRepository : IRepository<Product, ProductId>
{
    Task<Product?> GetBySKUAsync(SKU sku, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetBySpecificationAsync(ISpecification<Product> specification, CancellationToken cancellationToken = default);
}

// リポジトリの実装例
public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _dbContext;
    
    public ProductRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<Product?> GetByIdAsync(ProductId id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .Include(p => p.Categories)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }
    
    // その他のリポジトリメソッド
}
```

#### ドメインサービス（Domain Services）
ステートレスな操作で、単一のエンティティや値オブジェクトには属さないドメインロジックを含みます。

```csharp
// ドメインサービスの例
public class OrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IProductRepository _productRepository;
    private readonly ProductService _productService;
    
    // ドメインサービスの操作例
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
                throw new InvalidOperationException($"Insufficient stock for product: {productId}");
            }
            
            // 商品情報の取得と注文に追加
            var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
            if (product == null)
                throw new InvalidOperationException($"Product not found: {productId}");
                
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
}
```

#### ファクトリ（Factory）
複雑なオブジェクトや集約の作成を担当します。

```csharp
// ファクトリの例
public class ProductFactory
{
    public static Product CreateProduct(
        string name,
        string description,
        string sku,
        decimal price,
        string currency,
        int stockQuantity,
        bool isActive)
    {
        var productId = ProductId.CreateNew();
        var productSku = SKU.Create(sku);
        var productPrice = Price.Create(price, currency);
        
        return new Product(
            productId,
            name,
            description,
            productSku,
            productPrice,
            stockQuantity,
            isActive);
    }
}
```

#### 仕様（Specification）
ビジネスルールをカプセル化し、オブジェクトがそれを満たすかどうかを評価します。

```csharp
// 仕様パターンの例
public class InStockProductSpecification : Specification<Product>
{
    private readonly int _minStock;
    
    public InStockProductSpecification(int minStock = 1)
    {
        _minStock = minStock;
    }
    
    public override bool IsSatisfiedBy(Product entity)
    {
        return entity.StockQuantity >= _minStock;
    }
}

// 仕様パターンの使用例
public async Task<IEnumerable<Product>> GetAvailableProductsAsync(CancellationToken cancellationToken = default)
{
    var specification = ProductSpecification.Active().And(new InStockProductSpecification());
    
    return await _productRepository.GetBySpecificationAsync(specification, cancellationToken);
}
```

### アーキテクチャ

#### レイヤードアーキテクチャ
- **ドメイン層** - ビジネスロジックを含む
- **アプリケーション層** - ユースケースを実装し、ドメイン層とインフラストラクチャ層を連携
- **インフラストラクチャ層** - 技術的な実装の詳細を提供
- **プレゼンテーション層** - ユーザーインターフェースを実装

```
DDD.Domain/ - ドメイン層
DDD.Application/ - アプリケーション層
DDD.Infrastructure/ - インフラストラクチャ層
DDD.API/ - プレゼンテーション層
```

#### CQRS（Command Query Responsibility Segregation）
コマンド（データの変更）とクエリ（データの取得）を分離するパターンです。

```csharp
// コマンドの例
public record CreateProductCommand(
    string Name,
    string Description,
    string Sku,
    decimal Price,
    string Currency,
    int StockQuantity,
    bool IsActive,
    List<Guid>? CategoryIds = null) : CommandBase<Guid>;

// クエリの例
public record GetProductsQuery(
    bool ActiveOnly = false) : QueryBase<IEnumerable<ProductDto>>;
```

## 横断的関心事（Cross-Cutting Concerns）

- **トランザクション管理**
- **ロギング**
- **例外処理**
- **認証・認可**

```csharp
// UnitOfWorkパターンの例
public class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly AppDbContext _dbContext;
    
    public UnitOfWork(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<T?> ExecuteWithTransactionAsync<T>(
        Func<Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var result = await operation();
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
```

## DDDの利点

1. **共通言語によるコミュニケーションの改善** - 開発者とドメインエキスパートが同じ用語で会話できる
2. **ビジネスロジックの明確な分離** - ドメイン層にビジネスロジックを集中させる
3. **メンテナンス性の向上** - ドメインモデルがビジネスルールを反映するため、変更が容易
4. **テスト容易性** - ドメイン層はインフラストラクチャに依存しないため、テストが容易
5. **拡張性** - 境界づけられたコンテキストにより、システムを独立して進化させることができる

## DDDの課題

1. **学習曲線** - 概念を理解するのに時間がかかる
2. **オーバーエンジニアリングの可能性** - 単純なCRUDアプリケーションには過剰な場合がある
3. **実装の複雑さ** - 正しく実装するには経験が必要
4. **パフォーマンスのオーバーヘッド** - 追加の抽象化レイヤーによるオーバーヘッドの可能性

## まとめ

ドメイン駆動設計は、複雑なビジネスロジックを持つシステムに適したアプローチです。ビジネスドメインを中心に据え、ソフトウェアの構造をビジネスの実際の構造に合わせることで、ビジネスの変化に対応しやすいシステムを構築することができます。