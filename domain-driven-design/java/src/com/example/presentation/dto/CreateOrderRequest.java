package com.example.presentation.dto;

import jakarta.validation.constraints.Min;
import jakarta.validation.constraints.NotBlank;
import jakarta.validation.constraints.NotNull;
import lombok.Data;

/**
 * 注文作成リクエストDTO
 */
@Data
public class CreateOrderRequest {
    @NotBlank(message = "Customer ID is required")
    private String customerId;
    
    // 配送先住所
    @NotBlank(message = "Shipping street address is required")
    private String shippingStreetAddress;
    @NotBlank(message = "Shipping city is required")
    private String shippingCity;
    @NotBlank(message = "Shipping state is required")
    private String shippingState;
    @NotBlank(message = "Shipping zip code is required")
    private String shippingZipCode;
    @NotBlank(message = "Shipping country is required")
    private String shippingCountry;
    
    // 請求先住所
    @NotBlank(message = "Billing street address is required")
    private String billingStreetAddress;
    @NotBlank(message = "Billing city is required")
    private String billingCity;
    @NotBlank(message = "Billing state is required")
    private String billingState;
    @NotBlank(message = "Billing zip code is required")
    private String billingZipCode;
    @NotBlank(message = "Billing country is required")
    private String billingCountry;
}