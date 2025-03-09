package com.example.application.ports.service;

import com.example.application.domain.Category;
import com.example.application.domain.Product;
import com.example.application.ports.input.ProductService;
import com.example.application.ports.output.CategoryRepository;
import com.example.application.ports.output.NotificationService;
import com.example.application.ports.output.ProductRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.math.BigDecimal;
import java.util.List;
import java.util.Optional;
import java.util.UUID;

@Service
@RequiredArgsConstructor
public class ProductServiceImpl implements ProductService {
    
    private final ProductRepository productRepository;
    private final CategoryRepository categoryRepository;
    private final NotificationService notificationService;
    
    private static final int LOW_STOCK_THRESHOLD = 5;

    @Override
    @Transactional
    public Product createProduct(String name, String description, BigDecimal price, int stockQuantity, UUID categoryId) {
        Category category = categoryRepository.findById(categoryId)
                .orElseThrow(() -> new IllegalArgumentException("指定されたカテゴリが見つかりません: " + categoryId));
        
        Product product = Product.create(name, description, price, stockQuantity, category);
        return productRepository.save(product);
    }

    @Override
    @Transactional(readOnly = true)
    public Optional<Product> getProduct(UUID id) {
        return productRepository.findById(id);
    }

    @Override
    @Transactional(readOnly = true)
    public List<Product> getAllProducts() {
        return productRepository.findAll();
    }

    @Override
    @Transactional(readOnly = true)
    public List<Product> getProductsByCategory(UUID categoryId) {
        return productRepository.findByCategory(categoryId);
    }

    @Override
    @Transactional
    public Product updateProduct(UUID id, String name, String description, BigDecimal price) {
        Product product = productRepository.findById(id)
                .orElseThrow(() -> new IllegalArgumentException("指定された商品が見つかりません: " + id));
        
        product.updateDetails(name, description, price);
        return productRepository.save(product);
    }

    @Override
    @Transactional
    public void deleteProduct(UUID id) {
        if (!productRepository.findById(id).isPresent()) {
            throw new IllegalArgumentException("指定された商品が見つかりません: " + id);
        }
        
        productRepository.deleteById(id);
    }

    @Override
    @Transactional
    public Product addStock(UUID id, int quantity) {
        Product product = productRepository.findById(id)
                .orElseThrow(() -> new IllegalArgumentException("指定された商品が見つかりません: " + id));
        
        product.addStock(quantity);
        return productRepository.save(product);
    }

    @Override
    @Transactional
    public Product removeStock(UUID id, int quantity) {
        Product product = productRepository.findById(id)
                .orElseThrow(() -> new IllegalArgumentException("指定された商品が見つかりません: " + id));
        
        product.removeStock(quantity);
        
        // 在庫が閾値を下回った場合に通知を送信
        if (product.getStockQuantity() < LOW_STOCK_THRESHOLD) {
            notificationService.sendLowStockNotification(product, LOW_STOCK_THRESHOLD);
        }
        
        return productRepository.save(product);
    }

    @Override
    @Transactional(readOnly = true)
    public List<Product> searchProductsByName(String keyword) {
        return productRepository.searchByName(keyword);
    }
}