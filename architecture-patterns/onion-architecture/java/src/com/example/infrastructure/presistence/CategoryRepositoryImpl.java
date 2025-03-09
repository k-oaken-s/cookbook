package com.example.infrastructure.persistence;

import com.example.application.interfaces.CategoryRepository;
import com.example.domain.models.Category;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Component;

import java.util.List;
import java.util.Optional;
import java.util.UUID;
import java.util.stream.Collectors;

@Component
@RequiredArgsConstructor
public class CategoryRepositoryImpl implements CategoryRepository {
    
    private final SpringDataCategoryRepository springDataCategoryRepository;
    private final CategoryPersistenceMapper categoryMapper;
    
    @Override
    public Optional<Category> findById(UUID id) {
        return springDataCategoryRepository.findById(id)
                .map(categoryMapper::toDomain);
    }
    
    @Override
    public List<Category> findAll() {
        return springDataCategoryRepository.findAll().stream()
                .map(categoryMapper::toDomain)
                .collect(Collectors.toList());
    }
    
    @Override
    public Category save(Category category) {
        CategoryEntity entity = categoryMapper.toEntity(category);
        CategoryEntity savedEntity = springDataCategoryRepository.save(entity);
        return categoryMapper.toDomain(savedEntity);
    }
    
    @Override
    public void deleteById(UUID id) {
        springDataCategoryRepository.deleteById(id);
    }
    
    @Override
    public Optional<Category> findByName(String name) {
        return springDataCategoryRepository.findByName(name)
                .map(categoryMapper::toDomain);
    }
}