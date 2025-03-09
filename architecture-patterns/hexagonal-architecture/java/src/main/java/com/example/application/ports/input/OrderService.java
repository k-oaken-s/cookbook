package com.example.application.ports.input;

import com.example.application.domain.Order;
import com.example.application.domain.OrderStatus;

import java.util.List;
import java.util.Optional;
import java.util.UUID;

/**
 * 注文管理に関するユースケースを定義する入力ポート（プライマリポート）
 */
public interface OrderService {
    
    /**
     * 新しい注文を作成する
     * 
     * @return 作成された注文
     */
    Order createOrder();
    
    /**
     * 注文に商品を追加する
     * 
     * @param orderId 注文ID
     * @param productId 商品ID
     * @param quantity 数量
     * @return 更新された注文
     */
    Order addOrderItem(UUID orderId, UUID productId, int quantity);
    
    /**
     * 注文から商品を削除する
     * 
     * @param orderId 注文ID
     * @param productId 商品ID
     * @return 更新された注文
     */
    Order removeOrderItem(UUID orderId, UUID productId);
    
    /**
     * 注文を確定する
     * 
     * @param orderId 注文ID
     * @return 確定された注文
     */
    Order placeOrder(UUID orderId);
    
    /**
     * 注文をキャンセルする
     * 
     * @param orderId 注文ID
     * @return キャンセルされた注文
     */
    Order cancelOrder(UUID orderId);
    
    /**
     * 注文を完了する
     * 
     * @param orderId 注文ID
     * @return 完了した注文
     */
    Order completeOrder(UUID orderId);
    
    /**
     * IDで注文を取得する
     * 
     * @param id 注文ID
     * @return 注文（存在しない場合はEmpty）
     */
    Optional<Order> getOrder(UUID id);
    
    /**
     * すべての注文を取得する
     * 
     * @return 注文リスト
     */
    List<Order> getAllOrders();
    
    /**
     * 特定のステータスの注文をすべて取得する
     * 
     * @param status 注文ステータス
     * @return 注文リスト
     */
    List<Order> getOrdersByStatus(OrderStatus status);
}