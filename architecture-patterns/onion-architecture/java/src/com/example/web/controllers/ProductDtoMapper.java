package com.example.web.controllers;

import com.example.domain.models.Product;
import com.example.web.models.response.CategoryResponse;
import com.example.web.models.response.ProductResponse;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Component;

@Component
@RequiredArgsConstructor
public class ProductDtoMapper {
    
    private final CategoryDtoMapper categoryDtoMapper;
    
    public ProductResponse toResponse(Product product) {
        if (product == null) {
            return null;
        }
        
        CategoryResponse categoryResponse = categoryDtoMapper.toResponse(product.getCategory());
        
        return ProductResponse.builder()
                .id(product.getId())
                .name(product.getName())
                .description(product.getDescription())
                .price(product.getPrice())
                .stockQuantity(product.getStockQuantity())
                .category(categoryResponse)
                .build();
    }
}