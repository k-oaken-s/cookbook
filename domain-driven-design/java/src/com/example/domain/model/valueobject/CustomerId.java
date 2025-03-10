package com.example.domain.model.valueobject;

import java.util.Objects;
import java.util.UUID;

/**
 * 顧客IDを表す値オブジェクト
 * エンティティの識別子として使用される
 */
public final class CustomerId {
    private final UUID id;

    private CustomerId(UUID id) {
        this.id = id;
    }

    public static CustomerId of(UUID id) {
        Objects.requireNonNull(id, "Customer ID cannot be null");
        return new CustomerId(id);
    }

    public static CustomerId of(String id) {
        return of(UUID.fromString(id));
    }

    public static CustomerId generateNew() {
        return new CustomerId(UUID.randomUUID());
    }

    public UUID getValue() {
        return id;
    }

    @Override
    public boolean equals(Object o) {
        if (this == o) return true;
        if (o == null || getClass() != o.getClass()) return false;
        CustomerId that = (CustomerId) o;
        return Objects.equals(id, that.id);
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