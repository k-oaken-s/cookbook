package com.example.adapters.primary.api;

import com.example.adapters.primary.api.response.OrderItemResponse;
import com.example.adapters.primary.api.response.OrderResponse;
import com.example.application.domain.Order;
import com.example.application.domain.OrderItem;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Component;

import java.util.List;
import java.util.stream.Collectors;

@Component
@RequiredArgsConstructor
public class OrderMapper {
    
    private final ProductMapper productMapper;
    
    public OrderResponse toResponse(Order order) {
        if (order == null) {
            return null;
        }
        
        List<OrderItemResponse> itemResponses = order.getItems().stream()
                .map(this::toOrderItemResponse)
                .collect(Collectors.toList());
        
        return new OrderResponse(
                order.getId(),
                itemResponses,
                order.getStatus(),
                order.calculateTotal(),
                order.getCreatedAt(),
                order.getUpdatedAt()
        );
    }
    
    private OrderItemResponse toOrderItemResponse(OrderItem orderItem) {
        return new OrderItemResponse(
                orderItem.getId(),
                productMapper.toResponse(orderItem.getProduct()),
                orderItem.getQuantity(),
                orderItem.calculateSubtotal()
        );
    }
}