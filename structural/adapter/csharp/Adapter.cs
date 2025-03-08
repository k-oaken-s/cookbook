using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DesignPatternsCookbook.Structural.Adapter
{
    #region 基本的なアダプターパターンの例

    // クライアントが期待するターゲットインターフェース
    public interface ITarget
    {
        string Request();
    }

    // 既存のクラス（適応されるクラス）
    public class Adaptee
    {
        // クライアントが直接使用できない既存のメソッド
        public string SpecificRequest()
        {
            return "Adapteeからの特殊なリクエスト";
        }
    }

    // アダプター（クラスアダプター：継承を使用）
    public class ClassAdapter : Adaptee, ITarget
    {
        public string Request()
        {
            // 既存のメソッドを呼び出して結果を変換
            return $"ClassAdapter: {SpecificRequest()}";
        }
    }

    // アダプター（オブジェクトアダプター：コンポジションを使用）
    public class ObjectAdapter : ITarget
    {
        private readonly Adaptee _adaptee;

        public ObjectAdapter(Adaptee adaptee)
        {
            _adaptee = adaptee;
        }

        public string Request()
        {
            // 既存のメソッドを呼び出して結果を変換
            return $"ObjectAdapter: {_adaptee.SpecificRequest()}";
        }
    }

    #endregion

    #region 実際のユースケース：レガシーAPIの適応

    // 最新のペイメント処理インターフェース（クライアントが使用したいインターフェース）
    public interface IModernPaymentProcessor
    {
        Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request);
        Task<bool> ValidatePaymentAsync(PaymentRequest request);
        Task<PaymentStatus> CheckPaymentStatusAsync(string transactionId);
    }

    // レガシーなペイメント処理クラス（既存のクラス）
    public class LegacyPaymentService
    {
        public string SubmitPayment(string cardNumber, string expiryDate, decimal amount, string currency)
        {
            // レガシーな支払い処理ロジック
            Console.WriteLine($"レガシーシステムでの支払い処理: {amount} {currency}");
            
            // トランザクションIDを返す
            return Guid.NewGuid().ToString();
        }

        public bool VerifyCardDetails(string cardNumber, string expiryDate)
        {
            // カード詳細の検証
            Console.WriteLine($"カード詳細の検証: {cardNumber.Substring(0, 4)}XXXXXXXXXXXX");
            
            // 検証結果を返す
            return true;
        }

        public string GetTransactionStatus(string transactionId)
        {
            // トランザクションステータスのチェック
            Console.WriteLine($"トランザクションステータスのチェック: {transactionId}");
            
            // ステータス文字列を返す
            return "COMPLETED";
        }
    }

    // アダプター：レガシーサービスを新しいインターフェースに適応させる
    public class PaymentServiceAdapter : IModernPaymentProcessor
    {
        private readonly LegacyPaymentService _legacyService;
        
        public PaymentServiceAdapter(LegacyPaymentService legacyService)
        {
            _legacyService = legacyService;
        }
        
        public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)
        {
            // レガシーメソッドの同期呼び出しを非同期操作に変換
            return await Task.Run(() => 
            {
                try
                {
                    // レガシーメソッドを呼び出す
                    string transactionId = _legacyService.SubmitPayment(
                        request.CardNumber,
                        request.ExpiryDate,
                        request.Amount,
                        request.Currency
                    );
                    
                    // 新しいインターフェースの形式に変換
                    return new PaymentResult
                    {
                        Success = true,
                        TransactionId = transactionId,
                        Message = "支払いが正常に処理されました"
                    };
                }
                catch (Exception ex)
                {
                    return new PaymentResult
                    {
                        Success = false,
                        Message = $"支払い処理エラー: {ex.Message}"
                    };
                }
            });
        }
        
        public async Task<bool> ValidatePaymentAsync(PaymentRequest request)
        {
            // 同期メソッドを非同期にラップ
            return await Task.Run(() => 
                _legacyService.VerifyCardDetails(request.CardNumber, request.ExpiryDate)
            );
        }
        
        public async Task<PaymentStatus> CheckPaymentStatusAsync(string transactionId)
        {
            // レガシーステータス文字列を列挙型に変換
            return await Task.Run(() => 
            {
                string status = _legacyService.GetTransactionStatus(transactionId);
                
                // 文字列のステータスを新しい列挙型に変換
                return status switch
                {
                    "COMPLETED" => PaymentStatus.Completed,
                    "PENDING" => PaymentStatus.Pending,
                    "FAILED" => PaymentStatus.Failed,
                    "REFUNDED" => PaymentStatus.Refunded,
                    _ => PaymentStatus.Unknown
                };
            });
        }
    }

    // 新しいインターフェースのデータ型
    public class PaymentRequest
    {
        public string CardNumber { get; set; }
        public string ExpiryDate { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
    }

    public class PaymentResult
    {
        public bool Success { get; set; }
        public string TransactionId { get; set; }
        public string Message { get; set; }
    }

    public enum PaymentStatus
    {
        Completed,
        Pending,
        Failed,
        Refunded,
        Unknown
    }

    #endregion

    #region 双方向アダプター

    // 新しいファイルフォーマットのインターフェース
    public interface IJsonDocument
    {
        string ToJson();
        void LoadFromJson(string jsonData);
        IEnumerable<KeyValuePair<string, string>> GetProperties();
    }

    // 古いファイルフォーマットのインターフェース
    public interface IXmlDocument
    {
        string ToXml();
        void LoadFromXml(string xmlData);
        IEnumerable<KeyValuePair<string, string>> GetAttributes();
    }

    // 双方向アダプター：両方のインターフェースを実装
    public class DocumentFormatAdapter : IJsonDocument, IXmlDocument
    {
        private readonly Dictionary<string, string> _properties = new Dictionary<string, string>();
        
        // IJsonDocumentの実装
        public string ToJson()
        {
            // プロパティをJSONに変換するロジック
            return $"{{ {string.Join(", ", GetJsonProperties())} }}";
        }
        
        public void LoadFromJson(string jsonData)
        {
            // JSON解析ロジック（簡略化）
            Console.WriteLine($"JSONからデータを読み込み: {jsonData}");
            
            // デモのために、解析せずにいくつかのプロパティを設定
            _properties.Clear();
            _properties.Add("name", "テストドキュメント");
            _properties.Add("version", "1.0");
        }
        
        public IEnumerable<KeyValuePair<string, string>> GetProperties()
        {
            return _properties;
        }
        
        // IXmlDocumentの実装
        public string ToXml()
        {
            // プロパティをXMLに変換するロジック
            return $"<document>{string.Join("", GetXmlAttributes())}</document>";
        }
        
        public void LoadFromXml(string xmlData)
        {
            // XML解析ロジック（簡略化）
            Console.WriteLine($"XMLからデータを読み込み: {xmlData}");
            
            // デモのために、解析せずにいくつかのプロパティを設定
            _properties.Clear();
            _properties.Add("name", "テストドキュメント");
            _properties.Add("version", "1.0");
        }
        
        public IEnumerable<KeyValuePair<string, string>> GetAttributes()
        {
            return _properties;
        }
        
        // ヘルパーメソッド
        private IEnumerable<string> GetJsonProperties()
        {
            foreach (var prop in _properties)
            {
                yield return $"\"{prop.Key}\": \"{prop.Value}\"";
            }
        }
        
        private IEnumerable<string> GetXmlAttributes()
        {
            foreach (var attr in _properties)
            {
                yield return $"<{attr.Key}>{attr.Value}</{attr.Key}>";
            }
        }
    }

    #endregion

    #region インターフェース適応例：データアクセス

    // アプリケーションが使用するリポジトリインターフェース
    public interface ICustomerRepository
    {
        Task<Customer> GetByIdAsync(int id);
        Task<IEnumerable<Customer>> GetAllAsync();
        Task AddAsync(Customer customer);
        Task UpdateAsync(Customer customer);
        Task DeleteAsync(int id);
    }

    // 依存するサードパーティORM（これを直接変更できない）
    public class ThirdPartyDatabaseClient
    {
        // サードパーティがサポートするメソッド
        public Customer FindById(int id)
        {
            // 実際のデータベースアクセスロジック（簡略化）
            Console.WriteLine($"ThirdPartyDatabaseClientからIDによる顧客取得: {id}");
            return new Customer { Id = id, Name = "サンプル顧客", Email = "sample@example.com" };
        }
        
        public List<Customer> FindAll()
        {
            // すべての顧客を取得（簡略化）
            Console.WriteLine("ThirdPartyDatabaseClientからすべての顧客取得");
            return new List<Customer>
            {
                new Customer { Id = 1, Name = "顧客1", Email = "customer1@example.com" },
                new Customer { Id = 2, Name = "顧客2", Email = "customer2@example.com" }
            };
        }
        
        public void Insert(Customer customer)
        {
            // 顧客を挿入（簡略化）
            Console.WriteLine($"ThirdPartyDatabaseClientで顧客を挿入: {customer.Name}");
        }
        
        public void Update(Customer customer)
        {
            // 顧客を更新（簡略化）
            Console.WriteLine($"ThirdPartyDatabaseClientで顧客を更新: {customer.Id}");
        }
        
        public void Delete(int id)
        {
            // 顧客を削除（簡略化）
            Console.WriteLine($"ThirdPartyDatabaseClientで顧客を削除: {id}");
        }
    }

    // アダプター：サードパーティORM → リポジトリインターフェース
    public class ThirdPartyRepositoryAdapter : ICustomerRepository
    {
        private readonly ThirdPartyDatabaseClient _databaseClient;
        
        public ThirdPartyRepositoryAdapter(ThirdPartyDatabaseClient databaseClient)
        {
            _databaseClient = databaseClient;
        }
        
        public async Task<Customer> GetByIdAsync(int id)
        {
            // 同期メソッドを非同期ラップ
            return await Task.Run(() => _databaseClient.FindById(id));
        }
        
        public async Task<IEnumerable<Customer>> GetAllAsync()
        {
            return await Task.Run(() => _databaseClient.FindAll());
        }
        
        public async Task AddAsync(Customer customer)
        {
            await Task.Run(() => _databaseClient.Insert(customer));
        }
        
        public async Task UpdateAsync(Customer customer)
        {
            await Task.Run(() => _databaseClient.Update(customer));
        }
        
        public async Task DeleteAsync(int id)
        {
            await Task.Run(() => _databaseClient.Delete(id));
        }
    }

    // ドメインオブジェクト
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }

    #endregion

    #region 使用例

    // アダプターパターンの使用例
    public class AdapterDemo
    {
        public static void RunBasicDemo()
        {
            Console.WriteLine("--- 基本的なアダプターパターンのデモ ---");
            
            // オリジナルのAdapteeを使用
            Adaptee adaptee = new Adaptee();
            Console.WriteLine($"Adaptee: {adaptee.SpecificRequest()}");
            
            // クラスアダプター（継承）
            ITarget classAdapter = new ClassAdapter();
            Console.WriteLine($"Target経由で呼び出し: {classAdapter.Request()}");
            
            // オブジェクトアダプター（コンポジション）
            ITarget objectAdapter = new ObjectAdapter(adaptee);
            Console.WriteLine($"Target経由で呼び出し: {objectAdapter.Request()}");
        }

        public static async Task RunPaymentAdapterDemo()
        {
            Console.WriteLine("\n--- 決済処理アダプターのデモ ---");
            
            // レガシーサービス（直接使用）
            LegacyPaymentService legacyService = new LegacyPaymentService();
            string transactionId = legacyService.SubmitPayment("4111111111111111", "12/25", 100.00m, "USD");
            Console.WriteLine($"レガシーサービスから直接支払い処理: {transactionId}");
            
            // アダプターを介した使用
            IModernPaymentProcessor modernProcessor = new PaymentServiceAdapter(legacyService);
            
            PaymentRequest request = new PaymentRequest
            {
                CardNumber = "4111111111111111",
                ExpiryDate = "12/25",
                Amount = 200.00m,
                Currency = "USD"
            };
            
            // 非同期APIを使用
            bool isValid = await modernProcessor.ValidatePaymentAsync(request);
            Console.WriteLine($"カード検証結果: {isValid}");
            
            PaymentResult result = await modernProcessor.ProcessPaymentAsync(request);
            Console.WriteLine($"支払い結果: {(result.Success ? "成功" : "失敗")}, トランザクションID: {result.TransactionId}");
            
            PaymentStatus status = await modernProcessor.CheckPaymentStatusAsync(result.TransactionId);
            Console.WriteLine($"トランザクションステータス: {status}");
        }

        public static void RunDocumentAdapterDemo()
        {
            Console.WriteLine("\n--- ドキュメントフォーマットアダプターのデモ ---");
            
            // 双方向アダプター
            DocumentFormatAdapter adapter = new DocumentFormatAdapter();
            
            // JSONとして使用
            adapter.LoadFromJson("{\"name\": \"テストドキュメント\", \"version\": \"1.0\"}");
            Console.WriteLine($"JSONドキュメント: {adapter.ToJson()}");
            
            // XMLとして使用（同じデータが異なるフォーマットで表示される）
            Console.WriteLine($"XMLドキュメント: {adapter.ToXml()}");
            
            // XMLからの読み込み
            adapter.LoadFromXml("<document><name>別のドキュメント</name><version>2.0</version></document>");
            
            // プロパティの取得
            Console.WriteLine("ドキュメントプロパティ:");
            foreach (var prop in adapter.GetProperties())
            {
                Console.WriteLine($"  {prop.Key}: {prop.Value}");
            }
        }

        public static async Task RunRepositoryAdapterDemo()
        {
            Console.WriteLine("\n--- リポジトリアダプターのデモ ---");
            
            // サードパーティの直接使用
            ThirdPartyDatabaseClient client = new ThirdPartyDatabaseClient();
            Customer customer1 = client.FindById(1);
            Console.WriteLine($"サードパーティクライアントから直接取得: {customer1.Name}");
            
            // アダプターを介した使用
            ICustomerRepository repository = new ThirdPartyRepositoryAdapter(client);
            
            // リポジトリパターンのインターフェースを通じてアクセス
            Customer customer2 = await repository.GetByIdAsync(2);
            Console.WriteLine($"リポジトリインターフェースを介して取得: {customer2.Name}");
            
            // 新しい顧客の追加
            Customer newCustomer = new Customer { Name = "新規顧客", Email = "new@example.com" };
            await repository.AddAsync(newCustomer);
            
            // すべての顧客を取得
            var allCustomers = await repository.GetAllAsync();
            Console.WriteLine("すべての顧客:");
            foreach (var c in allCustomers)
            {
                Console.WriteLine($"  ID: {c.Id}, 名前: {c.Name}");
            }
        }
        
        public static async Task RunAllDemos()
        {
            RunBasicDemo();
            await RunPaymentAdapterDemo();
            RunDocumentAdapterDemo();
            await RunRepositoryAdapterDemo();
        }
    }

    #endregion
}