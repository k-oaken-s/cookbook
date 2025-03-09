package com.example.domain.services;

import com.example.domain.models.Order;
import com.example.domain.models.OrderItem;
import com.example.domain.models.OrderStatus;
import com.example.domain.models.Product;
import jakarta.validation.constraints.NotNull;

import java.math.BigDecimal;
import java.time.LocalDateTime;
import java.util.List;

/**
 * 注文ドメインに関連する横断的な処理を提供するサービス
 */
public class OrderDomainService {
    
    /**
     * 注文が期限切れかどうかを判定する
     * 
     * @param order 対象の注文
     * @param expirationHours 期限（時間）
     * @return 期限切れの場合はtrue
     */
    public boolean isOrderExpired(@NotNull Order order, int expirationHours) {
        if (order == null) {
            throw new IllegalArgumentException("注文はnullにできません");
        }
        
        if (order.getStatus() != OrderStatus.CREATED) {
            return false; // 作成済み以外のステータスでは期限切れにならない
        }
        
        LocalDateTime expirationTime = order.getCreatedAt().plusHours(expirationHours);
        return LocalDateTime.now().isAfter(expirationTime);
    }
    
    /**
     * 注文に対する税金を計算する
     * 
     * @param order 対象の注文
     * @param taxRate 税率（0.0〜1.0）
     * @return 税金額
     */
    public BigDecimal calculateTax(@NotNull Order order, double taxRate) {
        if (order == null) {
            throw new IllegalArgumentException("注文はnullにできません");
        }
        
        if (taxRate < 0.0 || taxRate > 1.0) {
            throw new IllegalArgumentException("税率は0.0から1.0の間である必要があります");
        }
        
        BigDecimal subtotal = order.calculateTotal();
        return subtotal.multiply(BigDecimal.valueOf(taxRate));
    }
    
    /**
     * 注文に割引を適用する際の割引額を商品ごとに計算する
     * 
     * @param order 対象の注文
     * @param discountRates 商品カテゴリごとの割引率マップ
     * @return 総割引額
     */
    public BigDecimal calculateCategoryBasedDiscount(@NotNull Order order, @NotNull java.util.Map<java.util.UUID, Double> discountRates) {
        if (order == null || discountRates == null) {
            throw new IllegalArgumentException("引数はnullにできません");
        }
        
        BigDecimal totalDiscount = BigDecimal.ZERO;
        
        for (OrderItem item : order.getItems()) {
            Product product = item.getProduct();
            Double discountRate = discountRates.get(product.getCategory().getId());
            
            if (discountRate != null) {
                BigDecimal itemTotal = product.getPrice().multiply(BigDecimal.valueOf(item.getQuantity()));
                BigDecimal itemDiscount = itemTotal.multiply(BigDecimal.valueOf(discountRate));
                totalDiscount = totalDiscount.add(itemDiscount);
            }
        }
        
        return totalDiscount;
    }
    
    /**
     * 注文に含まれる商品の在庫を確認する
     * 
     * @param order 対象の注文
     * @return すべての商品に十分な在庫がある場合はtrue
     */
    public boolean hasEnoughStock(@NotNull Order order) {
        if (order == null) {
            throw new IllegalArgumentException("注文はnullにできません");
        }
        
        for (OrderItem item : order.getItems()) {
            Product product = item.getProduct();
            if (product.getStockQuantity() < item.getQuantity()) {
                return false;
            }
        }
        
        return true;
    }
}