package com.example.presentation.dtos;

public record CreateCategoryRequest(
    String name,
    String description
) {}