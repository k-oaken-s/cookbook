package com.example.domain.services;

import com.example.domain.models.Category;
import jakarta.validation.constraints.NotNull;

import java.util.Set;

/**
 * カテゴリドメインに関連する横断的な処理を提供するサービス
 */
public class CategoryDomainService {

    /**
     * カテゴリ階層関係をチェックし、循環参照が存在しないことを確認する
     * （オニオンアーキテクチャのサンプル例なので実装は省略）
     * 
     * @param categories カテゴリのセット
     * @return 循環参照がない場合はtrue
     */
    public boolean validateHierarchy(@NotNull Set<Category> categories) {
        // 実際の実装では、カテゴリ間の親子関係を追跡し、
        // 循環参照がないことを確認するロジックが入ります
        return true;
    }
    
    /**
     * 2つのカテゴリが関連しているかどうかを判定する
     * （オニオンアーキテクチャのサンプル例なので実装は省略）
     * 
     * @param category1 カテゴリ1
     * @param category2 カテゴリ2
     * @return 関連している場合はtrue
     */
    public boolean areRelated(@NotNull Category category1, @NotNull Category category2) {
        // 実際の実装では、カテゴリ間の関連性を判定するロジックが入ります
        return category1.getId().equals(category2.getId());
    }
}