package com.example.infrastructure.persistence;

import com.example.application.interfaces.OrderRepository;
import com.example.domain.models.Order;
import com.example.domain.models.OrderStatus;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Component;

import java.util.List;
import java.util.Optional;
import java.util.UUID;
import java.util.stream.Collectors;

@Component
@RequiredArgsConstructor
public class OrderRepositoryImpl implements OrderRepository {
    
    private final SpringDataOrderRepository springDataOrderRepository;
    private final OrderPersistenceMapper orderMapper;
    
    @Override
    public Optional<Order> findById(UUID id) {
        return springDataOrderRepository.findById(id)
                .map(orderMapper::toDomain);
    }
    
    @Override
    public List<Order> findAll() {
        return springDataOrderRepository.findAll().stream()
                .map(orderMapper::toDomain)
                .collect(Collectors.toList());
    }
    
    @Override
    public Order save(Order order) {
        OrderEntity entity = orderMapper.toEntity(order);
        OrderEntity savedEntity = springDataOrderRepository.save(entity);
        return orderMapper.toDomain(savedEntity);
    }
    
    @Override
    public void deleteById(UUID id) {
        springDataOrderRepository.deleteById(id);
    }
    
    @Override
    public List<Order> findByStatus(OrderStatus status) {
        return springDataOrderRepository.findByStatus(status).stream()
                .map(orderMapper::toDomain)
                .collect(Collectors.toList());
    }
}