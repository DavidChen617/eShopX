# eShopX API Spec

> 前端重寫參考文件。所有端點皆使用版本前綴 `/api/v1/`。
> 需要 JWT 的端點請在 Header 帶上 `Authorization: Bearer <accessToken>`。

---

## Auth

| Method | URL | Auth | 說明 |
|--------|-----|------|------|
| POST | `/api/v1/auth/register` | ✗ | 註冊 |
| POST | `/api/v1/auth/login` | ✗ | 本地登入 |
| POST | `/api/v1/auth/google` | ✗ | Google 登入 |
| POST | `/api/v1/auth/line` | ✗ | LINE 登入 |
| POST | `/api/v1/auth/refresh` | ✗ | 更新 Token |
| POST | `/api/v1/auth/logout` | ✗ | 登出 |

### POST `/api/v1/auth/register`
```json
// Request
{ "name": "string", "email": "string", "password": "string (min 8)" }

// Response 201
{ "userId": "uuid", "email": "string", "createdAt": "datetime" }
```

### POST `/api/v1/auth/login`
```json
// Request
{ "email": "string", "password": "string" }

// Response 200
{ "accessToken": "string", "refreshToken": "string", "userId": "uuid", "name": "string", "expiresAt": "datetime" }
```

### POST `/api/v1/auth/google`
```json
// Request（前端先走 Google PKCE OAuth flow 取得 code + codeVerifier）
{ "code": "string", "codeVerifier": "string" }

// Response 200
{ "accessToken": "string", "refreshToken": "string", "userId": "uuid", "name": "string", "expiresAt": "datetime", "sub": "string", "email": "string", "picture": "string?" }
```

### POST `/api/v1/auth/line`
```json
// Request（前端先走 LINE PKCE OAuth flow 取得 code + codeVerifier + nonce）
{ "code": "string", "codeVerifier": "string", "nonce": "string" }

// Response 200
{ "accessToken": "string", "refreshToken": "string", "userId": "uuid", "name": "string", "expiresAt": "datetime", "sub": "string", "email": "string" }
```

### POST `/api/v1/auth/refresh`
```json
// Request
{ "refreshToken": "string" }

// Response 200
{ "accessToken": "string", "refreshToken": "string", "expiresAt": "datetime" }
```

### POST `/api/v1/auth/logout`
```json
// Request
{ "refreshToken": "string" }

// Response 204
```

---

## Users

| Method | URL | Auth | 說明 |
|--------|-----|------|------|
| POST | `/api/v1/users/me/avatar` | ✓ | 上傳頭像 |

### POST `/api/v1/users/me/avatar`
```
// Request: multipart/form-data
file: File

// Response 200
{ "url": "string" }
```

> ⚠️ 前端目前有 `GET /api/users/me`、`PUT /api/users/me`、`DELETE /api/users/me/avatar`，後端尚未實作。

---

## Products（公開）

| Method | URL | Auth | 說明 |
|--------|-----|------|------|
| GET | `/api/v1/products` | ✗ | 取得商品列表 |
| GET | `/api/v1/products/{productId}` | ✗ | 取得單一商品 |
| GET | `/api/v1/products/search` | ✗ | 搜尋商品（Elasticsearch） |

### GET `/api/v1/products`
```
// Query Params
?categoryId=uuid&audience=0|1&isActive=bool&page=int&pageSize=int

// Response 200
{
  "items": [{ "productId": "uuid", "name": "string", "isActive": "bool", ... }],
  "totalCount": int, "page": int, "pageSize": int
}
```

### GET `/api/v1/products/{productId}`
```json
// Response 200
{
  "productId": "uuid", "name": "string", "description": "string?",
  "audience": 0,
  "categoryId": "uuid",
  "isActive": "bool",
  "tags": [{ "tagId": "uuid" }],
  "variants": [{
    "variantId": "uuid", "color": "string",
    "skus": [{ "skuId": "uuid", "sizeId": "uuid?", "price": 0.0, "stock": 0 }],
    "images": [{ "imageId": "uuid", "url": "string", "isPrimary": "bool", "sortOrder": 0 }]
  }]
}
```

