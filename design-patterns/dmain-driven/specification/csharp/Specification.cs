using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DesignPatternsCookbook.DomainDriven.Specification
{
    #region 仕様パターンの基本実装

    // 基本インターフェース
    public interface ISpecification<T>
    {
        bool IsSatisfiedBy(T entity);
        Expression<Func<T, bool>> ToExpression();
    }

    // 汎用仕様基底クラス
    public abstract class Specification<T> : ISpecification<T>
    {
        public bool IsSatisfiedBy(T entity)
        {
            var predicate = ToExpression().Compile();
            return predicate(entity);
        }

        public abstract Expression<Func<T, bool>> ToExpression();

        // 論理演算子のオーバーロード
        public static Specification<T> operator &(Specification<T> left, Specification<T> right)
        {
            return new AndSpecification<T>(left, right);
        }

        public static Specification<T> operator |(Specification<T> left, Specification<T> right)
        {
            return new OrSpecification<T>(left, right);
        }

        public static Specification<T> operator !(Specification<T> specification)
        {
            return new NotSpecification<T>(specification);
        }
    }

    // AND仕様
    public class AndSpecification<T> : Specification<T>
    {
        private readonly ISpecification<T> _left;
        private readonly ISpecification<T> _right;

        public AndSpecification(ISpecification<T> left, ISpecification<T> right)
        {
            _left = left;
            _right = right;
        }

        public override Expression<Func<T, bool>> ToExpression()
        {
            var leftExpression = _left.ToExpression();
            var rightExpression = _right.ToExpression();

            var paramExpr = Expression.Parameter(typeof(T), "x");

            var leftVisitor = new ParameterReplaceVisitor(leftExpression.Parameters[0], paramExpr);
            var left = leftVisitor.Visit(leftExpression.Body);

            var rightVisitor = new ParameterReplaceVisitor(rightExpression.Parameters[0], paramExpr);
            var right = rightVisitor.Visit(rightExpression.Body);

            return Expression.Lambda<Func<T, bool>>(
                Expression.AndAlso(left, right), paramExpr);
        }
    }

    // OR仕様
    public class OrSpecification<T> : Specification<T>
    {
        private readonly ISpecification<T> _left;
        private readonly ISpecification<T> _right;

        public OrSpecification(ISpecification<T> left, ISpecification<T> right)
        {
            _left = left;
            _right = right;
        }

        public override Expression<Func<T, bool>> ToExpression()
        {
            var leftExpression = _left.ToExpression();
            var rightExpression = _right.ToExpression();

            var paramExpr = Expression.Parameter(typeof(T), "x");

            var leftVisitor = new ParameterReplaceVisitor(leftExpression.Parameters[0], paramExpr);
            var left = leftVisitor.Visit(leftExpression.Body);

            var rightVisitor = new ParameterReplaceVisitor(rightExpression.Parameters[0], paramExpr);
            var right = rightVisitor.Visit(rightExpression.Body);

            return Expression.Lambda<Func<T, bool>>(
                Expression.OrElse(left, right), paramExpr);
        }
    }

    // NOT仕様
    public class NotSpecification<T> : Specification<T>
    {
        private readonly ISpecification<T> _specification;

        public NotSpecification(ISpecification<T> specification)
        {
            _specification = specification;
        }

        public override Expression<Func<T, bool>> ToExpression()
        {
            var expression = _specification.ToExpression();
            var param = expression.Parameters[0];
            var body = expression.Body;

            return Expression.Lambda<Func<T, bool>>(
                Expression.Not(body), param);
        }
    }

    // ExpressionTreeのパラメータを置換するためのヘルパークラス
    public class ParameterReplaceVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression _oldParameter;
        private readonly ParameterExpression _newParameter;

        public ParameterReplaceVisitor(ParameterExpression oldParameter, ParameterExpression newParameter)
        {
            _oldParameter = oldParameter;
            _newParameter = newParameter;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == _oldParameter ? _newParameter : base.VisitParameter(node);
        }
    }

    #endregion

    #region ドメインモデル

    // ドメインモデル：製品
    public class Product
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public ProductCategory Category { get; private set; }
        public decimal Price { get; private set; }
        public int StockQuantity { get; private set; }
        public bool IsActive { get; private set; }
        public int Weight { get; private set; } // グラム単位
        public DateTime CreatedAt { get; private set; }
        public DateTime? DiscontinuedAt { get; private set; }

        // コンストラクタ
        public Product(
            string name,
            ProductCategory category,
            decimal price,
            int stockQuantity,
            int weight)
        {
            Id = Guid.NewGuid();
            Name = name;
            Category = category;
            Price = price;
            StockQuantity = stockQuantity;
            IsActive = true;
            Weight = weight;
            CreatedAt = DateTime.UtcNow;
        }

        // プロダクトを非アクティブにする
        public void Discontinue()
        {
            IsActive = false;
            DiscontinuedAt = DateTime.UtcNow;
        }

        // 在庫を減らす
        public void RemoveStock(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be positive");

            if (StockQuantity < quantity)
                throw new InvalidOperationException("Not enough stock available");

            StockQuantity -= quantity;
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

    #endregion

    #region 製品関連の仕様

    // 有効な製品の仕様
    public class ActiveProductSpecification : Specification<Product>
    {
        public override Expression<Func<Product, bool>> ToExpression()
        {
            return product => product.IsActive;
        }
    }

    // 在庫がある製品の仕様
    public class InStockProductSpecification : Specification<Product>
    {
        public override Expression<Func<Product, bool>> ToExpression()
        {
            return product => product.StockQuantity > 0;
        }
    }

    // 特定の最小数量以上の在庫がある製品の仕様
    public class MinimumStockProductSpecification : Specification<Product>
    {
        private readonly int _minimumStock;

        public MinimumStockProductSpecification(int minimumStock)
        {
            _minimumStock = minimumStock;
        }

        public override Expression<Func<Product, bool>> ToExpression()
        {
            return product => product.StockQuantity >= _minimumStock;
        }
    }

    // 特定カテゴリの製品の仕様
    public class ProductCategorySpecification : Specification<Product>
    {
        private readonly ProductCategory _category;

        public ProductCategorySpecification(ProductCategory category)
        {
            _category = category;
        }

        public override Expression<Func<Product, bool>> ToExpression()
        {
            return product => product.Category == _category;
        }
    }

    // 特定の価格範囲内の製品の仕様
    public class PriceRangeSpecification : Specification<Product>
    {
        private readonly decimal _minPrice;
        private readonly decimal _maxPrice;

        public PriceRangeSpecification(decimal minPrice, decimal maxPrice)
        {
            _minPrice = minPrice;
            _maxPrice = maxPrice;
        }

        public override Expression<Func<Product, bool>> ToExpression()
        {
            return product => product.Price >= _minPrice && product.Price <= _maxPrice;
        }
    }

    // 軽量製品の仕様（例：配送料無料の対象など）
    public class LightweightProductSpecification : Specification<Product>
    {
        private readonly int _maxWeight; // グラム単位

        public LightweightProductSpecification(int maxWeight)
        {
            _maxWeight = maxWeight;
        }

        public override Expression<Func<Product, bool>> ToExpression()
        {
            return product => product.Weight <= _maxWeight;
        }
    }

    // 新製品の仕様（例：過去30日以内に追加された製品）
    public class NewProductSpecification : Specification<Product>
    {
        private readonly int _daysThreshold;

        public NewProductSpecification(int daysThreshold = 30)
        {
            _daysThreshold = daysThreshold;
        }

        public override Expression<Func<Product, bool>> ToExpression()
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-_daysThreshold);
            return product => product.CreatedAt >= cutoffDate;
        }
    }

    // 複合仕様の例：エクスプレス配送に適格な製品（軽量かつ在庫あり）
    public class ExpressShippingEligibleSpecification : Specification<Product>
    {
        public override Expression<Func<Product, bool>> ToExpression()
        {
            var inStock = new InStockProductSpecification();
            var lightweight = new LightweightProductSpecification(500); // 500g以下
            var active = new ActiveProductSpecification();

            return product => 
                product.StockQuantity > 0 && 
                product.Weight <= 500 && 
                product.IsActive;
        }
    }

    // 特別価格製品の仕様（複合仕様を組み合わせて生成）
    public static class ProductSpecifications
    {
        public static Specification<Product> SpecialOffers()
        {
            var active = new ActiveProductSpecification();
            var inStock = new InStockProductSpecification();
            var electronic = new ProductCategorySpecification(ProductCategory.Electronics);
            var clothing = new ProductCategorySpecification(ProductCategory.Clothing);
            var priceUnder100 = new PriceRangeSpecification(0, 100);
            var newProduct = new NewProductSpecification();

            // 有効かつ在庫があり、エレクトロニクスまたは衣類カテゴリで、
            // 100ドル未満で、かつ新製品である製品
            return active & inStock & (electronic | clothing) & priceUnder100 & newProduct;
        }

        public static Specification<Product> LowStockItems()
        {
            var active = new ActiveProductSpecification();
            var lowStock = new MinimumStockProductSpecification(1) & !new MinimumStockProductSpecification(10);
            
            return active & lowStock;
        }

        public static Specification<Product> FreeShippingEligible()
        {
            var active = new ActiveProductSpecification();
            var inStock = new InStockProductSpecification();
            var lightweight = new LightweightProductSpecification(1000); // 1kg以下
            
            return active & inStock & lightweight;
        }
    }

    #endregion

    #region リポジトリとの統合

    // 仕様パターンを使用したリポジトリのインターフェース
    public interface IProductRepository
    {
        IEnumerable<Product> Find(ISpecification<Product> specification);
        Task<IEnumerable<Product>> FindAsync(ISpecification<Product> specification);
        Task<int> CountAsync(ISpecification<Product> specification);
    }

    // Entity Framework Coreを使用した実装例
    public class EfProductRepository : IProductRepository
    {
        private readonly DbContext _context;

        public EfProductRepository(DbContext context)
        {
            _context = context;
        }

        public IEnumerable<Product> Find(ISpecification<Product> specification)
        {
            return _context.Set<Product>()
                .Where(specification.ToExpression().Compile())
                .ToList();
        }

        public async Task<IEnumerable<Product>> FindAsync(ISpecification<Product> specification)
        {
            // ここでは、ToExpressionが返す式をEFクエリに直接適用できる
            return await _context.Set<Product>()
                .Where(specification.ToExpression())
                .ToListAsync();
        }

        public async Task<int> CountAsync(ISpecification<Product> specification)
        {
            return await _context.Set<Product>()
                .CountAsync(specification.ToExpression());
        }
    }

    // 注：DbContextとその関連クラスはここでは実装していません
    // 実際のコードではEntity Frameworkの参照が必要です
    public class DbContext
    {
        public DbSet<T> Set<T>() where T : class => null;
    }

    public static class QueryableExtensions
    {
        public static List<T> ToList<T>(this IQueryable<T> queryable) => new List<T>();
        public static Task<List<T>> ToListAsync<T>(this IQueryable<T> queryable) => Task.FromResult(new List<T>());
        public static Task<int> CountAsync<T>(this IQueryable<T> queryable, Expression<Func<T, bool>> predicate) => Task.FromResult(0);
    }

    public class DbSet<T> : IQueryable<T> where T : class
    {
        public IEnumerator<T> GetEnumerator() => throw new NotImplementedException();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => throw new NotImplementedException();
        public Type ElementType => throw new NotImplementedException();
        public Expression Expression => throw new NotImplementedException();
        public IQueryProvider Provider => throw new NotImplementedException();

        public IQueryable<T> Where(Expression<Func<T, bool>> predicate) => this;
    }

    #endregion

    #region アプリケーションサービスでの使用例

    // アプリケーションサービス
    public class ProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<IEnumerable<Product>> GetSpecialOffersAsync()
        {
            var specification = ProductSpecifications.SpecialOffers();
            return await _productRepository.FindAsync(specification);
        }

        public async Task<IEnumerable<Product>> GetLowStockProductsAsync()
        {
            var specification = ProductSpecifications.LowStockItems();
            return await _productRepository.FindAsync(specification);
        }

        public async Task<IEnumerable<Product>> GetProductsByFiltersAsync(
            bool? active = null,
            int? minStock = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            ProductCategory? category = null)
        {
            // 動的に仕様を構築
            Specification<Product> specification = new TrueSpecification<Product>();

            if (active.HasValue)
            {
                var activeSpec = new ActiveProductSpecification();
                specification = active.Value ? specification & activeSpec : specification & !activeSpec;
            }

            if (minStock.HasValue)
            {
                specification = specification & new MinimumStockProductSpecification(minStock.Value);
            }

            if (minPrice.HasValue && maxPrice.HasValue)
            {
                specification = specification & new PriceRangeSpecification(minPrice.Value, maxPrice.Value);
            }

            if (category.HasValue)
            {
                specification = specification & new ProductCategorySpecification(category.Value);
            }

            return await _productRepository.FindAsync(specification);
        }
    }

    // 常にtrueを返す仕様（開始点として使用）
    public class TrueSpecification<T> : Specification<T>
    {
        public override Expression<Func<T, bool>> ToExpression()
        {
            return x => true;
        }
    }

    #endregion

    #region ドメインサービスでの使用例

    // ドメインサービス
    public class InventoryService
    {
        private readonly IProductRepository _productRepository;

        public InventoryService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<bool> IsProductAvailableForOrderAsync(Guid productId, int quantity)
        {
            // 製品固有の仕様
            var productIdSpecification = new ProductByIdSpecification(productId);
            
            // 有効で、十分な在庫がある製品の仕様
            var availabilitySpecification = 
                new ActiveProductSpecification() & 
                new MinimumStockProductSpecification(quantity);
            
            // 両方の仕様を組み合わせる
            var specification = productIdSpecification & availabilitySpecification;
            
            // 仕様を満たす製品の数を確認（0か1のはず）
            var count = await _productRepository.CountAsync(specification);
            return count > 0;
        }

        public async Task<IEnumerable<Product>> GetLowStockProductsAsync()
        {
            // 在庫が10個未満の有効な製品
            var lowStockSpecification = 
                new ActiveProductSpecification() & 
                new InStockProductSpecification() & 
                !new MinimumStockProductSpecification(10);
            
            return await _productRepository.FindAsync(lowStockSpecification);
        }

        public async Task<IEnumerable<Product>> GetProductsEligibleForPromotionAsync()
        {
            // 在庫が多くある製品（在庫処分セールの対象）
            var excessStockSpecification = 
                new ActiveProductSpecification() & 
                new MinimumStockProductSpecification(50);
            
            // 新製品ではない（3ヶ月以上経過した）製品
            var notNewSpecification = !new NewProductSpecification(90);
            
            return await _productRepository.FindAsync(excessStockSpecification & notNewSpecification);
        }
    }

    // 特定のIDを持つ製品の仕様
    public class ProductByIdSpecification : Specification<Product>
    {
        private readonly Guid _id;

        public ProductByIdSpecification(Guid id)
        {
            _id = id;
        }

        public override Expression<Func<Product, bool>> ToExpression()
        {
            return product => product.Id == _id;
        }
    }

    #endregion
}