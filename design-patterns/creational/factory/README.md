# ファクトリパターン (Factory Pattern)

## 概要

ファクトリパターンは、オブジェクトの生成ロジックをカプセル化するクリエイショナルデザインパターンです。クライアントコードから具体的なクラスのインスタンス化を分離し、柔軟で拡張性のあるコードを実現します。

## バリエーション

### 1. 単純ファクトリ (Simple Factory)

最も基本的な形式で、オブジェクト生成ロジックを単一のクラスにカプセル化します。

```csharp
public class SimpleProductFactory
{
    public IProduct CreateProduct(string type)
    {
        switch (type)
        {
            case "A": return new ConcreteProductA();
            case "B": return new ConcreteProductB();
            default: throw new ArgumentException("Unknown product type");
        }
    }
}
```

### 2. ファクトリメソッド (Factory Method)

サブクラスがどのクラスをインスタンス化するかを決定できるようにするパターン。抽象クリエイタークラスとその具象サブクラスで構成されます。

```csharp
// 抽象クリエーター
public abstract class DocumentCreator
{
    public abstract IDocument CreateDocument();
    
    public void EditDocument() 
    {
        var document = CreateDocument();
        document.Open();
        // 編集ロジック
        document.Save();
    }
}

// 具象クリエーター
public class PdfDocumentCreator : DocumentCreator
{
    public override IDocument CreateDocument()
    {
        return new PdfDocument();
    }
}
```

### 3. 抽象ファクトリ (Abstract Factory)

関連するオブジェクトのファミリーを作成するためのインターフェースを提供します。具体的な実装を指定せずに関連オブジェクトのグループを生成できます。

```csharp
// 抽象ファクトリ
public interface IUIControlFactory
{
    IButton CreateButton();
    ITextBox CreateTextBox();
}

// 具象ファクトリ
public class WindowsUIControlFactory : IUIControlFactory
{
    public IButton CreateButton() => new WindowsButton();
    public ITextBox CreateTextBox() => new WindowsTextBox();
}
```

## 目的

- オブジェクト生成ロジックをカプセル化する
- 具体的なクラスではなく、インターフェースを通じた作業を促進する
- クラスの実装詳細からクライアントコードを分離する
- 関連オブジェクトのグループを一貫して生成する

## 利点

- **疎結合**: クライアントコードは具体的なクラスに依存しません
- **カプセル化**: 生成ロジックを一箇所に集中させる
- **保守性**: 新製品タイプの追加が容易
- **テスト容易性**: モック実装への置き換えが簡単

## 欠点

- コードの複雑さが増加する
- 多数の新しいインターフェースとクラスが導入される
- 過剰な柔軟性は時に不要な複雑さをもたらす場合がある

## DDDにおけるファクトリパターン

ドメイン駆動設計（DDD）では、ファクトリは特に重要な役割を果たします：

1. **複雑なエンティティ作成のカプセル化**: 複雑な初期化ロジックや不変条件をファクトリに集中させる
2. **集約ルートの整合性保証**: エンティティが正しい状態で作成されることを保証
3. **ドメインルールに従ったオブジェクト生成**: ビジネスルールに基づいた生成を強制
4. **構築プロセスのドキュメント化**: ファクトリメソッド名が構築の意図を明確にする

```csharp
// ドメインファクトリの例
public class OrderFactory : IOrderFactory
{
    private readonly IProductRepository _productRepository;
    private readonly ICustomerRepository _customerRepository;
    
    public async Task<Order> CreateOrderAsync(Guid customerId, List<OrderItemInfo> items)
    {
        // 顧客の存在確認
        var customer = await _customerRepository.GetByIdAsync(customerId);
        if (customer == null)
            throw new ArgumentException("Customer not found");
            
        // 注文オブジェクトの作成
        var order = Order.CreateEmptyOrder(customerId);
        
        // 製品情報の取得と注文項目の追加
        foreach (var itemInfo in items)
        {
            var product = await _productRepository.GetByIdAsync(itemInfo.ProductId);
            if (product == null)
                throw new ArgumentException($"Product not found: {itemInfo.ProductId}");
                
            order.AddItem(product.Id, product.Name, itemInfo.Quantity, product.Price);
        }
        
        return order;
    }
}
```

## ベストプラクティス

- ファクトリメソッド名は明確な意図を伝える命名を心がける
- ファクトリに過剰な責任を負わせない
- 単一責任の原則を守る
- DI（依存性の注入）と組み合わせて使用する
- 抽象ファクトリは関連性のあるオブジェクト群にのみ使用する

## 実装例

サンプルコードは、`./csharp/Factory.cs` ファイルを参照してください。この実装例では、以下の要素が含まれています：

- 単純ファクトリの実装
- ファクトリメソッドパターンの実装
- 抽象ファクトリパターンの実装
- DDD向けのドメインファクトリの例

## 関連パターン

- **ビルダーパターン**: 複雑なオブジェクトを段階的に構築
- **プロトタイプパターン**: 既存のオブジェクトをクローンして新しいオブジェクトを作成
- **シングルトンパターン**: クラスのインスタンスが1つだけ存在することを保証
- **DIコンテナ**: ファクトリの高度な形式と見なすことができる
