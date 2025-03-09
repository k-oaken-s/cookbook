package com.example.infrastructure.persistence;

import com.example.domain.entities.Category;
import org.springframework.stereotype.Component;

@Component
public class CategoryMapper {

    public Category toDomain(CategoryEntity entity) {
        if (entity == null) {
            return null;
        }

        return new Category(
                entity.getId(),
                entity.getName(),
                entity.getDescription()
        );
    }

    public CategoryEntity toEntity(Category domain) {
        if (domain == null) {
            return null;
        }

        return CategoryEntity.builder()
                .id(domain.getId())
                .name(domain.getName())
                .description(domain.getDescription())
                .build();
    }
}