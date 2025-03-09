using System;
using System.Collections.Generic;

namespace DesignPatternsCookbook.Behavioral.Strategy
{
    #region 基本的な戦略パターンの実装

    // コンテキスト：戦略を使用するクラス
    public class SortContext
    {
        private ISortStrategy _strategy;

        public SortContext(ISortStrategy strategy)
        {
            _strategy = strategy;
        }

        // 実行時に戦略を変更できる
        public void SetSortStrategy(ISortStrategy strategy)
        {
            _strategy = strategy;
        }

        // 選択された戦略を使用する
        public void Sort<T>(IList<T> items) where T : IComparable<T>
        {
            _strategy.Sort(items);
            Console.WriteLine($"Sorted using {_strategy.GetType().Name}");
        }
    }

    // 戦略インターフェース
    public interface ISortStrategy
    {
        void Sort<T>(IList<T> items) where T : IComparable<T>;
    }

    // 具体的な戦略1: バブルソート
    public class BubbleSortStrategy : ISortStrategy
    {
        public void Sort<T>(IList<T> items) where T : IComparable<T>
        {
            int n = items.Count;
            for (int i = 0; i < n - 1; i++)
            {
                for (int j = 0; j < n - i - 1; j++)
                {
                    if (items[j].CompareTo(items[j + 1]) > 0)
                    {
                        // 要素の交換
                        T temp = items[j];
                        items[j] = items[j + 1];
                        items[j + 1] = temp;
                    }
                }
            }
        }
    }

    // 具体的な戦略2: クイックソート
    public class QuickSortStrategy : ISortStrategy
    {
        public void Sort<T>(IList<T> items) where T : IComparable<T>
        {
            QuickSort(items, 0, items.Count - 1);
        }

        private void QuickSort<T>(IList<T> items, int left, int right) where T : IComparable<T>
        {
            if (left < right)
            {
                int pivotIndex = Partition(items, left, right);
                QuickSort(items, left, pivotIndex - 1);
                QuickSort(items, pivotIndex + 1, right);
            }
        }

        private int Partition<T>(IList<T> items, int left, int right) where T : IComparable<T>
        {
            T pivot = items[right];
            int i = left - 1;

            for (int j = left; j < right; j++)
            {
                if (items[j].CompareTo(pivot) <= 0)
                {
                    i++;
                    T temp = items[i];
                    items[i] = items[j];
                    items[j] = temp;
                }
            }

            T temp1 = items[i + 1];
            items[i + 1] = items[right];
            items[right] = temp1;

            return i + 1;
        }
    }

    // 具体的な戦略3: 挿入ソート
    public class InsertionSortStrategy : ISortStrategy
    {
        public void Sort<T>(IList<T> items) where T : IComparable<T>
        {
            int n = items.Count;
            for (int i = 1; i < n; ++i)
            {
                T key = items[i];
                int j = i - 1;

                while (j >= 0 && items[j].CompareTo(key) > 0)
                {
                    items[j + 1] = items[j];
                    j = j - 1;
                }
                items[j + 1] = key;
            }
        }
    }

    // 使用例
    public class SortDemo
    {
        public static void Run()
        {
            // 初期配列
            var numbers = new List<int> { 23, 42, 15, 8, 4, 16 };

            // コンテキストをバブルソート戦略で初期化
            var context = new SortContext(new BubbleSortStrategy());
            
            Console.WriteLine("Original list: " + string.Join(", ", numbers));
            
            // バブルソートを使用
            context.Sort(numbers);
            Console.WriteLine("After bubble sort: " + string.Join(", ", numbers));
            
            // リストをシャッフル
            numbers = new List<int> { 23, 42, 15, 8, 4, 16 };
            
            // 戦略をクイックソートに変更
            context.SetSortStrategy(new QuickSortStrategy());
            context.Sort(numbers);
            Console.WriteLine("After quick sort: " + string.Join(", ", numbers));
        }
    }

    #endregion

    #region ビジネスロジックでの実践的な戦略パターン

    // 支払い処理の戦略パターン

    // 注文クラス
    public class Order
    {
        public Guid Id { get; private set; }
        public string CustomerName { get; private set; }
        public decimal TotalAmount { get; private set; }
        public List<OrderItem> Items { get; private set; }

        public Order(string customerName, decimal totalAmount)
        {
            Id = Guid.NewGuid();
            CustomerName = customerName;
            TotalAmount = totalAmount;
            Items = new List<OrderItem>();
        }

