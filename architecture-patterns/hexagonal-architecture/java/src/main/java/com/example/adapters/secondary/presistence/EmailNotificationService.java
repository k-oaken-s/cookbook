package com.example.adapters.secondary.services;

import com.example.application.domain.Order;
import com.example.application.domain.Product;
import com.example.application.ports.output.NotificationService;
import lombok.extern.slf4j.Slf4j;
import org.springframework.stereotype.Component;

@Component
@Slf4j
public class EmailNotificationService implements NotificationService {
    
    @Override
    public void sendOrderConfirmation(Order order) {
        log.info("注文確認メールを送信しました。注文ID: {}", order.getId());
        // 実際のメール送信ロジックがここに入る
    }
    
    @Override
    public void sendStockShortageAlert(Product product, int requiredQuantity) {
        log.warn("在庫不足アラート: 商品「{}」の現在の在庫数は{}ですが、{}個が要求されました。",
                product.getName(), product.getStockQuantity(), requiredQuantity);
        // 実際のアラート送信ロジックがここに入る
    }
    
    @Override
    public void sendLowStockNotification(Product product, int threshold) {
        log.info("在庫少量通知: 商品「{}」の在庫数が{}個になりました（閾値: {}個）",
                product.getName(), product.getStockQuantity(), threshold);
        // 実際の通知送信ロジックがここに入る
    }
}