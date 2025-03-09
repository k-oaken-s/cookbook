package com.example.web.controllers;

import com.example.application.interfaces.OrderService;
import com.example.domain.models.Order;
import com.example.domain.models.OrderStatus;
import com.example.web.models.request.AddOrderItemRequest;
import com.example.web.models.request.ApplyCategoryDiscountRequest;
import com.example.web.models.response.OrderResponse;
import lombok.RequiredArgsConstructor;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.math.BigDecimal;
import java.util.List;
import java.util.UUID;
import java.util.stream.Collectors;

@RestController
@RequestMapping("/api/orders")
@RequiredArgsConstructor
public class OrderController {
    
    private final OrderService orderService;
    private final OrderDtoMapper orderDtoMapper;
    
    @PostMapping
    public ResponseEntity<OrderResponse> createOrder() {
        Order order = orderService.createOrder();
        return ResponseEntity.status(HttpStatus.CREATED)
                .body(orderDtoMapper.toResponse(order));
    }
    
    @PostMapping("/{id}/items")
    public ResponseEntity<OrderResponse> addOrderItem(
            @PathVariable UUID id,
            @RequestBody AddOrderItemRequest request) {
        
        Order order = orderService.addOrderItem(id, request.getProductId(), request.getQuantity());
        return ResponseEntity.ok(orderDtoMapper.toResponse(order));
    }
    
    @DeleteMapping("/{id}/items/{productId}")
    public ResponseEntity<OrderResponse> removeOrderItem(
            @PathVariable UUID id,
            @PathVariable UUID productId) {
        
        Order order = orderService.removeOrderItem(id, productId);
        return ResponseEntity.ok(orderDtoMapper.toResponse(order));
    }
    
    @PostMapping("/{id}/place")
    public ResponseEntity<OrderResponse> placeOrder(@PathVariable UUID id) {
        Order order = orderService.placeOrder(id);
        return ResponseEntity.ok(orderDtoMapper.toResponse(order));
    }
    
    @PostMapping("/{id}/cancel")
    public ResponseEntity<OrderResponse> cancelOrder(@PathVariable UUID id) {
        Order order = orderService.cancelOrder(id);
        return ResponseEntity.ok(orderDtoMapper.toResponse(order));
    }
    
    @PostMapping("/{id}/complete")
    public ResponseEntity<OrderResponse> completeOrder(@PathVariable UUID id) {
        Order order = orderService.completeOrder(id);
        return ResponseEntity.ok(orderDtoMapper.toResponse(order));
    }
    
    @GetMapping("/{id}")
    public ResponseEntity<OrderResponse> getOrder(@PathVariable UUID id) {
        return orderService.getOrder(id)
                .map(order -> ResponseEntity.ok(orderDtoMapper.toResponse(order)))
                .orElse(ResponseEntity.notFound().build());
    }
    
    @GetMapping
    public ResponseEntity<List<OrderResponse>> getAllOrders() {
        List<OrderResponse> orders = orderService.getAllOrders().stream()
                .map(orderDtoMapper::toResponse)
                .collect(Collectors.toList());
        
        return ResponseEntity.ok(orders);
    }
    
    @GetMapping("/status/{status}")
    public ResponseEntity<List<OrderResponse>> getOrdersByStatus(@PathVariable OrderStatus status) {
        List<OrderResponse> orders = orderService.getOrdersByStatus(status).stream()
                .map(orderDtoMapper::toResponse)
                .collect(Collectors.toList());
        
        return ResponseEntity.ok(orders);
    }
    
    @PostMapping("/{id}/calculate-tax")
    public ResponseEntity<BigDecimal> calculateTax(
            @PathVariable UUID id,
            @RequestParam double taxRate) {
        
        BigDecimal taxAmount = orderService.calculateTax(id, taxRate);
        return ResponseEntity.ok(taxAmount);
    }
    
    @PostMapping("/{id}/apply-category-discount")
    public ResponseEntity<BigDecimal> applyCategoryDiscount(
            @PathVariable UUID id,
            @RequestBody ApplyCategoryDiscountRequest request) {
        
        BigDecimal discountAmount = orderService.applyCategoryBasedDiscount(id, request.getDiscountRates());
        return ResponseEntity.ok(discountAmount);
    }
}