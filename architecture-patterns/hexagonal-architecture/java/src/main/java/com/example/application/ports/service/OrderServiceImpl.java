package com.example.application.ports.service;

import com.example.application.domain.Order;
import com.example.application.domain.OrderStatus;
import com.example.application.domain.Product;
import com.example.application.ports.input.OrderService;
import com.example.application.ports.output.NotificationService;
import com.example.application.ports.output.OrderRepository;
import com.example.application.ports.output.ProductRepository;
import com.example.application.ports.output.ProductStockManager;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.util.List;
import java.util.Optional;
import java.util.UUID;

@Service
@RequiredArgsConstructor
public class OrderServiceImpl implements OrderService {
    
    private final OrderRepository orderRepository;
    private final ProductRepository productRepository;
    private final ProductStockManager productStockManager;
    private final NotificationService notificationService;

    @Override
    @Transactional
    public Order createOrder() {
        Order order = Order.create();
        return orderRepository.save(order);
    }

    @Override
    @Transactional
    public Order addOrderItem(UUID orderId, UUID productId, int quantity) {
        Order order = orderRepository.findById(orderId)
                .orElseThrow(() -> new IllegalArgumentException("指定された注文が見つかりません: " + orderId));
        
        Product product = productRepository.findById(productId)
                .orElseThrow(() -> new IllegalArgumentException("指定された商品が見つかりません: " + productId));
        
        // 在庫チェック
        if (!productStockManager.hasEnoughStock(productId, quantity)) {
            notificationService.sendStockShortageAlert(product, quantity);
            throw new IllegalStateException("商品の在庫が不足しています: " + productId);
        }
        
        order.addItem(product, quantity);
        return orderRepository.save(order);
    }

    @Override
    @Transactional
    public Order removeOrderItem(UUID orderId, UUID productId) {
        Order order = orderRepository.findById(orderId)
                .orElseThrow(() -> new IllegalArgumentException("指定された注文が見つかりません: " + orderId));
        
        order.removeItem(productId);
        return orderRepository.save(order);
    }

    @Override
    @Transactional
    public Order placeOrder(UUID orderId) {
        Order order = orderRepository.findById(orderId)
                .orElseThrow(() -> new IllegalArgumentException("指定された注文が見つかりません: " + orderId));
        
        // 在庫の予約
        order.getItems().forEach(item -> {
            if (!productStockManager.reserveStock(item.getProduct().getId(), item.getQuantity())) {
                throw new IllegalStateException("商品の在庫が不足しています: " + item.getProduct().getId());
            }
        });
        
        order.place();
        Order placedOrder = orderRepository.save(order);
        
        // 在庫の確定的な減少
        order.getItems().forEach(item -> {
            productStockManager.confirmStockReduction(item.getProduct().getId(), item.getQuantity());
        });
        
        // 注文確定の通知
        notificationService.sendOrderConfirmation(placedOrder);
        
        return placedOrder;
    }

    @Override
    @Transactional
    public Order cancelOrder(UUID orderId) {
        Order order = orderRepository.findById(orderId)
                .orElseThrow(() -> new IllegalArgumentException("指定された注文が見つかりません: " + orderId));
        
        order.cancel();
        Order cancelledOrder = orderRepository.save(order);
        
        // 予約済みの在庫を戻す
        order.getItems().forEach(item -> {
            productStockManager.releaseStock(item.getProduct().getId(), item.getQuantity());
        });
        
        return cancelledOrder;
    }

    @Override
    @Transactional
    public Order completeOrder(UUID orderId) {
        Order order = orderRepository.findById(orderId)
                .orElseThrow(() -> new IllegalArgumentException("指定された注文が見つかりません: " + orderId));
        
        order.complete();
        return orderRepository.save(order);
    }

    @Override
    @Transactional(readOnly = true)
    public Optional<Order> getOrder(UUID id) {
        return orderRepository.findById(id);
    }

    @Override
    @Transactional(readOnly = true)
    public List<Order> getAllOrders() {
        return orderRepository.findAll();
    }

    @Override
    @Transactional(readOnly = true)
    public List<Order> getOrdersByStatus(OrderStatus status) {
        return orderRepository.findByStatus(status);
    }
}