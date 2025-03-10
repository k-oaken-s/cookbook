package com.example.domain.model.entity;

import com.example.domain.model.valueobject.Money;
import com.example.domain.model.valueobject.ProductId;
import com.example.domain.model.valueobject.Quantity;

import java.util.Objects;

/**
 * 商品を表すエンティティ
 * エンティティは同一性によって識別される
 */
public class Product {
    private final ProductId id; // 識別子
    private String name;
    private String description;
    private Money price;
    private Quantity stockQuantity;
    private boolean active;

    // プライベートコンストラクタ - ファクトリメソッド経由で生成する
    private Product(ProductId id, String name, String description, Money price, Quantity stockQuantity) {
        this.id = id;
        this.name = name;
        this.description = description;
        this.price = price;
        this.stockQuantity = stockQuantity;
        this.active = true;
    }

    // ファクトリメソッド
    public static Product create(String name, String description, Money price, Quantity stockQuantity) {
        validateNewProduct(name, description, price);
        return new Product(ProductId.generateNew(), name, description, price, stockQuantity);
    }

    // 永続化からの復元用ファクトリメソッド
    public static Product reconstitute(ProductId id, String name, String description, Money price, 
                                       Quantity stockQuantity, boolean active) {
        Product product = new Product(id, name, description, price, stockQuantity);
        product.active = active;
        return product;
    }

    // バリデーションロジック
    private static void validateNewProduct(String name, String description, Money price) {
        Objects.requireNonNull(name, "Product name cannot be null");
        Objects.requireNonNull(description, "Product description cannot be null");
        Objects.requireNonNull(price, "Product price cannot be null");

        if (name.isBlank()) {
            throw new IllegalArgumentException("Product name cannot be empty");
        }
        if (description.isBlank()) {
            throw new IllegalArgumentException("Product description cannot be empty");
        }
        if (price.getAmount().signum() <= 0) {
            throw new IllegalArgumentException("Product price must be positive");
        }
    }

    // ドメインロジック
    public void updateStockQuantity(Quantity newQuantity) {
        Objects.requireNonNull(newQuantity, "Stock quantity cannot be null");
        this.stockQuantity = newQuantity;
    }

    public boolean hasEnoughStock(Quantity requiredQuantity) {
        return this.stockQuantity.isGreaterThan(requiredQuantity) || 
               this.stockQuantity.equals(requiredQuantity);
    }

    public void reduceStock(Quantity quantityToReduce) {
        if (!hasEnoughStock(quantityToReduce)) {
            throw new IllegalStateException("Not enough stock available");
        }
        this.stockQuantity = this.stockQuantity.subtract(quantityToReduce);
    }

    public void addStock(Quantity quantityToAdd) {
        this.stockQuantity = this.stockQuantity.add(quantityToAdd);
    }

    public void activate() {
        this.active = true;
    }

    public void deactivate() {
        this.active = false;
    }

    public void updatePrice(Money newPrice) {
        Objects.requireNonNull(newPrice, "New price cannot be null");
        if (newPrice.getAmount().signum() <= 0) {
            throw new IllegalArgumentException("Product price must be positive");
        }
        this.price = newPrice;
    }

    public void updateDetails(String name, String description) {
        Objects.requireNonNull(name, "Product name cannot be null");
        Objects.requireNonNull(description, "Product description cannot be null");

        if (name.isBlank()) {
            throw new IllegalArgumentException("Product name cannot be empty");
        }
        if (description.isBlank()) {
            throw new IllegalArgumentException("Product description cannot be empty");
        }

        this.name = name;
        this.description = description;
    }

    // ゲッター
    public ProductId getId() {
        return id;
    }

    public String getName() {
        return name;
    }

    public String getDescription() {
        return description;
    }

    public Money getPrice() {
        return price;
    }

    public Quantity getStockQuantity() {
        return stockQuantity;
    }

    public boolean isActive() {
        return active;
    }

    // エンティティの等価性は識別子によって判断
    @Override
    public boolean equals(Object o) {
        if (this == o) return true;
        if (o == null || getClass() != o.getClass()) return false;
        Product product = (Product) o;
        return Objects.equals(id, product.id);
    }

    @Override
    public int hashCode() {
        return Objects.hash(id);
    }

    @Override
    public String toString() {
        return "Product{" +
                "id=" + id +
                ", name='" + name + '\'' +
                ", price=" + price +
                ", stockQuantity=" + stockQuantity +
                ", active=" + active +
                '}';
    }
}