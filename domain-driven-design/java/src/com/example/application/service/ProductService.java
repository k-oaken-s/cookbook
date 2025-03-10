package com.example.application.service;

import com.example.domain.model.entity.Product;
import com.example.domain.model.valueobject.Money;
import com.example.domain.model.valueobject.ProductId;
import com.example.domain.model.valueobject.Quantity;
import com.example.domain.repository.ProductRepository;

import java.util.List;
import java.util.Optional;

/**
 * 商品に関するアプリケーションサービス
 * ユースケースの実装を担当
 */
public class ProductService {
    private final ProductRepository productRepository;

    public ProductService(ProductRepository productRepository) {
        this.productRepository = productRepository;
    }

    /**
     * 新しい商品を作成する
     * @param name 商品名
     * @param description 商品説明
     * @param price 価格
     * @param stockQuantity 在庫数量
     * @return 作成された商品のID
     */
    public ProductId createProduct(String name, String description, Money price, Quantity stockQuantity) {
        Product product = Product.create(name, description, price, stockQuantity);
        productRepository.save(product);
        return product.getId();
    }

    /**
     * 商品情報を更新する
     * @param productId 商品ID
     * @param name 商品名
     * @param description 商品説明
     * @throws IllegalArgumentException 商品が見つからない場合
     */
    public void updateProductDetails(ProductId productId, String name, String description) {
        Product product = productRepository.findById(productId)
                .orElseThrow(() -> new IllegalArgumentException("Product not found: " + productId));
        
        product.updateDetails(name, description);
        productRepository.save(product);
    }

    /**
     * 商品価格を更新する
     * @param productId 商品ID
     * @param newPrice 新しい価格
     * @throws IllegalArgumentException 商品が見つからない場合
     */
    public void updateProductPrice(ProductId productId, Money newPrice) {
        Product product = productRepository.findById(productId)
                .orElseThrow(() -> new IllegalArgumentException("Product not found: " + productId));
        
        product.updatePrice(newPrice);
        productRepository.save(product);
    }

    /**
     * 商品在庫を更新する
     * @param productId 商品ID
     * @param newQuantity 新しい在庫数量
     * @throws IllegalArgumentException 商品が見つからない場合
     */
    public void updateProductStock(ProductId productId, Quantity newQuantity) {
        Product product = productRepository.findById(productId)
                .orElseThrow(() -> new IllegalArgumentException("Product not found: " + productId));
        
        product.updateStockQuantity(newQuantity);
        productRepository.save(product);
    }

    /**
     * 商品在庫を増やす
     * @param productId 商品ID
     * @param quantityToAdd 追加する数量
     * @throws IllegalArgumentException 商品が見つからない場合
     */
    public void addProductStock(ProductId productId, Quantity quantityToAdd) {
        Product product = productRepository.findById(productId)
                .orElseThrow(() -> new IllegalArgumentException("Product not found: " + productId));
        
        product.addStock(quantityToAdd);
        productRepository.save(product);
    }

    /**
     * 商品在庫を減らす
     * @param productId 商品ID
     * @param quantityToReduce 減らす数量
     * @throws IllegalArgumentException 商品が見つからない場合
     * @throws IllegalStateException 在庫が足りない場合
     */
    public void reduceProductStock(ProductId productId, Quantity quantityToReduce) {
        Product product = productRepository.findById(productId)
                .orElseThrow(() -> new IllegalArgumentException("Product not found: " + productId));
        
        product.reduceStock(quantityToReduce);
        productRepository.save(product);
    }

    /**
     * 商品を有効化する
     * @param productId 商品ID
     * @throws IllegalArgumentException 商品が見つからない場合
     */
    public void activateProduct(ProductId productId) {
        Product product = productRepository.findById(productId)
                .orElseThrow(() -> new IllegalArgumentException("Product not found: " + productId));
        
        product.activate();
        productRepository.save(product);
    }

    /**
     * 商品を無効化する
     * @param productId 商品ID
     * @throws IllegalArgumentException 商品が見つからない場合
     */
    public void deactivateProduct(ProductId productId) {
        Product product = productRepository.findById(productId)
                .orElseThrow(() -> new IllegalArgumentException("Product not found: " + productId));
        
        product.deactivate();
        productRepository.save(product);
    }

    /**
     * 商品を検索する
     * @param productId 商品ID
     * @return 商品のオプショナル
     */
    public Optional<Product> findProduct(ProductId productId) {
        return productRepository.findById(productId);
    }

    /**
     * 商品名で商品を検索する
     * @param name 商品名
     * @return 商品のリスト
     */
    public List<Product> findProductsByName(String name) {
        return productRepository.findByName(name);
    }

    /**
     * アクティブな全ての商品を取得する
     * @return 商品のリスト
     */
    public List<Product> findAllActiveProducts() {
        return productRepository.findAllActive();
    }

    /**
     * 商品を削除する
     * @param productId 商品ID
     * @throws IllegalArgumentException 商品が見つからない場合
     */
    public void deleteProduct(ProductId productId) {
        if (!productRepository.findById(productId).isPresent()) {
            throw new IllegalArgumentException("Product not found: " + productId);
        }
        productRepository.deleteById(productId);
    }
}