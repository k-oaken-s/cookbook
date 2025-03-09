package com.example.application.ports.input;

import com.example.application.domain.Category;
import com.example.application.domain.Product;

import java.math.BigDecimal;
import java.util.List;
import java.util.Optional;
import java.util.UUID;

/**
 * 商品管理に関するユースケースを定義する入力ポート（プライマリポート）
 */
public interface ProductService {
    
    /**
     * 新しい商品を作成する
     * 
     * @param name 商品名
     * @param description 商品説明
     * @param price 価格
     * @param stockQuantity 在庫数
     * @param categoryId カテゴリID
     * @return 作成された商品
     */
    Product createProduct(String name, String description, BigDecimal price, int stockQuantity, UUID categoryId);
    
    /**
     * IDで商品を取得する
     * 
     * @param id 商品ID
     * @return 商品（存在しない場合はEmpty）
     */
    Optional<Product> getProduct(UUID id);
    
    /**
     * すべての商品を取得する
     * 
     * @return 商品リスト
     */
    List<Product> getAllProducts();
    
    /**
     * 特定のカテゴリに属する商品をすべて取得する
     * 
     * @param categoryId カテゴリID
     * @return 商品リスト
     */
    List<Product> getProductsByCategory(UUID categoryId);
    
    /**
     * 商品の詳細を更新する
     * 
     * @param id 商品ID
     * @param name 商品名
     * @param description 商品説明
     * @param price 価格
     * @return 更新された商品
     */
    Product updateProduct(UUID id, String name, String description, BigDecimal price);
    
    /**
     * 商品を削除する
     * 
     * @param id 商品ID
     */
    void deleteProduct(UUID id);
    
    /**
     * 商品の在庫を追加する
     * 
     * @param id 商品ID
     * @param quantity 追加する数量
     * @return 更新された商品
     */
    Product addStock(UUID id, int quantity);
    
    /**
     * 商品の在庫を削減する
     * 
     * @param id 商品ID
     * @param quantity 削減する数量
     * @return 更新された商品
     */
    Product removeStock(UUID id, int quantity);
    
    /**
     * 商品名で商品を検索する
     * 
     * @param keyword 検索キーワード
     * @return 検索結果の商品リスト
     */
    List<Product> searchProductsByName(String keyword);
}