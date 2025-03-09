package com.example.domain.models;

import lombok.Getter;

import java.math.BigDecimal;
import java.time.LocalDateTime;
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;
import java.util.UUID;

@Getter
public class Order {
    private final UUID id;
    private final List<OrderItem> items;
    private OrderStatus status;
    private final LocalDateTime createdAt;
    private LocalDateTime updatedAt;

    public Order(UUID id, List<OrderItem> items, OrderStatus status, LocalDateTime createdAt, LocalDateTime updatedAt) {
        this.id = id;
        this.items = new ArrayList<>(items);
        this.status = status;
        this.createdAt = createdAt;
        this.updatedAt = updatedAt;
    }

    public static Order create() {
        return new Order(
                UUID.randomUUID(),
                new ArrayList<>(),
                OrderStatus.CREATED,
                LocalDateTime.now(),
                LocalDateTime.now()
        );
    }

    public void addItem(Product product, int quantity) {
        if (status != OrderStatus.CREATED) {
            throw new IllegalStateException("注文項目は作成済みステータスの時のみ追加できます");
        }
        if (quantity <= 0) {
            throw new IllegalArgumentException("数量は正の数である必要があります");
        }

        // 既存の項目があれば数量を更新
        for (OrderItem item : items) {
            if (item.getProduct().getId().equals(product.getId())) {
                item.updateQuantity(item.getQuantity() + quantity);
                this.updatedAt = LocalDateTime.now();
                return;
            }
        }

        // 新しい項目を追加
        items.add(new OrderItem(UUID.randomUUID(), product, quantity));
        this.updatedAt = LocalDateTime.now();
    }

    public void removeItem(UUID productId) {
        if (status != OrderStatus.CREATED) {
            throw new IllegalStateException("注文項目は作成済みステータスの時のみ削除できます");
        }

        items.removeIf(item -> item.getProduct().getId().equals(productId));
        this.updatedAt = LocalDateTime.now();
    }

    public void place() {
        if (status != OrderStatus.CREATED) {
            throw new IllegalStateException("注文は作成済みステータスの時のみ確定できます");
        }
        if (items.isEmpty()) {
            throw new IllegalStateException("注文項目が空です");
        }

        this.status = OrderStatus.PLACED;
        this.updatedAt = LocalDateTime.now();
    }

    public void cancel() {
        if (status != OrderStatus.PLACED) {
            throw new IllegalStateException("注文は確定済みステータスの時のみキャンセルできます");
        }

        this.status = OrderStatus.CANCELLED;
        this.updatedAt = LocalDateTime.now();
    }

    public void complete() {
        if (status != OrderStatus.PLACED) {
            throw new IllegalStateException("注文は確定済みステータスの時のみ完了できます");
        }

        this.status = OrderStatus.COMPLETED;
        this.updatedAt = LocalDateTime.now();
    }

    public BigDecimal calculateTotal() {
        return items.stream()
                .map(OrderItem::calculateSubtotal)
                .reduce(BigDecimal.ZERO, BigDecimal::add);
    }

    public List<OrderItem> getItems() {
        return Collections.unmodifiableList(items);
    }
}