        public void AddItem(string productName, decimal price, int quantity)
        {
            Items.Add(new OrderItem(productName, price, quantity));
            TotalAmount = Items.Sum(item => item.Price * item.Quantity);
        }
    }

    public class OrderItem
    {
        public string ProductName { get; }
        public decimal Price { get; }
        public int Quantity { get; }

        public OrderItem(string productName, decimal price, int quantity)
        {
            ProductName = productName;
            Price = price;
            Quantity = quantity;
        }
    }

    // 支払い処理のコンテキスト
    public class PaymentProcessor
    {
        private IPaymentStrategy _paymentStrategy;

        public PaymentProcessor(IPaymentStrategy paymentStrategy)
        {
            _paymentStrategy = paymentStrategy;
        }

        public void SetPaymentStrategy(IPaymentStrategy paymentStrategy)
        {
            _paymentStrategy = paymentStrategy;
        }

        public PaymentResult ProcessPayment(Order order)
        {
            Console.WriteLine($"Processing payment for order {order.Id} using {_paymentStrategy.GetType().Name}");
            return _paymentStrategy.Pay(order.TotalAmount);
        }
    }

    // 支払い戦略インターフェース
    public interface IPaymentStrategy
    {
        PaymentResult Pay(decimal amount);
    }

    // 支払い結果
    public class PaymentResult
    {
        public bool Success { get; }
        public string TransactionId { get; }
        public string Message { get; }

        public PaymentResult(bool success, string transactionId, string message)
        {
            Success = success;
            TransactionId = transactionId;
            Message = message;
        }
    }

    // クレジットカード支払い戦略
    public class CreditCardPaymentStrategy : IPaymentStrategy
    {
        private readonly string _cardNumber;
        private readonly string _cvv;
        private readonly string _expiryDate;
        private readonly string _cardHolderName;

        public CreditCardPaymentStrategy(string cardNumber, string cvv, string expiryDate, string cardHolderName)
        {
            _cardNumber = cardNumber;
            _cvv = cvv;
            _expiryDate = expiryDate;
            _cardHolderName = cardHolderName;
        }

        public PaymentResult Pay(decimal amount)
        {
            // クレジットカード処理のロジック...
            Console.WriteLine($"Processing credit card payment of ${amount}");
            Console.WriteLine($"Card Details: {MaskCardNumber(_cardNumber)}, Expiry: {_expiryDate}, Holder: {_cardHolderName}");
            
            // 実際の実装では外部のクレジットカード処理サービスを呼び出す
            var transactionId = Guid.NewGuid().ToString();
            return new PaymentResult(true, transactionId, "Credit card payment successful");
        }

        private string MaskCardNumber(string cardNumber)
        {
            if (cardNumber.Length <= 4)
                return cardNumber;

            return "XXXX-XXXX-XXXX-" + cardNumber.Substring(cardNumber.Length - 4);
        }
    }

    // PayPal支払い戦略
    public class PayPalPaymentStrategy : IPaymentStrategy
    {
        private readonly string _email;
        private readonly string _password;

        public PayPalPaymentStrategy(string email, string password)
        {
            _email = email;
            _password = password;
        }

        public PaymentResult Pay(decimal amount)
        {
            // PayPal処理のロジック...
            Console.WriteLine($"Processing PayPal payment of ${amount}");
            Console.WriteLine($"PayPal Account: {_email}");
            
            // 実際の実装ではPayPal APIを呼び出す
            var transactionId = Guid.NewGuid().ToString();
            return new PaymentResult(true, transactionId, "PayPal payment successful");
        }
    }

    // 銀行振込支払い戦略
    public class BankTransferPaymentStrategy : IPaymentStrategy
    {
        private readonly string _accountNumber;
        private readonly string _bankCode;
        private readonly string _accountName;

        public BankTransferPaymentStrategy(string accountNumber, string bankCode, string accountName)
        {
            _accountNumber = accountNumber;
            _bankCode = bankCode;
            _accountName = accountName;
        }

        public PaymentResult Pay(decimal amount)
        {
            // 銀行振込処理のロジック...
            Console.WriteLine($"Processing bank transfer payment of ${amount}");
            Console.WriteLine($"Bank Details: Account: {_accountNumber}, Bank Code: {_bankCode}, Name: {_accountName}");
            
            // 実際の実装では銀行APIや処理サービスを呼び出す
            var transactionId = Guid.NewGuid().ToString();
            return new PaymentResult(true, transactionId, "Bank transfer initiated");
        }
    }

