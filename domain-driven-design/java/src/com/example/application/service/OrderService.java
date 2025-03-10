package com.example.application.service;

import com.example.domain.event.DomainEventPublisher;
import com.example.domain.model.aggregate.Order;
import com.example.domain.model.entity.Customer;
import com.example.domain.model.valueobject.*;
import com.example.domain.repository.CustomerRepository;
import com.example.domain.repository.OrderRepository;
import com.example.domain.repository.ProductRepository;
import com.example.domain.service.DiscountService;
import com.example.domain.service.InventoryService;

import java.util.List;
import java.util.Optional;
import java.util.UUID;

/**
 * 注文に関するアプリケーションサービス
 * ユースケースの実装を担当
 */
public class OrderService {
    private final OrderRepository orderRepository;
    private final CustomerRepository customerRepository;
    private final ProductRepository productRepository;
    private final InventoryService inventoryService;
    private final DiscountService discountService;
    private final DomainEventPublisher eventPublisher;

    public OrderService(OrderRepository orderRepository, 
                       CustomerRepository customerRepository,
                       ProductRepository productRepository,
                       InventoryService inventoryService,
                       DiscountService discountService,
                       DomainEventPublisher eventPublisher) {
        this.orderRepository = orderRepository;
        this.customerRepository = customerRepository;
        this.productRepository = productRepository;
        this.inventoryService = inventoryService;
        this.discountService = discountService;
        this.eventPublisher = eventPublisher;
    }

    /**
     * 新規注文を作成する
     * @param customerId 顧客ID
     * @param shippingAddress 配送先住所
     * @param billingAddress 請求先住所
     * @return 作成された注文のID
     * @throws IllegalArgumentException 顧客が見つからない場合
     */
    public OrderId createOrder(CustomerId customerId, Address shippingAddress, Address billingAddress) {
        // 顧客の存在確認
        Customer customer = customerRepository.findById(customerId)
                .orElseThrow(() -> new IllegalArgumentException("Customer not found: " + customerId));
        
        if (!customer.isActive()) {
            throw new IllegalArgumentException("Customer is not active: " + customerId);
        }
        
        // 注文の作成
        Order order = Order.create(customerId, shippingAddress, billingAddress);
        orderRepository.save(order);
        
        // ドメインイベントの発行
        order.getDomainEvents().forEach(eventPublisher::publish);
        order.clearDomainEvents();
        
        return order.getId();
    }
    
    /**
     * 注文に商品を追加する
     * @param orderId 注文ID
     * @param productId 商品ID
     * @param quantity 数量
     * @throws IllegalArgumentException 注文または商品が見つからない場合
     * @throws IllegalStateException 商品が在庫不足の場合
     */
    public void addOrderItem(OrderId orderId, ProductId productId, Quantity quantity) {
        // 注文の取得
        Order order = orderRepository.findById(orderId)
                .orElseThrow(() -> new IllegalArgumentException("Order not found: " + orderId));
        
        // 商品の取得
        var product = productRepository.findById(productId)
                .orElseThrow(() -> new IllegalArgumentException("Product not found: " + productId));
        
        // 在庫チェック
        if (!product.hasEnoughStock(quantity)) {
            throw new IllegalStateException("Not enough stock for product: " + productId);
        }
        
        // 注文項目の追加
        order.addOrderItem(productId, product.getName(), product.getPrice(), quantity);
        orderRepository.save(order);
        
        // ドメインイベントの発行
        order.getDomainEvents().forEach(eventPublisher::publish);
        order.clearDomainEvents();
    }
    
    /**
     * 注文から商品を削除する
     * @param orderId 注文ID
     * @param orderItemId 注文項目ID
     * @throws IllegalArgumentException 注文または注文項目が見つからない場合
     */
    public void removeOrderItem(OrderId orderId, UUID orderItemId) {
        // 注文の取得
        Order order = orderRepository.findById(orderId)
                .orElseThrow(() -> new IllegalArgumentException("Order not found: " + orderId));
        
        // 注文項目の削除
        order.removeOrderItem(orderItemId);
        orderRepository.save(order);
    }
    
