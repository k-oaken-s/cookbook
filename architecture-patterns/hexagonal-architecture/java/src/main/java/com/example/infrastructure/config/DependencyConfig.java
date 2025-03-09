package com.example.infrastructure.config;

import com.example.adapters.secondary.persistence.JpaCategoryRepository;
import com.example.adapters.secondary.persistence.JpaOrderRepository;
import com.example.adapters.secondary.persistence.JpaProductRepository;
import com.example.adapters.secondary.services.EmailNotificationService;
import com.example.adapters.secondary.services.ProductStockManagerImpl;
import com.example.application.ports.input.CategoryService;
import com.example.application.ports.input.OrderService;
import com.example.application.ports.input.ProductService;
import com.example.application.ports.output.CategoryRepository;
import com.example.application.ports.output.NotificationService;
import com.example.application.ports.output.OrderRepository;
import com.example.application.ports.output.ProductRepository;
import com.example.application.ports.output.ProductStockManager;
import com.example.application.ports.service.CategoryServiceImpl;
import com.example.application.ports.service.OrderServiceImpl;
import com.example.application.ports.service.ProductServiceImpl;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;

@Configuration
public class DependencyConfig {

    // 入力ポート（プライマリポート）
    @Bean
    public ProductService productService(
            ProductRepository productRepository,
            CategoryRepository categoryRepository,
            NotificationService notificationService) {
        return new ProductServiceImpl(productRepository, categoryRepository, notificationService);
    }

    @Bean
    public CategoryService categoryService(CategoryRepository categoryRepository) {
        return new CategoryServiceImpl(categoryRepository);
    }

    @Bean
    public OrderService orderService(
            OrderRepository orderRepository,
            ProductRepository productRepository,
            ProductStockManager productStockManager,
            NotificationService notificationService) {
        return new OrderServiceImpl(orderRepository, productRepository, productStockManager, notificationService);
    }

    // 出力ポート（セカンダリポート）
    @Bean
    public ProductRepository productRepository(JpaProductRepository jpaProductRepository) {
        return jpaProductRepository;
    }

    @Bean
    public CategoryRepository categoryRepository(JpaCategoryRepository jpaCategoryRepository) {
        return jpaCategoryRepository;
    }

    @Bean
    public OrderRepository orderRepository(JpaOrderRepository jpaOrderRepository) {
        return jpaOrderRepository;
    }

    @Bean
    public NotificationService notificationService(EmailNotificationService emailNotificationService) {
        return emailNotificationService;
    }

    @Bean
    public ProductStockManager productStockManager(ProductStockManagerImpl productStockManagerImpl) {
        return productStockManagerImpl;
    }
}