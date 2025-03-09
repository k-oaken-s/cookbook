package com.example.application.interfaces;

import com.example.domain.models.Order;
import com.example.domain.models.OrderStatus;

import java.util.List;
import java.util.Optional;
import java.util.UUID;

/**
 * 注文の永続化を担当するリポジトリインターフェース
 */
public interface OrderRepository {
    
    /**
     * 指定されたIDの注文を取得する
     *
     * @param id 注文ID
     * @return 注文のOptional
     */
    Optional<Order> findById(UUID id);
    
    /**
     * すべての注文を取得する
     *
     * @return 注文のリスト
     */
    List<Order> findAll();
    
    /**
     * 注文を保存する
     *
     * @param order 保存する注文
     * @return 保存された注文
     */
    Order save(Order order);
    
    /**
     * 指定されたIDの注文を削除する
     *
     * @param id 削除する注文のID
     */
    void deleteById(UUID id);
    
    /**
     * 特定のステータスに一致する注文をすべて取得する
     *
     * @param status 注文ステータス
     * @return 注文のリスト
     */
    List<Order> findByStatus(OrderStatus status);
}