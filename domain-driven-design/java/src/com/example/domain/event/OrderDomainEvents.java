package com.example.domain.event;

import com.example.domain.model.valueobject.CustomerId;
import com.example.domain.model.valueobject.Money;
import com.example.domain.model.valueobject.OrderId;
import com.example.domain.model.valueobject.ProductId;
import com.example.domain.model.valueobject.Quantity;

import java.time.LocalDateTime;

/**
 * 注文作成イベント
 */
public class OrderCreatedEvent {
    private final OrderId orderId;
    private final CustomerId customerId;
    private final LocalDateTime occurredAt;

    public OrderCreatedEvent(OrderId orderId, CustomerId customerId, LocalDateTime occurredAt) {
        this.orderId = orderId;
        this.customerId = customerId;
        this.occurredAt = occurredAt;
    }

    public OrderId getOrderId() {
        return orderId;
    }

    public CustomerId getCustomerId() {
        return customerId;
    }

    public LocalDateTime getOccurredAt() {
        return occurredAt;
    }
}

/**
 * 注文項目追加イベント
 */
public class OrderItemAddedEvent {
    private final OrderId orderId;
    private final ProductId productId;
    private final Quantity quantity;
    private final LocalDateTime occurredAt;

    public OrderItemAddedEvent(OrderId orderId, ProductId productId, Quantity quantity, LocalDateTime occurredAt) {
        this.orderId = orderId;
        this.productId = productId;
        this.quantity = quantity;
        this.occurredAt = occurredAt;
    }

    public OrderId getOrderId() {
        return orderId;
    }

    public ProductId getProductId() {
        return productId;
    }

    public Quantity getQuantity() {
        return quantity;
    }

    public LocalDateTime getOccurredAt() {
        return occurredAt;
    }
}

/**
 * 注文支払い完了イベント
 */
public class OrderPaidEvent {
    private final OrderId orderId;
    private final Money amount;
    private final LocalDateTime occurredAt;

    public OrderPaidEvent(OrderId orderId, Money amount, LocalDateTime occurredAt) {
        this.orderId = orderId;
        this.amount = amount;
        this.occurredAt = occurredAt;
    }

    public OrderId getOrderId() {
        return orderId;
    }

    public Money getAmount() {
        return amount;
    }

    public LocalDateTime getOccurredAt() {
        return occurredAt;
    }
}

/**
 * 注文キャンセルイベント
 */
public class OrderCancelledEvent {
    private final OrderId orderId;
    private final LocalDateTime occurredAt;

    public OrderCancelledEvent(OrderId orderId, LocalDateTime occurredAt) {
        this.orderId = orderId;
        this.occurredAt = occurredAt;
    }

    public OrderId getOrderId() {
        return orderId;
    }

    public LocalDateTime getOccurredAt() {
        return occurredAt;
    }
}

/**
 * 商品在庫切れイベント
 */
public class ProductOutOfStockEvent {
    private final ProductId productId;
    private final LocalDateTime occurredAt;

    public ProductOutOfStockEvent(ProductId productId, LocalDateTime occurredAt) {
        this.productId = productId;
        this.occurredAt = occurredAt;
    }

    public ProductId getProductId() {
        return productId;
    }

    public LocalDateTime getOccurredAt() {
        return occurredAt;
    }
}