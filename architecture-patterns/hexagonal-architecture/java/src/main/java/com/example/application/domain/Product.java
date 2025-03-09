package com.example.application.domain;

import lombok.Getter;

import java.math.BigDecimal;
import java.util.UUID;

@Getter
public class Product {
    private final UUID id;
    private String name;
    private String description;
    private BigDecimal price;
    private int stockQuantity;
    private final Category category;

    public Product(UUID id, String name, String description, BigDecimal price, int stockQuantity, Category category) {
        validateName(name);
        validatePrice(price);
        validateStockQuantity(stockQuantity);
        validateCategory(category);
        
        this.id = id;
        this.name = name;
        this.description = description;
        this.price = price;
        this.stockQuantity = stockQuantity;
        this.category = category;
    }

    // ファクトリーメソッド
    public static Product create(String name, String description, BigDecimal price, int stockQuantity, Category category) {
        return new Product(UUID.randomUUID(), name, description, price, stockQuantity, category);
    }

    public void updateDetails(String name, String description, BigDecimal price) {
        validateName(name);
        validatePrice(price);
        
        this.name = name;
        this.description = description;
        this.price = price;
    }

    public void addStock(int quantity) {
        if (quantity <= 0) {
            throw new IllegalArgumentException("追加する在庫数は正の数である必要があります");
        }
        this.stockQuantity += quantity;
    }

    public void removeStock(int quantity) {
        if (quantity <= 0) {
            throw new IllegalArgumentException("削減する在庫数は正の数である必要があります");
        }
        if (this.stockQuantity < quantity) {
            throw new IllegalStateException("在庫が足りません");
        }
        this.stockQuantity -= quantity;
    }

    private void validateName(String name) {
        if (name == null || name.isBlank()) {
            throw new IllegalArgumentException("商品名は必須です");
        }
        if (name.length() > 100) {
            throw new IllegalArgumentException("商品名は100文字以内にしてください");
        }
    }

    private void validatePrice(BigDecimal price) {
        if (price == null) {
            throw new IllegalArgumentException("価格は必須です");
        }
        if (price.compareTo(BigDecimal.ZERO) < 0) {
            throw new IllegalArgumentException("価格は0以上である必要があります");
        }
    }

    private void validateStockQuantity(int stockQuantity) {
        if (stockQuantity < 0) {
            throw new IllegalArgumentException("在庫数は0以上である必要があります");
        }
    }
    
    private void validateCategory(Category category) {
        if (category == null) {
            throw new IllegalArgumentException("カテゴリは必須です");
        }
    }
}