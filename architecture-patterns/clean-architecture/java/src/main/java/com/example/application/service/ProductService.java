package com.example.application.services;

import com.example.domain.entities.Product;
import com.example.domain.usecases.*;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.math.BigDecimal;
import java.util.List;
import java.util.Optional;
import java.util.UUID;

@Service
@RequiredArgsConstructor
public class ProductService {
    private final CreateProductUseCase createProductUseCase;
    private final GetProductUseCase getProductUseCase;
    private final GetAllProductsUseCase getAllProductsUseCase;
    private final UpdateProductUseCase updateProductUseCase;
    private final DeleteProductUseCase deleteProductUseCase;
    private final UpdateProductStockUseCase updateProductStockUseCase;

    @Transactional
    public Product createProduct(String name, String description, BigDecimal price, int stockQuantity, UUID categoryId) {
        CreateProductUseCase.CreateProductInput input = new CreateProductUseCase.CreateProductInput(
                name, description, price, stockQuantity, categoryId
        );
        return createProductUseCase.execute(input);
    }

    @Transactional(readOnly = true)
    public Optional<Product> getProduct(UUID id) {
        return getProductUseCase.execute(id);
    }

    @Transactional(readOnly = true)
    public List<Product> getAllProducts() {
        return getAllProductsUseCase.execute();
    }

    @Transactional
    public Product updateProduct(UUID id, String name, String description, BigDecimal price) {
        UpdateProductUseCase.UpdateProductInput input = new UpdateProductUseCase.UpdateProductInput(
                id, name, description, price
        );
        return updateProductUseCase.execute(input);
    }

    @Transactional
    public void deleteProduct(UUID id) {
        deleteProductUseCase.execute(id);
    }

    @Transactional
    public Product addProductStock(UUID id, int quantity) {
        return updateProductStockUseCase.addStock(id, quantity);
    }

    @Transactional
    public Product removeProductStock(UUID id, int quantity) {
        return updateProductStockUseCase.removeStock(id, quantity);
    }
}