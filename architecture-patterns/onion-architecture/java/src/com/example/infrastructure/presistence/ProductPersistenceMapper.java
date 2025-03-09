package com.example.infrastructure.persistence;

import com.example.domain.models.Product;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Component;

@Component
@RequiredArgsConstructor
public class ProductPersistenceMapper {
    
    private final CategoryPersistenceMapper categoryMapper;
    
    public Product toDomain(ProductEntity entity) {
        if (entity == null) {
            return null;
        }
        
        return new Product(
                entity.getId(),
                entity.getName(),
                entity.getDescription(),
                entity.getPrice(),
                entity.getStockQuantity(),
                categoryMapper.toDomain(entity.getCategory())
        );
    }
    
    public ProductEntity toEntity(Product domain) {
        if (domain == null) {
            return null;
        }
        
        return ProductEntity.builder()
                .id(domain.getId())
                .name(domain.getName())
                .description(domain.getDescription())
                .price(domain.getPrice())
                .stockQuantity(domain.getStockQuantity())
                .category(categoryMapper.toEntity(domain.getCategory()))
                .build();
    }
}