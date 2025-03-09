package com.example.web.controllers;

import com.example.domain.models.Order;
import com.example.domain.models.OrderItem;
import com.example.web.models.response.OrderItemResponse;
import com.example.web.models.response.OrderResponse;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Component;

import java.util.List;
import java.util.stream.Collectors;

@Component
@RequiredArgsConstructor
public class OrderDtoMapper {
    
    private final ProductDtoMapper productDtoMapper;
    
    public OrderResponse toResponse(Order order) {
        if (order == null) {
            return null;
        }
        
        List<OrderItemResponse> itemResponses = order.getItems().stream()
                .map(this::toOrderItemResponse)
                .collect(Collectors.toList());
        
        return OrderResponse.builder()
                .id(order.getId())
                .items(itemResponses)
                .status(order.getStatus())
                .total(order.calculateTotal())
                .createdAt(order.getCreatedAt())
                .updatedAt(order.getUpdatedAt())
                .build();
    }
    
    private OrderItemResponse toOrderItemResponse(OrderItem orderItem) {
        return OrderItemResponse.builder()
                .id(orderItem.getId())
                .product(productDtoMapper.toResponse(orderItem.getProduct()))
                .quantity(orderItem.getQuantity())
                .subtotal(orderItem.calculateSubtotal())
                .build();
    }
}