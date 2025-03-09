package com.example.adapters.primary.api;

import com.example.adapters.primary.api.request.AddStockRequest;
import com.example.adapters.primary.api.request.CreateProductRequest;
import com.example.adapters.primary.api.request.RemoveStockRequest;
import com.example.adapters.primary.api.request.UpdateProductRequest;
import com.example.adapters.primary.api.response.ProductResponse;
import com.example.application.domain.Product;
import com.example.application.ports.input.ProductService;
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
    private final ProductMapper productMapper;
    
    @PostMapping
    public ResponseEntity<ProductResponse> createProduct(@RequestBody CreateProductRequest request) {
        Product product = productService.createProduct(
                request.getName(),
                request.getDescription(),
                request.getPrice(),
                request.getStockQuantity(),
                request.getCategoryId()
        );
        
        return ResponseEntity.status(HttpStatus.CREATED)
                .body(productMapper.toResponse(product));
    }
    
    @GetMapping("/{id}")
    public ResponseEntity<ProductResponse> getProduct(@PathVariable UUID id) {
        return productService.getProduct(id)
                .map(product -> ResponseEntity.ok(productMapper.toResponse(product)))
                .orElse(ResponseEntity.notFound().build());
    }
    
    @GetMapping
    public ResponseEntity<List<ProductResponse>> getAllProducts() {
        List<ProductResponse> products = productService.getAllProducts().stream()
                .map(productMapper::toResponse)
                .collect(Collectors.toList());
        
        return ResponseEntity.ok(products);
    }
    
    @GetMapping("/category/{categoryId}")
    public ResponseEntity<List<ProductResponse>> getProductsByCategory(@PathVariable UUID categoryId) {
        List<ProductResponse> products = productService.getProductsByCategory(categoryId).stream()
                .map(productMapper::toResponse)
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
        
        return ResponseEntity.ok(productMapper.toResponse(updatedProduct));
    }
    
    @DeleteMapping("/{id}")
    public ResponseEntity<Void> deleteProduct(@PathVariable UUID id) {
        productService.deleteProduct(id);
        return ResponseEntity.noContent().build();
    }
    
    @PostMapping("/{id}/add-stock")
    public ResponseEntity<ProductResponse> addStock(
            @PathVariable UUID id,
            @RequestBody AddStockRequest request) {
        
        Product product = productService.addStock(id, request.getQuantity());
        return ResponseEntity.ok(productMapper.toResponse(product));
    }
    
    @PostMapping("/{id}/remove-stock")
    public ResponseEntity<ProductResponse> removeStock(
            @PathVariable UUID id,
            @RequestBody RemoveStockRequest request) {
        
        Product product = productService.removeStock(id, request.getQuantity());
        return ResponseEntity.ok(productMapper.toResponse(product));
    }
    
    @GetMapping("/search")
    public ResponseEntity<List<ProductResponse>> searchProducts(@RequestParam String keyword) {
        List<ProductResponse> products = productService.searchProductsByName(keyword).stream()
                .map(productMapper::toResponse)
                .collect(Collectors.toList());
        
        return ResponseEntity.ok(products);
    }
}