    // 使用例
    public class PaymentDemo
    {
        public static void Run()
        {
            // 注文の作成
            var order = new Order("John Doe", 0);
            order.AddItem("Laptop", 1200, 1);
            order.AddItem("Mouse", 25, 1);
            order.AddItem("Keyboard", 45, 1);
            
            Console.WriteLine($"Order created: {order.Id}");
            Console.WriteLine($"Total amount: ${order.TotalAmount}");
            
            // クレジットカード支払い
            var creditCardStrategy = new CreditCardPaymentStrategy(
                "1234-5678-9012-3456", 
                "123", 
                "12/25", 
                "John Doe");
            var paymentProcessor = new PaymentProcessor(creditCardStrategy);
            var ccResult = paymentProcessor.ProcessPayment(order);
            
            Console.WriteLine($"Credit Card Payment Result: {(ccResult.Success ? "Success" : "Failure")}");
            Console.WriteLine($"Transaction ID: {ccResult.TransactionId}");
            Console.WriteLine($"Message: {ccResult.Message}");
            
            // PayPal支払いに切り替え
            var payPalStrategy = new PayPalPaymentStrategy("john.doe@example.com", "password");
            paymentProcessor.SetPaymentStrategy(payPalStrategy);
            var ppResult = paymentProcessor.ProcessPayment(order);
            
            Console.WriteLine($"PayPal Payment Result: {(ppResult.Success ? "Success" : "Failure")}");
            Console.WriteLine($"Transaction ID: {ppResult.TransactionId}");
            Console.WriteLine($"Message: {ppResult.Message}");
        }
    }

    #endregion

    #region DDD + 戦略パターン：配送料金計算

    // 値オブジェクト: 住所
    public class Address
    {
        public string Street { get; }
        public string City { get; }
        public string State { get; }
        public string Country { get; }
        public string ZipCode { get; }

        public Address(string street, string city, string state, string country, string zipCode)
        {
            Street = street;
            City = city;
            State = state;
            Country = country;
            ZipCode = zipCode;
        }
    }

    // 値オブジェクト: 金額
    public class Money
    {
        public decimal Amount { get; }
        public string Currency { get; }

        public Money(decimal amount, string currency)
        {
            Amount = amount;
            Currency = currency;
        }

        public static Money USD(decimal amount) => new Money(amount, "USD");
        public static Money EUR(decimal amount) => new Money(amount, "EUR");
        public static Money Zero(string currency) => new Money(0, currency);

        public Money Add(Money other)
        {
            if (Currency != other.Currency)
                throw new InvalidOperationException("Cannot add money with different currencies");

            return new Money(Amount + other.Amount, Currency);
        }

        public override string ToString() => $"{Amount} {Currency}";
    }

    // 配送情報
    public class ShippingInfo
    {
        public Money PackageWeight { get; }
        public Address OriginAddress { get; }
        public Address DestinationAddress { get; }
        public bool IsExpress { get; }
        public bool IsInsured { get; }
        public DateTime ShippingDate { get; }

        public ShippingInfo(
            Money packageWeight,
            Address originAddress,
            Address destinationAddress,
            bool isExpress,
            bool isInsured,
            DateTime shippingDate)
        {
            PackageWeight = packageWeight;
            OriginAddress = originAddress;
            DestinationAddress = destinationAddress;
            IsExpress = isExpress;
            IsInsured = isInsured;
            ShippingDate = shippingDate;
        }
    }

    // 配送料金計算の戦略インターフェース
    public interface IShippingCostStrategy
    {
        Money CalculateShippingCost(ShippingInfo shippingInfo);
        bool CanCalculate(ShippingInfo shippingInfo);
    }

    // 国内配送料金計算戦略
    public class DomesticShippingCostStrategy : IShippingCostStrategy
    {
        public Money CalculateShippingCost(ShippingInfo shippingInfo)
        {
            // 基本料金
            var baseCost = Money.USD(5.00m);
            
            // 重量加算（1kgあたり2ドル）
            var weightSurcharge = Money.USD(2.00m * shippingInfo.PackageWeight.Amount);
            
            // 速達料金
            var expressSurcharge = shippingInfo.IsExpress ? Money.USD(10.00m) : Money.Zero("USD");
            
            // 保険料金
            var insuranceCost = shippingInfo.IsInsured ? Money.USD(5.00m) : Money.Zero("USD");
            
            // 合計
            var totalCost = baseCost
                .Add(weightSurcharge)
                .Add(expressSurcharge)
                .Add(insuranceCost);
                
            return totalCost;
        }

