package com.example.application.interfaces;

import com.example.domain.models.Order;
import com.example.domain.models.Product;

/**
 * 通知の送信を担当するインターフェース
 */
public interface NotificationService {
    
    /**
     * 注文確定の通知を送信する
     *
     * @param order 確定された注文
     */
    void sendOrderConfirmation(Order order);
    
    /**
     * 在庫不足の通知を送信する
     *
     * @param product 在庫不足の商品
     * @param requiredQuantity 要求された数量
     */
    void sendStockShortageAlert(Product product, int requiredQuantity);
    
    /**
     * 商品の在庫が少なくなった通知を送信する
     *
     * @param product 在庫が少ない商品
     * @param threshold 閾値
     */
    void sendLowStockNotification(Product product, int threshold);
}