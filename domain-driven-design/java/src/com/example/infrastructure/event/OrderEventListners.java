package com.example.infrastructure.event;

import com.example.domain.event.OrderCancelledEvent;
import com.example.domain.event.OrderCreatedEvent;
import com.example.domain.event.OrderItemAddedEvent;
import com.example.domain.event.OrderPaidEvent;
import com.example.domain.event.ProductOutOfStockEvent;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.context.event.EventListener;
import org.springframework.stereotype.Component;

/**
 * 注文関連のドメインイベントリスナー
 */
@Component
public class OrderEventListeners {
    private static final Logger logger = LoggerFactory.getLogger(OrderEventListeners.class);

    /**
     * 注文作成イベントのリスナー
     * @param event 注文作成イベント
     */
    @EventListener
    public void handleOrderCreatedEvent(OrderCreatedEvent event) {
        logger.info("Order created: {}, Customer: {}, Time: {}",
                event.getOrderId(), event.getCustomerId(), event.getOccurredAt());
        
        // ここで注文作成に関連する他のサービスを呼び出す
        // 例: 顧客通知サービス、管理者通知サービスなど
    }

    /**
     * 注文項目追加イベントのリスナー
     * @param event 注文項目追加イベント
     */
    @EventListener
    public void handleOrderItemAddedEvent(OrderItemAddedEvent event) {
        logger.info("Item added to order: {}, Product: {}, Quantity: {}, Time: {}",
                event.getOrderId(), event.getProductId(), event.getQuantity(), event.getOccurredAt());
        
        // ここで注文項目追加に関連する他のサービスを呼び出す
        // 例: 在庫監視サービス、推奨エンジンなど
    }

    /**
     * 注文支払い完了イベントのリスナー
     * @param event 注文支払い完了イベント
     */
    @EventListener
    public void handleOrderPaidEvent(OrderPaidEvent event) {
        logger.info("Order paid: {}, Amount: {}, Time: {}",
                event.getOrderId(), event.getAmount(), event.getOccurredAt());
        
        // ここで注文支払いに関連する他のサービスを呼び出す
        // 例: 会計サービス、出荷サービス、顧客通知サービスなど
    }

    /**
     * 注文キャンセルイベントのリスナー
     * @param event 注文キャンセルイベント
     */
    @EventListener
    public void handleOrderCancelledEvent(OrderCancelledEvent event) {
        logger.info("Order cancelled: {}, Time: {}",
                event.getOrderId(), event.getOccurredAt());
        
        // ここで注文キャンセルに関連する他のサービスを呼び出す
        // 例: 在庫戻しサービス、会計サービス、顧客通知サービスなど
    }

    /**
     * 商品在庫切れイベントのリスナー
     * @param event 商品在庫切れイベント
     */
    @EventListener
    public void handleProductOutOfStockEvent(ProductOutOfStockEvent event) {
        logger.info("Product out of stock: {}, Time: {}",
                event.getProductId(), event.getOccurredAt());
        
        // ここで在庫切れに関連する他のサービスを呼び出す
        // 例: 発注サービス、管理者通知サービス、商品ページ更新サービスなど
    }
}