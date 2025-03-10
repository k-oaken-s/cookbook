package com.example.infrastructure.repository;

import com.example.domain.model.aggregate.Order;
import com.example.domain.model.entity.OrderItem;
import com.example.domain.model.valueobject.*;
import com.example.domain.repository.OrderRepository;
import com.example.infrastructure.persistence.OrderEntity;
import com.example.infrastructure.persistence.OrderItemEntity;
import org.springframework.stereotype.Repository;
import org.springframework.transaction.annotation.Transactional;

import jakarta.persistence.EntityManager;
import jakarta.persistence.PersistenceContext;
import jakarta.persistence.TypedQuery;
import java.time.LocalDateTime;
import java.util.ArrayList;
import java.util.List;
import java.util.Optional;
import java.util.stream.Collectors;

/**
 * JPA を使用した OrderRepository の実装
 */
@Repository
public class JpaOrderRepository implements OrderRepository {

    @PersistenceContext
    private EntityManager entityManager;

    @Override
    public Optional<Order> findById(OrderId id) {
        OrderEntity orderEntity = entityManager.find(OrderEntity.class, id.getValue());
        return Optional.ofNullable(orderEntity).map(this::mapToDomainModel);
    }

    @Override
    public List<Order> findByCustomerId(CustomerId customerId) {
        TypedQuery<OrderEntity> query = entityManager.createQuery(
                "SELECT o FROM OrderEntity o WHERE o.customerId = :customerId",
                OrderEntity.class);
        query.setParameter("customerId", customerId.getValue());
        
        return query.getResultList().stream()
                .map(this::mapToDomainModel)
                .collect(Collectors.toList());
    }

    @Override
    public List<Order> findByStatus(OrderStatus status) {
        TypedQuery<OrderEntity> query = entityManager.createQuery(
                "SELECT o FROM OrderEntity o WHERE o.status = :status",
                OrderEntity.class);
        query.setParameter("status", status);
        
        return query.getResultList().stream()
                .map(this::mapToDomainModel)
                .collect(Collectors.toList());
    }

    @Override
    public List<Order> findByCreatedAtBetween(LocalDateTime startDate, LocalDateTime endDate) {
        TypedQuery<OrderEntity> query = entityManager.createQuery(
                "SELECT o FROM OrderEntity o WHERE o.createdAt BETWEEN :startDate AND :endDate",
                OrderEntity.class);
        query.setParameter("startDate", startDate);
        query.setParameter("endDate", endDate);
        
        return query.getResultList().stream()
                .map(this::mapToDomainModel)
                .collect(Collectors.toList());
    }

    @Override
    @Transactional
    public Order save(Order order) {
        OrderEntity orderEntity = mapToEntity(order);
        
        if (entityManager.find(OrderEntity.class, orderEntity.getId()) == null) {
            entityManager.persist(orderEntity);
        } else {
            orderEntity = entityManager.merge(orderEntity);
        }
        
        return mapToDomainModel(orderEntity);
    }

    @Override
    @Transactional
    public void deleteById(OrderId id) {
        OrderEntity orderEntity = entityManager.find(OrderEntity.class, id.getValue());
        if (orderEntity != null) {
            entityManager.remove(orderEntity);
        }
    }

    // ドメインモデルからJPAエンティティへの変換
    private OrderEntity mapToEntity(Order order) {
        OrderEntity orderEntity = new OrderEntity();
        orderEntity.setId(order.getId().getValue());
        orderEntity.setCustomerId(order.getCustomerId().getValue());
        orderEntity.setStatus(order.getStatus());
        orderEntity.setTotalAmount(order.getTotalAmount().getAmount());
        orderEntity.setCurrency(order.getTotalAmount().getCurrency().getCurrencyCode());
        orderEntity.setShippingStreetAddress(order.getShippingAddress().getStreetAddress());
        orderEntity.setShippingCity(order.getShippingAddress().getCity());
        orderEntity.setShippingState(order.getShippingAddress().getState());
        orderEntity.setShippingZipCode(order.getShippingAddress().getZipCode());
        orderEntity.setShippingCountry(order.getShippingAddress().getCountry());
        orderEntity.setBillingStreetAddress(order.getBillingAddress().getStreetAddress());
        orderEntity.setBillingCity(order.getBillingAddress().getCity());
        orderEntity.setBillingState(order.getBillingAddress().getState());
        orderEntity.setBillingZipCode(order.getBillingAddress().getZipCode());
        orderEntity.setBillingCountry(order.getBillingAddress().getCountry());
        orderEntity.setCreatedAt(order.getCreatedAt());
        orderEntity.setLastModifiedAt(order.getLastModifiedAt());
        orderEntity.setPaidAt(order.getPaidAt());
        orderEntity.setShippedAt(order.getShippedAt());
        orderEntity.setCancelledAt(order.getCancelledAt());
        
        // 注文項目の変換
        List<OrderItemEntity> orderItemEntities = new ArrayList<>();
        for (OrderItem orderItem : order.getOrderItems()) {
            OrderItemEntity itemEntity = new OrderItemEntity();
            itemEntity.setId(orderItem.getId());
            itemEntity.setOrder(orderEntity);
            itemEntity.setProductId(orderItem.getProductId().getValue());
            itemEntity.setProductName(orderItem.getProductName());
            itemEntity.setUnitPrice(orderItem.getUnitPrice().getAmount());
            itemEntity.setCurrency(orderItem.getUnitPrice().getCurrency().getCurrencyCode());
            itemEntity.setQuantity(orderItem.getQuantity().getValue());
            orderItemEntities.add(itemEntity);
        }
        
        orderEntity.setOrderItems(orderItemEntities);
        
        return orderEntity;
    }

    // JPAエンティティからドメインモデルへの変換
    private Order mapToDomainModel(OrderEntity entity) {
        // 住所の作成
        Address shippingAddress = Address.of(
                entity.getShippingStreetAddress(),
                entity.getShippingCity(),
                entity.getShippingState(),
                entity.getShippingZipCode(),
                entity.getShippingCountry()
        );
        
        Address billingAddress = Address.of(
                entity.getBillingStreetAddress(),
                entity.getBillingCity(),
                entity.getBillingState(),
                entity.getBillingZipCode(),
                entity.getBillingCountry()
        );
        
        // 注文項目の変換
        List<OrderItem> orderItems = entity.getOrderItems().stream()
                .map(item -> OrderItem.reconstitute(
                        item.getId(),
                        ProductId.of(item.getProductId()),
                        item.getProductName(),
                        Money.of(item.getUnitPrice(), java.util.Currency.getInstance(item.getCurrency())),
                        Quantity.of(item.getQuantity())
                ))
                .collect(Collectors.toList());
        
        // 注文の作成
        return Order.reconstitute(
                OrderId.of(entity.getId()),
                CustomerId.of(entity.getCustomerId()),
                shippingAddress,
                billingAddress,
                entity.getStatus(),
                orderItems,
                Money.of(entity.getTotalAmount(), java.util.Currency.getInstance(entity.getCurrency())),
                entity.getCreatedAt(),
                entity.getLastModifiedAt(),
                entity.getPaidAt(),
                entity.getShippedAt(),
                entity.getCancelledAt()
        );
    }
}