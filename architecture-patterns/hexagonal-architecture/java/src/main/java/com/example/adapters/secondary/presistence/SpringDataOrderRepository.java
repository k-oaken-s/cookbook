package com.example.adapters.secondary.persistence;

import com.example.application.domain.OrderStatus;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import java.util.List;
import java.util.UUID;

@Repository
public interface SpringDataOrderRepository extends JpaRepository<OrderEntity, UUID> {
    List<OrderEntity> findByStatus(OrderStatus status);
}