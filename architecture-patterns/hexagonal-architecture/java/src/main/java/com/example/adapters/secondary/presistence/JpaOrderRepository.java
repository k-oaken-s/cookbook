package com.example.adapters.secondary.persistence;

import com.example.application.domain.Order;
import com.example.application.domain.OrderStatus;
import com.example.application.ports.output.OrderRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Component;

import java.util.List;
import java.util.Optional;
import java.util.UUID;
import java.util.stream.Collectors;

@Component
@RequiredArgsConstructor
public class JpaOrderRepository implements OrderRepository {
    
    private final SpringDataOrderRepository orderRepository;
    private final OrderPersistenceMapper orderMapper;
    
    @Override
    public Optional<Order> findById(UUID id) {
        return orderRepository.findById(id)
                .map(orderMapper::toDomain);
    }
    
    @Override
    public List<Order> findAll() {
        return orderRepository.findAll().stream()
                .map(orderMapper::toDomain)
                .collect(Collectors.toList());
    }
    
    @Override
    public Order save(Order order) {
        OrderEntity entity = orderMapper.toEntity(order);
        OrderEntity savedEntity = orderRepository.save(entity);
        return orderMapper.toDomain(savedEntity);
    }
    
    @Override
    public void deleteById(UUID id) {
        orderRepository.deleteById(id);
    }
    
    @Override
    public List<Order> findByStatus(OrderStatus status) {
        return orderRepository.findByStatus(status).stream()
                .map(orderMapper::toDomain)
                .collect(Collectors.toList());
    }
}