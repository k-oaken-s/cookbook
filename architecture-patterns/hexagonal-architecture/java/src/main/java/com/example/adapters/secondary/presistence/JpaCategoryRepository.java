package com.example.adapters.secondary.persistence;

import com.example.application.domain.Category;
import com.example.application.ports.output.CategoryRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Component;

import java.util.List;
import java.util.Optional;
import java.util.UUID;
import java.util.stream.Collectors;

@Component
@RequiredArgsConstructor
public class JpaCategoryRepository implements CategoryRepository {
    
    private final SpringDataCategoryRepository categoryRepository;
    private final CategoryPersistenceMapper categoryMapper;
    
    @Override
    public Optional<Category> findById(UUID id) {
        return categoryRepository.findById(id)
                .map(categoryMapper::toDomain);
    }
    
    @Override
    public List<Category> findAll() {
        return categoryRepository.findAll().stream()
                .map(categoryMapper::toDomain)
                .collect(Collectors.toList());
    }
    
    @Override
    public Category save(Category category) {
        CategoryEntity entity = categoryMapper.toEntity(category);
        CategoryEntity savedEntity = categoryRepository.save(entity);
        return categoryMapper.toDomain(savedEntity);
    }
    
    @Override
    public void deleteById(UUID id) {
        categoryRepository.deleteById(id);
    }
    
    @Override
    public Optional<Category> findByName(String name) {
        return categoryRepository.findByName(name)
                .map(categoryMapper::toDomain);
    }
}