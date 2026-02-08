# K6 壓力測試

## 安裝 k6

```bash
# macOS
brew install k6

# Docker
docker pull grafana/k6
```

## 前置條件

1. 啟動 API 服務
2. 確保 Redis 和 PostgreSQL 正常運行
3. 確保 Kafka 正常運行（閃購測試需要）

## 測試腳本

### 1. 限流測試 (Rate Limit)

驗證: 超過限制返回 429

```bash
k6 run tests/k6/rate-limit-test.js

# 指定 API 地址
k6 run --env API_BASE=http://localhost:5000 tests/k6/rate-limit-test.js
```

**預期結果**: 部分請求返回 429 Too Many Requests

---

### 2. 樂觀鎖測試 (Optimistic Lock)

驗證: 100 併發下單同商品 (庫存 10)，最終只有 10 單成功

```bash
# 準備:
# 1. 建立一個庫存為 10 的商品
# 2. 取得有效的 JWT Token

k6 run \
  --env API_BASE=http://localhost:5000 \
  --env TOKEN=your_jwt_token \
  --env PRODUCT_ID=your_product_id \
  tests/k6/optimistic-lock-test.js
```

**預期結果**:
- 成功訂單: 10
- 衝突/庫存不足: 90

---

### 3. 閃購測試 (Flash Sale)

驗證: 1000 用戶搶 100 件商品，無超賣

```bash
# 準備:
# 1. 呼叫 WarmUpStockAsync 預熱庫存到 Redis
# 2. 取得有效的 JWT Token

k6 run \
  --env API_BASE=http://localhost:5000 \
  --env TOKEN=your_jwt_token \
  --env FLASH_SALE_ITEM_ID=your_flash_sale_item_id \
  tests/k6/flash-sale-test.js
```

**預期結果**:
- 成功搶購 <= 100
- 無超賣

---

## 測試資料準備腳本

```bash
# 建立測試商品 (庫存 10)
curl -X POST http://localhost:5000/api/products \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "name": "Test Product",
    "price": 100,
    "stockQuantity": 10,
    "isActive": true
  }'

# 預熱閃購庫存到 Redis (需要實作 API 或直接用 redis-cli)
redis-cli SET "flashsale:stock:$FLASH_SALE_ITEM_ID" 100
redis-cli SET "flashsale:purchaselimit:$FLASH_SALE_ITEM_ID" 5
```

---

## 常見問題

### Q: 401 Unauthorized
Token 無效或過期，重新登入取得新 Token

### Q: 429 太多
Rate Limiter 正常運作，這是預期行為

### Q: 成功訂單數 > 庫存
超賣發生！檢查樂觀鎖設定是否正確
