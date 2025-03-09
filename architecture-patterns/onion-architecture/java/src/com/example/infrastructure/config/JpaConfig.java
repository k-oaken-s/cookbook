package com.example.infrastructure.config;

import org.springframework.boot.autoconfigure.domain.EntityScan;
import org.springframework.context.annotation.Configuration;
import org.springframework.data.jpa.repository.config.EnableJpaRepositories;

@Configuration
@EntityScan("com.example.infrastructure.persistence")
@EnableJpaRepositories("com.example.infrastructure.persistence")
public class JpaConfig {
    // JPA設定
}