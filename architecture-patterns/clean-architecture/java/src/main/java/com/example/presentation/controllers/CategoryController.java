package com.example.presentation.controllers;

import com.example.application.services.CategoryService;
import com.example.domain.entities.Category;
import com.example.presentation.dtos.CategoryResponse;
import com.example.presentation.dtos.CreateCategoryRequest;
import com.example.presentation.dtos.UpdateCategoryRequest;
import lombok.RequiredArgsConstructor;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.util.List;
import java.util.UUID;
import java.util.stream.Collectors;

@RestController
@RequestMapping("/api/categories")
@RequiredArgsConstructor
public class CategoryController {
    private final CategoryService categoryService;
    private final CategoryDtoMapper categoryDtoMapper;

    @PostMapping
    public ResponseEntity<CategoryResponse> createCategory(@RequestBody CreateCategoryRequest request) {
        Category category = categoryService.createCategory(
                request.getName(),
                request.getDescription()
        );
        return new ResponseEntity<>(categoryDtoMapper.toResponse(category), HttpStatus.CREATED);
    }

    @GetMapping("/{id}")
    public ResponseEntity<CategoryResponse> getCategory(@PathVariable UUID id) {
        return categoryService.getCategory(id)
                .map(category -> ResponseEntity.ok(categoryDtoMapper.toResponse(category)))
                .orElse(ResponseEntity.notFound().build());
    }

    @GetMapping
    public ResponseEntity<List<CategoryResponse>> getAllCategories() {
        List<CategoryResponse> categories = categoryService.getAllCategories().stream()
                .map(categoryDtoMapper::toResponse)
                .collect(Collectors.toList());
        return ResponseEntity.ok(categories);
    }

    @PutMapping("/{id}")
    public ResponseEntity<CategoryResponse> updateCategory(
            @PathVariable UUID id,
            @RequestBody UpdateCategoryRequest request) {
        return categoryService.updateCategory(
                id,
                request.getName(),
                request.getDescription()
        )
                .map(category -> ResponseEntity.ok(categoryDtoMapper.toResponse(category)))
                .orElse(ResponseEntity.notFound().build());
    }

    @DeleteMapping("/{id}")
    public ResponseEntity<Void> deleteCategory(@PathVariable UUID id) {
        categoryService.deleteCategory(id);
        return ResponseEntity.noContent().build();
    }
}