### GET `/api/v1/products/search`
```
// Query Params
?keyword=string&categoryId=uuid&minPrice=decimal&maxPrice=decimal&isActive=bool&page=int&pageSize=int

// Response 200
{
  "page": int, "pageSize": int, "totalCount": int, "totalPages": int,
  "items": [{ "productId": "uuid", "name": "string", "price": 0.0, "primaryImageUrl": "string?", ... }]
}
```

---

## Cart

| Method | URL | Auth | 說明 |
|--------|-----|------|------|
| GET | `/api/v1/cart` | ✓ | 取得購物車 |
| POST | `/api/v1/cart/items` | ✓ | 加入商品 |
| PUT | `/api/v1/cart/items/{skuId}` | ✓ | 更新數量 |
| DELETE | `/api/v1/cart/items/{skuId}` | ✓ | 移除商品 |
| DELETE | `/api/v1/cart` | ✓ | 清空購物車 |

> ⚠️ 前端目前 URL 帶 `userId`（如 `/api/carts/{userId}/items`），後端不需要 userId，從 JWT 取得。

### GET `/api/v1/cart`
```json
// Response 200
{
  "cartId": "uuid?",
  "userId": "uuid",
  "items": [{
    "itemId": "uuid", "skuId": "uuid", "quantity": 0,
    "productName": "string", "color": "string", "sizeName": "string?",
    "unitPrice": 0.0, "primaryImageUrl": "string?"
  }]
}
```
> Cart 不存在時回傳 `cartId: null, items: []`，不回 404。

### POST `/api/v1/cart/items`
```json
// Request
{ "skuId": "uuid", "quantity": int }
// Response 204
```

### PUT `/api/v1/cart/items/{skuId}`
```json
// Request
{ "quantity": int }
// Response 204
```

### DELETE `/api/v1/cart/items/{skuId}`
```
// Response 204
```

### DELETE `/api/v1/cart`
```
// Response 204
```

---

## Orders

| Method | URL | Auth | 說明 |
|--------|-----|------|------|
| POST | `/api/v1/orders/logistics/start` | ✓ | 開始物流選擇（ECPay） |
| GET | `/api/v1/orders/logistics/status` | ✓ | 確認物流選擇狀態 |
| POST | `/api/v1/orders` | ✓ | 建立訂單 |
| GET | `/api/v1/orders` | ✓ | 取得訂單列表 |
| GET | `/api/v1/orders/{orderId}` | ✓ | 取得單一訂單 |

### POST `/api/v1/orders/logistics/start`
```json
// Request
{ "receiverName": "string", "receiverPhone": "string", "goodsAmount": decimal }

// Response 200：回傳 HTML，前端整頁導向 ECPay 門市選擇
// Content-Type: text/html
```

### GET `/api/v1/orders/logistics/status`
```json
// Response 200
{
  "isReady": bool,
  "logisticsSubType": "string?",  // e.g. "UNIMART"
  "receiverStoreName": "string?",
  "receiverAddress": "string?"
}
```

### POST `/api/v1/orders`
```json
// Request
{ "paymentMethod": 0, "receiverName": "string", "receiverPhone": "string" }
// paymentMethod: 0=LinePay, 1=PayPal, 2=ECPay

// Response 201
{ "orderId": "uuid", "totalAmount": decimal, "paymentUrl": "string?", "createdAt": "datetime" }
```
> 前端收到 `paymentUrl` 後導向付款頁（LinePay / PayPal）。

### GET `/api/v1/orders`
```
// Query Params
?status=string&page=int&pageSize=int

// Response 200
{
  "items": [{ "orderId": "uuid", "status": "string", "totalAmount": decimal, "createdAt": "datetime", ... }],
  "totalCount": int, "page": int, "pageSize": int
}
```

---

## Payments（Callback，瀏覽器跳轉，非前端主動呼叫）

| Method | URL | Auth | 說明 |
|--------|-----|------|------|
| GET | `/api/v1/payments/linepay/confirm` | ✗ | LinePay 付款成功 callback |
| GET | `/api/v1/payments/linepay/cancel` | ✗ | LinePay 付款取消 callback |
| GET | `/api/v1/payments/paypal/return` | ✗ | PayPal 付款成功 callback |
| GET | `/api/v1/payments/paypal/cancel` | ✗ | PayPal 付款取消 callback |

