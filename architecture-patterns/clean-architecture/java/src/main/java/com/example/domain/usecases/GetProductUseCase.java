package com.example.domain.usecases;

import com.example.domain.entities.Product;
import com.example.domain.repositories.ProductRepository;
import lombok.RequiredArgsConstructor;

import java.util.Optional;
import java.util.UUID;

@RequiredArgsConstructor
public class GetProductUseCase {
    private final ProductRepository productRepository;

    public Optional<Product> execute(UUID id) {
        if (id == null) {
            throw new IllegalArgumentException("商品IDは必須です");
        }
        return productRepository.findById(id);
    }
}
