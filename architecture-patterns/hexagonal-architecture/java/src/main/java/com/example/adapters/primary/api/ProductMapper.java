package com.example.adapters.primary.api;

import com.example.adapters.primary.api.response.CategoryResponse;
import com.example.adapters.primary.api.response.ProductResponse;
import com.example.application.domain.Category;
import com.example.application.domain.Product;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Component;

@Component
@RequiredArgsConstructor
public class ProductMapper {
    
    private final CategoryMapper categoryMapper;
    
    public ProductResponse toResponse(Product product) {
        if (product == null) {
            return null;
        }
        
        CategoryResponse categoryResponse = categoryMapper.toResponse(product.getCategory());
        
        return new ProductResponse(
                product.getId(),
                product.getName(),
                product.getDescription(),
                product.getPrice(),
                product.getStockQuantity(),
                categoryResponse
        );
    }
}