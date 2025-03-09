package com.example.presentation.dtos;

import java.math.BigDecimal;
import java.util.UUID;

public record CreateProductRequest(
    String name,
    String description,
    BigDecimal price,
    int stockQuantity,
    UUID categoryId
) {}