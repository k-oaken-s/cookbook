# デコレーターパターン (Decorator Pattern)

## 概要

デコレーターパターンは、既存のオブジェクトに動的に新しい機能を追加できるようにする構造パターンです。このパターンは、継承を使用して機能を拡張する代わりに、オブジェクトをラップして新たな振る舞いを追加します。

## 目的

- 既存のオブジェクトに動的に責任を追加する
- サブクラス化の代替手段として機能を拡張する
- 実行時に柔軟に機能を組み合わせられるようにする
- 単一責任の原則に従い、機能を分離する

## 構造

```
+----------------+       +----------------+
|                |       |                |
|    Component   |<------|    Decorator   |
|    Interface   |       |    (Abstract)  |
|                |       |                |
+----------------+       +----------------+
      ^                        ^
      |                        |
+----------------+     +----------------+
|                |     |                |
|    Concrete    |     |    Concrete    |
|    Component   |     |    Decorator   |
|                |     |                |
+----------------+     +----------------+
```

1. **Component（コンポーネント）**: デコレーターとコンポーネントの共通インターフェース
2. **ConcreteComponent（具象コンポーネント）**: 基本機能を提供する具体的な実装
3. **Decorator（デコレーター）**: コンポーネントへの参照を持つ抽象クラス
4. **ConcreteDecorator（具象デコレーター）**: コンポーネントに具体的な機能を追加する実装

## 基本的な実装例

```csharp
// コンポーネントインターフェース
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

    public virtual string Operation()
    {
        return _component.Operation();
    }
}

// 具象デコレーターA
public class ConcreteDecoratorA : Decorator
{
    public ConcreteDecoratorA(IComponent component) : base(component) { }

    public override string Operation()
    {
        return $"ConcreteDecoratorA({base.Operation()})";
    }
}

// 具象デコレーターB
public class ConcreteDecoratorB : Decorator
{
    public ConcreteDecoratorB(IComponent component) : base(component) { }

    public override string Operation()
    {
        return $"ConcreteDecoratorB({base.Operation()})";
    }
}
```

## デコレーターパターンの実用例

デコレーターパターンは以下のようなシナリオでよく使用されます：

### 1. データの加工チェーン

入出力ストリームの処理チェーン：

```csharp
// データソースインターフェース
public interface IDataSource
{
    void WriteData(string data);
    string ReadData();
}

// 基本実装
public class FileDataSource : IDataSource { ... }

// デコレーター
public class EncryptionDecorator : DataSourceDecorator { ... }
public class CompressionDecorator : DataSourceDecorator { ... }
public class LoggingDecorator : DataSourceDecorator { ... }

// 使用例
IDataSource source = new LoggingDecorator(
    new CompressionDecorator(
        new EncryptionDecorator(
            new FileDataSource("data.txt")
        )
    )
);
```

### 2. Web API処理

HTTPリクエスト/レスポンスの処理チェーン：

```csharp
// ハンドラーインターフェース
public interface IHttpHandler
{
    string HandleRequest(HttpRequest request);
}

// デコレーター
public class AuthenticationDecorator : HttpHandlerDecorator { ... }
public class CachingDecorator : HttpHandlerDecorator { ... }
public class ContentTypeDecorator : HttpHandlerDecorator { ... }
public class LoggingDecorator : HttpHandlerDecorator { ... }

// 使用例
IHttpHandler handler = new LoggingDecorator(
    new CachingDecorator(
        new AuthenticationDecorator(
            new ContentTypeDecorator(
                new BaseHttpHandler()
            )
        )
    )
);
```

### 3. ビジネスロジックでの適用

注文処理のカスタマイズ：

```csharp
// 注文処理インターフェース
public interface IOrderProcessor
{
    OrderResult ProcessOrder(Order order);
}

// 様々な処理デコレーター
public class InventoryCheckDecorator : OrderProcessorDecorator { ... }
public class DiscountDecorator : OrderProcessorDecorator { ... }
public class TaxCalculationDecorator : OrderProcessorDecorator { ... }
public class NotificationDecorator : OrderProcessorDecorator { ... }

// 使用例
IOrderProcessor processor = new NotificationDecorator(
    new TaxCalculationDecorator(
        new DiscountDecorator(
            new InventoryCheckDecorator(
                new BaseOrderProcessor()
            )
        )
    )
);
```

