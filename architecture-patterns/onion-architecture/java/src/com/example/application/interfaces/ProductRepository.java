package com.example.application.interfaces;

import com.example.domain.models.Category;
import com.example.domain.models.Product;

import java.util.List;
import java.util.Optional;
import java.util.UUID;

/**
 * 商品の永続化を担当するリポジトリインターフェース
 */
public interface ProductRepository {
    
    /**
     * 指定されたIDの商品を取得する
     *
     * @param id 商品ID
     * @return 商品のOptional
     */
    Optional<Product> findById(UUID id);
    
    /**
     * すべての商品を取得する
     *
     * @return 商品のリスト
     */
    List<Product> findAll();
    
    /**
     * 特定のカテゴリに属する商品をすべて取得する
     *
     * @param categoryId カテゴリID
     * @return 商品のリスト
     */
    List<Product> findByCategory(UUID categoryId);
    
    /**
     * 商品を保存する
     *
     * @param product 保存する商品
     * @return 保存された商品
     */
    Product save(Product product);
    
    /**
     * 指定されたIDの商品を削除する
     *
     * @param id 削除する商品のID
     */
    void deleteById(UUID id);
    
    /**
     * 商品名に指定されたキーワードを含む商品を検索する
     *
     * @param keyword 検索キーワード
     * @return 検索結果の商品リスト
     */
    List<Product> searchByName(String keyword);
}