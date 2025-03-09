# 仕様パターン (Specification Pattern)

## 概要

仕様パターン（Specification Pattern）は、ビジネスルールをオブジェクトに変換し、それらを組み合わせて複雑な条件を構築できるようにするパターンです。このパターンは、エンティティがある条件を満たすかどうかの評価と、その条件に基づいたエンティティのフィルタリングを分離します。

## 目的

- ビジネスルールや条件をカプセル化する
- ルールを再利用可能なオブジェクトとして定義する
- 複雑な条件を小さな単位に分解し、組み合わせる
- 検証ロジックとフィルタリングロジックを統一する
- クエリロジックをリポジトリやデータアクセスレイヤーと分離する

## 基本構造

```csharp
// 基本インターフェース
public interface ISpecification<T>
{
    bool IsSatisfiedBy(T entity);
    Expression<Func<T, bool>> ToExpression();
}

// 基本実装
public abstract class Specification<T> : ISpecification<T>
{
    public bool IsSatisfiedBy(T entity)
    {
        var predicate = ToExpression().Compile();
        return predicate(entity);
    }

    public abstract Expression<Func<T, bool>> ToExpression();
    
    // 論理演算子のオーバーロード
    public static Specification<T> operator &(Specification<T> left, Specification<T> right) => new AndSpecification<T>(left, right);
    public static Specification<T> operator |(Specification<T> left, Specification<T> right) => new OrSpecification<T>(left, right);
    public static Specification<T> operator !(Specification<T> spec) => new NotSpecification<T>(spec);
}

// 複合仕様（AND）
public class AndSpecification<T> : Specification<T>
{
    private readonly ISpecification<T> _left;
    private readonly ISpecification<T> _right;
    
    public AndSpecification(ISpecification<T> left, ISpecification<T> right) { _left = left; _right = right; }
    
    public override Expression<Func<T, bool>> ToExpression()
    {
        // 2つの式を結合するロジック
    }
}
```

## 主要コンポーネント

1. **仕様インターフェース (`ISpecification<T>`)**: 特定の型Tに対する仕様の契約を定義
2. **抽象仕様クラス (`Specification<T>`)**: 基本的な機能と論理演算子を提供
3. **具体的な仕様クラス**: 特定のビジネスルールを実装（例: `ActiveProductSpecification`）
4. **複合仕様クラス**: 複数の仕様を組み合わせる（AND, OR, NOT操作）

## 使用例

1. **単純な仕様の定義**:
```csharp
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
```

2. **仕様の組み合わせ**:
```csharp
// 複合仕様の作成
var availableProductsSpec = new ActiveProductSpecification() & new InStockProductSpecification();

// または演算子の使用
var specialOffersSpec = new ElectronicsProductSpecification() | new ClothingProductSpecification();

// 否定の使用
var discontinuedProductsSpec = !new ActiveProductSpecification();
```

3. **リポジトリとの統合**:
```csharp
// 仕様を使用したリポジトリのクエリ
public async Task<IEnumerable<Product>> FindAsync(ISpecification<Product> specification)
{
    return await _dbContext.Products
        .Where(specification.ToExpression())
        .ToListAsync();
}

// 使用例
var products = await repository.FindAsync(
    new ActiveProductSpecification() & 
    new InStockProductSpecification() &
    new PriceRangeSpecification(100, 500)
);
```

## 仕様パターンの利点

1. **ビジネスルールの分離**: ルールが明確に定義され、その実装がドメインモデルやクエリロジックから分離される
2. **再利用性**: 仕様は様々な場所（検証、フィルタリング、ビジネスルール）で再利用可能
3. **組み合わせの柔軟性**: 基本的な仕様を論理演算子で結合して、複雑な条件を構築できる
4. **ORM互換性**: Expression Treeを通じてEntity Frameworkなどのクエリプロバイダと統合可能
5. **テスト容易性**: 仕様は独立してテスト可能、モックの必要性が減少
6. **ドメイン言語の明確化**: 仕様名がビジネスルールを明確に表現（例: `CustomerIsEligibleForDiscount`）

## 仕様パターンの欠点

1. **複雑さの増加**: 追加の抽象化レイヤーにより、コードがやや複雑になる
2. **パフォーマンスへの影響**: 評価時の式ツリーの結合と処理にオーバーヘッドがある場合も
3. **学習曲線**: チームがパターンに慣れるまでに時間がかかる
4. **過剰設計のリスク**: 単純なケースでは不必要な複雑さをもたらす可能性

## 使用シナリオ

仕様パターンは以下のような状況で特に有効です：

1. **複雑なフィルタリング条件**: 多数の条件を組み合わせる必要がある場合
2. **動的なクエリビルディング**: 実行時に条件を構築する必要がある場合
3. **一貫したビジネスルール**: 同じルールを検証とフィルタリングの両方に使用する場合
4. **ドメインロジックの明示化**: クエリロジックをドメイン言語で表現したい場合
5. **ORMの最適化**: データベースレベルでのフィルタリングをサポートしたい場合

## DDDでの仕様パターン

ドメイン駆動設計（DDD）では、仕様パターンは以下の場面で特に有用です：

1. **リポジトリのクエリ抽象化**: リポジトリが実装の詳細を隠しながら、複雑なクエリをサポートする
2. **ドメインルールの表現**: ドメイン固有の制約やルールをコードで明示的に表現
3. **集約間のルール**: 複数の集約にまたがるルールをエンカプセル化
4. **ビジネスプロセスの決定**: ビジネスプロセスで条件分岐を決定するためのルール

## バリエーション

1. **単純仕様**: 基本的な仕様パターン（IsSatisfiedByメソッドのみ）
2. **クエリ仕様**: ORM互換のための式ツリーをサポート（ToExpressionメソッド）
3. **構築仕様**: オブジェクトを構築するために使用
4. **検証仕様**: 検証ルールとエラーメッセージを提供

## 実装例の詳細

サンプルコードは、`./csharp/Specification.cs` ファイルを参照してください。この実装例では、以下の要素が含まれています：

- 仕様パターンの基本的なインフラストラクチャ（基底クラス、論理演算子など）
- 製品関連の様々な仕様（有効製品、在庫あり、価格範囲など）
- 複合仕様の例
- Entity Frameworkと統合したリポジトリの例
- アプリケーションサービスでの使用例
- ドメインサービスでの活用例

## 関連パターン

- **リポジトリパターン**: 仕様を使用してデータアクセスをフィルタリング
- **ストラテジーパターン**: アルゴリズムをカプセル化（仕様は条件を評価するためのストラテジーとみなせる）
- **コンポジットパターン**: 複合仕様は基本的にコンポジットパターンを用いている
- **ビジターパターン**: 複雑な仕様ツリーを処理するために使用できる
- **ビルダーパターン**: 複雑な仕様を段階的に構築するために使用できる
