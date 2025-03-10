package com.example.domain.model.entity;

import com.example.domain.model.valueobject.Money;
import com.example.domain.model.valueobject.ProductId;
import com.example.domain.model.valueobject.Quantity;

import java.util.Objects;
import java.util.UUID;

/**
 * 注文項目を表す子エンティティ
 * OrderEntityの一部として所有される
 */
public class OrderItem {
    private final UUID id; // エンティティとしての識別子
    private final ProductId productId; // 関連する商品のID
    private final String productName; // 注文時点での商品名（商品が変更されても注文項目は元の情報を保持）
    private final Money unitPrice; // 注文時点での単価
    private Quantity quantity; // 数量

    // プライベートコンストラクタ
    private OrderItem(UUID id, ProductId productId, String productName, Money unitPrice, Quantity quantity) {
        this.id = id;
        this.productId = productId;
        this.productName = productName;
        this.unitPrice = unitPrice;
        this.quantity = quantity;
    }

    // ファクトリメソッド
    public static OrderItem create(ProductId productId, String productName, Money unitPrice, Quantity quantity) {
        Objects.requireNonNull(productId, "Product ID cannot be null");
        Objects.requireNonNull(productName, "Product name cannot be null");
        Objects.requireNonNull(unitPrice, "Unit price cannot be null");
        Objects.requireNonNull(quantity, "Quantity cannot be null");

        if (productName.isBlank()) {
            throw new IllegalArgumentException("Product name cannot be empty");
        }
        if (unitPrice.getAmount().signum() <= 0) {
            throw new IllegalArgumentException("Unit price must be positive");
        }
        if (quantity.isZero()) {
            throw new IllegalArgumentException("Quantity must be greater than zero");
        }

        return new OrderItem(UUID.randomUUID(), productId, productName, unitPrice, quantity);
    }

    // 永続化からの復元用ファクトリメソッド
    public static OrderItem reconstitute(UUID id, ProductId productId, String productName, Money unitPrice, Quantity quantity) {
        return new OrderItem(id, productId, productName, unitPrice, quantity);
    }

    // ドメインロジック
    public void updateQuantity(Quantity newQuantity) {
        Objects.requireNonNull(newQuantity, "Quantity cannot be null");
        if (newQuantity.isZero()) {
            throw new IllegalArgumentException("Quantity must be greater than zero");
        }
        this.quantity = newQuantity;
    }

    // 金額計算
    public Money calculateTotalPrice() {
        return unitPrice.multiply(quantity.getValue());
    }

    // ゲッター
    public UUID getId() {
        return id;
    }

    public ProductId getProductId() {
        return productId;
    }

    public String getProductName() {
        return productName;
    }

    public Money getUnitPrice() {
        return unitPrice;
    }

    public Quantity getQuantity() {
        return quantity;
    }

    // エンティティの等価性は識別子によって判断
    @Override
    public boolean equals(Object o) {
        if (this == o) return true;
        if (o == null || getClass() != o.getClass()) return false;
        OrderItem orderItem = (OrderItem) o;
        return Objects.equals(id, orderItem.id);
    }

    @Override
    public int hashCode() {
        return Objects.hash(id);
    }

    @Override
    public String toString() {
        return "OrderItem{" +
                "id=" + id +
                ", productId=" + productId +
                ", productName='" + productName + '\'' +
                ", unitPrice=" + unitPrice +
                ", quantity=" + quantity +
                '}';
    }
}