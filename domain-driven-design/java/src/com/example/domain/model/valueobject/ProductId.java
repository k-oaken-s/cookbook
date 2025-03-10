package com.example.domain.model.valueobject;

import java.util.Objects;
import java.util.UUID;

/**
 * 商品IDを表す値オブジェクト
 * エンティティの識別子として使用される
 */
public final class ProductId {
    private final UUID id;

    private ProductId(UUID id) {
        this.id = id;
    }

    public static ProductId of(UUID id) {
        Objects.requireNonNull(id, "Product ID cannot be null");
        return new ProductId(id);
    }

    public static ProductId of(String id) {
        return of(UUID.fromString(id));
    }

    public static ProductId generateNew() {
        return new ProductId(UUID.randomUUID());
    }

    public UUID getValue() {
        return id;
    }

    @Override
    public boolean equals(Object o) {
        if (this == o) return true;
        if (o == null || getClass() != o.getClass()) return false;
        ProductId productId = (ProductId) o;
        return Objects.equals(id, productId.id);
    }

    @Override
    public int hashCode() {
        return Objects.hash(id);
    }

    @Override
    public String toString() {
        return id.toString();
    }
}