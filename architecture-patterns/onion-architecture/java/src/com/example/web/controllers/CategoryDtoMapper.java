package com.example.web.controllers;

import com.example.domain.models.Category;
import com.example.web.models.response.CategoryResponse;
import org.springframework.stereotype.Component;

@Component
public class CategoryDtoMapper {
    
    public CategoryResponse toResponse(Category category) {
        if (category == null) {
            return null;
        }
        
        return CategoryResponse.builder()
                .id(category.getId())
                .name(category.getName())
                .description(category.getDescription())
                .build();
    }
}