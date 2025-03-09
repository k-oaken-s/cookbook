package com.example.presentation.controllers;

import com.example.application.services.ProductService;
import com.example.domain.entities.Product;
import com.example.presentation.dtos.CreateProductRequest;
import com.example.presentation.dtos.ProductResponse;
import com.example.presentation.dtos.UpdateProductRequest;
import com.example.presentation.dtos.UpdateStockRequest;
import lombok.RequiredArgsConstructor;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.util.List;
import java.util.UUID;
import java.util.stream.Collectors;

@RestController
@RequestMapping("/api/products")
@RequiredArgsConstructor
public class ProductController {
    private final ProductService productService;
    private final ProductDtoMapper productDtoMapper;

    @PostMapping
    public ResponseEntity<ProductResponse> createProduct(@RequestBody CreateProductRequest request) {
        Product product = productService.createProduct(
                request.getName(),
                request.getDescription(),
                request.getPrice(),
                request.getStockQuantity(),
                request.getCategoryId()
        );
        return new ResponseEntity<>(productDtoMapper.toResponse(product), HttpStatus.CREATED);
    }

    @GetMapping("/{id}")
    public ResponseEntity<ProductResponse> getProduct(@PathVariable UUID id) {
        return productService.getProduct(id)
                .map(product -> ResponseEntity.ok(productDtoMapper.toResponse(product)))
                .orElse(ResponseEntity.notFound().build());
    }

    @GetMapping
    public ResponseEntity<List<ProductResponse>> getAllProducts() {
        List<ProductResponse> products = productService.getAllProducts().stream()
                .map(productDtoMapper::toResponse)
                .collect(Collectors.toList());
        return ResponseEntity.ok(products);
    }

    @PutMapping("/{id}")
    public ResponseEntity<ProductResponse> updateProduct(
            @PathVariable UUID id,
            @RequestBody UpdateProductRequest request) {
        Product updatedProduct = productService.updateProduct(
                id,
                request.getName(),
                request.getDescription(),
                request.getPrice()
        );
        return ResponseEntity.ok(productDtoMapper.toResponse(updatedProduct));
    }

    @DeleteMapping("/{id}")
    public ResponseEntity<Void> deleteProduct(@PathVariable UUID id) {
        productService.deleteProduct(id);
        return ResponseEntity.noContent().build();
    }

    @PostMapping("/{id}/add-stock")
    public ResponseEntity<ProductResponse> addStock(
            @PathVariable UUID id,
            @RequestBody UpdateStockRequest request) {
        Product product = productService.addProductStock(id, request.getQuantity());
        return ResponseEntity.ok(productDtoMapper.toResponse(product));
    }

    @PostMapping("/{id}/remove-stock")
    public ResponseEntity<ProductResponse> removeStock(
            @PathVariable UUID id,
            @RequestBody UpdateStockRequest request) {
        Product product = productService.removeProductStock(id, request.getQuantity());
        return ResponseEntity.ok(productDtoMapper.toResponse(product));
    }
}