package com.example.adapters.primary.api.request;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.util.UUID;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class AddOrderItemRequest {
    private UUID productId;
    private int quantity;
}