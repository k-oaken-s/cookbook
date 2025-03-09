package com.example.domain.models;

import lombok.Getter;

import java.util.UUID;

@Getter
public class Category {
    private final UUID id;
    private String name;
    private String description;

    public Category(UUID id, String name, String description) {
        validateName(name);
        
        this.id = id;
        this.name = name;
        this.description = description;
    }

    public static Category create(String name, String description) {
        return new Category(UUID.randomUUID(), name, description);
    }

    public void update(String name, String description) {
        validateName(name);
        
        this.name = name;
        this.description = description;
    }

    private void validateName(String name) {
        if (name == null || name.isBlank()) {
            throw new IllegalArgumentException("カテゴリ名は必須です");
        }
        if (name.length() > 50) {
            throw new IllegalArgumentException("カテゴリ名は50文字以内にしてください");
        }
    }
}