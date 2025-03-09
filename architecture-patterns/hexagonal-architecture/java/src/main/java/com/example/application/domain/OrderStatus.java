package com.example.application.domain;

public enum OrderStatus {
    CREATED,    // 作成済み
    PLACED,     // 確定済み
    CANCELLED,  // キャンセル済み
    COMPLETED   // 完了
}