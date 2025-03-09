package com.example.adapters.secondary.persistence;

import com.example.application.domain.Product;
import com.example.application.ports.output.ProductRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Component;

import java.util.List;
import java.util.Optional;
import java.util.UUID;
import java.util.stream.Collectors;

@Component
@RequiredArgsConstructor
public class JpaProductRepository implements ProductRepository {
    
    private final SpringDataProductRepository productRepository;
    private final ProductPersistenceMapper productMapper;
    
    @Override
    public Optional<Product> findById(UUID id) {
        return productRepository.findById(id)
                .map(productMapper::toDomain);
    }
    
    @Override
    public List<Product> findAll() {
        return productRepository.findAll().stream()
                .map(productMapper::toDomain)
                .collect(Collectors.toList());
    }
    
    @Override
    public List<Product> findByCategory(UUID categoryId) {
        return productRepository.findByCategoryId(categoryId).stream()
                .map(productMapper::toDomain)
                .collect(Collectors.toList());
    }
    
    @Override
    public Product save(Product product) {
        ProductEntity entity = productMapper.toEntity(product);
        ProductEntity savedEntity = productRepository.save(entity);
        return productMapper.toDomain(savedEntity);
    }
    
    @Override
    public void deleteById(UUID id) {
        productRepository.deleteById(id);
    }
    
    @Override
    public List<Product> searchByName(String keyword) {
        return productRepository.searchByNameContaining(keyword).stream()
                .map(productMapper::toDomain)
                .collect(Collectors.toList());
    }
}