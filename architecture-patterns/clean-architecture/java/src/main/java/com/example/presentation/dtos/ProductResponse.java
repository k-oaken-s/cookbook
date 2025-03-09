package com.example.presentation.dtos;

import java.math.BigDecimal;
import java.util.UUID;

public record ProductResponse(
    UUID id,
    String name,
    String description,
    BigDecimal price,
    int stockQuantity,
    CategoryResponse category
) {}