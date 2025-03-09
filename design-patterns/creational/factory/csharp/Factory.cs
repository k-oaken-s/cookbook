using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DesignPatternsCookbook.Creational.Factory
{
    #region 単純なファクトリ (Simple Factory)

    // 製品インターフェース
    public interface IProduct
    {
        string GetName();
        decimal GetPrice();
    }

    // 具体的な製品クラス
    public class ConcreteProductA : IProduct
    {
        public string GetName() => "Product A";
        public decimal GetPrice() => 10.0m;
    }

    public class ConcreteProductB : IProduct
    {
        public string GetName() => "Product B";
        public decimal GetPrice() => 20.0m;
    }

    // 単純なファクトリクラス
    public class SimpleProductFactory
    {
        public IProduct CreateProduct(string type)
        {
            switch (type.ToLower())
            {
                case "a":
                    return new ConcreteProductA();
                case "b":
                    return new ConcreteProductB();
                default:
                    throw new ArgumentException($"Unknown product type: {type}");
            }
        }
    }

    // 使用例
    public class SimpleFactoryClient
    {
        private readonly SimpleProductFactory _factory;

        public SimpleFactoryClient(SimpleProductFactory factory)
        {
            _factory = factory;
        }

        public void OrderProduct(string type, int quantity)
        {
            var product = _factory.CreateProduct(type);
            var totalPrice = product.GetPrice() * quantity;

            Console.WriteLine($"Ordered {quantity} of {product.GetName()}");
            Console.WriteLine($"Total price: ${totalPrice}");
        }
    }

    #endregion

    #region ファクトリメソッド (Factory Method)

    // 製品インターフェース
    public interface IDocument
    {
        void Open();
        void Save();
        void Close();
    }

    // 具体的な製品クラス
    public class PdfDocument : IDocument
    {
        public void Open() => Console.WriteLine("Opening PDF document...");
        public void Save() => Console.WriteLine("Saving PDF document...");
        public void Close() => Console.WriteLine("Closing PDF document...");
    }

    public class WordDocument : IDocument
    {
        public void Open() => Console.WriteLine("Opening Word document...");
        public void Save() => Console.WriteLine("Saving Word document...");
        public void Close() => Console.WriteLine("Closing Word document...");
    }

    // 抽象クリエーターのインターフェース
    public interface IDocumentCreator
    {
        IDocument CreateDocument();  // ファクトリメソッド
        void EditDocument();         // 操作
    }

    // 具体的なクリエーター
    public class PdfDocumentCreator : IDocumentCreator
    {
        public IDocument CreateDocument()
        {
            return new PdfDocument();
        }

        public void EditDocument()
        {
            var document = CreateDocument();
            document.Open();
            // PDFドキュメント編集のロジック
            document.Save();
            document.Close();
        }
    }

    public class WordDocumentCreator : IDocumentCreator
    {
        public IDocument CreateDocument()
        {
            return new WordDocument();
        }

        public void EditDocument()
        {
            var document = CreateDocument();
            document.Open();
            // Wordドキュメント編集のロジック
            document.Save();
            document.Close();
        }
    }

    // 使用例
    public class DocumentManager
    {
        private readonly IDocumentCreator _documentCreator;

        public DocumentManager(IDocumentCreator documentCreator)
        {
            _documentCreator = documentCreator;
        }

        public void ManageDocument()
        {
            _documentCreator.EditDocument();
        }
    }

    #endregion

    #region 抽象ファクトリ (Abstract Factory)

    // 製品ファミリー1: UI Controls
    public interface IButton
    {
        void Click();
        void Render();
    }

    public interface ITextBox
    {
        void SetText(string text);
        string GetText();
        void Render();
    }

    // 製品ファミリー2: Windows用UI実装
    public class WindowsButton : IButton
    {
        public void Click() => Console.WriteLine("Windows button clicked");
        public void Render() => Console.WriteLine("Rendering a Windows-style button");
    }

    public class WindowsTextBox : ITextBox
    {
        private string _text = "";

        public void SetText(string text) => _text = text;
        public string GetText() => _text;
        public void Render() => Console.WriteLine($"Rendering a Windows-style textbox with text: {_text}");
    }

    // 製品ファミリー3: MacOS用UI実装
    public class MacOSButton : IButton
    {
        public void Click() => Console.WriteLine("MacOS button clicked");
        public void Render() => Console.WriteLine("Rendering a MacOS-style button");
    }

    public class MacOSTextBox : ITextBox
    {
        private string _text = "";

        public void SetText(string text) => _text = text;
        public string GetText() => _text;
        public void Render() => Console.WriteLine($"Rendering a MacOS-style textbox with text: {_text}");
    }

    // 抽象ファクトリ
    public interface IUIControlFactory
    {
        IButton CreateButton();
        ITextBox CreateTextBox();
    }

    // 具象ファクトリ 1
    public class WindowsUIControlFactory : IUIControlFactory
    {
        public IButton CreateButton() => new WindowsButton();
        public ITextBox CreateTextBox() => new WindowsTextBox();
    }

    // 具象ファクトリ 2
    public class MacOSUIControlFactory : IUIControlFactory
    {
        public IButton CreateButton() => new MacOSButton();
        public ITextBox CreateTextBox() => new MacOSTextBox();
    }

    // 使用例
    public class UIApplication
    {
        private readonly IButton _button;
        private readonly ITextBox _textBox;

        public UIApplication(IUIControlFactory factory)
        {
            _button = factory.CreateButton();
            _textBox = factory.CreateTextBox();
        }

        public void RenderUI()
        {
            _textBox.SetText("Hello, World!");
            _button.Render();
            _textBox.Render();
        }

        public void ButtonClick()
        {
            _button.Click();
        }
    }

    #endregion

    #region ドメイン駆動設計におけるファクトリの応用例

    // 値オブジェクト
    public class Money
    {
        public decimal Amount { get; }
        public string Currency { get; }

        public Money(decimal amount, string currency)
        {
            Amount = amount;
            Currency = currency;
        }

        // 値オブジェクトのためのファクトリメソッド
        public static Money FromUsd(decimal amount) => new Money(amount, "USD");
        public static Money FromEur(decimal amount) => new Money(amount, "EUR");
        public static Money FromJpy(decimal amount) => new Money(Math.Round(amount), "JPY");
    }

    // ドメインエンティティ
    public class Order
    {
        public Guid Id { get; private set; }
        public Guid CustomerId { get; private set; }
        public List<OrderItem> Items { get; private set; } = new List<OrderItem>();
        public Money TotalAmount { get; private set; }
        public OrderStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }

        // プライベートコンストラクタ - ファクトリメソッド経由でのみインスタンス化
        private Order() { }

        private Order(Guid customerId)
        {
            Id = Guid.NewGuid();
            CustomerId = customerId;
            Status = OrderStatus.Created;
            CreatedAt = DateTime.UtcNow;
            TotalAmount = Money.FromUsd(0);
        }

        // ファクトリメソッド
        public static Order CreateEmptyOrder(Guid customerId)
        {
            if (customerId == Guid.Empty)
                throw new ArgumentException("Customer ID cannot be empty");

            return new Order(customerId);
        }

        // 項目追加メソッド
        public void AddItem(Guid productId, string productName, int quantity, Money unitPrice)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be positive");

            var item = new OrderItem(Id, productId, productName, quantity, unitPrice);
            Items.Add(item);

            // 合計金額の再計算
            RecalculateTotalAmount();
        }

        private void RecalculateTotalAmount()
        {
            decimal total = 0;
            foreach (var item in Items)
            {
                total += item.Quantity * item.UnitPrice.Amount;
            }
            TotalAmount = Money.FromUsd(total);
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

        // インターナルコンストラクタ - Orderからのみ作成可能
        internal OrderItem(Guid orderId, Guid productId, string productName, int quantity, Money unitPrice)
        {
            Id = Guid.NewGuid();
            OrderId = orderId;
            ProductId = productId;
            ProductName = productName;
            Quantity = quantity;
            UnitPrice = unitPrice;
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

    // ドメインファクトリ
    public interface IOrderFactory
    {
        Task<Order> CreateOrderAsync(Guid customerId, List<OrderItemInfo> items);
    }

    public class OrderItemInfo
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class OrderFactory : IOrderFactory
    {
        private readonly IProductRepository _productRepository;
        private readonly ICustomerRepository _customerRepository;

        public OrderFactory(IProductRepository productRepository, ICustomerRepository customerRepository)
        {
            _productRepository = productRepository;
            _customerRepository = customerRepository;
        }

        public async Task<Order> CreateOrderAsync(Guid customerId, List<OrderItemInfo> items)
        {
            // 顧客の存在確認
            var customer = await _customerRepository.GetByIdAsync(customerId);
            if (customer == null)
                throw new ArgumentException($"Customer with ID {customerId} not found");

            // 注文オブジェクトの作成
            var order = Order.CreateEmptyOrder(customerId);

            // 製品情報の取得と注文項目の追加
            foreach (var itemInfo in items)
            {
                var product = await _productRepository.GetByIdAsync(itemInfo.ProductId);
                if (product == null)
                    throw new ArgumentException($"Product with ID {itemInfo.ProductId} not found");

                order.AddItem(
                    product.Id,
                    product.Name,
                    itemInfo.Quantity,
                    product.Price
                );
            }

            return order;
        }
    }

    // リポジトリインターフェース（実装は省略）
    public interface IProductRepository
    {
        Task<Product> GetByIdAsync(Guid id);
    }

    public interface ICustomerRepository
    {
        Task<Customer> GetByIdAsync(Guid id);
    }

    // ドメインオブジェクト（実装は省略）
    public class Product
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Money Price { get; set; }
    }

    public class Customer
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    // ユースケース
    public class CreateOrderUseCase
    {
        private readonly IOrderFactory _orderFactory;
        private readonly IOrderRepository _orderRepository;

        public CreateOrderUseCase(IOrderFactory orderFactory, IOrderRepository orderRepository)
        {
            _orderFactory = orderFactory;
            _orderRepository = orderRepository;
        }

        public async Task<Order> ExecuteAsync(Guid customerId, List<OrderItemInfo> items)
        {
            var order = await _orderFactory.CreateOrderAsync(customerId, items);
            await _orderRepository.AddAsync(order);
            return order;
        }
    }

    public interface IOrderRepository
    {
        Task AddAsync(Order order);
    }

    #endregion
}