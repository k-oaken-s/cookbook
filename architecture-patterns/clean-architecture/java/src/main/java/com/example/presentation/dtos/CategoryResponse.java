package com.example.presentation.dtos;

import java.util.UUID;

public record CategoryResponse(
    UUID id,
    String name,
    String description
) {}