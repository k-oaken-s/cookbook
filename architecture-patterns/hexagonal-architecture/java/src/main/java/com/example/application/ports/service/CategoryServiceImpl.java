package com.example.application.ports.service;

import com.example.application.domain.Category;
import com.example.application.ports.input.CategoryService;
import com.example.application.ports.output.CategoryRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.util.List;
import java.util.Optional;
import java.util.UUID;

@Service
@RequiredArgsConstructor
public class CategoryServiceImpl implements CategoryService {
    
    private final CategoryRepository categoryRepository;

    @Override
    @Transactional
    public Category createCategory(String name, String description) {
        // カテゴリ名の重複チェック
        categoryRepository.findByName(name)
                .ifPresent(category -> {
                    throw new IllegalArgumentException("同じ名前のカテゴリが既に存在します: " + name);
                });
        
        Category category = Category.create(name, description);
        return categoryRepository.save(category);
    }

    @Override
    @Transactional(readOnly = true)
    public Optional<Category> getCategory(UUID id) {
        return categoryRepository.findById(id);
    }

    @Override
    @Transactional(readOnly = true)
    public List<Category> getAllCategories() {
        return categoryRepository.findAll();
    }

    @Override
    @Transactional
    public Optional<Category> updateCategory(UUID id, String name, String description) {
        // カテゴリ名の重複チェック（同じIDのカテゴリを除く）
        categoryRepository.findByName(name)
                .ifPresent(existingCategory -> {
                    if (!existingCategory.getId().equals(id)) {
                        throw new IllegalArgumentException("同じ名前のカテゴリが既に存在します: " + name);
                    }
                });
        
        return categoryRepository.findById(id)
                .map(category -> {
                    category.update(name, description);
                    return categoryRepository.save(category);
                });
    }

    @Override
    @Transactional
    public void deleteCategory(UUID id) {
        categoryRepository.deleteById(id);
    }

    @Override
    @Transactional(readOnly = true)
    public Optional<Category> getCategoryByName(String name) {
        return categoryRepository.findByName(name);
    }
}