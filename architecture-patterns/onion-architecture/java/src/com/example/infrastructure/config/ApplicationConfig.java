package com.example.infrastructure.config;

import com.example.domain.services.CategoryDomainService;
import com.example.domain.services.OrderDomainService;
import com.example.domain.services.ProductDomainService;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;

@Configuration
public class ApplicationConfig {
    
    @Bean
    public ProductDomainService productDomainService() {
        return new ProductDomainService();
    }
    
    @Bean
    public CategoryDomainService categoryDomainService() {
        return new CategoryDomainService();
    }
    
    @Bean
    public OrderDomainService orderDomainService() {
        return new OrderDomainService();
    }
}