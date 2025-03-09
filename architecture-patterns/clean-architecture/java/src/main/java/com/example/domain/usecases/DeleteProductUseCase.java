package com.example.domain.usecases;

import com.example.domain.repositories.ProductRepository;
import lombok.RequiredArgsConstructor;

import java.util.UUID;

@RequiredArgsConstructor
public class DeleteProductUseCase {
    private final ProductRepository productRepository;

    public void execute(UUID id) {
        if (id == null) {
            throw new IllegalArgumentException("商品IDは必須です");
        }
        
        // 商品の存在チェック
        if (productRepository.findById(id).isEmpty()) {
            throw new IllegalArgumentException("指定された商品が見つかりません: " + id);
        }
        
        productRepository.deleteById(id);
    }
}