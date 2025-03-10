package com.example.domain.model.aggregate;

import com.example.domain.event.OrderCancelledEvent;
import com.example.domain.event.OrderCreatedEvent;
import com.example.domain.event.OrderItemAddedEvent;
import com.example.domain.event.OrderPaidEvent;
import com.example.domain.model.entity.OrderItem;
import com.example.domain.model.valueobject.*;

import java.time.LocalDateTime;
import java.util.*;

/**
 * 注文を表す集約ルート
 * 注文に関連する全てのエンティティと値オブジェクトの一貫性を保証する
 */
public class Order {
    private final OrderId id; // 集約ルートの識別子
    private final CustomerId customerId; // 関連する顧客ID
    private Address shippingAddress;
    private Address billingAddress;
    private OrderStatus status;
    private final List<OrderItem> orderItems;
    private Money totalAmount;
    private final LocalDateTime createdAt;
    private LocalDateTime lastModifiedAt;
    private LocalDateTime paidAt;
    private LocalDateTime shippedAt;
    private LocalDateTime cancelledAt;
    
    // ドメインイベントのリスト
    private final List<Object> domainEvents;

    // プライベートコンストラクタ - ファクトリメソッド経由で生成する
    private Order(OrderId id, CustomerId customerId, Address shippingAddress, Address billingAddress) {
        this.id = id;
        this.customerId = customerId;
        this.shippingAddress = shippingAddress;
        this.billingAddress = billingAddress;
        this.status = OrderStatus.CREATED;
        this.orderItems = new ArrayList<>();
        this.totalAmount = Money.zero(Currency.getInstance("JPY")); // デフォルト通貨
        this.createdAt = LocalDateTime.now();
        this.lastModifiedAt = LocalDateTime.now();
        this.domainEvents = new ArrayList<>();
    }

    // ファクトリメソッド - 新規注文作成
    public static Order create(CustomerId customerId, Address shippingAddress, Address billingAddress) {
        Objects.requireNonNull(customerId, "Customer ID cannot be null");
        Objects.requireNonNull(shippingAddress, "Shipping address cannot be null");
        Objects.requireNonNull(billingAddress, "Billing address cannot be null");

        OrderId orderId = OrderId.generateNew();
        Order order = new Order(orderId, customerId, shippingAddress, billingAddress);
        
        // ドメインイベントを登録
        order.domainEvents.add(new OrderCreatedEvent(orderId, customerId, LocalDateTime.now()));
        
        return order;
    }

    // 永続化からの復元用ファクトリメソッド
    public static Order reconstitute(OrderId id, CustomerId customerId, Address shippingAddress, Address billingAddress,
                                   OrderStatus status, List<OrderItem> orderItems, Money totalAmount,
                                   LocalDateTime createdAt, LocalDateTime lastModifiedAt,
                                   LocalDateTime paidAt, LocalDateTime shippedAt, LocalDateTime cancelledAt) {
        Order order = new Order(id, customerId, shippingAddress, billingAddress);
        order.status = status;
        order.orderItems.addAll(orderItems);
        order.totalAmount = totalAmount;
        order.lastModifiedAt = lastModifiedAt;
        order.paidAt = paidAt;
        order.shippedAt = shippedAt;
        order.cancelledAt = cancelledAt;
        return order;
    }

    // ドメインロジック - 注文項目の追加
    public void addOrderItem(ProductId productId, String productName, Money unitPrice, Quantity quantity) {
        // 注文が編集可能か確認
        if (!status.isEditable()) {
            throw new IllegalStateException("Cannot modify order in status: " + status);
        }

        // 既存の注文項目があるか確認
        Optional<OrderItem> existingItem = findOrderItemByProductId(productId);
        
        if (existingItem.isPresent()) {
            // 既存の注文項目がある場合は数量を更新
            OrderItem item = existingItem.get();
            item.updateQuantity(item.getQuantity().add(quantity));
        } else {
            // 新しい注文項目を作成
            OrderItem newItem = OrderItem.create(productId, productName, unitPrice, quantity);
            orderItems.add(newItem);
            
            // ドメインイベントを登録
            domainEvents.add(new OrderItemAddedEvent(id, productId, quantity, LocalDateTime.now()));
        }

        // 合計金額を再計算
        recalculateTotalAmount();
        this.lastModifiedAt = LocalDateTime.now();
    }

    // 注文項目を製品IDで検索
    private Optional<OrderItem> findOrderItemByProductId(ProductId productId) {
        return orderItems.stream()
                .filter(item -> item.getProductId().equals(productId))
                .findFirst();
    }

    // 注文項目を削除
    public void removeOrderItem(UUID orderItemId) {
        // 注文が編集可能か確認
        if (!status.isEditable()) {
            throw new IllegalStateException("Cannot modify order in status: " + status);
        }

        boolean removed = orderItems.removeIf(item -> item.getId().equals(orderItemId));
        
        if (!removed) {
            throw new IllegalArgumentException("Order item not found with ID: " + orderItemId);
        }

        // 注文項目が空になった場合はエラー
        if (orderItems.isEmpty()) {
            throw new IllegalStateException("Order must have at least one item");
        }

        // 合計金額を再計算
        recalculateTotalAmount();
        this.lastModifiedAt = LocalDateTime.now();
    }

