package com.example.presentation.controllers;

import com.example.domain.entities.Product;
import com.example.presentation.dtos.ProductResponse;
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

        return new ProductResponse(
                product.getId(),
                product.getName(),
                product.getDescription(),
                product.getPrice(),
                product.getStockQuantity(),
                categoryDtoMapper.toResponse(product.getCategory())
        );
    }
}
