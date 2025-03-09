package com.example.presentation.controllers;

import com.example.domain.entities.Category;
import com.example.presentation.dtos.CategoryResponse;
import org.springframework.stereotype.Component;

@Component
public class CategoryDtoMapper {

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