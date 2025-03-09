package com.example.infrastructure.persistence;

import com.example.application.interfaces.ProductRepository;
import com.example.domain.models.Product;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Component;

import java.util.List;
import java.util.Optional;
import java.util.UUID;
import java.util.stream.Collectors;

@Component
@RequiredArgsConstructor
public class ProductRepositoryImpl implements ProductRepository {
    
    private final SpringDataProductRepository springDataProductRepository;
    private final ProductPersistenceMapper productMapper;
    
    @Override
    public Optional<Product> findById(UUID id) {
        return springDataProductRepository.findById(id)
                .map(productMapper::toDomain);
    }
    
    @Override
    public List<Product> findAll() {
        return springDataProductRepository.findAll().stream()
                .map(productMapper::toDomain)
                .collect(Collectors.toList());
    }
    
    @Override
    public List<Product> findByCategory(UUID categoryId) {
        return springDataProductRepository.findByCategoryId(categoryId).stream()
                .map(productMapper::toDomain)
                .collect(Collectors.toList());
    }
    
    @Override
    public Product save(Product product) {
        ProductEntity entity = productMapper.toEntity(product);
        ProductEntity savedEntity = springDataProductRepository.save(entity);
        return productMapper.toDomain(savedEntity);
    }
    
    @Override
    public void deleteById(UUID id) {
        springDataProductRepository.deleteById(id);
    }
    
    @Override
    public List<Product> searchByName(String keyword) {
        return springDataProductRepository.searchByNameContaining(keyword).stream()
                .map(productMapper::toDomain)
                .collect(Collectors.toList());
    }
}