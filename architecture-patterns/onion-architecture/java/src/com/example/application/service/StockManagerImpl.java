package com.example.application.services;

import com.example.application.interfaces.ProductRepository;
import com.example.application.interfaces.StockManager;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;

import java.util.Map;
import java.util.UUID;
import java.util.concurrent.ConcurrentHashMap;

@Service
@RequiredArgsConstructor
public class StockManagerImpl implements StockManager {
    
    private final ProductRepository productRepository;
    
    // 予約済み在庫を追跡するためのマップ
    private final Map<UUID, Integer> reservedStock = new ConcurrentHashMap<>();
    
    @Override
    public boolean hasEnoughStock(UUID productId, int quantity) {
        return productRepository.findById(productId)
                .map(product -> {
                    int reserved = reservedStock.getOrDefault(productId, 0);
                    return product.getStockQuantity() - reserved >= quantity;
                })
                .orElse(false);
    }
    
    @Override
    public boolean reserveStock(UUID productId, int quantity) {
        if (!hasEnoughStock(productId, quantity)) {
            return false;
        }
        
        reservedStock.compute(productId, (id, currentReserved) -> 
                (currentReserved == null) ? quantity : currentReserved + quantity);
        
        return true;
    }
    
    @Override
    public void releaseStock(UUID productId, int quantity) {
        reservedStock.compute(productId, (id, currentReserved) -> {
            if (currentReserved == null || currentReserved < quantity) {
                return 0;
            }
            return currentReserved - quantity;
        });
    }
    
    @Override
    public void confirmStockReduction(UUID productId, int quantity) {
        // 予約済み在庫を減らす
        releaseStock(productId, quantity);
        
        // 実際の在庫を減らす
        productRepository.findById(productId).ifPresent(product -> {
            product.removeStock(quantity);
            productRepository.save(product);
        });
    }
}