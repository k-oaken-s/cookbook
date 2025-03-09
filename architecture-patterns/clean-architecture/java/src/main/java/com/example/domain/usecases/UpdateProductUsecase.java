package com.example.domain.usecases;

import com.example.domain.entities.Product;
import com.example.domain.repositories.ProductRepository;
import lombok.RequiredArgsConstructor;

import java.math.BigDecimal;
import java.util.UUID;

@RequiredArgsConstructor
public class UpdateProductUseCase {
    private final ProductRepository productRepository;

    public Product execute(UpdateProductInput input) {
        // 商品の存在チェック
        Product product = productRepository.findById(input.getId())
                .orElseThrow(() -> new IllegalArgumentException("指定された商品が見つかりません: " + input.getId()));

        // 商品の更新
        product.updateDetails(
                input.getName(),
                input.getDescription(),
                input.getPrice()
        );

        // 商品の保存
        return productRepository.save(product);
    }

    // 入力データを表すクラス
    public record UpdateProductInput(
            UUID id,
            String name,
            String description,
            BigDecimal price
    ) {
        // バリデーション
        public UpdateProductInput {
            if (id == null) {
                throw new IllegalArgumentException("商品IDは必須です");
            }
            if (name == null || name.isBlank()) {
                throw new IllegalArgumentException("商品名は必須です");
            }
            if (price == null || price.compareTo(BigDecimal.ZERO) < 0) {
                throw new IllegalArgumentException("価格は0以上である必要があります");
            }
        }
    }
}