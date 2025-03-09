package com.example.presentation.dtos;

import java.time.LocalDateTime;

public record ErrorResponse(
    String message,
    int status,
    LocalDateTime timestamp
) {}