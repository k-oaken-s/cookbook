package com.example.domain.usecases;

import com.example.domain.entities.Category;
import com.example.domain.entities.Product;
import com.example.domain.repositories.CategoryRepository;
import com.example.domain.repositories.ProductRepository;
import lombok.RequiredArgsConstructor;

import java.math.BigDecimal;
import java.util.UUID;

@RequiredArgsConstructor
public class CreateProductUseCase {
    private final ProductRepository productRepository;
    private final CategoryRepository categoryRepository;

    public Product execute(CreateProductInput input) {
        // カテゴリの存在チェック
        Category category = categoryRepository.findById(input.getCategoryId())
                .orElseThrow(() -> new IllegalArgumentException("指定されたカテゴリが見つかりません: " + input.getCategoryId()));

        // 商品の作成
        Product product = Product.create(
                input.getName(),
                input.getDescription(),
                input.getPrice(),
                input.getStockQuantity(),
                category
        );

        // 商品の保存
        return productRepository.save(product);
    }

    // 入力データを表すクラス
    public record CreateProductInput(
            String name,
            String description,
            BigDecimal price,
            int stockQuantity,
            UUID categoryId
    ) {
        // バリデーション
        public CreateProductInput {
            if (name == null || name.isBlank()) {
                throw new IllegalArgumentException("商品名は必須です");
            }
            if (price == null || price.compareTo(BigDecimal.ZERO) < 0) {
                throw new IllegalArgumentException("価格は0以上である必要があります");
            }
            if (stockQuantity < 0) {
                throw new IllegalArgumentException("在庫数は0以上である必要があります");
            }
            if (categoryId == null) {
                throw new IllegalArgumentException("カテゴリIDは必須です");
            }
        }
    }
}