package com.example.domain.services;

import com.example.domain.models.Product;
import jakarta.validation.constraints.NotNull;

import java.math.BigDecimal;

/**
 * 商品ドメインに関連する横断的な処理を提供するサービス
 */
public class ProductDomainService {

    /**
     * 値引き商品の最小価格を設定する
     */
    private static final BigDecimal MINIMUM_DISCOUNTED_PRICE = new BigDecimal("100");

    /**
     * 2つの商品を比較し、同じカテゴリに属しているかを判定する
     * 
     * @param product1 比較する商品1
     * @param product2 比較する商品2
     * @return 同じカテゴリに属している場合はtrue
     */
    public boolean isSameCategory(@NotNull Product product1, @NotNull Product product2) {
        if (product1 == null || product2 == null) {
            throw new IllegalArgumentException("商品はnullにできません");
        }
        
        return product1.getCategory().getId().equals(product2.getCategory().getId());
    }

    /**
     * 商品に与えられた割引率を適用したときの価格を計算する
     * 
     * @param product 対象商品
     * @param discountRate 割引率（0.0〜1.0）
     * @return 割引後の価格
     */
    public BigDecimal calculateDiscountedPrice(@NotNull Product product, double discountRate) {
        if (product == null) {
            throw new IllegalArgumentException("商品はnullにできません");
        }
        
        if (discountRate < 0.0 || discountRate > 1.0) {
            throw new IllegalArgumentException("割引率は0.0から1.0の間である必要があります");
        }
        
        BigDecimal originalPrice = product.getPrice();
        BigDecimal discountAmount = originalPrice.multiply(BigDecimal.valueOf(discountRate));
        BigDecimal discountedPrice = originalPrice.subtract(discountAmount);
        
        // 最低価格を下回る場合は最低価格を返す
        if (discountedPrice.compareTo(MINIMUM_DISCOUNTED_PRICE) < 0) {
            return MINIMUM_DISCOUNTED_PRICE;
        }
        
        return discountedPrice;
    }

    /**
     * 商品が値引き対象かどうかを判定する
     * 
     * @param product 対象商品
     * @return 値引き対象の場合はtrue
     */
    public boolean isEligibleForDiscount(@NotNull Product product) {
        if (product == null) {
            throw new IllegalArgumentException("商品はnullにできません");
        }
        
        // 在庫が多すぎる商品は値引き対象
        boolean hasExcessStock = product.getStockQuantity() > 100;
        
        // 価格が高すぎる商品は値引き対象
        boolean isExpensive = product.getPrice().compareTo(new BigDecimal("10000")) > 0;
        
        return hasExcessStock || isExpensive;
    }
}