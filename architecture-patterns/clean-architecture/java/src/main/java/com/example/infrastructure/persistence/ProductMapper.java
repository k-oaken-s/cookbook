package com.example.infrastructure.persistence;

import com.example.domain.entities.Category;
import com.example.domain.entities.Product;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Component;

import java.util.UUID;

@Component
@RequiredArgsConstructor
public class ProductMapper {
    private final CategoryMapper categoryMapper;

    public Product toDomain(ProductEntity entity) {
        if (entity == null) {
            return null;
        }

        Category category = categoryMapper.toDomain(entity.getCategory());

        return new Product(
                entity.getId(),
                entity.getName(),
                entity.getDescription(),
                entity.getPrice(),
                entity.getStockQuantity(),
                category
        );
    }

    public ProductEntity toEntity(Product domain) {
        if (domain == null) {
            return null;
        }

        CategoryEntity categoryEntity = categoryMapper.toEntity(domain.getCategory());

        return ProductEntity.builder()
                .id(domain.getId())
                .name(domain.getName())
                .description(domain.getDescription())
                .price(domain.getPrice())
                .stockQuantity(domain.getStockQuantity())
                .category(categoryEntity)
                .build();
    }
}