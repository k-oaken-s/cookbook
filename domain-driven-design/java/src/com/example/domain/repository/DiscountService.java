package com.example.domain.service;

import com.example.domain.model.aggregate.Order;
import com.example.domain.model.entity.Customer;
import com.example.domain.model.valueobject.Money;

import java.math.BigDecimal;
import java.time.LocalDate;
import java.time.LocalDateTime;
import java.util.Currency;

/**
 * 割引を計算するドメインサービス
 * 複数のエンティティに関連するロジックはドメインサービスに配置する
 */
public class DiscountService {

    /**
     * 注文に対する割引を計算する
     * @param order 対象の注文
     * @param customer 注文した顧客
     * @return 割引後の金額
     */
    public Money calculateDiscount(Order order, Customer customer) {
        Money orderTotal = order.getTotalAmount();
        Money discount = Money.zero(orderTotal.getCurrency());
        
        // 合計金額に基づく割引
        discount = discount.add(calculateVolumeDiscount(orderTotal));
        
        // 会員登録期間に基づく割引
        discount = discount.add(calculateLoyaltyDiscount(customer, orderTotal));
        
        // シーズン割引
        discount = discount.add(calculateSeasonalDiscount(orderTotal));
        
        // 割引額が合計金額を超えないようにする
        if (discount.isGreaterThan(orderTotal)) {
            return orderTotal;
        }
        
        return discount;
    }
    
    /**
     * 合計金額に基づく割引を計算
     * @param total 合計金額
     * @return 割引額
     */
    private Money calculateVolumeDiscount(Money total) {
        Currency currency = total.getCurrency();
        
        // 5000円以上で5%割引
        Money threshold = Money.of(new BigDecimal("5000"), currency);
        if (total.isGreaterThan(threshold)) {
            return total.multiply(0.05);
        }
        
        return Money.zero(currency);
    }
    
    /**
     * 会員登録期間に基づく割引を計算
     * @param customer 顧客
     * @param total 合計金額
     * @return 割引額
     */
    private Money calculateLoyaltyDiscount(Customer customer, Money total) {
        Currency currency = total.getCurrency();
        LocalDateTime registeredAt = customer.getRegisteredAt();
        LocalDateTime now = LocalDateTime.now();
        
        // 1年以上会員の場合は3%割引
        if (registeredAt.plusYears(1).isBefore(now)) {
            return total.multiply(0.03);
        }
        
        return Money.zero(currency);
    }
    
    /**
     * シーズン割引を計算
     * @param total 合計金額
     * @return 割引額
     */
    private Money calculateSeasonalDiscount(Money total) {
        Currency currency = total.getCurrency();
        LocalDate today = LocalDate.now();
        int month = today.getMonthValue();
        
        // 12月は年末セールで10%割引
        if (month == 12) {
            return total.multiply(0.1);
        }
        
        // 特定の月にセール実施
        if (month == 3 || month == 7) {
            return total.multiply(0.08);
        }
        
        return Money.zero(currency);
    }
}