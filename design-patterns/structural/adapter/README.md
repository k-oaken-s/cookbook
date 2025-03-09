# アダプターパターン (Adapter Pattern)

## 概要

アダプターパターンは、既存のクラスのインターフェースを、クライアントが期待する別のインターフェースに変換するための構造パターンです。このパターンを使用すると、互換性のないインターフェースを持つクラスが協調して動作できるようになります。

## 目的

- 互換性のないインターフェースを持つクラス間の協調を可能にする
- 既存のクラスを変更せずに新しいインターフェースで使用できるようにする
- サードパーティライブラリやレガシーコードを最新のコードと統合する
- 異なるシステム間の橋渡しをする

## 構造

```
+----------------+       +----------------+       +----------------+
|                |       |                |       |                |
|    Client      |------>|    Target      |<------+    Adapter     |
|                |       |    Interface   |       |                |
+----------------+       +----------------+       +----------------+
                                                         |
                                                         |
                                                         v
                                                  +----------------+
                                                  |                |
                                                  |    Adaptee     |
                                                  |                |
                                                  +----------------+
```

1. **Target（ターゲット）**: クライアントが使用するインターフェース
2. **Adaptee（適応されるクラス）**: 既存のクラスで、互換性のないインターフェースを持つ
3. **Adapter（アダプター）**: TargetインターフェースをAdapteeに適応させるクラス
4. **Client（クライアント）**: Targetインターフェースを使用するコード

## アダプターの2つの主要な実装方法

### 1. クラスアダプター（継承を使用）

```csharp
// 継承を使用したアダプター
public class ClassAdapter : Adaptee, ITarget
{
    public string Request()
    {
        // 既存のメソッドを呼び出して結果を変換
        return $"ClassAdapter: {SpecificRequest()}";
    }
}
```

**特徴**:
- 多重継承をサポートする言語（C++など）で最も効果的
- 1つのAdapteeにのみ適応可能
- オーバーライドによりAdapteeの振る舞いを拡張できる

### 2. オブジェクトアダプター（コンポジションを使用）

```csharp
// コンポジションを使用したアダプター
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
```

**特徴**:
- より柔軟でオブジェクト指向の原則に沿っている
- 実行時にAdapteeを変更可能
- 複数のAdapteeに対応可能
- すべてのプログラミング言語で実装可能

## アダプターパターンの使用例

### 1. レガシーコードの統合

古いAPIやライブラリを新しいコードベースと統合する：

```csharp
// モダンなインターフェース
public interface IModernPaymentProcessor
{
    Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request);
}

// レガシーなサービス
public class LegacyPaymentService
{
    public string SubmitPayment(string cardNumber, string expiryDate, decimal amount, string currency)
    {
        // 同期的な処理
        return "transaction_id";
    }
}

// アダプター
public class PaymentServiceAdapter : IModernPaymentProcessor
{
    private readonly LegacyPaymentService _legacyService;
    
    public PaymentServiceAdapter(LegacyPaymentService legacyService)
    {
        _legacyService = legacyService;
    }
    
    public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)
    {
        // 同期メソッドを非同期に変換
        return await Task.Run(() => 
        {
            string transactionId = _legacyService.SubmitPayment(
                request.CardNumber,
                request.ExpiryDate,
                request.Amount,
                request.Currency
            );
            
            return new PaymentResult { TransactionId = transactionId };
        });
    }
}
```

### 2. サードパーティライブラリの適応

外部ライブラリを自分のアプリケーションのインターフェースに適応させる：

```csharp
// アプリケーションが使用するインターフェース
public interface ICustomerRepository
{
    Task<Customer> GetByIdAsync(int id);
    Task<IEnumerable<Customer>> GetAllAsync();
}

// サードパーティのORM
public class ThirdPartyDatabaseClient
{
    public Customer FindById(int id) { /* ... */ }
    public List<Customer> FindAll() { /* ... */ }
}

// アダプター
public class ThirdPartyRepositoryAdapter : ICustomerRepository
{
    private readonly ThirdPartyDatabaseClient _client;
    
    public ThirdPartyRepositoryAdapter(ThirdPartyDatabaseClient client)
    {
        _client = client;
    }
    
    public async Task<Customer> GetByIdAsync(int id)
    {
        return await Task.Run(() => _client.FindById(id));
    }
    
    public async Task<IEnumerable<Customer>> GetAllAsync()
    {
        return await Task.Run(() => _client.FindAll());
    }
}
```

### 3. 複数のフォーマット間の変換

異なるデータフォーマット間の変換を行う：

```csharp
// アダプター
public class DocumentFormatAdapter : IJsonDocument, IXmlDocument
{
    // JSONインターフェースの実装
    public string ToJson() { /* ... */ }
    public void LoadFromJson(string jsonData) { /* ... */ }
    
    // XMLインターフェースの実装
    public string ToXml() { /* ... */ }
    public void LoadFromXml(string xmlData) { /* ... */ }
}
```

## アダプターパターンの利点

1. **シングルレスポンシビリティの原則**: 変換ロジックが単一クラスに集中
2. **オープン/クローズドの原則**: 既存コードを変更せずに拡張
3. **インターフェース分離**: クライアントは必要なインターフェースだけを使用
4. **レガシーコードの再利用**: 古いコードを新しいシステムで活用
5. **段階的移行**: 大きなシステムを少しずつ更新可能

## アダプターパターンの欠点

1. **複雑さの増加**: 追加のクラスとインターフェースが必要
2. **パフォーマンスのオーバーヘッド**: 間接レイヤーによる若干の遅延
3. **全ての不一致を解決できるわけではない**: 機能の大きな違いがある場合は困難
4. **デバッグの難しさ**: 呼び出し連鎖が長くなる

## 関連パターン

- **ブリッジパターン**: 抽象と実装を分離（アダプターは既存のクラスを適応、ブリッジは設計段階から分離）
- **デコレーターパターン**: 機能を追加（アダプターはインターフェースを変換）
- **ファサードパターン**: シンプルなインターフェースを提供（アダプターはインターフェースを適応）
- **プロキシパターン**: アクセスを制御（アダプターはインターフェースを変更）

## 実装例

サンプルコードは、`./csharp/Adapter.cs` ファイルを参照してください。この実装例では、以下の要素が含まれています：

1. **基本的なアダプターパターン**:
   - クラスアダプター（継承）
   - オブジェクトアダプター（コンポジション）

2. **レガシーAPIの適応**:
   - 同期APIを非同期インターフェースに適応

3. **双方向アダプター**:
   - 複数のインターフェースを実装するアダプター

4. **データアクセスの適応**:
   - サードパーティデータベースクライアントをリポジトリインターフェースに適応