        public bool CanCalculate(ShippingInfo shippingInfo)
        {
            // 出発地と配送先が同じ国である場合に適用
            return shippingInfo.OriginAddress.Country == shippingInfo.DestinationAddress.Country;
        }
    }

    // 国際配送料金計算戦略
    public class InternationalShippingCostStrategy : IShippingCostStrategy
    {
        // 国際ゾーン料金テーブル
        private readonly Dictionary<string, decimal> _zoneRates = new Dictionary<string, decimal>
        {
            { "Zone1", 15.00m },
            { "Zone2", 25.00m },
            { "Zone3", 40.00m },
            { "Zone4", 60.00m }
        };

        // 国からゾーンへのマッピング
        private readonly Dictionary<string, string> _countryZones = new Dictionary<string, string>
        {
            { "USA", "Zone1" },
            { "Canada", "Zone1" },
            { "Mexico", "Zone1" },
            { "UK", "Zone2" },
            { "France", "Zone2" },
            { "Germany", "Zone2" },
            { "China", "Zone3" },
            { "Japan", "Zone3" },
            { "Australia", "Zone4" },
            { "New Zealand", "Zone4" }
        };

        public Money CalculateShippingCost(ShippingInfo shippingInfo)
        {
            // 配送先の国からゾーンを決定
            string destinationCountry = shippingInfo.DestinationAddress.Country;
            string zone = GetZone(destinationCountry);
            
            // 基本料金（ゾーンに基づく）
            decimal baseRate = _zoneRates[zone];
            var baseCost = Money.USD(baseRate);
            
            // 重量加算（国際配送では1kgあたり5ドル）
            var weightSurcharge = Money.USD(5.00m * shippingInfo.PackageWeight.Amount);
            
            // 速達料金（国際配送では通常の2倍）
            var expressSurcharge = shippingInfo.IsExpress ? Money.USD(20.00m) : Money.Zero("USD");
            
            // 保険料金（国際配送では高め）
            var insuranceCost = shippingInfo.IsInsured ? Money.USD(15.00m) : Money.Zero("USD");
            
            // 合計
            var totalCost = baseCost
                .Add(weightSurcharge)
                .Add(expressSurcharge)
                .Add(insuranceCost);
                
            return totalCost;
        }

        public bool CanCalculate(ShippingInfo shippingInfo)
        {
            // 出発地と配送先が異なる国で、配送先の国がサポートされている場合に適用
            return shippingInfo.OriginAddress.Country != shippingInfo.DestinationAddress.Country &&
                   _countryZones.ContainsKey(shippingInfo.DestinationAddress.Country);
        }

        private string GetZone(string country)
        {
            if (_countryZones.TryGetValue(country, out var zone))
                return zone;
                
            return "Zone4"; // デフォルトは最も高いゾーン
        }
    }

    // 特別休日配送戦略
    public class HolidayShippingCostStrategy : IShippingCostStrategy
    {
        private readonly IShippingCostStrategy _baseStrategy;
        
        // 休日リスト
        private readonly List<(int month, int day)> _holidays = new List<(int month, int day)>
        {
            (12, 25), // クリスマス
            (1, 1),   // 元旦
            (7, 4),   // 独立記念日
            (11, 24)  // 感謝祭
        };

        public HolidayShippingCostStrategy(IShippingCostStrategy baseStrategy)
        {
            _baseStrategy = baseStrategy;
        }

        public Money CalculateShippingCost(ShippingInfo shippingInfo)
        {
            // 基本戦略による通常の料金計算
            var baseCost = _baseStrategy.CalculateShippingCost(shippingInfo);
            
            // 休日割増（通常料金の50%加算）
            var holidaySurcharge = new Money(baseCost.Amount * 0.5m, baseCost.Currency);
            
            return baseCost.Add(holidaySurcharge);
        }

        public bool CanCalculate(ShippingInfo shippingInfo)
        {
            // 基本戦略が計算可能で、かつ配送日が休日である場合に適用
            return _baseStrategy.CanCalculate(shippingInfo) && IsHoliday(shippingInfo.ShippingDate);
        }

