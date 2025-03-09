package com.example.application.services;

import com.example.domain.entities.Category;
import com.example.domain.repositories.CategoryRepository;
import com.example.domain.usecases.CreateCategoryUseCase;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.util.List;
import java.util.Optional;
import java.util.UUID;

@Service
@RequiredArgsConstructor
public class CategoryService {
    private final CategoryRepository categoryRepository;
    private final CreateCategoryUseCase createCategoryUseCase;

    @Transactional
    public Category createCategory(String name, String description) {
        CreateCategoryUseCase.CreateCategoryInput input = new CreateCategoryUseCase.CreateCategoryInput(
                name, description
        );
        return createCategoryUseCase.execute(input);
    }

    @Transactional(readOnly = true)
    public Optional<Category> getCategory(UUID id) {
        return categoryRepository.findById(id);
    }

    @Transactional(readOnly = true)
    public List<Category> getAllCategories() {
        return categoryRepository.findAll();
    }

    @Transactional
    public void deleteCategory(UUID id) {
        categoryRepository.deleteById(id);
    }

    @Transactional
    public Optional<Category> updateCategory(UUID id, String name, String description) {
        return categoryRepository.findById(id)
                .map(category -> {
                    category.update(name, description);
                    return categoryRepository.save(category);
                });
    }
}