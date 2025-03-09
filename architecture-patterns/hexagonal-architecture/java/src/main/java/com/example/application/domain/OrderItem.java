package com.example.application.domain;

import lombok.Getter;

import java.math.BigDecimal;
import java.util.UUID;

@Getter
public class OrderItem {
    private final UUID id;
    private final Product product;
    private int quantity;

    public OrderItem(UUID id, Product product, int quantity) {
        if (product == null) {
            throw new IllegalArgumentException("商品は必須です");
        }
        if (quantity <= 0) {
            throw new IllegalArgumentException("数量は正の数である必要があります");
        }

        this.id = id;
        this.product = product;
        this.quantity = quantity;
    }

    public void updateQuantity(int quantity) {
        if (quantity <= 0) {
            throw new IllegalArgumentException("数量は正の数である必要があります");
        }
        this.quantity = quantity;
    }

    public BigDecimal calculateSubtotal() {
        return product.getPrice().multiply(BigDecimal.valueOf(quantity));
    }
}