        private bool IsHoliday(DateTime date)
        {
            return _holidays.Any(h => h.month == date.Month && h.day == date.Day);
        }
    }

    // 配送料金計算サービス
    public class ShippingCostService
    {
        private readonly List<IShippingCostStrategy> _strategies;

        public ShippingCostService(IEnumerable<IShippingCostStrategy> strategies)
        {
            _strategies = new List<IShippingCostStrategy>(strategies);
        }

        public Money CalculateShippingCost(ShippingInfo shippingInfo)
        {
            // 適用可能な最初の戦略を見つける
            var applicableStrategy = _strategies.FirstOrDefault(s => s.CanCalculate(shippingInfo));
            
            if (applicableStrategy == null)
                throw new InvalidOperationException("No applicable shipping cost strategy found for the given shipping info");
                
            return applicableStrategy.CalculateShippingCost(shippingInfo);
        }
    }

    // 使用例
    public class ShippingDemo
    {
        public static void Run()
        {
            // 住所の作成
            var usAddress1 = new Address("123 Main St", "New York", "NY", "USA", "10001");
            var usAddress2 = new Address("456 Elm St", "Los Angeles", "CA", "USA", "90001");
            var ukAddress = new Address("10 Downing St", "London", "England", "UK", "SW1A 2AA");
            
            // 戦略のセットアップ
            var domesticStrategy = new DomesticShippingCostStrategy();
            var internationalStrategy = new InternationalShippingCostStrategy();
            var holidayStrategy = new HolidayShippingCostStrategy(domesticStrategy);
            
            // サービスの作成
            var shippingService = new ShippingCostService(new List<IShippingCostStrategy> 
            {
                holidayStrategy,     // 優先度の高い戦略を先に配置
                internationalStrategy,
                domesticStrategy
            });
            
            // 国内配送の例
            var domesticShipping = new ShippingInfo(
                Money.USD(2.5m),     // 2.5kg
                usAddress1,          // ニューヨークから
                usAddress2,          // ロサンゼルスへ
                false,               // 速達ではない
                true,                // 保険あり
                DateTime.Now         // 今日配送
            );
            
            // 国際配送の例
            var internationalShipping = new ShippingInfo(
                Money.USD(1.5m),     // 1.5kg
                usAddress1,          // ニューヨークから
                ukAddress,           // ロンドンへ
                true,                // 速達
                true,                // 保険あり
                DateTime.Now         // 今日配送
            );
            
            // 休日配送の例
            var holidayShipping = new ShippingInfo(
                Money.USD(2.0m),     // 2.0kg
                usAddress1,          // ニューヨークから
                usAddress2,          // ロサンゼルスへ
                false,               // 速達ではない
                false,               // 保険なし
                new DateTime(DateTime.Now.Year, 12, 25) // クリスマスに配送
            );
            
            // 料金計算
            var domesticCost = shippingService.CalculateShippingCost(domesticShipping);
            var internationalCost = shippingService.CalculateShippingCost(internationalShipping);
            var holidayCost = shippingService.CalculateShippingCost(holidayShipping);
            
            // 結果の表示
            Console.WriteLine($"Domestic Shipping Cost: {domesticCost}");
            Console.WriteLine($"International Shipping Cost: {internationalCost}");
            Console.WriteLine($"Holiday Shipping Cost: {holidayCost}");
        }
    }

    #endregion

    #region LINQ拡張 - 条件に応じた戦略の適用

    // LINQ拡張メソッドを使って戦略パターンを適用する例

    public static class Extensions
    {
        public static IEnumerable<T> Sort<T>(this IEnumerable<T> source, ISortStrategy strategy) where T : IComparable<T>
        {
            var list = source.ToList();
            strategy.Sort(list);
            return list;
        }
    }

    // フィルタリング戦略
    public interface IFilterStrategy<T>
    {
        bool ShouldInclude(T item);
    }

    // 価格フィルター
    public class PriceFilterStrategy : IFilterStrategy<Product>
    {
        private readonly decimal _minPrice;
        private readonly decimal _maxPrice;

        public PriceFilterStrategy(decimal minPrice, decimal maxPrice)
        {
            _minPrice = minPrice;
            _maxPrice = maxPrice;
        }

        public bool ShouldInclude(Product item)
        {
            return item.Price >= _minPrice && item.Price <= _maxPrice;
        }
    }