    // 注文項目の数量を更新
    public void updateOrderItemQuantity(UUID orderItemId, Quantity newQuantity) {
        // 注文が編集可能か確認
        if (!status.isEditable()) {
            throw new IllegalStateException("Cannot modify order in status: " + status);
        }

        // 注文項目を検索して更新
        OrderItem item = orderItems.stream()
                .filter(i -> i.getId().equals(orderItemId))
                .findFirst()
                .orElseThrow(() -> new IllegalArgumentException("Order item not found with ID: " + orderItemId));

        item.updateQuantity(newQuantity);

        // 合計金額を再計算
        recalculateTotalAmount();
        this.lastModifiedAt = LocalDateTime.now();
    }

    // 配送先住所を更新
    public void updateShippingAddress(Address newAddress) {
        Objects.requireNonNull(newAddress, "Shipping address cannot be null");
        
        // 注文が編集可能か確認
        if (!status.isEditable()) {
            throw new IllegalStateException("Cannot modify order in status: " + status);
        }

        this.shippingAddress = newAddress;
        this.lastModifiedAt = LocalDateTime.now();
    }

    // 請求先住所を更新
    public void updateBillingAddress(Address newAddress) {
        Objects.requireNonNull(newAddress, "Billing address cannot be null");
        
        // 注文が編集可能か確認
        if (!status.isEditable()) {
            throw new IllegalStateException("Cannot modify order in status: " + status);
        }

        this.billingAddress = newAddress;
        this.lastModifiedAt = LocalDateTime.now();
    }

    // 注文をキャンセル
    public void cancel() {
        // キャンセル可能か確認
        if (!status.isCancellable()) {
            throw new IllegalStateException("Cannot cancel order in status: " + status);
        }

        this.status = OrderStatus.CANCELLED;
        this.cancelledAt = LocalDateTime.now();
        this.lastModifiedAt = LocalDateTime.now();
        
        // ドメインイベントを登録
        domainEvents.add(new OrderCancelledEvent(id, LocalDateTime.now()));
    }

    // 注文の支払い処理
    public void markAsPaid() {
        if (status != OrderStatus.CREATED && status != OrderStatus.PENDING_PAYMENT) {
            throw new IllegalStateException("Cannot mark as paid in status: " + status);
        }

        this.status = OrderStatus.PAID;
        this.paidAt = LocalDateTime.now();
        this.lastModifiedAt = LocalDateTime.now();
        
        // ドメインイベントを登録
        domainEvents.add(new OrderPaidEvent(id, totalAmount, LocalDateTime.now()));
    }

    // 注文の発送処理
    public void markAsShipped() {
        if (!status.isShippable()) {
            throw new IllegalStateException("Cannot mark as shipped in status: " + status);
        }

        this.status = OrderStatus.SHIPPED;
        this.shippedAt = LocalDateTime.now();
        this.lastModifiedAt = LocalDateTime.now();
    }

    // 注文の配達完了処理
    public void markAsDelivered() {
        if (status != OrderStatus.SHIPPED) {
            throw new IllegalStateException("Cannot mark as delivered in status: " + status);
        }

        this.status = OrderStatus.DELIVERED;
        this.lastModifiedAt = LocalDateTime.now();
    }

    // 合計金額を再計算
    private void recalculateTotalAmount() {
        // 最初の注文項目から通貨を取得
        if (orderItems.isEmpty()) {
            this.totalAmount = Money.zero(Currency.getInstance("JPY"));
            return;
        }

        Currency currency = orderItems.get(0).getUnitPrice().getCurrency();
        Money total = Money.zero(currency);

        for (OrderItem item : orderItems) {
            total = total.add(item.calculateTotalPrice());
        }

        this.totalAmount = total;
    }

    // ドメインイベントの取得と消去
    public List<Object> getDomainEvents() {
        return new ArrayList<>(domainEvents);
    }

    public void clearDomainEvents() {
        domainEvents.clear();
    }

    // ゲッター
    public OrderId getId() {
        return id;
    }

    public CustomerId getCustomerId() {
        return customerId;
    }

    public Address getShippingAddress() {
        return shippingAddress;
    }

    public Address getBillingAddress() {
        return billingAddress;
    }

    public OrderStatus getStatus() {
        return status;
    }

    public List<OrderItem> getOrderItems() {
        return Collections.unmodifiableList(orderItems);
    }

    public Money getTotalAmount() {
        return totalAmount;
    }

    public LocalDateTime getCreatedAt() {
        return createdAt;
    }

    public LocalDateTime getLastModifiedAt() {
        return lastModifiedAt;
    }

    public LocalDateTime getPaidAt() {
        return paidAt;
    }

    public LocalDateTime getShippedAt() {
        return shippedAt;
    }

    public LocalDateTime getCancelledAt() {
        return cancelledAt;
    }

    // エンティティの等価性は識別子によって判断
    @Override
    public boolean equals(Object o) {
        if (this == o) return true;
        if (o == null || getClass() != o.getClass()) return false;
        Order order = (Order) o;
        return Objects.equals(id, order.id);
    }

    @Override
    public int hashCode() {
        return Objects.hash(id);
    }

    @Override
    public String toString() {
        return "Order{" +
                "id=" + id +
                ", customerId=" + customerId +
                ", status=" + status +
                ", totalAmount=" + totalAmount +
                ", itemCount=" + orderItems.size() +
                '}';
    }
}