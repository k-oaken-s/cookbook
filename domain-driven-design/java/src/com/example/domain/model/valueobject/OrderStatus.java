package com.example.domain.model.valueobject;

/**
 * 注文の状態を表す列挙型
 * DDDでは列挙型も値オブジェクトの一種として扱うことができる
 */
public enum OrderStatus {
    CREATED("作成済み"),
    PENDING_PAYMENT("支払い待ち"),
    PAID("支払い完了"),
    PROCESSING("処理中"),
    SHIPPED("発送済み"),
    DELIVERED("配達完了"),
    CANCELLED("キャンセル済み"),
    RETURNED("返品済み");

    private final String displayName;

    OrderStatus(String displayName) {
        this.displayName = displayName;
    }

    public String getDisplayName() {
        return displayName;
    }

    public boolean isEditable() {
        return this == CREATED || this == PENDING_PAYMENT;
    }

    public boolean isCancellable() {
        return this == CREATED || this == PENDING_PAYMENT || this == PAID || this == PROCESSING;
    }

    public boolean isShippable() {
        return this == PAID || this == PROCESSING;
    }

    public boolean isFinalized() {
        return this == DELIVERED || this == CANCELLED || this == RETURNED;
    }
}