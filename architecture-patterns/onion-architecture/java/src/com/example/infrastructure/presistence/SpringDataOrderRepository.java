package com.example.infrastructure.persistence;

import com.example.domain.models.OrderStatus;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import java.util.List;
import java.util.UUID;

@Repository
public interface SpringDataOrderRepository extends JpaRepository<OrderEntity, UUID> {
    List<OrderEntity> findByStatus(OrderStatus status);
}