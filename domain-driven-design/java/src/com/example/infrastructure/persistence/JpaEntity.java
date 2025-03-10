package com.example.infrastructure.persistence;

import com.example.domain.model.valueobject.OrderStatus;
import jakarta.persistence.*;
import lombok.Data;

import java.math.BigDecimal;
import java.time.LocalDateTime;
import java.util.ArrayList;
import java.util.List;
import java.util.UUID;

/**
 * 注文のJPAエンティティ
 */
@Entity
@Table(name = "orders")
@Data
public class OrderEntity {
    @Id
    private UUID id;
    
    private UUID customerId;
    
    @Enumerated(EnumType.STRING)
    private OrderStatus status;
    
    private BigDecimal totalAmount;
    private String currency;
    
    // 配送先住所
    private String shippingStreetAddress;
    private String shippingCity;
    private String shippingState;
    private String shippingZipCode;
    private String shippingCountry;
    
    // 請求先住所
    private String billingStreetAddress;
    private String billingCity;
    private String billingState;
    private String billingZipCode;
    private String billingCountry;
    
    // 日時情報
    private LocalDateTime createdAt;
    private LocalDateTime lastModifiedAt;
    private LocalDateTime paidAt;
    private LocalDateTime shippedAt;
    private LocalDateTime cancelledAt;
    
    // 注文項目のリレーション
    @OneToMany(mappedBy = "order", cascade = CascadeType.ALL, orphanRemoval = true)
    private List<OrderItemEntity> orderItems = new ArrayList<>();
}

/**
 * 注文項目のJPAエンティティ
 */
@Entity
@Table(name = "order_items")
@Data
public class OrderItemEntity {
    @Id
    private UUID id;
    
    @ManyToOne
    @JoinColumn(name = "order_id")
    private OrderEntity order;
    
    private UUID productId;
    private String productName;
    private BigDecimal unitPrice;
    private String currency;
    private int quantity;
}

/**
 * 商品のJPAエンティティ
 */
@Entity
@Table(name = "products")
@Data
public class ProductEntity {
    @Id
    private UUID id;
    
    private String name;
    private String description;
    private BigDecimal price;
    private String currency;
    private int stockQuantity;
    private boolean active;
}

/**
 * 顧客のJPAエンティティ
 */
@Entity
@Table(name = "customers")
@Data
public class CustomerEntity {
    @Id
    private UUID id;
    
    private String firstName;
    private String lastName;
    private String email;
    private String phoneNumber;
    private boolean active;
    private LocalDateTime registeredAt;
    private LocalDateTime lastModifiedAt;
    
    // 住所のリレーション
    @OneToMany(mappedBy = "customer", cascade = CascadeType.ALL, orphanRemoval = true)
    private List<AddressEntity> addresses = new ArrayList<>();
}

/**
 * 住所のJPAエンティティ
 */
@Entity
@Table(name = "addresses")
@Data
public class AddressEntity {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;
    
    @ManyToOne
    @JoinColumn(name = "customer_id")
    private CustomerEntity customer;
    
    private String streetAddress;
    private String city;
    private String state;
    private String zipCode;
    private String country;
}