    /**
     * 注文の支払い処理を行う
     * @param orderId 注文ID
     * @throws IllegalArgumentException 注文が見つからない場合
     * @throws IllegalStateException 注文状態が不正な場合
     */
    public void payOrder(OrderId orderId) {
        // 注文の取得
        Order order = orderRepository.findById(orderId)
                .orElseThrow(() -> new IllegalArgumentException("Order not found: " + orderId));
        
        // 顧客の取得
        Customer customer = customerRepository.findById(order.getCustomerId())
                .orElseThrow(() -> new IllegalArgumentException("Customer not found: " + order.getCustomerId()));
        
        // 在庫の確認
        List<ProductId> outOfStockProducts = inventoryService.checkInventoryForOrder(order);
        if (!outOfStockProducts.isEmpty()) {
            throw new IllegalStateException("Products out of stock: " + outOfStockProducts);
        }
        
        // 割引計算
        Money discount = discountService.calculateDiscount(order, customer);
        // 割引処理は実装省略
        
        // 支払い処理（実際にはここで外部決済サービスを呼び出し）
        
        // 在庫を減らす
        inventoryService.reduceInventoryForOrder(order);
        
        // 注文を支払い済みにする
        order.markAsPaid();
        orderRepository.save(order);
        
        // ドメインイベントの発行
        order.getDomainEvents().forEach(eventPublisher::publish);
        order.clearDomainEvents();
    }
    
    /**
     * 注文のキャンセル処理を行う
     * @param orderId 注文ID
     * @throws IllegalArgumentException 注文が見つからない場合
     * @throws IllegalStateException 注文状態が不正な場合
     */
    public void cancelOrder(OrderId orderId) {
        // 注文の取得
        Order order = orderRepository.findById(orderId)
                .orElseThrow(() -> new IllegalArgumentException("Order not found: " + orderId));
        
        // 注文のキャンセル
        order.cancel();
        
        // 支払い済みの場合は在庫を戻す
        if (order.getStatus() == OrderStatus.PAID || order.getStatus() == OrderStatus.PROCESSING) {
            inventoryService.restoreInventoryForOrder(order);
        }
        
        orderRepository.save(order);
        
        // ドメインイベントの発行
        order.getDomainEvents().forEach(eventPublisher::publish);
        order.clearDomainEvents();
    }
    
    /**
     * 注文の発送処理を行う
     * @param orderId 注文ID
     * @throws IllegalArgumentException 注文が見つからない場合
     * @throws IllegalStateException 注文状態が不正な場合
     */
    public void shipOrder(OrderId orderId) {
        // 注文の取得
        Order order = orderRepository.findById(orderId)
                .orElseThrow(() -> new IllegalArgumentException("Order not found: " + orderId));
        
        // 注文の発送
        order.markAsShipped();
        orderRepository.save(order);
    }
    
    /**
     * 注文の配達完了処理を行う
     * @param orderId 注文ID
     * @throws IllegalArgumentException 注文が見つからない場合
     * @throws IllegalStateException 注文状態が不正な場合
     */
    public void deliverOrder(OrderId orderId) {
        // 注文の取得
        Order order = orderRepository.findById(orderId)
                .orElseThrow(() -> new IllegalArgumentException("Order not found: " + orderId));
        
        // 注文の配達完了
        order.markAsDelivered();
        orderRepository.save(order);
    }
    
    /**
     * 注文を検索する
     * @param orderId 注文ID
     * @return 注文のオプショナル
     */
    public Optional<Order> findOrder(OrderId orderId) {
        return orderRepository.findById(orderId);
    }
    
    /**
     * 顧客の注文履歴を取得する
     * @param customerId 顧客ID
     * @return 注文のリスト
     */
    public List<Order> findOrdersByCustomerId(CustomerId customerId) {
        return orderRepository.findByCustomerId(customerId);
    }
    
    /**
     * 特定の状態の注文を取得する
     * @param status 注文の状態
     * @return 注文のリスト
     */
    public List<Order> findOrdersByStatus(OrderStatus status) {
        return orderRepository.findByStatus(status);
    }
}