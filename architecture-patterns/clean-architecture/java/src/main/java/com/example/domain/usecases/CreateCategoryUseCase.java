package com.example.domain.usecases;

import com.example.domain.entities.Category;
import com.example.domain.repositories.CategoryRepository;
import lombok.RequiredArgsConstructor;

@RequiredArgsConstructor
public class CreateCategoryUseCase {
    private final CategoryRepository categoryRepository;

    public Category execute(CreateCategoryInput input) {
        // カテゴリ名の重複チェック
        categoryRepository.findByName(input.getName())
                .ifPresent(category -> {
                    throw new IllegalArgumentException("同じ名前のカテゴリが既に存在します: " + input.getName());
                });

        // カテゴリの作成
        Category category = Category.create(
                input.getName(),
                input.getDescription()
        );

        // カテゴリの保存
        return categoryRepository.save(category);
    }

    // 入力データを表すクラス
    public record CreateCategoryInput(
            String name,
            String description
    ) {
        // バリデーション
        public CreateCategoryInput {
            if (name == null || name.isBlank()) {
                throw new IllegalArgumentException("カテゴリ名は必須です");
            }
        }
    }
}