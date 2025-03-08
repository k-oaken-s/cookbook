using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace DesignPatternsCookbook.DomainDriven.UnitOfWork
{
    #region ドメインモデル

    // 基本エンティティクラス
    public abstract class Entity
    {
        public Guid Id { get; protected set; }

        protected Entity()
        {
            Id = Guid.NewGuid();
        }
    }

    // 製品エンティティ
    public class Product : Entity
    {
        public string Name { get; private set; }
        public string Description { get; private set; }
        public decimal Price { get; private set; }
        public int StockQuantity { get; private set; }
        public bool IsActive { get; private set; }

        public Product(string name, string description, decimal price, int stockQuantity)
        {
            Name = name;
            Description = description;
            Price = price;
            StockQuantity = stockQuantity;
            IsActive = true;
        }

        public void UpdateDetails(string name, string description, decimal price)
        {
            Name = name;
            Description = description;
            Price = price;
        }

        public void ChangeStock(int newQuantity)
        {
            if (newQuantity < 0)
                throw new ArgumentException("Stock quantity cannot be negative");

            StockQuantity = newQuantity;
        }

        public void Deactivate()
        {
            IsActive = false;
        }

        public void Activate()
        {
            IsActive = true;
        }
    }

    // 顧客エンティティ
    public class Customer : Entity
    {
        public string Name { get; private set; }
        public string Email { get; private set; }
        public string Phone { get; private set; }
        public string Address { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; }

        public Customer(string name, string email, string phone, string address)
        {
            Name = name;
            Email = email;
            Phone = phone;
            Address = address;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
        }

        public void UpdateDetails(string name, string email, string phone, string address)
        {
            Name = name;
            Email = email;
            Phone = phone;
            Address = address;
        }

        public void Deactivate()
        {
            IsActive = false;
        }

        public void Activate()
        {
            IsActive = true;
        }
    }

    // 注文エンティティ
    public class Order : Entity
    {
        public Guid CustomerId { get; private set; }
        public DateTime OrderDate { get; private set; }
        public OrderStatus Status { get; private set; }
        public decimal TotalAmount { get; private set; }
        private readonly List<OrderItem> _items = new List<OrderItem>();
        public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

        public Order(Guid customerId)
        {
            CustomerId = customerId;
            OrderDate = DateTime.UtcNow;
            Status = OrderStatus.Created;
            TotalAmount = 0;
        }

        public void AddItem(Guid productId, string productName, decimal price, int quantity)
        {
            if (Status != OrderStatus.Created)
                throw new InvalidOperationException("Cannot add items to a confirmed order");

            var existingItem = _items.FirstOrDefault(i => i.ProductId == productId);
            if (existingItem != null)
            {
                existingItem.UpdateQuantity(existingItem.Quantity + quantity);
            }
            else
            {
                var newItem = new OrderItem(Id, productId, productName, price, quantity);
                _items.Add(newItem);
            }

            RecalculateTotalAmount();
        }

        public void RemoveItem(Guid productId)
        {
            if (Status != OrderStatus.Created)
                throw new InvalidOperationException("Cannot remove items from a confirmed order");

            var item = _items.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                _items.Remove(item);
                RecalculateTotalAmount();
            }
        }

        public void ConfirmOrder()
        {
            if (Status != OrderStatus.Created)
                throw new InvalidOperationException("Order is already confirmed");

            if (!_items.Any())
                throw new InvalidOperationException("Cannot confirm an empty order");

            Status = OrderStatus.Confirmed;
        }

        public void ShipOrder()
        {
            if (Status != OrderStatus.Confirmed)
                throw new InvalidOperationException("Order must be confirmed before shipping");

            Status = OrderStatus.Shipped;
        }

        public void CompleteOrder()
        {
            if (Status != OrderStatus.Shipped)
                throw new InvalidOperationException("Order must be shipped before completion");

            Status = OrderStatus.Completed;
        }

        public void CancelOrder()
        {
            if (Status == OrderStatus.Shipped || Status == OrderStatus.Completed)
                throw new InvalidOperationException("Cannot cancel a shipped or completed order");

            Status = OrderStatus.Cancelled;
        }

        private void RecalculateTotalAmount()
        {
            TotalAmount = _items.Sum(i => i.Price * i.Quantity);
        }
    }

    // 注文項目エンティティ
    public class OrderItem : Entity
    {
        public Guid OrderId { get; private set; }
        public Guid ProductId { get; private set; }
        public string ProductName { get; private set; }
        public decimal Price { get; private set; }
        public int Quantity { get; private set; }

        public OrderItem(Guid orderId, Guid productId, string productName, decimal price, int quantity)
        {
            OrderId = orderId;
            ProductId = productId;
            ProductName = productName;
            Price = price;
            Quantity = quantity;
        }

        public void UpdateQuantity(int newQuantity)
        {
            if (newQuantity <= 0)
                throw new ArgumentException("Quantity must be positive");

            Quantity = newQuantity;
        }
    }

    public enum OrderStatus
    {
        Created,
        Confirmed,
        Shipped,
        Completed,
        Cancelled
    }

    #endregion

    #region リポジトリインターフェース

    // 汎用リポジトリインターフェース
    public interface IRepository<T> where T : Entity
    {
        Task<T> GetByIdAsync(Guid id);
        Task<IEnumerable<T>> GetAllAsync();
        Task AddAsync(T entity);
        void Update(T entity);
        void Remove(T entity);
    }

    // 製品リポジトリインターフェース
    public interface IProductRepository : IRepository<Product>
    {
        Task<IEnumerable<Product>> GetActiveProductsAsync();
        Task<IEnumerable<Product>> GetProductsWithLowStockAsync(int threshold);
    }

    // 顧客リポジトリインターフェース
    public interface ICustomerRepository : IRepository<Customer>
    {
        Task<IEnumerable<Customer>> GetActiveCustomersAsync();
        Task<Customer> GetByEmailAsync(string email);
    }

    // 注文リポジトリインターフェース
    public interface IOrderRepository : IRepository<Order>
    {
        Task<IEnumerable<Order>> GetOrdersByCustomerAsync(Guid customerId);
        Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status);
        Task<IEnumerable<OrderItem>> GetOrderItemsAsync(Guid orderId);
    }

    #endregion

    #region Unit of Workインターフェース

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

    #endregion

    #region Entity Framework実装

    // リポジトリ基底クラス - Entity Framework実装
    public abstract class EfRepository<T> : IRepository<T> where T : Entity
    {
        protected readonly AppDbContext _dbContext;
        protected readonly DbSet<T> _dbSet;

        protected EfRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = dbContext.Set<T>();
        }

        public async Task<T> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public void Update(T entity)
        {
            _dbContext.Entry(entity).State = EntityState.Modified;
        }

        public void Remove(T entity)
        {
            _dbSet.Remove(entity);
        }
    }

    // 製品リポジトリ - Entity Framework実装
    public class EfProductRepository : EfRepository<Product>, IProductRepository
    {
        public EfProductRepository(AppDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IEnumerable<Product>> GetActiveProductsAsync()
        {
            return await _dbSet.Where(p => p.IsActive).ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsWithLowStockAsync(int threshold)
        {
            return await _dbSet.Where(p => p.IsActive && p.StockQuantity < threshold).ToListAsync();
        }
    }

    // 顧客リポジトリ - Entity Framework実装
    public class EfCustomerRepository : EfRepository<Customer>, ICustomerRepository
    {
        public EfCustomerRepository(AppDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IEnumerable<Customer>> GetActiveCustomersAsync()
        {
            return await _dbSet.Where(c => c.IsActive).ToListAsync();
        }

        public async Task<Customer> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.Email == email);
        }
    }

    // 注文リポジトリ - Entity Framework実装
    public class EfOrderRepository : EfRepository<Order>, IOrderRepository
    {
        public EfOrderRepository(AppDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IEnumerable<Order>> GetOrdersByCustomerAsync(Guid customerId)
        {
            return await _dbSet.Where(o => o.CustomerId == customerId)
                              .Include(o => o.Items)
                              .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status)
        {
            return await _dbSet.Where(o => o.Status == status)
                              .Include(o => o.Items)
                              .ToListAsync();
        }

        public async Task<IEnumerable<OrderItem>> GetOrderItemsAsync(Guid orderId)
        {
            return await _dbContext.Set<OrderItem>()
                                  .Where(oi => oi.OrderId == orderId)
                                  .ToListAsync();
        }
    }

    // Unit of Work - Entity Framework実装
    public class EfUnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _dbContext;
        private IDbContextTransaction _transaction;
        private bool _disposed = false;

        public IProductRepository Products { get; }
        public ICustomerRepository Customers { get; }
        public IOrderRepository Orders { get; }

        public EfUnitOfWork(AppDbContext dbContext)
        {
            _dbContext = dbContext;
            Products = new EfProductRepository(dbContext);
            Customers = new EfCustomerRepository(dbContext);
            Orders = new EfOrderRepository(dbContext);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _dbContext.Database.BeginTransactionAsync();
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
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _transaction?.Dispose();
                    _dbContext.Dispose();
                }

                _disposed = true;
            }
        }
    }

    // DbContext - Entity Framework
    public class AppDbContext : DbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 製品エンティティの設定
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.Property(e => e.StockQuantity).IsRequired();
                entity.Property(e => e.IsActive).IsRequired();
            });

            // 顧客エンティティの設定
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.Address).HasMaxLength(200);
                entity.Property(e => e.IsActive).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
            });

            // 注文エンティティの設定
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CustomerId).IsRequired();
                entity.Property(e => e.OrderDate).IsRequired();
                entity.Property(e => e.Status).IsRequired();
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
            });

            // 注文項目エンティティの設定
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OrderId).IsRequired();
                entity.Property(e => e.ProductId).IsRequired();
                entity.Property(e => e.ProductName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Quantity).IsRequired();

                // 注文項目と注文の関連付け
                entity.HasOne<Order>()
                      .WithMany(o => o.Items)
                      .HasForeignKey(oi => oi.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }

    // 注: これらのクラスはEntity Frameworkの概念を示すためのものであり、
    // 実際のコードではEntity Frameworkのパッケージ参照が必要です
    public class DbContext : IDisposable
    {
        public DbSet<T> Set<T>() where T : class => null;
        public void Dispose() { }
        public Database Database => null;
        public EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class => null;
        public Task<int> SaveChangesAsync() => Task.FromResult(0);
    }

    public class DbContextOptions<T> where T : DbContext { }

    public class DbSet<T> : IQueryable<T> where T : class
    {
        public IEnumerator<T> GetEnumerator() => Enumerable.Empty<T>().GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
        public Type ElementType => typeof(T);
        public Expression Expression => null;
        public IQueryProvider Provider => null;

        public ValueTask<T> FindAsync(params object[] keyValues) => new ValueTask<T>(default(T));
        public Task<List<T>> ToListAsync() => Task.FromResult(new List<T>());
        public DbSet<T> Include<TProperty>(System.Linq.Expressions.Expression<Func<T, TProperty>> navigationPropertyPath) => this;
        public Task AddAsync(T entity) => Task.CompletedTask;
        public void Remove(T entity) { }
    }

    public static class QueryableExtensions
    {
        public static Task<List<T>> ToListAsync<T>(this IQueryable<T> source) => Task.FromResult(new List<T>());
        public static Task<T> FirstOrDefaultAsync<T>(this IQueryable<T> source, Func<T, bool> predicate = null) => Task.FromResult(default(T));
        public static IQueryable<T> Where<T>(this IQueryable<T> source, Func<T, bool> predicate) => Enumerable.Empty<T>().AsQueryable();
    }

    public class Database
    {
        public Task<IDbContextTransaction> BeginTransactionAsync() => Task.FromResult<IDbContextTransaction>(null);
    }

    public interface IDbContextTransaction : IDisposable
    {
        Task CommitAsync();
        Task RollbackAsync();
    }

    public class EntityEntry<TEntity> where TEntity : class
    {
        public EntityState State { get; set; }
    }

    public enum EntityState
    {
        Detached,
        Unchanged,
        Deleted,
        Modified,
        Added
    }

    public class ModelBuilder
    {
        public EntityTypeBuilder<TEntity> Entity<TEntity>() where TEntity : class => null;
    }

    public class EntityTypeBuilder<TEntity> where TEntity : class
    {
        public EntityTypeBuilder<TEntity> HasKey(Func<TEntity, object> keyExpression) => this;
        public PropertyBuilder<TProperty> Property<TProperty>(System.Linq.Expressions.Expression<Func<TEntity, TProperty>> propertyExpression) => null;
        public IndexBuilder<TEntity> HasIndex(System.Linq.Expressions.Expression<Func<TEntity, object>> indexExpression) => null;
        public ReferenceNavigationBuilder<TEntity, TRelatedEntity> HasOne<TRelatedEntity>() where TRelatedEntity : class => null;
        public ReferenceNavigationBuilder<TEntity, TRelatedEntity> HasOne<TRelatedEntity>(System.Linq.Expressions.Expression<Func<TEntity, TRelatedEntity>> navigationExpression) where TRelatedEntity : class => null;
    }

    public class PropertyBuilder<TProperty>
    {
        public PropertyBuilder<TProperty> IsRequired() => this;
        public PropertyBuilder<TProperty> HasMaxLength(int maxLength) => this;
        public PropertyBuilder<TProperty> HasColumnType(string typeName) => this;
    }

    public class IndexBuilder<TEntity> where TEntity : class
    {
        public IndexBuilder<TEntity> IsUnique() => this;
    }

    public class ReferenceNavigationBuilder<TEntity, TRelatedEntity>
        where TEntity : class
        where TRelatedEntity : class
    {
        public ReferenceNavigationBuilder<TEntity, TRelatedEntity> WithMany(System.Linq.Expressions.Expression<Func<TRelatedEntity, IEnumerable<TEntity>>> navigationExpression) => this;
        public ReferenceNavigationBuilder<TEntity, TRelatedEntity> HasForeignKey(System.Linq.Expressions.Expression<Func<TEntity, object>> foreignKeyExpression) => this;
        public ReferenceNavigationBuilder<TEntity, TRelatedEntity> OnDelete(DeleteBehavior behavior) => this;
    }

    public enum DeleteBehavior
    {
        Cascade,
        ClientSetNull,
        Restrict,
        SetNull
    }

    #endregion

    #region アプリケーションサービス

    // 注文サービス
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

                // 注文項目の追加
                foreach (var itemDto in items)
                {
                    // 製品の存在と在庫確認
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

        public async Task ConfirmOrderAsync(Guid orderId)
        {
            // トランザクションの開始
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // 注文の取得
                var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
                if (order == null)
                    throw new ArgumentException($"Order with ID {orderId} not found");

                // 注文の確認
                order.ConfirmOrder();
                _unitOfWork.Orders.Update(order);
                await _unitOfWork.SaveChangesAsync();

                // トランザクションのコミット
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception)
            {
                // エラーが発生した場合はロールバック
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task CancelOrderAsync(Guid orderId)
        {
            // トランザクションの開始
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // 注文の取得
                var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
                if (order == null)
                    throw new ArgumentException($"Order with ID {orderId} not found");

                // 注文のキャンセル（これは在庫を戻す必要がある）
                order.CancelOrder();
                _unitOfWork.Orders.Update(order);

                // 注文項目の取得
                var orderItems = await _unitOfWork.Orders.GetOrderItemsAsync(orderId);

                // 各製品の在庫を戻す
                foreach (var item in orderItems)
                {
                    var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
                    if (product != null)
                    {
                        product.ChangeStock(product.StockQuantity + item.Quantity);
                        _unitOfWork.Products.Update(product);
                    }
                }

                await _unitOfWork.SaveChangesAsync();

                // トランザクションのコミット
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception)
            {
                // エラーが発生した場合はロールバック
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
    }

    // 注文項目DTO
    public class OrderItemDto
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }

    #endregion

    #region Dapperを使用した実装例

    // Dapperを使用したUnit of Workの実装

    // Dapper用リポジトリ基底クラス
    public abstract class DapperRepository<T> : IRepository<T> where T : Entity
    {
        protected readonly IDbConnection _connection;
        protected readonly IDbTransaction _transaction;
        protected readonly string _tableName;

        protected DapperRepository(IDbConnection connection, IDbTransaction transaction, string tableName)
        {
            _connection = connection;
            _transaction = transaction;
            _tableName = tableName;
        }

        public virtual async Task<T> GetByIdAsync(Guid id)
        {
            string sql = $"SELECT * FROM {_tableName} WHERE Id = @Id";
            return await _connection.QuerySingleOrDefaultAsync<T>(sql, new { Id = id }, _transaction);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            string sql = $"SELECT * FROM {_tableName}";
            return await _connection.QueryAsync<T>(sql, transaction: _transaction);
        }

        public abstract Task AddAsync(T entity);
        public abstract void Update(T entity);
        public abstract void Remove(T entity);
    }

    // Dapper用製品リポジトリ
    public class DapperProductRepository : DapperRepository<Product>, IProductRepository
    {
        public DapperProductRepository(IDbConnection connection, IDbTransaction transaction)
            : base(connection, transaction, "Products")
        {
        }

        public override async Task AddAsync(Product entity)
        {
            string sql = @"
                INSERT INTO Products (Id, Name, Description, Price, StockQuantity, IsActive)
                VALUES (@Id, @Name, @Description, @Price, @StockQuantity, @IsActive)";

            await _connection.ExecuteAsync(sql, entity, _transaction);
        }

        public override void Update(Product entity)
        {
            string sql = @"
                UPDATE Products
                SET Name = @Name, Description = @Description, Price = @Price,
                    StockQuantity = @StockQuantity, IsActive = @IsActive
                WHERE Id = @Id";

            _connection.Execute(sql, entity, _transaction);
        }

        public override void Remove(Product entity)
        {
            string sql = "DELETE FROM Products WHERE Id = @Id";
            _connection.Execute(sql, new { entity.Id }, _transaction);
        }

        public async Task<IEnumerable<Product>> GetActiveProductsAsync()
        {
            string sql = "SELECT * FROM Products WHERE IsActive = @IsActive";
            return await _connection.QueryAsync<Product>(sql, new { IsActive = true }, _transaction);
        }

        public async Task<IEnumerable<Product>> GetProductsWithLowStockAsync(int threshold)
        {
            string sql = "SELECT * FROM Products WHERE StockQuantity < @Threshold AND IsActive = @IsActive";
            return await _connection.QueryAsync<Product>(sql, new { Threshold = threshold, IsActive = true }, _transaction);
        }
    }

    // Dapper用Unit of Work
    public class DapperUnitOfWork : IUnitOfWork
    {
        private readonly IDbConnection _connection;
        private IDbTransaction _transaction;
        private bool _disposed = false;

        public IProductRepository Products { get; }
        public ICustomerRepository Customers { get; }
        public IOrderRepository Orders { get; }

        public DapperUnitOfWork(string connectionString)
        {
            _connection = new SqlConnection(connectionString);
            _connection.Open();

            Products = new DapperProductRepository(_connection, _transaction);
            // 他のリポジトリも同様に初期化...
            Customers = null; // 実際の実装では適切なリポジトリを設定
            Orders = null;    // 実際の実装では適切なリポジトリを設定
        }

        public async Task<int> SaveChangesAsync()
        {
            // Dapperでは各操作で即座にデータベースに保存されるため、
            // このメソッドは互換性のために残していますが、特に何もしません
            return await Task.FromResult(0);
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = _connection.BeginTransaction();
            
            // リポジトリにトランザクションを再設定する必要がある
            Products = new DapperProductRepository(_connection, _transaction);
            // 他のリポジトリも同様に再設定...
            
            await Task.CompletedTask;
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                _transaction?.Commit();
            }
            finally
            {
                _transaction?.Dispose();
                _transaction = null;
                
                // トランザクションなしでリポジトリを再設定
                Products = new DapperProductRepository(_connection, null);
                // 他のリポジトリも同様に再設定...
            }
            
            await Task.CompletedTask;
        }

        public async Task RollbackTransactionAsync()
        {
            try
            {
                _transaction?.Rollback();
            }
            finally
            {
                _transaction?.Dispose();
                _transaction = null;
                
                // トランザクションなしでリポジトリを再設定
                Products = new DapperProductRepository(_connection, null);
                // 他のリポジトリも同様に再設定...
            }
            
            await Task.CompletedTask;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _transaction?.Dispose();
                    _connection.Dispose();
                }

                _disposed = true;
            }
        }
    }

    // 注: これらのクラスはDapperの概念を示すためのものであり、
    // 実際のコードではDapperのパッケージ参照が必要です
    public static class DapperExtensions
    {
        public static Task<T> QuerySingleOrDefaultAsync<T>(this IDbConnection connection, string sql, object param = null, IDbTransaction transaction = null)
            => Task.FromResult(default(T));

        public static Task<IEnumerable<T>> QueryAsync<T>(this IDbConnection connection, string sql, object param = null, IDbTransaction transaction = null)
            => Task.FromResult(Enumerable.Empty<T>());

        public static Task<int> ExecuteAsync(this IDbConnection connection, string sql, object param = null, IDbTransaction transaction = null)
            => Task.FromResult(0);

        public static int Execute(this IDbConnection connection, string sql, object param = null, IDbTransaction transaction = null)
            => 0;
    }

    public class SqlConnection : IDbConnection
    {
        public SqlConnection(string connectionString) { }
        public string ConnectionString { get; set; }
        public int ConnectionTimeout => 0;
        public string Database => string.Empty;
        public ConnectionState State => ConnectionState.Open;

        public IDbTransaction BeginTransaction() => null;
        public IDbTransaction BeginTransaction(IsolationLevel il) => null;
        public void ChangeDatabase(string databaseName) { }
        public void Close() { }
        public IDbCommand CreateCommand() => null;
        public void Dispose() { }
        public void Open() { }
    }

    #endregion

    #region InMemory実装（テスト用）

    // インメモリ実装のUnit of Work（テスト用）

    // インメモリ製品リポジトリ
    public class InMemoryProductRepository : IProductRepository
    {
        private readonly Dictionary<Guid, Product> _products = new Dictionary<Guid, Product>();

        public Task<Product> GetByIdAsync(Guid id)
        {
            _products.TryGetValue(id, out var product);
            return Task.FromResult(product);
        }

        public Task<IEnumerable<Product>> GetAllAsync()
        {
            return Task.FromResult<IEnumerable<Product>>(_products.Values.ToList());
        }

        public Task AddAsync(Product entity)
        {
            _products[entity.Id] = entity;
            return Task.CompletedTask;
        }

        public void Update(Product entity)
        {
            _products[entity.Id] = entity;
        }

        public void Remove(Product entity)
        {
            _products.Remove(entity.Id);
        }

        public Task<IEnumerable<Product>> GetActiveProductsAsync()
        {
            return Task.FromResult<IEnumerable<Product>>(_products.Values.Where(p => p.IsActive).ToList());
        }

        public Task<IEnumerable<Product>> GetProductsWithLowStockAsync(int threshold)
        {
            return Task.FromResult<IEnumerable<Product>>(_products.Values.Where(p => p.IsActive && p.StockQuantity < threshold).ToList());
        }
    }

    // インメモリ顧客リポジトリ
    public class InMemoryCustomerRepository : ICustomerRepository
    {
        private readonly Dictionary<Guid, Customer> _customers = new Dictionary<Guid, Customer>();

        public Task<Customer> GetByIdAsync(Guid id)
        {
            _customers.TryGetValue(id, out var customer);
            return Task.FromResult(customer);
        }

        public Task<IEnumerable<Customer>> GetAllAsync()
        {
            return Task.FromResult<IEnumerable<Customer>>(_customers.Values.ToList());
        }

        public Task AddAsync(Customer entity)
        {
            _customers[entity.Id] = entity;
            return Task.CompletedTask;
        }

        public void Update(Customer entity)
        {
            _customers[entity.Id] = entity;
        }

        public void Remove(Customer entity)
        {
            _customers.Remove(entity.Id);
        }

        public Task<IEnumerable<Customer>> GetActiveCustomersAsync()
        {
            return Task.FromResult<IEnumerable<Customer>>(_customers.Values.Where(c => c.IsActive).ToList());
        }

        public Task<Customer> GetByEmailAsync(string email)
        {
            return Task.FromResult(_customers.Values.FirstOrDefault(c => c.Email == email));
        }
    }

    // インメモリ注文リポジトリ
    public class InMemoryOrderRepository : IOrderRepository
    {
        private readonly Dictionary<Guid, Order> _orders = new Dictionary<Guid, Order>();
        private readonly Dictionary<Guid, List<OrderItem>> _orderItems = new Dictionary<Guid, List<OrderItem>>();

        public Task<Order> GetByIdAsync(Guid id)
        {
            _orders.TryGetValue(id, out var order);
            return Task.FromResult(order);
        }

        public Task<IEnumerable<Order>> GetAllAsync()
        {
            return Task.FromResult<IEnumerable<Order>>(_orders.Values.ToList());
        }

        public Task AddAsync(Order entity)
        {
            _orders[entity.Id] = entity;
            _orderItems[entity.Id] = entity.Items.ToList();
            return Task.CompletedTask;
        }

        public void Update(Order entity)
        {
            _orders[entity.Id] = entity;
            _orderItems[entity.Id] = entity.Items.ToList();
        }

        public void Remove(Order entity)
        {
            _orders.Remove(entity.Id);
            _orderItems.Remove(entity.Id);
        }

        public Task<IEnumerable<Order>> GetOrdersByCustomerAsync(Guid customerId)
        {
            return Task.FromResult<IEnumerable<Order>>(_orders.Values.Where(o => o.CustomerId == customerId).ToList());
        }

        public Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status)
        {
            return Task.FromResult<IEnumerable<Order>>(_orders.Values.Where(o => o.Status == status).ToList());
        }

        public Task<IEnumerable<OrderItem>> GetOrderItemsAsync(Guid orderId)
        {
            _orderItems.TryGetValue(orderId, out var items);
            return Task.FromResult<IEnumerable<OrderItem>>(items ?? new List<OrderItem>());
        }
    }

    // インメモリUnit of Work
    public class InMemoryUnitOfWork : IUnitOfWork
    {
        public IProductRepository Products { get; }
        public ICustomerRepository Customers { get; }
        public IOrderRepository Orders { get; }

        private bool _isTransactionActive = false;
        private readonly Dictionary<Guid, Product> _productBackup = new Dictionary<Guid, Product>();
        private readonly Dictionary<Guid, Customer> _customerBackup = new Dictionary<Guid, Customer>();
        private readonly Dictionary<Guid, Order> _orderBackup = new Dictionary<Guid, Order>();
        private bool _disposed = false;

        public InMemoryUnitOfWork()
        {
            Products = new InMemoryProductRepository();
            Customers = new InMemoryCustomerRepository();
            Orders = new InMemoryOrderRepository();
        }

        public Task<int> SaveChangesAsync()
        {
            // インメモリ実装では即時保存されるため、このメソッドは何もしない
            return Task.FromResult(0);
        }

        public Task BeginTransactionAsync()
        {
            if (_isTransactionActive)
                throw new InvalidOperationException("Transaction already active");

            _isTransactionActive = true;
            
            // 現在の状態をバックアップ
            // 実際の実装では、各リポジトリの内部状態の深いコピーを作成する必要がある
            
            return Task.CompletedTask;
        }

        public Task CommitTransactionAsync()
        {
            if (!_isTransactionActive)
                throw new InvalidOperationException("No active transaction to commit");

            _isTransactionActive = false;
            
            // バックアップをクリア
            _productBackup.Clear();
            _customerBackup.Clear();
            _orderBackup.Clear();
            
            return Task.CompletedTask;
        }

        public Task RollbackTransactionAsync()
        {
            if (!_isTransactionActive)
                throw new InvalidOperationException("No active transaction to rollback");

            _isTransactionActive = false;
            
            // バックアップから状態を復元
            // 実際の実装では、バックアップからリポジトリの状態を復元する
            
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // リソースの解放
                }

                _disposed = true;
            }
        }
    }

    #endregion

    #region 使用例

    // Unit of Workパターンの使用例
    public class OrderProcessingDemo
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly OrderService _orderService;

        public OrderProcessingDemo(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _orderService = new OrderService(unitOfWork);
        }

        public async Task RunDemoAsync()
        {
            try
            {
                // 顧客の作成
                var customer = new Customer("John Doe", "john.doe@example.com", "123-456-7890", "123 Main St");
                await _unitOfWork.Customers.AddAsync(customer);
                await _unitOfWork.SaveChangesAsync();
                Console.WriteLine($"Created customer: {customer.Name} (ID: {customer.Id})");

                // 製品の作成
                var laptop = new Product("Laptop", "High-performance laptop", 1200.00m, 10);
                var phone = new Product("Smartphone", "Latest smartphone", 800.00m, 20);
                var headphones = new Product("Headphones", "Noise-cancelling headphones", 200.00m, 30);

                await _unitOfWork.Products.AddAsync(laptop);
                await _unitOfWork.Products.AddAsync(phone);
                await _unitOfWork.Products.AddAsync(headphones);
                await _unitOfWork.SaveChangesAsync();
                Console.WriteLine("Created products");

                // 注文の作成
                var orderItems = new List<OrderItemDto>
                {
                    new OrderItemDto { ProductId = laptop.Id, Quantity = 1 },
                    new OrderItemDto { ProductId = headphones.Id, Quantity = 2 }
                };

                Console.WriteLine("Creating order...");
                var orderId = await _orderService.CreateOrderAsync(customer.Id, orderItems);
                Console.WriteLine($"Order created: {orderId}");

                // 注文の確認
                Console.WriteLine("Confirming order...");
                await _orderService.ConfirmOrderAsync(orderId);
                Console.WriteLine("Order confirmed");

                // 在庫の確認
                var updatedLaptop = await _unitOfWork.Products.GetByIdAsync(laptop.Id);
                var updatedHeadphones = await _unitOfWork.Products.GetByIdAsync(headphones.Id);
                Console.WriteLine($"Laptop stock after order: {updatedLaptop.StockQuantity}");
                Console.WriteLine($"Headphones stock after order: {updatedHeadphones.StockQuantity}");

                // エラーが発生するケース
                try
                {
                    Console.WriteLine("Attempting to create order with insufficient stock...");
                    var tooManyItems = new List<OrderItemDto>
                    {
                        new OrderItemDto { ProductId = phone.Id, Quantity = 30 } // 在庫は20しかない
                    };

                    await _orderService.CreateOrderAsync(customer.Id, tooManyItems);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Expected error occurred: {ex.Message}");

                    // 在庫が変更されていないことを確認
                    var phoneAfterFailedOrder = await _unitOfWork.Products.GetByIdAsync(phone.Id);
                    Console.WriteLine($"Phone stock after failed order: {phoneAfterFailedOrder.StockQuantity}");
                }

                // 注文のキャンセル
                Console.WriteLine("Cancelling order...");
                await _orderService.CancelOrderAsync(orderId);
                Console.WriteLine("Order cancelled");

                // 在庫が戻っていることを確認
                updatedLaptop = await _unitOfWork.Products.GetByIdAsync(laptop.Id);
                updatedHeadphones = await _unitOfWork.Products.GetByIdAsync(headphones.Id);
                Console.WriteLine($"Laptop stock after cancellation: {updatedLaptop.StockQuantity}");
                Console.WriteLine($"Headphones stock after cancellation: {updatedHeadphones.StockQuantity}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }
        }
    }

    #endregion
}