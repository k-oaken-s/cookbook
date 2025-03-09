package com.example.adapters.secondary.persistence;

import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;
import org.springframework.stereotype.Repository;

import java.util.List;
import java.util.UUID;

@Repository
public interface SpringDataProductRepository extends JpaRepository<ProductEntity, UUID> {
    List<ProductEntity> findByCategoryId(UUID categoryId);
    
    @Query("SELECT p FROM ProductEntity p WHERE p.name LIKE %:keyword%")
    List<ProductEntity> searchByNameContaining(@Param("keyword") String keyword);
}