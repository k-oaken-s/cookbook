package com.example.domain.usecases;

import com.example.domain.entities.Product;
import com.example.domain.repositories.ProductRepository;
import lombok.RequiredArgsConstructor;

import java.util.UUID;

@RequiredArgsConstructor
public class UpdateProductStockUseCase {
    private final ProductRepository productRepository;

    public Product addStock(UUID id, int quantity) {
        if (id == null) {
            throw new IllegalArgumentException("商品IDは必須です");
        }
        if (quantity <= 0) {
            throw new IllegalArgumentException("追加する在庫数は正の数である必要があります");
        }

        // 商品の存在チェック
        Product product = productRepository.findById(id)
                .orElseThrow(() -> new IllegalArgumentException("指定された商品が見つかりません: " + id));

        // 在庫の追加
        product.addStock(quantity);

        // 商品の保存
        return productRepository.save(product);
    }

    public Product removeStock(UUID id, int quantity) {
        if (id == null) {
            throw new IllegalArgumentException("商品IDは必須です");
        }
        if (quantity <= 0) {
            throw new IllegalArgumentException("削減する在庫数は正の数である必要があります");
        }

        // 商品の存在チェック
        Product product = productRepository.findById(id)
                .orElseThrow(() -> new IllegalArgumentException("指定された商品が見つかりません: " + id));

        // 在庫の削減
        product.removeStock(quantity);

        // 商品の保存
        return productRepository.save(product);
    }
}