## デコレーターパターンの利点

1. **単一責任の原則に従う**: 各デコレーターは単一の機能に集中
2. **柔軟な機能追加**: 既存コードを変更せずに機能を追加
3. **実行時の機能変更**: 動的に機能の構成を変更可能
4. **組み合わせの爆発を防ぐ**: 多機能の場合に継承より効率的
5. **機能の分離**: 分離された機能を組み合わせてカスタマイズ
6. **オープン/クローズドの原則**: 拡張に対してオープン、修正に対してクローズド

## デコレーターパターンの欠点

1. **コードの複雑化**: 多数の小さなオブジェクトが生成される
2. **デコレーター順序の重要性**: 機能適用順序によって結果が変わる場合がある
3. **型の透過性**: デコレーターがラップするオブジェクトへの直接アクセスが難しくなる
4. **初期化コードの複雑化**: 多くのデコレーターを追加する場合、コードが冗長になりがち
5. **デバッグの難しさ**: デコレーションの層が多いとデバッグが困難

## デコレーターとプロキシの違い

デコレーターとプロキシパターンは似ていますが、目的が異なります：

- **デコレーター**: 既存のオブジェクトに動的に機能を追加
- **プロキシ**: オブジェクトへのアクセスを制御（遅延初期化、アクセス制御、ロギングなど）

## デコレーターと継承の比較

機能拡張の方法として、デコレーターと継承を比較すると：

- **継承**: コンパイル時に固定された機能拡張
- **デコレーター**: 実行時に動的に組み合わせられる機能拡張

## デコレーターパターンの実装のバリエーション

### 1. 透過的デコレーター

インターフェースを完全に透過的に実装するデコレーター：

```csharp
// ベースインターフェースと同じインターフェースを維持
public class TransparentDecorator : IComponent
{
    private readonly IComponent _component;

    public TransparentDecorator(IComponent component)
    {
        _component = component;
    }

    public string Operation()
    {
        // 修飾された操作
        return $"Decorated {_component.Operation()}";
    }
}
```

### 2. 動的デコレーター

より柔軟な動的デコレーター、例えばデリゲートを使用：

```csharp
public class DynamicDecorator : IComponent
{
    private readonly IComponent _component;
    private readonly Func<string, string> _operation;

    public DynamicDecorator(IComponent component, Func<string, string> operation)
    {
        _component = component;
        _operation = operation;
    }

    public string Operation()
    {
        return _operation(_component.Operation());
    }
}

// 使用例
var component = new ConcreteComponent();
var decorator = new DynamicDecorator(component, s => $"Dynamic({s})");
```

## 実装例の詳細

サンプルコードは、`./csharp/Decorator.cs` ファイルを参照してください。この実装例では、以下の要素が含まれています：

1. **基本的なデコレーターパターンの実装**:
   - コンポーネントインターフェースと具象クラス
   - 基本デコレーターと具象デコレーター

2. **データストリーム処理の例**:
   - ファイルデータソースの実装
   - 暗号化、圧縮、ロギングデコレーター

3. **Webリクエスト処理の例**:
   - HTTPハンドラーインターフェース
   - 認証、キャッシュ、ロギングなどのデコレーター

4. **ビジネスロジックの例**:
   - 注文処理サービス
   - 在庫チェック、割引、税金計算、通知などのデコレーター

## 関連パターン

- **アダプターパターン**: 既存クラスのインターフェースを変換
- **コンポジットパターン**: オブジェクトツリーを構築（デコレーターは単一コンポーネントに集中）
- **ストラテジーパターン**: アルゴリズムを交換可能にする
- **チェーンオブレスポンシビリティ**: 複数のハンドラーでリクエストを処理（デコレーターは機能を追加）
- **プロキシパターン**: オブジェクトアクセスの制御