    // カテゴリフィルター
    public class CategoryFilterStrategy : IFilterStrategy<Product>
    {
        private readonly string _category;

        public CategoryFilterStrategy(string category)
        {
            _category = category;
        }

        public bool ShouldInclude(Product item)
        {
            return item.Category == _category;
        }
    }

    // 在庫フィルター
    public class StockFilterStrategy : IFilterStrategy<Product>
    {
        private readonly int _minStock;

        public StockFilterStrategy(int minStock)
        {
            _minStock = minStock;
        }

        public bool ShouldInclude(Product item)
        {
            return item.Stock >= _minStock;
        }
    }

    // コンポジットフィルター（複数のフィルターを組み合わせる）
    public class CompositeFilterStrategy<T> : IFilterStrategy<T>
    {
        private readonly List<IFilterStrategy<T>> _strategies;
        private readonly bool _isAnd; // trueならAND条件、falseならOR条件

        public CompositeFilterStrategy(bool isAnd = true)
        {
            _strategies = new List<IFilterStrategy<T>>();
            _isAnd = isAnd;
        }

        public void AddStrategy(IFilterStrategy<T> strategy)
        {
            _strategies.Add(strategy);
        }

        public bool ShouldInclude(T item)
        {
            if (!_strategies.Any())
                return true;

            if (_isAnd)
            {
                // AND条件: すべての戦略がtrueを返す必要がある
                return _strategies.All(s => s.ShouldInclude(item));
            }
            else
            {
                // OR条件: いずれかの戦略がtrueを返せばよい
                return _strategies.Any(s => s.ShouldInclude(item));
            }
        }
    }

    // 製品クラス
    public class Product
    {
        public string Name { get; }
        public decimal Price { get; }
        public string Category { get; }
        public int Stock { get; }

        public Product(string name, decimal price, string category, int stock)
        {
            Name = name;
            Price = price;
            Category = category;
            Stock = stock;
        }

        public override string ToString()
        {
            return $"{Name} - {Price:C} ({Category}) [Stock: {Stock}]";
        }
    }

    // LINQ拡張メソッド
    public static class FilterExtensions
    {
        public static IEnumerable<T> ApplyFilter<T>(this IEnumerable<T> source, IFilterStrategy<T> filterStrategy)
        {
            return source.Where(item => filterStrategy.ShouldInclude(item));
        }
    }

    // 使用例
    public class FilterDemo
    {
        public static void Run()
        {
            // 製品リスト
            var products = new List<Product>
            {
                new Product("Laptop", 1200.00m, "Electronics", 10),
                new Product("Smartphone", 800.00m, "Electronics", 15),
                new Product("Headphones", 150.00m, "Electronics", 5),
                new Product("T-shirt", 25.00m, "Clothing", 50),
                new Product("Jeans", 65.00m, "Clothing", 20),
                new Product("Book", 15.00m, "Books", 30),
                new Product("Tablet", 350.00m, "Electronics", 8),
                new Product("Dress", 80.00m, "Clothing", 12),
                new Product("Desk", 250.00m, "Furniture", 3)
            };

            // 個別のフィルター戦略
            var electronicFilter = new CategoryFilterStrategy("Electronics");
            var priceFilter = new PriceFilterStrategy(100, 1000);
            var stockFilter = new StockFilterStrategy(10);

            // 複合フィルター（AND条件）
            var andFilter = new CompositeFilterStrategy<Product>();
            andFilter.AddStrategy(electronicFilter);
            andFilter.AddStrategy(priceFilter);
            andFilter.AddStrategy(stockFilter);

            // 複合フィルター（OR条件）
            var orFilter = new CompositeFilterStrategy<Product>(false);
            orFilter.AddStrategy(new CategoryFilterStrategy("Books"));
            orFilter.AddStrategy(new PriceFilterStrategy(0, 50));

            // フィルターの適用
            Console.WriteLine("All Products:");
            foreach (var product in products)
            {
                Console.WriteLine(product);
            }

            Console.WriteLine("\nElectronics Products $100-$1000 with stock >= 10:");
            var filteredProducts = products.ApplyFilter(andFilter);
            foreach (var product in filteredProducts)
            {
                Console.WriteLine(product);
            }

            Console.WriteLine("\nBooks OR Products under $50:");
            var orFilteredProducts = products.ApplyFilter(orFilter);
            foreach (var product in orFilteredProducts)
            {
                Console.WriteLine(product);
            }
        }
    }

    #endregion
}