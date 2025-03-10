package com.example.presentation.controller;

import com.example.application.service.OrderService;
import com.example.domain.model.aggregate.Order;
import com.example.domain.model.valueobject.*;
import com.example.presentation.dto.AddOrderItemRequest;
import com.example.presentation.dto.CreateOrderRequest;
import com.example.presentation.dto.OrderResponse;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import jakarta.validation.Valid;
import java.util.List;
import java.util.UUID;
import java.util.stream.Collectors;

/**
 * 注文に関するREST APIコントローラー
 */
@RestController
@RequestMapping("/api/orders")
public class OrderController {
    private final OrderService orderService;

    public OrderController(OrderService orderService) {
        this.orderService = orderService;
    }

    /**
     * 新規注文を作成する
     * @param request 注文作成リクエスト
     * @return 作成された注文のIDを含むレスポンス
     */
    @PostMapping
    public ResponseEntity<OrderResponse> createOrder(@Valid @RequestBody CreateOrderRequest request) {
        // リクエストからドメインオブジェクトへの変換
        CustomerId customerId = CustomerId.of(request.getCustomerId());
        
        Address shippingAddress = Address.of(
                request.getShippingStreetAddress(),
                request.getShippingCity(),
                request.getShippingState(),
                request.getShippingZipCode(),
                request.getShippingCountry()
        );
        
        Address billingAddress = Address.of(
                request.getBillingStreetAddress(),
                request.getBillingCity(),
                request.getBillingState(),
                request.getBillingZipCode(),
                request.getBillingCountry()
        );
        
        // アプリケーションサービスの呼び出し
        OrderId orderId = orderService.createOrder(customerId, shippingAddress, billingAddress);
        
        // レスポンスの作成
        OrderResponse response = new OrderResponse(orderId.getValue().toString());
        return new ResponseEntity<>(response, HttpStatus.CREATED);
    }

    /**
     * 注文に商品を追加する
     * @param orderId 注文ID
     * @param request 商品追加リクエスト
     * @return HTTPステータス
     */
    @PostMapping("/{orderId}/items")
    public ResponseEntity<Void> addOrderItem(@PathVariable String orderId, 
                                             @Valid @RequestBody AddOrderItemRequest request) {
        // リクエストからドメインオブジェクトへの変換
        OrderId orderIdObj = OrderId.of(orderId);
        ProductId productId = ProductId.of(request.getProductId());
        Quantity quantity = Quantity.of(request.getQuantity());
        
        // アプリケーションサービスの呼び出し
        orderService.addOrderItem(orderIdObj, productId, quantity);
        
        return ResponseEntity.ok().build();
    }

    /**
     * 注文から商品を削除する
     * @param orderId 注文ID
     * @param orderItemId 注文項目ID
     * @return HTTPステータス
     */
    @DeleteMapping("/{orderId}/items/{orderItemId}")
    public ResponseEntity<Void> removeOrderItem(@PathVariable String orderId, 
                                               @PathVariable String orderItemId) {
        // パラメータからドメインオブジェクトへの変換
        OrderId orderIdObj = OrderId.of(orderId);
        UUID orderItemIdObj = UUID.fromString(orderItemId);
        
        // アプリケーションサービスの呼び出し
        orderService.removeOrderItem(orderIdObj, orderItemIdObj);
        
        return ResponseEntity.ok().build();
    }

    /**
     * 注文を支払い処理する
     * @param orderId 注文ID
     * @return HTTPステータス
     */
    @PostMapping("/{orderId}/pay")
    public ResponseEntity<Void> payOrder(@PathVariable String orderId) {
        // パラメータからドメインオブジェクトへの変換
        OrderId orderIdObj = OrderId.of(orderId);
        
        // アプリケーションサービスの呼び出し
        orderService.payOrder(orderIdObj);
        
        return ResponseEntity.ok().build();
    }

    /**
     * 注文をキャンセルする
     * @param orderId 注文ID
     * @return HTTPステータス
     */
    @PostMapping("/{orderId}/cancel")
    public ResponseEntity<Void> cancelOrder(@PathVariable String orderId) {
        // パラメータからドメインオブジェクトへの変換
        OrderId orderIdObj = OrderId.of(orderId);
        
        // アプリケーションサービスの呼び出し
        orderService.cancelOrder(orderIdObj);
        
        return ResponseEntity.ok().build();
    }

    /**
     * 注文を発送済みにする
     * @param orderId 注文ID
     * @return HTTPステータス
     */
    @PostMapping("/{orderId}/ship")
    public ResponseEntity<Void> shipOrder(@PathVariable String orderId) {
        // パラメータからドメインオブジェクトへの変換
        OrderId orderIdObj = OrderId.of(orderId);
        
        // アプリケーションサービスの呼び出し
        orderService.shipOrder(orderIdObj);
        
        return ResponseEntity.ok().build();
    }

    /**
     * 注文を配達完了にする
     * @param orderId 注文ID
     * @return HTTPステータス
     */
    @PostMapping("/{orderId}/deliver")
    public ResponseEntity<Void> deliverOrder(@PathVariable String orderId) {
        // パラメータからドメインオブジェクトへの変換
        OrderId orderIdObj = OrderId.of(orderId);
        
        // アプリケーションサービスの呼び出し
        orderService.deliverOrder(orderIdObj);
        
        return ResponseEntity.ok().build();
    }

    /**
     * 注文を取得する
     * @param orderId 注文ID
     * @return 注文情報
     */
    @GetMapping("/{orderId}")
    public ResponseEntity<Order> getOrder(@PathVariable String orderId) {
        // パラメータからドメインオブジェクトへの変換
        OrderId orderIdObj = OrderId.of(orderId);
        
        // アプリケーションサービスの呼び出し
        return orderService.findOrder(orderIdObj)
                .map(order -> new ResponseEntity<>(order, HttpStatus.OK))
                .orElse(new ResponseEntity<>(HttpStatus.NOT_FOUND));
    }

    /**
     * 顧客の注文履歴を取得する
     * @param customerId 顧客ID
     * @return 注文のリスト
     */
    @GetMapping("/customer/{customerId}")
    public ResponseEntity<List<Order>> getCustomerOrders(@PathVariable String customerId) {
        // パラメータからドメインオブジェクトへの変換
        CustomerId customerIdObj = CustomerId.of(customerId);
        
        // アプリケーションサービスの呼び出し
        List<Order> orders = orderService.findOrdersByCustomerId(customerIdObj);
        
        return new ResponseEntity<>(orders, HttpStatus.OK);
    }

    /**
     * 特定の状態の注文を取得する
     * @param status 注文状態
     * @return 注文のリスト
     */
    @GetMapping("/status/{status}")
    public ResponseEntity<List<Order>> getOrdersByStatus(@PathVariable String status) {
        try {
            // パラメータからドメインオブジェクトへの変換
            OrderStatus orderStatus = OrderStatus.valueOf(status.toUpperCase());
            
            // アプリケーションサービスの呼び出し
            List<Order> orders = orderService.findOrdersByStatus(orderStatus);
            
            return new ResponseEntity<>(orders, HttpStatus.OK);
        } catch (IllegalArgumentException e) {
            return new ResponseEntity<>(HttpStatus.BAD_REQUEST);
        }
    }
}