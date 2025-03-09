package com.example.adapters.primary.api;

import com.example.adapters.primary.api.response.CategoryResponse;
import com.example.application.domain.Category;
import org.springframework.stereotype.Component;

@Component
public class CategoryMapper {
    
    public CategoryResponse toResponse(Category category) {
        if (category == null) {
            return null;
        }
        
        return new CategoryResponse(
                category.getId(),
                category.getName(),
                category.getDescription()
        );
    }
}