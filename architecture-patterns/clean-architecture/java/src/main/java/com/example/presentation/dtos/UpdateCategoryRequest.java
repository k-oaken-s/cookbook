package com.example.presentation.dtos;

public record UpdateCategoryRequest(
    String name,
    String description
) {}