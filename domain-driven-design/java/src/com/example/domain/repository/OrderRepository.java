package com.example.domain.repository;

import com.example.domain.model.aggregate.Order;
import com.example.domain.model.valueobject.CustomerId;
import com.example.domain.model.valueobject.OrderId;
import com.example.domain.model.valueobject.OrderStatus;

import java.time.LocalDateTime;
import java.util.List;
import java.util.Optional;

/**
 * 注文リポジトリのインターフェース
 * 集約ルートに対するリポジトリ
 */
public interface OrderRepository {
    /**
     * 注文IDによる注文の検索
     * @param id 注文ID
     * @return 注文のOptional
     */
    Optional<Order> findById(OrderId id);

    /**
     * 顧客IDによる注文の検索
     * @param customerId 顧客ID
     * @return 見つかった注文のリスト
     */
    List<Order> findByCustomerId(CustomerId customerId);

    /**
     * 注文ステータスによる注文の検索
     * @param status 注文ステータス
     * @return 見つかった注文のリスト
     */
    List<Order> findByStatus(OrderStatus status);

    /**
     * 期間内に作成された注文の検索
     * @param startDate 開始日時
     * @param endDate 終了日時
     * @return 見つかった注文のリスト
     */
    List<Order> findByCreatedAtBetween(LocalDateTime startDate, LocalDateTime endDate);

    /**
     * 注文の保存（新規作成または更新）
     * @param order 保存する注文
     * @return 保存された注文
     */
    Order save(Order order);

    /**
     * 注文の削除
     * @param id 削除する注文のID
     */
    void deleteById(OrderId id);
}