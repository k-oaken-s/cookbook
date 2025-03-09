package com.example.presentation.dtos;

import java.math.BigDecimal;

public record UpdateProductRequest(
    String name,
    String description,
    BigDecimal price
) {}