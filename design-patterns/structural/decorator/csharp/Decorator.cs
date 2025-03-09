// structural/decorator/csharp/Decorator.cs

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesignPatternsCookbook.Structural.Decorator
{
    #region 基本的なデコレーターパターンの実装

    // 基本コンポーネントインターフェース
    public interface IComponent
    {
        string Operation();
    }

    // 具象コンポーネント
    public class ConcreteComponent : IComponent
    {
        public string Operation()
        {
            return "ConcreteComponent";
        }
    }

    // 基本デコレーター
    public abstract class Decorator : IComponent
    {
        protected IComponent _component;

        public Decorator(IComponent component)
        {
            _component = component;
        }

        // デフォルトでは、すべての操作をデコレートされたコンポーネントに委譲
        public virtual string Operation()
        {
            return _component.Operation();
        }
    }

    // 具象デコレーターA
    public class ConcreteDecoratorA : Decorator
    {
        public ConcreteDecoratorA(IComponent component) : base(component)
        {
        }

                public override string Operation()
        {
            return $"ConcreteDecoratorA({base.Operation()})";
        }
    }

    // 具象デコレーターB
    public class ConcreteDecoratorB : Decorator
    {
        public ConcreteDecoratorB(IComponent component) : base(component)
        {
        }

        public override string Operation()
        {
            return $"ConcreteDecoratorB({base.Operation()})";
        }

        // デコレーターBは基本機能に加えて追加の機能を持つ
        public string AddedBehavior()
        {
            return "AddedBehavior";
        }
    }

    #endregion

    #region 実際のデータストリーム処理の例

    // 基本データソースインターフェース
    public interface IDataSource
    {
        void WriteData(string data);
        string ReadData();
    }

    // 具象コンポーネント: ファイルデータソース
    public class FileDataSource : IDataSource
    {
        private string _filename;

        public FileDataSource(string filename)
        {
            _filename = filename;
        }

        public void WriteData(string data)
        {
            Console.WriteLine($"Writing data to file {_filename}: {data}");
            // 実際の実装では、ファイルに書き込む
            // File.WriteAllText(_filename, data);
        }

        public string ReadData()
        {
            Console.WriteLine($"Reading data from file {_filename}");
            // 実際の実装では、ファイルから読み込む
            // return File.ReadAllText(_filename);
            return $"Data from {_filename}";
        }
    }

    // 基本デコレーター
    public abstract class DataSourceDecorator : IDataSource
    {
        protected IDataSource _wrappee;

        public DataSourceDecorator(IDataSource source)
        {
            _wrappee = source;
        }

        // デフォルトでは、すべての操作をラップされたオブジェクトに委譲
        public virtual void WriteData(string data)
        {
            _wrappee.WriteData(data);
        }

        public virtual string ReadData()
        {
            return _wrappee.ReadData();
        }
    }

    // データを暗号化するデコレーター
    public class EncryptionDecorator : DataSourceDecorator
    {
        public EncryptionDecorator(IDataSource source) : base(source)
        {
        }

        public override void WriteData(string data)
        {
            // データを暗号化して書き込む
            string encryptedData = Encrypt(data);
            Console.WriteLine($"Encrypted data: {encryptedData}");
            base.WriteData(encryptedData);
        }

        public override string ReadData()
        {
            // 暗号化されたデータを読み込んで復号する
            string encryptedData = base.ReadData();
            string decryptedData = Decrypt(encryptedData);
            Console.WriteLine($"Decrypted data: {decryptedData}");
            return decryptedData;
        }

        private string Encrypt(string data)
        {
            // 簡略化された暗号化ロジック（実際の実装では強力な暗号化を使用）
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = (byte)(bytes[i] + 1); // 単純なシフト暗号
            }
            return Convert.ToBase64String(bytes);
        }

        private string Decrypt(string data)
        {
            // 簡略化された復号ロジック
            try
            {
                byte[] bytes = Convert.FromBase64String(data);
                for (int i = 0; i < bytes.Length; i++)
                {
                    bytes[i] = (byte)(bytes[i] - 1); // 単純なシフト暗号の復号
                }
                return Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                // デモ用に例外を飲み込む（実際のコードではより適切なエラー処理が必要）
                return "Error decrypting data: " + data;
            }
        }
    }

    // データを圧縮するデコレーター
    public class CompressionDecorator : DataSourceDecorator
    {
        public CompressionDecorator(IDataSource source) : base(source)
        {
        }

        public override void WriteData(string data)
        {
            // データを圧縮して書き込む
            string compressedData = Compress(data);
            Console.WriteLine($"Compressed data: {compressedData}");
            base.WriteData(compressedData);
        }

        public override string ReadData()
        {
            // 圧縮されたデータを読み込んで展開する
            string compressedData = base.ReadData();
            string decompressedData = Decompress(compressedData);
            Console.WriteLine($"Decompressed data: {decompressedData}");
            return decompressedData;
        }

        private string Compress(string data)
        {
            // 簡略化された圧縮ロジック（実際の実装では本物の圧縮アルゴリズムを使用）
            // このデモでは、単純な Run-Length Encoding を使用
            if (string.IsNullOrEmpty(data))
                return data;

            StringBuilder sb = new StringBuilder();
            char currentChar = data[0];
            int count = 1;

            for (int i = 1; i < data.Length; i++)
            {
                if (data[i] == currentChar)
                {
                    count++;
                }
                else
                {
                    sb.Append(count);
                    sb.Append(currentChar);
                    currentChar = data[i];
                    count = 1;
                }
            }

            sb.Append(count);
            sb.Append(currentChar);
            return sb.ToString();
        }

        private string Decompress(string data)
        {
            // 簡略化された展開ロジック
            try
            {
                if (string.IsNullOrEmpty(data))
                    return data;

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < data.Length; i += 2)
                {
                    if (i + 1 >= data.Length)
                        break;

                    if (char.IsDigit(data[i]))
                    {
                        int count = int.Parse(data[i].ToString());
                        char c = data[i + 1];
                        for (int j = 0; j < count; j++)
                        {
                            sb.Append(c);
                        }
                    }
                    else
                    {
                        // 圧縮形式でない場合は、そのまま返す
                        return data;
                    }
                }
                return sb.ToString();
            }
            catch
            {
                // デモ用に例外を飲み込む
                return "Error decompressing data: " + data;
            }
        }
    }

    // ロギングデコレーター
    public class LoggingDecorator : DataSourceDecorator
    {
        public LoggingDecorator(IDataSource source) : base(source)
        {
        }

        public override void WriteData(string data)
        {
            Console.WriteLine($"[LOG] Writing data: Length={data.Length}");
            base.WriteData(data);
        }

        public override string ReadData()
        {
            Console.WriteLine("[LOG] Reading data");
            string result = base.ReadData();
            Console.WriteLine($"[LOG] Read completed: Length={result.Length}");
            return result;
        }
    }

    #endregion

    #region ウェブAPI処理の例

    // 基本HTTPリクエストハンドラーインターフェース
    public interface IHttpHandler
    {
        string HandleRequest(HttpRequest request);
    }

    // HTTPリクエスト
    public class HttpRequest
    {
        public string Method { get; set; }
        public string Path { get; set; }
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
        public string Body { get; set; }
    }

    // 具象コンポーネント: 基本ハンドラー
    public class BaseHttpHandler : IHttpHandler
    {
        public string HandleRequest(HttpRequest request)
        {
            // 実際の実装では、リクエストを処理してレスポンスを返す
            return $"Response to {request.Method} {request.Path}";
        }
    }

    // 基本デコレーター
    public abstract class HttpHandlerDecorator : IHttpHandler
    {
        protected IHttpHandler _handler;

        public HttpHandlerDecorator(IHttpHandler handler)
        {
            _handler = handler;
        }

        public virtual string HandleRequest(HttpRequest request)
        {
            return _handler.HandleRequest(request);
        }
    }

    // 認証デコレーター
    public class AuthenticationDecorator : HttpHandlerDecorator
    {
        public AuthenticationDecorator(IHttpHandler handler) : base(handler)
        {
        }

        public override string HandleRequest(HttpRequest request)
        {
            // リクエストから認証トークンを確認
            if (!request.Headers.ContainsKey("Authorization"))
            {
                return "Error: Unauthorized - Missing Authorization header";
            }

            string authToken = request.Headers["Authorization"];
            if (!ValidateToken(authToken))
            {
                return "Error: Unauthorized - Invalid token";
            }

            Console.WriteLine("[AUTH] Authentication successful");
            return base.HandleRequest(request);
        }

        private bool ValidateToken(string token)
        {
            // 実際の実装では、トークンを検証するロジック
            // このデモでは、単純な検証
            return token.StartsWith("Bearer ") && token.Length > 10;
        }
    }

    // コンテンツタイプチェックデコレーター
    public class ContentTypeDecorator : HttpHandlerDecorator
    {
        private readonly string[] _supportedTypes;

        public ContentTypeDecorator(IHttpHandler handler, string[] supportedTypes) : base(handler)
        {
            _supportedTypes = supportedTypes;
        }

        public override string HandleRequest(HttpRequest request)
        {
            // POST/PUTリクエストの場合のみContentTypeをチェック
            if (request.Method == "POST" || request.Method == "PUT")
            {
                if (!request.Headers.ContainsKey("Content-Type"))
                {
                    return "Error: Bad Request - Missing Content-Type header";
                }

                string contentType = request.Headers["Content-Type"];
                if (!_supportedTypes.Contains(contentType))
                {
                    return $"Error: Unsupported Media Type - {contentType} not supported";
                }

                Console.WriteLine($"[CONTENT] Content-Type check passed: {contentType}");
            }

            return base.HandleRequest(request);
        }
    }

    // レスポンスキャッシュデコレーター
    public class CachingDecorator : HttpHandlerDecorator
    {
        private readonly Dictionary<string, CacheEntry> _cache = new Dictionary<string, CacheEntry>();
        private readonly TimeSpan _cacheDuration;

        private class CacheEntry
        {
            public string Response { get; set; }
            public DateTime Expiration { get; set; }
        }

        public CachingDecorator(IHttpHandler handler, TimeSpan cacheDuration) : base(handler)
        {
            _cacheDuration = cacheDuration;
        }

        public override string HandleRequest(HttpRequest request)
        {
            // GETリクエストのみキャッシュ
            if (request.Method != "GET")
            {
                return base.HandleRequest(request);
            }

            string cacheKey = $"{request.Method}:{request.Path}";
            
            // キャッシュにあるか確認
            if (_cache.TryGetValue(cacheKey, out var cacheEntry))
            {
                if (cacheEntry.Expiration > DateTime.Now)
                {
                    Console.WriteLine($"[CACHE] Cache hit for {cacheKey}");
                    return cacheEntry.Response;
                }
                else
                {
                    Console.WriteLine($"[CACHE] Cache expired for {cacheKey}");
                    _cache.Remove(cacheKey);
                }
            }

            // キャッシュにない場合は処理して保存
            string response = base.HandleRequest(request);
            
            _cache[cacheKey] = new CacheEntry
            {
                Response = response,
                Expiration = DateTime.Now.Add(_cacheDuration)
            };
            
            Console.WriteLine($"[CACHE] Cached response for {cacheKey}");
            return response;
        }
    }

    // リクエスト/レスポンスのロギングデコレーター
    public class HttpLoggingDecorator : HttpHandlerDecorator
    {
        public HttpLoggingDecorator(IHttpHandler handler) : base(handler)
        {
        }

        public override string HandleRequest(HttpRequest request)
        {
            // リクエストのログ記録
            Console.WriteLine($"[LOG] Request: {request.Method} {request.Path}");
            foreach (var header in request.Headers)
            {
                Console.WriteLine($"[LOG] Header: {header.Key}={header.Value}");
            }
            
            if (!string.IsNullOrEmpty(request.Body))
            {
                Console.WriteLine($"[LOG] Body: {request.Body}");
            }

            // 処理を委譲
            string response = base.HandleRequest(request);

            // レスポンスのログ記録
            Console.WriteLine($"[LOG] Response: {response}");

            return response;
        }
    }

    #endregion

    #region ビジネスロジックの例（ドメイン駆動設計）

    // 基本的な注文処理インターフェース
    public interface IOrderProcessor
    {
        OrderResult ProcessOrder(Order order);
    }

    // 注文エンティティ
    public class Order
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
        public decimal TotalAmount { get; set; }
        public string ShippingAddress { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
    }

    // 注文項目
    public class OrderItem
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    // 注文処理結果
    public class OrderResult
    {
        public bool Success { get; set; }
        public string OrderId { get; set; }
        public string Message { get; set; }
        public decimal FinalAmount { get; set; }
    }

    // 基本注文処理（具象コンポーネント）
    public class BaseOrderProcessor : IOrderProcessor
    {
        public OrderResult ProcessOrder(Order order)
        {
            // 基本的な注文処理ロジック
            Console.WriteLine($"Processing order: {order.Id}");
            Console.WriteLine($"Customer: {order.CustomerName}");
            Console.WriteLine($"Items: {order.Items.Count}");
            Console.WriteLine($"Total Amount: ${order.TotalAmount}");

            // 実際の実装では、データベースに注文を保存するなど
            return new OrderResult
            {
                Success = true,
                OrderId = order.Id.ToString(),
                Message = "Order processed successfully",
                FinalAmount = order.TotalAmount
            };
        }
    }

    // 基本デコレーター
    public abstract class OrderProcessorDecorator : IOrderProcessor
    {
        protected IOrderProcessor _processor;

        public OrderProcessorDecorator(IOrderProcessor processor)
        {
            _processor = processor;
        }

        public virtual OrderResult ProcessOrder(Order order)
        {
            return _processor.ProcessOrder(order);
        }
    }

    // 在庫チェックデコレーター
    public class InventoryCheckDecorator : OrderProcessorDecorator
    {
        // 単純化のため、メモリ内の在庫データを使用
        private readonly Dictionary<Guid, int> _inventoryLevels = new Dictionary<Guid, int>
        {
            { Guid.Parse("00000000-0000-0000-0000-000000000001"), 10 },
            { Guid.Parse("00000000-0000-0000-0000-000000000002"), 5 },
            { Guid.Parse("00000000-0000-0000-0000-000000000003"), 0 }
        };

        public InventoryCheckDecorator(IOrderProcessor processor) : base(processor)
        {
        }

        public override OrderResult ProcessOrder(Order order)
        {
            Console.WriteLine("[INVENTORY] Checking inventory levels");

            // 在庫レベルをチェック
            foreach (var item in order.Items)
            {
                if (!_inventoryLevels.TryGetValue(item.ProductId, out int stock))
                {
                    return new OrderResult
                    {
                        Success = false,
                        Message = $"Product {item.ProductName} (ID: {item.ProductId}) not found in inventory"
                    };
                }

                if (stock < item.Quantity)
                {
                    return new OrderResult
                    {
                        Success = false,
                        Message = $"Insufficient stock for {item.ProductName} (Available: {stock}, Requested: {item.Quantity})"
                    };
                }

                Console.WriteLine($"[INVENTORY] {item.ProductName}: {stock} available, {item.Quantity} requested");
            }

            Console.WriteLine("[INVENTORY] All items in stock");
            return base.ProcessOrder(order);
        }
    }

    // 割引適用デコレーター
    public class DiscountDecorator : OrderProcessorDecorator
    {
        private readonly decimal _discountPercentage;
        private readonly decimal _minimumOrderAmount;

        public DiscountDecorator(IOrderProcessor processor, decimal discountPercentage, decimal minimumOrderAmount) : base(processor)
        {
            _discountPercentage = discountPercentage;
            _minimumOrderAmount = minimumOrderAmount;
        }

        public override OrderResult ProcessOrder(Order order)
        {
            // 最低注文金額を満たしていない場合は割引なし
            if (order.TotalAmount < _minimumOrderAmount)
            {
                Console.WriteLine($"[DISCOUNT] No discount applied (Order amount ${order.TotalAmount} below minimum ${_minimumOrderAmount})");
                return base.ProcessOrder(order);
            }

            // 割引を計算
            decimal discountAmount = order.TotalAmount * (_discountPercentage / 100);
            decimal discountedTotal = order.TotalAmount - discountAmount;

            Console.WriteLine($"[DISCOUNT] Applied {_discountPercentage}% discount");
            Console.WriteLine($"[DISCOUNT] Original amount: ${order.TotalAmount}");
            Console.WriteLine($"[DISCOUNT] Discount amount: ${discountAmount}");
            Console.WriteLine($"[DISCOUNT] New total: ${discountedTotal}");

            // 割引後の金額で注文を更新
            order.TotalAmount = discountedTotal;

            // 基本処理に委譲
            OrderResult result = base.ProcessOrder(order);

            // 割引情報をメッセージに追加
            result.Message += $" (with {_discountPercentage}% discount)";
            result.FinalAmount = discountedTotal;

            return result;
        }
    }

    // 通知デコレーター
    public class NotificationDecorator : OrderProcessorDecorator
    {
        public NotificationDecorator(IOrderProcessor processor) : base(processor)
        {
        }

        public override OrderResult ProcessOrder(Order order)
        {
            // 基本処理に委譲
            OrderResult result = base.ProcessOrder(order);

            if (result.Success)
            {
                // 注文が成功した場合、通知を送信
                SendEmailNotification(order, result);
                SendSmsNotification(order, result);
            }

            return result;
        }

        private void SendEmailNotification(Order order, OrderResult result)
        {
            Console.WriteLine("[NOTIFICATION] Sending order confirmation email");
            Console.WriteLine($"[NOTIFICATION] To: {order.CustomerEmail}");
            Console.WriteLine($"[NOTIFICATION] Subject: Order Confirmation - {result.OrderId}");
            Console.WriteLine($"[NOTIFICATION] Body: Thank you for your order! Your order ID is {result.OrderId}.");
        }

        private void SendSmsNotification(Order order, OrderResult result)
        {
            Console.WriteLine("[NOTIFICATION] Sending order confirmation SMS");
            Console.WriteLine($"[NOTIFICATION] Message: Your order {result.OrderId} has been confirmed. Total: ${result.FinalAmount}");
        }
    }

    // 税金計算デコレーター
    public class TaxCalculationDecorator : OrderProcessorDecorator
    {
        private readonly decimal _taxRate;

        public TaxCalculationDecorator(IOrderProcessor processor, decimal taxRate) : base(processor)
        {
            _taxRate = taxRate;
        }

        public override OrderResult ProcessOrder(Order order)
        {
            // 税金を計算
            decimal taxAmount = order.TotalAmount * (_taxRate / 100);
            decimal totalWithTax = order.TotalAmount + taxAmount;

            Console.WriteLine($"[TAX] Calculating tax at {_taxRate}%");
            Console.WriteLine($"[TAX] Original amount: ${order.TotalAmount}");
            Console.WriteLine($"[TAX] Tax amount: ${taxAmount}");
            Console.WriteLine($"[TAX] Total with tax: ${totalWithTax}");

            // 税込み金額で注文を更新
            order.TotalAmount = totalWithTax;

            // 基本処理に委譲
            OrderResult result = base.ProcessOrder(order);

            // 税金情報をメッセージに追加
            result.Message += $" (including {_taxRate}% tax)";
            result.FinalAmount = totalWithTax;

            return result;
        }
    }

    #endregion

    #region 使用例

    // デコレーターパターンの使用例
    public class DecoratorDemo
    {
        public static void RunBasicDemo()
        {
            Console.WriteLine("--- 基本的なデコレーターパターンのデモ ---");
            
            // 基本コンポーネントを作成
            IComponent component = new ConcreteComponent();
            Console.WriteLine("1. 基本コンポーネント:");
            Console.WriteLine(component.Operation());
            
            // デコレーターAで包む
            IComponent decoratorA = new ConcreteDecoratorA(component);
            Console.WriteLine("\n2. デコレーターAで包んだコンポーネント:");
            Console.WriteLine(decoratorA.Operation());
            
            // デコレーターBでさらに包む
            IComponent decoratorB = new ConcreteDecoratorB(decoratorA);
            Console.WriteLine("\n3. デコレーターBでさらに包んだコンポーネント:");
            Console.WriteLine(decoratorB.Operation());
            
            // 直接デコレーターBで包むことも可能
            IComponent decoratorB2 = new ConcreteDecoratorB(component);
            Console.WriteLine("\n4. デコレーターBで直接包んだコンポーネント:");
            Console.WriteLine(decoratorB2.Operation());
        }

        public static void RunDataSourceDemo()
        {
            Console.WriteLine("\n--- データソースデコレーターのデモ ---");
            
            // 基本コンポーネントを作成
            IDataSource source = new FileDataSource("data.txt");
            
            // 暗号化と圧縮デコレーターでラップ
            IDataSource encrypted = new EncryptionDecorator(source);
            IDataSource encryptedAndCompressed = new CompressionDecorator(encrypted);
            
            // さらにロギングデコレーターでラップ
            IDataSource withLogging = new LoggingDecorator(encryptedAndCompressed);
            
            // テストデータ
            string testData = "This is a test message that will be stored in the file.";
            
            Console.WriteLine("\n1. 元のデータ:");
            Console.WriteLine(testData);
            
            Console.WriteLine("\n2. データの書き込み（デコレーターがすべて適用される）:");
            withLogging.WriteData(testData);
            
            Console.WriteLine("\n3. データの読み込み（デコレーターが逆順に適用される）:");
            string retrievedData = withLogging.ReadData();
            
            Console.WriteLine("\n4. 復元されたデータ:");
            Console.WriteLine(retrievedData);
            
            // 異なるデコレーターの組み合わせも可能
            Console.WriteLine("\n5. 別の組み合わせでテスト（ロギング + 暗号化）:");
            IDataSource loggedAndEncrypted = new LoggingDecorator(new EncryptionDecorator(source));
            loggedAndEncrypted.WriteData("Another test message with different decorators.");
            string anotherData = loggedAndEncrypted.ReadData();
            Console.WriteLine($"Retrieved data: {anotherData}");
        }

        public static void RunHttpHandlerDemo()
        {
            Console.WriteLine("\n--- HTTPハンドラーデコレーターのデモ ---");
            
            // 基本的なHTTPハンドラー
            IHttpHandler baseHandler = new BaseHttpHandler();
            
            // 様々なデコレーターでラップ
            IHttpHandler handler = new HttpLoggingDecorator(
                new CachingDecorator(
                    new AuthenticationDecorator(
                        new ContentTypeDecorator(
                            baseHandler, 
                            new[] { "application/json", "application/xml" }
                        )
                    ),
                    TimeSpan.FromMinutes(10)
                )
            );
            
            // テストリクエスト（有効な認証と内容タイプ）
            HttpRequest validRequest = new HttpRequest
            {
                Method = "POST",
                Path = "/api/users",
                Headers = new Dictionary<string, string>
                {
                    { "Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9" },
                    { "Content-Type", "application/json" }
                },
                Body = "{\"name\":\"John Doe\",\"email\":\"john@example.com\"}"
            };
            
            Console.WriteLine("\n1. 有効なリクエストのテスト:");
            string response1 = handler.HandleRequest(validRequest);
            Console.WriteLine($"最終レスポンス: {response1}");
            
            // 認証なしのリクエスト
            HttpRequest unauthorizedRequest = new HttpRequest
            {
                Method = "GET",
                Path = "/api/users/1",
                Headers = new Dictionary<string, string>()
            };
            
            Console.WriteLine("\n2. 認証なしリクエストのテスト:");
            string response2 = handler.HandleRequest(unauthorizedRequest);
            Console.WriteLine($"最終レスポンス: {response2}");
            
            // サポートされていないContent-Typeのリクエスト
            HttpRequest invalidContentRequest = new HttpRequest
            {
                Method = "POST",
                Path = "/api/users",
                Headers = new Dictionary<string, string>
                {
                    { "Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9" },
                    { "Content-Type", "text/plain" }
                },
                Body = "Plain text data"
            };
            
            Console.WriteLine("\n3. 無効なContent-Typeリクエストのテスト:");
            string response3 = handler.HandleRequest(invalidContentRequest);
            Console.WriteLine($"最終レスポンス: {response3}");
            
            // キャッシュのテスト（同じGETリクエストを2回実行）
            HttpRequest cacheableRequest = new HttpRequest
            {
                Method = "GET",
                Path = "/api/products",
                Headers = new Dictionary<string, string>
                {
                    { "Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9" }
                }
            };
            
            Console.WriteLine("\n4. キャッシュのテスト (最初のリクエスト):");
            string response4 = handler.HandleRequest(cacheableRequest);
            Console.WriteLine($"最終レスポンス: {response4}");
            
            Console.WriteLine("\n5. キャッシュのテスト (2回目のリクエスト - キャッシュから取得されるはず):");
            string response5 = handler.HandleRequest(cacheableRequest);
            Console.WriteLine($"最終レスポンス: {response5}");
        }

        public static void RunOrderProcessingDemo()
        {
            Console.WriteLine("\n--- 注文処理デコレーターのデモ ---");
            
            // 基本注文処理
            IOrderProcessor baseProcessor = new BaseOrderProcessor();
            
            // デコレーターをスタック
            IOrderProcessor processor = new NotificationDecorator(
                new TaxCalculationDecorator(
                    new DiscountDecorator(
                        new InventoryCheckDecorator(
                            baseProcessor
                        ),
                        10.0m,  // 10%割引
                        100.0m  // 最低注文金額$100
                    ),
                    8.0m  // 8%の税率
                )
            );
            
            // 有効な注文を作成
            Order validOrder = new Order
            {
                CustomerName = "Alice Johnson",
                CustomerEmail = "alice@example.com",
                ShippingAddress = "123 Main St, Anytown, AN 12345",
                Items = new List<OrderItem>
                {
                    new OrderItem
                    {
                        ProductId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                        ProductName = "Laptop",
                        Quantity = 1,
                        UnitPrice = 999.99m
                    },
                    new OrderItem
                    {
                        ProductId = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                        ProductName = "Mouse",
                        Quantity = 2,
                        UnitPrice = 24.99m
                    }
                },
                TotalAmount = 1049.97m  // 999.99 + (2 * 24.99)
            };
            
            Console.WriteLine("\n1. 有効な注文処理のテスト:");
            OrderResult result1 = processor.ProcessOrder(validOrder);
            Console.WriteLine($"処理結果: {(result1.Success ? "成功" : "失敗")}");
            Console.WriteLine($"メッセージ: {result1.Message}");
            Console.WriteLine($"最終金額: ${result1.FinalAmount}");
            
            // 在庫不足の注文
            Order insufficientStockOrder = new Order
            {
                CustomerName = "Bob Smith",
                CustomerEmail = "bob@example.com",
                ShippingAddress = "456 Oak St, Othertown, OT 67890",
                Items = new List<OrderItem>
                {
                    new OrderItem
                    {
                        ProductId = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                        ProductName = "Mouse",
                        Quantity = 10,  // 在庫は5しかない
                        UnitPrice = 24.99m
                    }
                },
                TotalAmount = 249.90m  // 10 * 24.99
            };
            
            Console.WriteLine("\n2. 在庫不足注文処理のテスト:");
            OrderResult result2 = processor.ProcessOrder(insufficientStockOrder);
            Console.WriteLine($"処理結果: {(result2.Success ? "成功" : "失敗")}");
            Console.WriteLine($"メッセージ: {result2.Message}");
            
            // 存在しない製品を含む注文
            Order nonExistentProductOrder = new Order
            {
                CustomerName = "Charlie Brown",
                CustomerEmail = "charlie@example.com",
                ShippingAddress = "789 Pine St, Sometown, ST 54321",
                Items = new List<OrderItem>
                {
                    new OrderItem
                    {
                        ProductId = Guid.Parse("00000000-0000-0000-0000-000000000004"),  // 存在しないID
                        ProductName = "Unknown Product",
                        Quantity = 1,
                        UnitPrice = 50.00m
                    }
                },
                TotalAmount = 50.00m
            };
            
            Console.WriteLine("\n3. 存在しない製品の注文処理テスト:");
            OrderResult result3 = processor.ProcessOrder(nonExistentProductOrder);
            Console.WriteLine($"処理結果: {(result3.Success ? "成功" : "失敗")}");
            Console.WriteLine($"メッセージ: {result3.Message}");
            
            // 割引適用されない小額注文
            Order smallOrder = new Order
            {
                CustomerName = "David Williams",
                CustomerEmail = "david@example.com",
                ShippingAddress = "321 Elm St, Lasttown, LT 13579",
                Items = new List<OrderItem>
                {
                    new OrderItem
                    {
                        ProductId = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                        ProductName = "Mouse",
                        Quantity = 1,
                        UnitPrice = 24.99m
                    }
                },
                TotalAmount = 24.99m
            };
            
            Console.WriteLine("\n4. 少額注文処理のテスト (割引適用されない):");
            OrderResult result4 = processor.ProcessOrder(smallOrder);
            Console.WriteLine($"処理結果: {(result4.Success ? "成功" : "失敗")}");
            Console.WriteLine($"メッセージ: {result4.Message}");
            Console.WriteLine($"最終金額: ${result4.FinalAmount}");
        }

        public static void RunAllDemos()
        {
            RunBasicDemo();
            RunDataSourceDemo();
            RunHttpHandlerDemo();
            RunOrderProcessingDemo();
        }
    }

    #endregion
}