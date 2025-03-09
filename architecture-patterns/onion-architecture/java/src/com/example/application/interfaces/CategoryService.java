package com.example.application.interfaces;

import com.example.domain.models.Category;

import java.util.List;
import java.util.Optional;
import java.util.UUID;

/**
 * カテゴリ管理に関するサービスインターフェース
 */
public interface CategoryService {
    
    /**
     * 新しいカテゴリを作成する
     * 
     * @param name カテゴリ名
     * @param description カテゴリ説明
     * @return 作成されたカテゴリ
     */
    Category createCategory(String name, String description);
    
    /**
     * IDでカテゴリを取得する
     * 
     * @param id カテゴリID
     * @return カテゴリ（存在しない場合はEmpty）
     */
    Optional<Category> getCategory(UUID id);
    
    /**
     * すべてのカテゴリを取得する
     * 
     * @return カテゴリリスト
     */
    List<Category> getAllCategories();
    
    /**
     * カテゴリを更新する
     * 
     * @param id カテゴリID
     * @param name カテゴリ名
     * @param description カテゴリ説明
     * @return 更新されたカテゴリ（存在しない場合はEmpty）
     */
    Optional<Category> updateCategory(UUID id, String name, String description);
    
    /**
     * カテゴリを削除する
     * 
     * @param id カテゴリID
     */
    void deleteCategory(UUID id);
    
    /**
     * カテゴリ名でカテゴリを検索する
     * 
     * @param name カテゴリ名
     * @return カテゴリ（存在しない場合はEmpty）
     */
    Optional<Category> getCategoryByName(String name);
}