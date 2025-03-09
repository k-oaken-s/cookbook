package com.example.application.ports.output;

import java.util.UUID;

/**
 * 商品在庫の管理を担当する出力ポート（セカンダリポート）
 */
public interface ProductStockManager {
    
    /**
     * 商品の在庫が指定した数量以上あるかを確認する
     *
     * @param productId 商品ID
     * @param quantity 確認する数量
     * @return 在庫が十分にある場合はtrue
     */
    boolean hasEnoughStock(UUID productId, int quantity);
    
    /**
     * 商品の在庫を予約する（在庫を確保するが、まだ引き落とさない）
     *
     * @param productId 商品ID
     * @param quantity 予約する数量
     * @return 予約が成功した場合はtrue
     */
    boolean reserveStock(UUID productId, int quantity);
    
    /**
     * 商品の在庫を解放する（予約した在庫を戻す）
     *
     * @param productId 商品ID
     * @param quantity 解放する数量
     */
    void releaseStock(UUID productId, int quantity);
    
    /**
     * 商品の在庫を確定的に減らす
     *
     * @param productId 商品ID
     * @param quantity 減らす数量
     */
    void confirmStockReduction(UUID productId, int quantity);
}