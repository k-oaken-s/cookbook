package com.example.domain.repositories;

import com.example.domain.entities.Category;

import java.util.List;
import java.util.Optional;
import java.util.UUID;

public interface CategoryRepository {
    
    /**
     * 指定されたIDのカテゴリを取得する
     *
     * @param id カテゴリID
     * @return カテゴリのOptional
     */
    Optional<Category> findById(UUID id);
    
    /**
     * すべてのカテゴリを取得する
     *
     * @return カテゴリのリスト
     */
    List<Category> findAll();
    
    /**
     * カテゴリを保存する
     *
     * @param category 保存するカテゴリ
     * @return 保存されたカテゴリ
     */
    Category save(Category category);
    
    /**
     * 指定されたIDのカテゴリを削除する
     *
     * @param id 削除するカテゴリのID
     */
    void deleteById(UUID id);
    
    /**
     * カテゴリ名でカテゴリを検索する
     *
     * @param name カテゴリ名
     * @return 検索結果のカテゴリのOptional
     */
    Optional<Category> findByName(String name);
}