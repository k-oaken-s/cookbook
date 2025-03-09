package com.example.adapters.primary.api;

import com.example.adapters.primary.api.request.CreateCategoryRequest;
import com.example.adapters.primary.api.request.UpdateCategoryRequest;
import com.example.adapters.primary.api.response.CategoryResponse;
import com.example.application.domain.Category;
import com.example.application.ports.input.CategoryService;
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
    private final CategoryMapper categoryMapper;
    
    @PostMapping
    public ResponseEntity<CategoryResponse> createCategory(@RequestBody CreateCategoryRequest request) {
        Category category = categoryService.createCategory(
                request.getName(),
                request.getDescription()
        );
        
        return ResponseEntity.status(HttpStatus.CREATED)
                .body(categoryMapper.toResponse(category));
    }
    
    @GetMapping("/{id}")
    public ResponseEntity<CategoryResponse> getCategory(@PathVariable UUID id) {
        return categoryService.getCategory(id)
                .map(category -> ResponseEntity.ok(categoryMapper.toResponse(category)))
                .orElse(ResponseEntity.notFound().build());
    }
    
    @GetMapping
    public ResponseEntity<List<CategoryResponse>> getAllCategories() {
        List<CategoryResponse> categories = categoryService.getAllCategories().stream()
                .map(categoryMapper::toResponse)
                .collect(Collectors.toList());
        
        return ResponseEntity.ok(categories);
    }
    
    @PutMapping("/{id}")
    public ResponseEntity<CategoryResponse> updateCategory(
            @PathVariable UUID id,
            @RequestBody UpdateCategoryRequest request) {
        
        return categoryService.updateCategory(id, request.getName(), request.getDescription())
                .map(category -> ResponseEntity.ok(categoryMapper.toResponse(category)))
                .orElse(ResponseEntity.notFound().build());
    }
    
    @DeleteMapping("/{id}")
    public ResponseEntity<Void> deleteCategory(@PathVariable UUID id) {
        categoryService.deleteCategory(id);
        return ResponseEntity.noContent().build();
    }
    
    @GetMapping("/by-name")
    public ResponseEntity<CategoryResponse> getCategoryByName(@RequestParam String name) {
        return categoryService.getCategoryByName(name)
                .map(category -> ResponseEntity.ok(categoryMapper.toResponse(category)))
                .orElse(ResponseEntity.notFound().build());
    }
}