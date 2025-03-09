package com.example.adapters.primary.api;

import com.example.adapters.primary.api.request.AddOrderItemRequest;
import com.example.adapters.primary.api.response.OrderResponse;
import com.example.application.domain.Order;
import com.example.application.domain.OrderStatus;
import com.example.application.ports.input.OrderService;
import lombok.RequiredArgsConstructor;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.util.List;
import java.util.UUID;
import java.util.stream.Collectors;

@RestController
@RequestMapping("/api/orders")
@RequiredArgsConstructor
public class OrderController {
    
    private final OrderService orderService;
    private final OrderMapper orderMapper;
    
    @PostMapping
    public ResponseEntity<OrderResponse> createOrder() {
        Order order = orderService.createOrder();
        return ResponseEntity.status(HttpStatus.CREATED)
                .body(orderMapper.toResponse(order));
    }
    
    @PostMapping("/{id}/items")
    public ResponseEntity<OrderResponse> addOrderItem(
            @PathVariable UUID id,
            @RequestBody AddOrderItemRequest request) {
        
        Order order = orderService.addOrderItem(id, request.getProductId(), request.getQuantity());
        return ResponseEntity.ok(orderMapper.toResponse(order));
    }
    
    @DeleteMapping("/{id}/items/{productId}")
    public ResponseEntity<OrderResponse> removeOrderItem(
            @PathVariable UUID id,
            @PathVariable UUID productId) {
        
        Order order = orderService.removeOrderItem(id, productId);
        return ResponseEntity.ok(orderMapper.toResponse(order));
    }
    
    @PostMapping("/{id}/place")
    public ResponseEntity<OrderResponse> placeOrder(@PathVariable UUID id) {
        Order order = orderService.placeOrder(id);
        return ResponseEntity.ok(orderMapper.toResponse(order));
    }
    
    @PostMapping("/{id}/cancel")
    public ResponseEntity<OrderResponse> cancelOrder(@PathVariable UUID id) {
        Order order = orderService.cancelOrder(id);
        return ResponseEntity.ok(orderMapper.toResponse(order));
    }
    
    @PostMapping("/{id}/complete")
    public ResponseEntity<OrderResponse> completeOrder(@PathVariable UUID id) {
        Order order = orderService.completeOrder(id);
        return ResponseEntity.ok(orderMapper.toResponse(order));
    }
    
    @GetMapping("/{id}")
    public ResponseEntity<OrderResponse> getOrder(@PathVariable UUID id) {
        return orderService.getOrder(id)
                .map(order -> ResponseEntity.ok(orderMapper.toResponse(order)))
                .orElse(ResponseEntity.notFound().build());
    }
    
    @GetMapping
    public ResponseEntity<List<OrderResponse>> getAllOrders() {
        List<OrderResponse> orders = orderService.getAllOrders().stream()
                .map(orderMapper::toResponse)
                .collect(Collectors.toList());
        
        return ResponseEntity.ok(orders);
    }
    
    @GetMapping("/status/{status}")
    public ResponseEntity<List<OrderResponse>> getOrdersByStatus(@PathVariable OrderStatus status) {
        List<OrderResponse> orders = orderService.getOrdersByStatus(status).stream()
                .map(orderMapper::toResponse)
                .collect(Collectors.toList());
        
        return ResponseEntity.ok(orders);
    }
}