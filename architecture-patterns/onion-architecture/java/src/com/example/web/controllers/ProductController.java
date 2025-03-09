package com.example.web.controllers;

import com.example.application.interfaces.ProductService;
import com.example.domain.models.Product;
import com.example.web.models.request.CreateProductRequest;
import com.example.web.models.request.UpdateProductRequest;
import com.example.web.models.request.UpdateStockRequest;
import com.example.web.models.response.ProductResponse;
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
        
        return ResponseEntity.status(HttpStatus.CREATED)
                .body(productDtoMapper.toResponse(product));
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
    
    @GetMapping("/category/{categoryId}")
    public ResponseEntity<List<ProductResponse>> getProductsByCategory(@PathVariable UUID categoryId) {
        List<ProductResponse> products = productService.getProductsByCategory(categoryId).stream()
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
        
        Product product = productService.addStock(id, request.getQuantity());
        return ResponseEntity.ok(productDtoMapper.toResponse(product));
    }
    
    @PostMapping("/{id}/remove-stock")
    public ResponseEntity<ProductResponse> removeStock(
            @PathVariable UUID id,
            @RequestBody UpdateStockRequest request) {
        
        Product product = productService.removeStock(id, request.getQuantity());
        return ResponseEntity.ok(productDtoMapper.toResponse(product));
    }
    
    @GetMapping("/search")
    public ResponseEntity<List<ProductResponse>> searchProducts(@RequestParam String keyword) {
        List<ProductResponse> products = productService.searchProductsByName(keyword).stream()
                .map(productDtoMapper::toResponse)
                .collect(Collectors.toList());
        
        return ResponseEntity.ok(products);
    }
    
    @PostMapping("/{id}/apply-discount")
    public ResponseEntity<ProductResponse> applyDiscount(
            @PathVariable UUID id,
            @RequestParam double discountRate) {
        
        Product product = productService.getProduct(id)
                .orElseThrow(() -> new IllegalArgumentException("指定された商品が見つかりません: " + id));
        
        // 割引を適用して商品レスポンスに割引価格を設定
        var discountedPrice = productService.applyDiscount(id, discountRate);
        
        ProductResponse response = productDtoMapper.toResponse(product);
        response.setDiscountedPrice(discountedPrice);
        
        return ResponseEntity.ok(response);
    }
}