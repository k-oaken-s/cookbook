package com.example.domain.repository;

import com.example.domain.model.entity.Product;
import com.example.domain.model.valueobject.ProductId;

import java.util.List;
import java.util.Optional;

/**
 * 商品リポジトリのインターフェース
 * DDDではリポジトリはドメイン層で定義され、実装はインフラ層で行われる
 */
public interface ProductRepository {
    /**
     * 商品IDによる商品の検索
     * @param id 商品ID
     * @return 商品のOptional
     */
    Optional<Product> findById(ProductId id);

    /**
     * 商品名による商品の検索
     * @param name 商品名
     * @return 見つかった商品のリスト
     */
    List<Product> findByName(String name);

    /**
     * アクティブな全ての商品を取得
     * @return アクティブな商品のリスト
     */
    List<Product> findAllActive();

    /**
     * 商品の保存（新規作成または更新）
     * @param product 保存する商品
     * @return 保存された商品
     */
    Product save(Product product);

    /**
     * 商品の削除
     * @param id 削除する商品のID
     */
    void deleteById(ProductId id);
}