package com.example.domain.event;

/**
 * ドメインイベントのパブリッシャーインターフェース
 * イベント発行の抽象化を提供します
 */
public interface DomainEventPublisher {
    /**
     * ドメインイベントを発行します
     * @param event 発行するイベント
     */
    void publish(Object event);
}