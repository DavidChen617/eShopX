# eShopX 高併發架構 - 進度追蹤

> 目標: 1K-10K QPS 電商平台

---

## 已完成

### Phase 1 - 核心修復
- [x] **1.1 庫存樂觀鎖** - PostgreSQL xmin + 重試機制 (`71a0fa4`)
- [x] **1.2 修復 Redis Keys()** - 改用 KeysAsync + SCAN (`298d183`)
- [x] **1.3 Rate Limiting** - 全域 100/s, 閃購 5/s per user (`5a1afc9`)
- [x] **1.4 DB 連線池優化** - Batch + RetryOnFailure (`4b8998f`)

### Phase 2 - 閃購功能
- [x] **2.1 Redis 庫存預扣** - Lua Script 原子扣減 (`46da4ba`)
- [x] **2.2 閃購購買 API** - FlashSalePurchaseEndpoint (`46da4ba`)
- [x] **2.3 Kafka 整合** - Producer + Consumer (`46da4ba`)
- [x] **2.4 快取雪崩防護** - TTL ±10% 隨機偏移 (`212a85d`)

---

## 待完成 (Phase 3 - 可選)

### 3.1 讀寫分離
- [ ] 新增 `ReadOnlyDbContext.cs` - 連接 Replica
- [ ] 修改查詢 Handler 使用 ReadOnly Context
- [ ] 設定 PostgreSQL 主從複製

**效益**: 讀寫分離可提升讀取性能 2-3 倍

### 3.2 監控告警
- [ ] 安裝 `prometheus-net` 套件
- [ ] 新增 `/metrics` 端點
- [ ] 設定 Grafana Dashboard
- [ ] 設定告警規則 (QPS、延遲、錯誤率)

**效益**: 即時監控系統健康狀態

### 3.3 快取預熱
- [ ] 閃購開始前自動載入庫存到 Redis
- [ ] 新增 `FlashSaleWarmUpService.cs`

---

## 驗證清單

- [ ] 樂觀鎖測試: 100 併發下單同商品 (庫存 10)，最終只有 10 單成功
- [ ] 限流測試: k6 壓測，超過限制返回 429
- [ ] 閃購測試: 1000 用戶搶 100 件，無超賣
- [ ] Kafka 測試: 訂單非同步建立成功

---

## 閃購架構流程

```
用戶請求
    ↓
Rate Limiter (5 req/sec per user)
    ↓
FlashSalePurchaseEndpoint
    ↓
Redis Lua Script (原子扣減 + 限購檢查)
    ↓ 成功
Kafka Producer → flash-sale-orders
    ↓
返回 202 Accepted
    ↓ (異步)
Kafka Consumer
    ↓
建立訂單 (樂觀鎖)
    ↓ 失敗
Redis 回滾庫存
```

---

## Commit 歷史

| Commit | 說明 |
|--------|------|
| `71a0fa4` | feat(order): add optimistic locking to prevent overselling |
| `298d183` | fix(redis): replace blocking Keys() with KeysAsync() |
| `5a1afc9` | feat(api): add rate limiting for DDoS protection |
| `4b8998f` | feat(db): optimize connection pool and add retry logic |
| `46da4ba` | feat(flash-sale): implement flash sale purchase with Kafka |
| `212a85d` | fix(cache): add TTL jitter to prevent cache avalanche |
