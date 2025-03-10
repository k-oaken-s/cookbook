package com.example.domain.service;

import com.example.domain.event.DomainEventPublisher;
import com.example.domain.event.ProductOutOfStockEvent;
import com.example.domain.model.aggregate.Order;
import com.example.domain.model.entity.OrderItem;
import com.example.domain.model.entity.Product;
import com.example.domain.model.valueobject.ProductId;
import com.example.domain.model.valueobject.Quantity;
import com.example.domain.repository.ProductRepository;

import java.time.LocalDateTime;
import java.util.ArrayList;
import java.util.List;
import java.util.Optional;

/**
 * 在庫を管理するドメインサービス
 */
public class InventoryService {
    private final ProductRepository productRepository;
    private final DomainEventPublisher eventPublisher;

    public InventoryService(ProductRepository productRepository, DomainEventPublisher eventPublisher) {
        this.productRepository = productRepository;
        this.eventPublisher = eventPublisher;
    }

    /**
     * 注文に含まれる全ての商品が在庫十分かチェックする
     * @param order 対象の注文
     * @return 在庫が不足している商品のID
     */
    public List<ProductId> checkInventoryForOrder(Order order) {
        List<ProductId> outOfStockProducts = new ArrayList<>();
        
        for (OrderItem item : order.getOrderItems()) {
            ProductId productId = item.getProductId();
            Quantity requiredQuantity = item.getQuantity();
            
            Optional<Product> productOpt = productRepository.findById(productId);
            
            if (productOpt.isEmpty()) {
                outOfStockProducts.add(productId);
                continue;
            }
            
            Product product = productOpt.get();
            if (!product.hasEnoughStock(requiredQuantity)) {
                outOfStockProducts.add(productId);
                // 在庫切れイベントを発行
                eventPublisher.publish(new ProductOutOfStockEvent(productId, LocalDateTime.now()));
            }
        }
        
        return outOfStockProducts;
    }
    
    /**
     * 注文確定時に在庫を減らす
     * @param order 確定した注文
     * @throws IllegalStateException 在庫が足りない場合
     */
    public void reduceInventoryForOrder(Order order) {
        List<ProductId> outOfStockProducts = checkInventoryForOrder(order);
        
        if (!outOfStockProducts.isEmpty()) {
            throw new IllegalStateException("Products out of stock: " + outOfStockProducts);
        }
        
        for (OrderItem item : order.getOrderItems()) {
            ProductId productId = item.getProductId();
            Quantity quantity = item.getQuantity();
            
            Product product = productRepository.findById(productId)
                    .orElseThrow(() -> new IllegalStateException("Product not found: " + productId));
            
            product.reduceStock(quantity);
            productRepository.save(product);
        }
    }
    
    /**
     * 注文キャンセル時に在庫を戻す
     * @param order キャンセルされた注文
     */
    public void restoreInventoryForOrder(Order order) {
        for (OrderItem item : order.getOrderItems()) {
            ProductId productId = item.getProductId();
            Quantity quantity = item.getQuantity();
            
            Optional<Product> productOpt = productRepository.findById(productId);
            
            if (productOpt.isPresent()) {
                Product product = productOpt.get();
                product.addStock(quantity);
                productRepository.save(product);
            }
        }
    }
    
    /**
     * 商品の在庫が閾値を下回っているかチェック
     * @param product チェックする商品
     * @param threshold 閾値
     * @return 閾値を下回っていればtrue
     */
    public boolean isStockBelowThreshold(Product product, Quantity threshold) {
        return product.getStockQuantity().isLessThan(threshold);
    }
}