> 這些端點由付款閘道 redirect 瀏覽器過來，後端處理完後 redirect 至前端：
> 成功：`{FrontendDomain}/orders/{orderId}?payment=success`
> 取消/失敗：`{FrontendDomain}/orders/{orderId}?payment=cancel`

> ⚠️ 前端不需要主動呼叫這些端點。LinePay `confirmUrl` / `cancelUrl`、PayPal `returnUrl` / `cancelUrl` 由**後端 PaymentGateway 自動產生**並帶給付款閘道，前端只需處理 redirect 後的結果 query param。

---

## ECPay（Callback，非前端呼叫）

| Method | URL | Auth | 說明 |
|--------|-----|------|------|
| POST | `/api/ecpay/client-reply` | ✗ | ECPay 門市選擇結果 callback |

> ECPay 選完門市後瀏覽器會被 redirect 至 `{FrontendDomain}/checkout?logistics=done`
> 前端偵測到 `?logistics=done` 後呼叫 `GET /api/v1/orders/logistics/status` 確認。

---

## Admin - Orders

| Method | URL | Auth (Admin) | 說明 |
|--------|-----|------|------|
| GET | `/api/v1/admin/orders` | ✓ | 取得所有訂單 |
| POST | `/api/v1/admin/orders/{orderId}/ship` | ✓ | 出貨 |
| POST | `/api/v1/admin/orders/{orderId}/complete` | ✓ | 完成訂單 |
| POST | `/api/v1/admin/orders/print-label` | ✓ | 列印標籤 |

### GET `/api/v1/admin/orders`
```
// Query Params
?status=string&page=int&pageSize=int
```

---

## Admin - Products

| Method | URL | Auth (Admin) | 說明 |
|--------|-----|------|------|
| POST | `/api/v1/admin/products/images` | ✓ | Pre-upload 圖片（multi-file） |
| POST | `/api/v1/admin/products` | ✓ | 建立商品 |
| PUT | `/api/v1/admin/products/{productId}` | ✓ | 更新商品 |
| DELETE | `/api/v1/admin/products/{productId}` | ✓ | 刪除商品 |
| POST | `/api/v1/admin/products/{productId}/publish` | ✓ | 上架 |
| POST | `/api/v1/admin/products/{productId}/unpublish` | ✓ | 下架 |
| POST | `/api/v1/admin/products/{productId}/variants` | ✓ | 新增 Variant |
| POST | `/api/v1/admin/products/{productId}/variants/{variantId}/images` | ✓ | 上傳圖片至 Variant |

### POST `/api/v1/admin/products/images`（Pre-upload）
```
// Request: multipart/form-data
files: File[]  （多檔）

// Response 200
[{ "fileName": "string", "url": "string", "publicId": "string" }]
```

### POST `/api/v1/admin/products`
```json
// Request
{
  "name": "string", "description": "string?", "audience": 0,
  "categoryId": "uuid", "tagIds": ["uuid"],
  "variants": [{
    "color": "string",
    "skus": [{ "sizeId": "uuid?", "price": decimal, "stock": int }],
    "images": [{ "url": "string", "publicId": "string", "isPrimary": bool, "sortOrder": int }]
  }]
}

// Response 201
{ "productId": "uuid", "name": "string", "isActive": false, "createdAt": "datetime" }
```

### PUT `/api/v1/admin/products/{productId}`
```json
// Request（完整替換，diff 由後端計算）
{
  "name": "string", "description": "string?", "audience": 0,
  "categoryId": "uuid", "tagIds": ["uuid"],
  "variants": [{
    "variantId": "uuid?",  // null = 新增
    "color": "string",
    "skus": [{ "skuId": "uuid?", "sizeId": "uuid?", "price": decimal, "stock": int }],
    "images": [{ "url": "string", "publicId": "string", "isPrimary": bool, "sortOrder": int }]
  }]
}

// Response 204
```

---

## 前端現有 URL 與後端對照

