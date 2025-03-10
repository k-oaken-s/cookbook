package com.example.domain.model.valueobject;

import java.util.Objects;

/**
 * 数量を表す値オブジェクト
 */
public final class Quantity {
    private final int value;

    private Quantity(int value) {
        this.value = value;
    }

    public static Quantity of(int value) {
        if (value < 0) {
            throw new IllegalArgumentException("Quantity cannot be negative");
        }
        return new Quantity(value);
    }

    public static Quantity zero() {
        return new Quantity(0);
    }

    // 値オブジェクトの操作は新しいインスタンスを返す
    public Quantity add(Quantity quantity) {
        return new Quantity(this.value + quantity.value);
    }

    public Quantity subtract(Quantity quantity) {
        int result = this.value - quantity.value;
        if (result < 0) {
            throw new IllegalArgumentException("Result cannot be negative");
        }
        return new Quantity(result);
    }

    public Quantity multiply(int multiplier) {
        if (multiplier < 0) {
            throw new IllegalArgumentException("Multiplier cannot be negative");
        }
        return new Quantity(this.value * multiplier);
    }

    public boolean isGreaterThan(Quantity other) {
        return this.value > other.value;
    }

    public boolean isLessThan(Quantity other) {
        return this.value < other.value;
    }

    public boolean isZero() {
        return this.value == 0;
    }

    public int getValue() {
        return value;
    }

    @Override
    public boolean equals(Object o) {
        if (this == o) return true;
        if (o == null || getClass() != o.getClass()) return false;
        Quantity quantity = (Quantity) o;
        return value == quantity.value;
    }

    @Override
    public int hashCode() {
        return Objects.hash(value);
    }

    @Override
    public String toString() {
        return String.valueOf(value);
    }
}