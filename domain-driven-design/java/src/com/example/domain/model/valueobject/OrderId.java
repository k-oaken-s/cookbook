package com.example.domain.model.valueobject;

import java.util.Objects;
import java.util.UUID;

/**
 * 注文IDを表す値オブジェクト
 * エンティティの識別子として使用される
 */
public final class OrderId {
    private final UUID id;

    private OrderId(UUID id) {
        this.id = id;
    }

    public static OrderId of(UUID id) {
        Objects.requireNonNull(id, "Order ID cannot be null");
        return new OrderId(id);
    }

    public static OrderId of(String id) {
        return of(UUID.fromString(id));
    }

    public static OrderId generateNew() {
        return new OrderId(UUID.randomUUID());
    }

    public UUID getValue() {
        return id;
    }

    @Override
    public boolean equals(Object o) {
        if (this == o) return true;
        if (o == null || getClass() != o.getClass()) return false;
        OrderId orderId = (OrderId) o;
        return Objects.equals(id, orderId.id);
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