| 前端 URL | 後端正確 URL | 狀態 |
|---------|------------|------|
| `POST /api/auth/login` | `POST /api/v1/auth/login` | ⚠️ 缺 /v1/ |
| `POST /api/auth/register` | `POST /api/v1/auth/register` | ⚠️ 缺 /v1/ |
| `POST /api/auth/google/callback` | `POST /api/v1/auth/google` | ⚠️ 路徑不同 |
| `POST /api/auth/line/callback` | `POST /api/v1/auth/line` | ⚠️ 路徑不同 |
| `POST /api/payments/line/request` | — | ❌ 不需要，paymentUrl 由建立訂單時回傳 |
| `POST /api/payments/paypal/create-order` | — | ❌ 不需要，paymentUrl 由建立訂單時回傳 |
| `GET /api/users/me` | — | ❌ 後端未實作 |
| `PUT /api/users/me` | — | ❌ 後端未實作 |
| `POST /api/users/me/avatar` | `POST /api/v1/users/me/avatar` | ⚠️ 缺 /v1/ |
| `DELETE /api/users/me/avatar` | — | ❌ 後端未實作 |
| `GET /api/orders` | `GET /api/v1/orders` | ⚠️ 缺 /v1/ |
| `GET /api/orders/{orderId}` | `GET /api/v1/orders/{orderId}` | ⚠️ 缺 /v1/ |
| `GET /api/products` | `GET /api/v1/products` | ⚠️ 缺 /v1/ |
| `GET /api/products/{productId}` | `GET /api/v1/products/{productId}` | ⚠️ 缺 /v1/ |
| `GET /api/carts/{userId}` | `GET /api/v1/cart` | ⚠️ 路徑不同，不需要 userId |
| `POST /api/carts/{userId}/items` | `POST /api/v1/cart/items` | ⚠️ 路徑不同 |
| `PUT /api/carts/{userId}/items/{productId}` | `PUT /api/v1/cart/items/{skuId}` | ⚠️ 路徑不同，用 skuId |
| `DELETE /api/carts/{userId}/items/{productId}` | `DELETE /api/v1/cart/items/{skuId}` | ⚠️ 路徑不同，用 skuId |
| `DELETE /api/carts/{userId}/items` | `DELETE /api/v1/cart` | ⚠️ 路徑不同 |
| `POST /api/sellers/apply` | — | ❌ 無 Seller 概念 |
| `GET /api/sellers/pending` | — | ❌ 無 Seller 概念 |
| `POST /api/sellers/{userId}/approve` | — | ❌ 無 Seller 概念 |
| `GET /api/homepage/banners` | — | ❌ 後端未實作 |
| `GET /api/homepage/categories` | — | ❌ 後端未實作 |
| `GET /api/homepage/flash-sale` | — | ❌ 後端未實作 |
| `GET /api/homepage/recommend` | — | ❌ 後端未實作 |
| `GET /api/homepage/reviews` | — | ❌ 後端未實作 |
| `GET /api/products/{productId}/reviews` | — | ❌ 後端未實作 |
| `POST /api/orders/{orderId}/review` | — | ❌ 後端未實作 |
| `GET /api/products/mine` | — | ❌ 無 Seller 概念 |
| `POST /api/products` | `POST /api/v1/admin/products` | ⚠️ 需加 /admin/ |
| `PUT /api/products/{productId}` | `PUT /api/v1/admin/products/{productId}` | ⚠️ 需加 /admin/ |
| `DELETE /api/products/{productId}` | `DELETE /api/v1/admin/products/{productId}` | ⚠️ 需加 /admin/ |

---

## 後端尚未實作（前端需要）

| 功能 | 建議端點 |
|------|---------|
| 取得使用者資料 | `GET /api/v1/users/me` |
| 更新使用者資料 | `PUT /api/v1/users/me` |
| 刪除頭像 | `DELETE /api/v1/users/me/avatar` |
| 首頁 Banner | `GET /api/v1/homepage/banners` |
| 首頁分類 | `GET /api/v1/homepage/categories` |
| 商品評論列表 | `GET /api/v1/products/{productId}/reviews` |
| 建立評論 | `POST /api/v1/orders/{orderId}/review` |
