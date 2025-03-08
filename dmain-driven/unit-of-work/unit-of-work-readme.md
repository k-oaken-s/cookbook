# Unit of Workパターン (Unit of Work Pattern)

## 概要

Unit of Workパターンは、トランザクションの境界内で実行される一連のデータベース操作をグループ化し、それらを単一の作業単位として管理するデザインパターンです。このパターンは、データの整合性を保つために、複数のリポジトリにまたがる操作を一貫して処理することを可能にします。

## 目的

- 複数の操作をトランザクション的に管理する
- データの整合性を保証する
- ビジネス操作全体の成功または失敗を保証する
- パフォーマンスを最適化する（バッチ更新）
- リポジトリの協調を促進する

## 構造

```
+-------------------+       +-------------------+
|                   |       |                   |
| ApplicationService|------>|   UnitOfWork      |
|                   |       |                   |
+-------------------+       +-------------------+
                                    |
                                    | Manages
                                    v
+-------------------+       +-------------------+       +-------------------+
|                   |       |                   |       |                   |
|  Repository A     |       |  Repository B     |       |  Repository C     |
|                   |       |                   |       |                   |
+-------------------+       +-------------------+       +-------------------+
        |                           |                          |
        |                           |                          |
        v                           v                          v
+-----------------------------------------------------------+
|                                                           |
|                     Data Source                           |
|                                                           |
+-----------------------------------------------------------+
```

## 基本的な実装例

```csharp
// Unit of Workインターフェース
public interface IUnitOfWork : IDisposable
{
    IProductRepository Products { get; }
    ICustomerRepository Customers { get; }
    IOrderRepository Orders { get; }

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}

// Entity Framework実装
public class EfUnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction _transaction;

    public IProductRepository Products { get; }
    public ICustomerRepository Customers { get; }
    public IOrderRepository Orders { get; }

    public EfUnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        Products = new EfProductRepository(context);
        Customers = new EfCustomerRepository(context);
        Orders = new EfOrderRepository(context);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        try
        {
            await _transaction?.CommitAsync();
        }
        finally
        {
            if (_transaction != null)
            {
                _transaction.Dispose();
                _transaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync()
    {
        try
        {
            await _transaction?.RollbackAsync();
        }
        finally
        {
            if (_transaction != null)
            {
                _transaction.Dispose();
                _transaction = null;
            }
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
```

## 使用例

```csharp
public class OrderService
{
    private readonly IUnitOfWork _unitOfWork;

    public OrderService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> CreateOrderAsync(Guid customerId, List<OrderItemDto> items)
    {
        // トランザクションの開始
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // 顧客の存在確認
            var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
            if (customer == null)
                throw new ArgumentException($"Customer with ID {customerId} not found");

            // 注文の作成
            var order = new Order(customerId);

            // 注文項目の追加と在庫の更新
            foreach (var itemDto in items)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(itemDto.ProductId);
                if (product == null)
                    throw new ArgumentException($"Product with ID {itemDto.ProductId} not found");

                if (product.StockQuantity < itemDto.Quantity)
                    throw new InvalidOperationException($"Not enough stock for product {product.Name}");

                // 在庫の減少
                product.ChangeStock(product.StockQuantity - itemDto.Quantity);
                _unitOfWork.Products.Update(product);

                // 注文項目の追加
                order.AddItem(product.Id, product.Name, product.Price, itemDto.Quantity);
            }

            // 注文の保存
            await _unitOfWork.Orders.AddAsync(order);
            await _unitOfWork.SaveChangesAsync();

            // トランザクションのコミット
            await _unitOfWork.CommitTransactionAsync();

            return order.Id;
        }
        catch (Exception)
        {
            // エラーが発生した場合はロールバック
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}
```

## Unit of Workパターンの利点

1. **データ整合性の保証**: すべての変更が成功するか、すべてが失敗するかのどちらか
2. **トランザクション管理の集中化**: トランザクション管理ロジックの重複を防ぐ
3. **複数リポジトリの調整**: 異なるリポジトリ間の変更を協調させる
4. **パフォーマンスの最適化**: 変更をバッチ処理することでデータベースラウンドトリップを減らす
5. **エラー処理の一元化**: 例外発生時のロールバック処理を統一
6. **ドメインロジックからの分離**: データ永続化の詳細をドメインロジックから隔離

## Unit of Workパターンの欠点

1. **複雑さの増加**: 追加の抽象化レイヤーを導入
2. **学習曲線**: チームがパターンに慣れるまで時間がかかる
3. **柔軟性の低下**: 特定のデータアクセス技術に依存する場合がある
4. **オーバーヘッド**: 小規模なアプリケーションでは過剰設計になる可能性

## 一般的な実装バリエーション

### 1. ORM統合型

Entity FrameworkやNHibernateなどのORMは、すでにUnit of Work機能を内蔵しています：

- Entity FrameworkのDbContext
- NHibernateのISession
- DapperのIDbTransaction

### 2. 独立型

ORMに依存しない独立したUnit of Work実装：

- カスタムトランザクション管理
- マルチデータソース対応
- イベント駆動型の変更追跡

### 3. リポジトリファクトリ型

トランザクションコンテキストに基づいてリポジトリを生成：

```csharp
public class UnitOfWorkRepositoryFactory
{
    private readonly IDbConnection _connection;
    private IDbTransaction _transaction;
    
    public IProductRepository CreateProductRepository()
    {
        return new DapperProductRepository(_connection, _transaction);
    }
    
    // 他のファクトリメソッド...
}
```

## ドメイン駆動設計でのUnit of Work

DDDでUnit of Workを適用する際の考慮事項：

1. **集約の一貫性**: 集約単位での変更を一貫して保存
2. **ドメインイベントとの統合**: ドメインイベントの発行をUnit of Workのコミット時に行う
3. **リポジトリパターンとの組み合わせ**: 集約ルート単位のリポジトリと組み合わせる
4. **境界づけられたコンテキスト**: 各境界づけられたコンテキスト内でUnit of Workを管理

## 実装例の詳細

サンプルコードは、`./csharp/UnitOfWork.cs` ファイルを参照してください。この実装例では、以下の要素が含まれています：

1. **Entity Framework実装**:
   - DbContextベースのUnit of Work
   - トランザクション管理
   - 複数リポジトリの連携

2. **Dapper実装**:
   - ADO.NETトランザクションを使用したUnit of Work
   - 手動トランザクション管理
   - 軽量なデータアクセス

3. **インメモリ実装**:
   - テスト用のUnit of Work
   - トランザクションのシミュレーション
   - メモリ内データストア

4. **アプリケーションサービスでの使用例**:
   - 注文処理ユースケース
   - 例外処理とロールバック
   - トランザクション境界の明示

## 関連パターン

- **リポジトリパターン**: データアクセスの抽象化を提供、Unit of Workと密接に連携
- **ファクトリパターン**: Unit of Workをファクトリとして使用し、リポジトリを生成
- **トランザクションスクリプト**: 手続き型の代替手法
- **ドメインイベント**: Unit of Workのコミットフェーズでイベントを発行
- **集約パターン (DDD)**: 一貫性と整合性の境界を定義
