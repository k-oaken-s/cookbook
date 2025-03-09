package com.example.infrastructure.persistence;

import com.example.domain.models.Order;
import com.example.domain.models.OrderItem;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Component;

import java.util.ArrayList;
import java.util.List;
import java.util.stream.Collectors;

@Component
@RequiredArgsConstructor
public class OrderPersistenceMapper {
    
    private final ProductPersistenceMapper productMapper;
    
    public Order toDomain(OrderEntity entity) {
        if (entity == null) {
            return null;
        }
        
        List<OrderItem> items = entity.getItems().stream()
                .map(this::toDomainOrderItem)
                .collect(Collectors.toList());
        
        return new Order(
                entity.getId(),
                items,
                entity.getStatus(),
                entity.getCreatedAt(),
                entity.getUpdatedAt()
        );
    }
    
    public OrderEntity toEntity(Order domain) {
        if (domain == null) {
            return null;
        }
        
        OrderEntity orderEntity = OrderEntity.builder()
                .id(domain.getId())
                .status(domain.getStatus())
                .createdAt(domain.getCreatedAt())
                .updatedAt(domain.getUpdatedAt())
                .items(new ArrayList<>())
                .build();
        
        List<OrderItemEntity> itemEntities = domain.getItems().stream()
                .map(item -> toEntityOrderItem(item, orderEntity))
                .collect(Collectors.toList());
        
        orderEntity.setItems(itemEntities);
        
        return orderEntity;
    }
    
    private OrderItem toDomainOrderItem(OrderItemEntity entity) {
        return new OrderItem(
                entity.getId(),
                productMapper.toDomain(entity.getProduct()),
                entity.getQuantity()
        );
    }
    
    private OrderItemEntity toEntityOrderItem(OrderItem domain, OrderEntity orderEntity) {
        return OrderItemEntity.builder()
                .id(domain.getId())
                .order(orderEntity)
                .product(productMapper.toEntity(domain.getProduct()))
                .quantity(domain.getQuantity())
                .build();
    }
}