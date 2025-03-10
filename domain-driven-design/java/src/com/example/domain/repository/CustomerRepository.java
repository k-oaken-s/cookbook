package com.example.domain.repository;

import com.example.domain.model.entity.Customer;
import com.example.domain.model.valueobject.CustomerId;

import java.util.List;
import java.util.Optional;

/**
 * 顧客リポジトリのインターフェース
 */
public interface CustomerRepository {
    /**
     * 顧客IDによる顧客の検索
     * @param id 顧客ID
     * @return 顧客のOptional
     */
    Optional<Customer> findById(CustomerId id);

    /**
     * メールアドレスによる顧客の検索
     * @param email メールアドレス
     * @return 顧客のOptional
     */
    Optional<Customer> findByEmail(String email);

    /**
     * 姓名による顧客の検索
     * @param firstName 名
     * @param lastName 姓
     * @return 見つかった顧客のリスト
     */
    List<Customer> findByName(String firstName, String lastName);

    /**
     * アクティブな全ての顧客を取得
     * @return アクティブな顧客のリスト
     */
    List<Customer> findAllActive();

    /**
     * 顧客の保存（新規作成または更新）
     * @param customer 保存する顧客
     * @return 保存された顧客
     */
    Customer save(Customer customer);

    /**
     * 顧客の削除
     * @param id 削除する顧客のID
     */
    void deleteById(CustomerId id);
}