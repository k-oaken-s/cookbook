package com.example.application.services;

import com.example.application.interfaces.*;
import com.example.domain.models.Order;
import com.example.domain.models.OrderStatus;
import com.example.domain.models.Product;
import com.example.domain.services.OrderDomainService;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.math.BigDecimal;
import java.util.List;
import java.util.Map;
import java.util.Optional;
import java.util.UUID;

@Service
@RequiredArgsConstructor
public class OrderServiceImpl implements OrderService {
    
    private final OrderRepository orderRepository;
    private final ProductRepository productRepository;
    private final StockManager stockManager;
    private final NotificationService notificationService;
    private final OrderDomainService orderDomainService;

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
        if (!stockManager.hasEnoughStock(productId, quantity)) {
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
        
        // ドメインサービスを使用して在庫チェック
        if (!orderDomainService.hasEnoughStock(order)) {
            throw new IllegalStateException("一部の商品の在庫が不足しています");
        }
        
        // 在庫の予約
        order.getItems().forEach(item -> {
            if (!stockManager.reserveStock(item.getProduct().getId(), item.getQuantity())) {
                throw new IllegalStateException("商品の在庫が不足しています: " + item.getProduct().getId());
            }
        });
        
        // 注文の確定
        order.place();
        Order placedOrder = orderRepository.save(order);
        
        // 在庫の確定的な減少
        order.getItems().forEach(item -> {
            stockManager.confirmStockReduction(item.getProduct().getId(), item.getQuantity());
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
            stockManager.releaseStock(item.getProduct().getId(), item.getQuantity());
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
    
    @Override
    @Transactional(readOnly = true)
    public BigDecimal calculateTax(UUID orderId, double taxRate) {
        Order order = orderRepository.findById(orderId)
                .orElseThrow(() -> new IllegalArgumentException("指定された注文が見つかりません: " + orderId));
        
        // ドメインサービスを使用して税金を計算
        return orderDomainService.calculateTax(order, taxRate);
    }
    
    @Override
    @Transactional(readOnly = true)
    public BigDecimal applyCategoryBasedDiscount(UUID orderId, Map<UUID, Double> discountRates) {
        Order order = orderRepository.findById(orderId)
                .orElseThrow(() -> new IllegalArgumentException("指定された注文が見つかりません: " + orderId));
        
        // ドメインサービスを使用してカテゴリ別の割引を計算
        return orderDomainService.calculateCategoryBasedDiscount(order, discountRates);
    }
}