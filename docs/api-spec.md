# eShopX API Spec

> 前端重寫參考文件。所有端點皆使用版本前綴 `/api/v1/`。
> 需要 JWT 的端點請在 Header 帶上 `Authorization: Bearer <accessToken>`。

## Response Envelope

所有 JSON response 統一包在 `ApiResponse` / `ApiResponse<T>` 中：

```json
// 成功（有資料）
{
  "isSuccess": true,
  "code": "ok",
  "data": { ... }
}

// 成功（無資料）
{
  "isSuccess": true,
  "code": "ok"
}

// 失敗
{
  "isSuccess": false,
  "code": "error_code",
  "problem": {
    "status": 400,
    "title": "error_code",
    "detail": "human readable message"
  }
}
```

---

## Auth

| Method | URL | Auth | 說明 |
|--------|-----|:----:|------|
| POST | `/api/v1/auth/register` | ✗ | 註冊 |
| POST | `/api/v1/auth/login` | ✗ | 本地登入 |
| POST | `/api/v1/auth/google` | ✗ | Google 登入 |
| POST | `/api/v1/auth/line` | ✗ | LINE 登入 |
| POST | `/api/v1/auth/refresh` | ✗ | 更新 Token |
| POST | `/api/v1/auth/logout` | ✗ | 登出 |

### POST `/api/v1/auth/register`
```json
// Request
{ "name": "string", "email": "string", "password": "string" }

// 201 ApiResponse<RegisterUserResponse>
{ "userId": "uuid", "email": "string", "createdAt": "datetime" }

// 400 ApiResponse  →  email_conflict
```

### POST `/api/v1/auth/login`
```json
// Request
{ "email": "string", "password": "string" }

// 200 ApiResponse<LoginResponse>
{ "accessToken": "string", "refreshToken": "string", "userId": "uuid", "name": "string", "expiresAt": "datetime" }

// 400 ApiResponse  →  credentials_invalid
```

### POST `/api/v1/auth/google`
```json
// Request（前端先走 Google PKCE OAuth flow）
{ "code": "string", "codeVerifier": "string", "state": "string" }

// 200 ApiResponse<GoogleAuthResponse>
{ "accessToken": "string", "refreshToken": "string", "userId": "uuid", "name": "string", "expiresAt": "datetime", "sub": "string", "email": "string", "picture": "string?" }
```

### POST `/api/v1/auth/line`
```json
// Request（前端先走 LINE PKCE OAuth flow）
{ "code": "string", "codeVerifier": "string?", "nonce": "string?" }

// 200 ApiResponse<LineAuthResponse>
{ "accessToken": "string", "refreshToken": "string", "userId": "uuid", "name": "string", "expiresAt": "datetime", "sub": "string", "email": "string" }
```

### POST `/api/v1/auth/refresh`
```json
// Request
{ "refreshToken": "string" }

// 200 ApiResponse<RefreshTokenResponse>
{ "accessToken": "string", "refreshToken": "string", "expiresAt": "datetime" }

// 400 ApiResponse  →  refresh_token_revoked / refresh_token_expired
// 404 ApiResponse  →  refresh_token_not_found
```

### POST `/api/v1/auth/logout`
```json
// Request
{ "refreshToken": "string" }

// 204
```

---

## Users

| Method | URL | Auth | 說明 |
|--------|-----|:----:|------|
| POST | `/api/v1/users/me/avatar` | ✓ | 上傳頭像 |

### POST `/api/v1/users/me/avatar`
```
// Request: multipart/form-data
file: File

// 200 ApiResponse<UpdateUserAvatarResponse>
{ "url": "string" }

// 400 ApiResponse
// 404 ApiResponse
```

---

## Categories

| Method | URL | Auth | 說明 |
|--------|-----|:----:|------|
| GET | `/api/v1/categories` | ✗ | 取得分類列表 |

### GET `/api/v1/categories`
```json
// 200 ApiResponse<CategoryResponse[]>
[{ "id": "uuid", "name": "string", "createdAt": "datetime" }]
```

---

## Sizes

| Method | URL | Auth | 說明 |
|--------|-----|:----:|------|
| GET | `/api/v1/sizes` | ✗ | 取得尺寸列表 |

### GET `/api/v1/sizes`
```json
// 200 ApiResponse<SizeResponse[]>
[{ "id": "uuid", "name": "string", "type": "Clothing|Pants|Shoes|Hat", "createdAt": "datetime" }]
```

---

## Tags

| Method | URL | Auth | 說明 |
|--------|-----|:----:|------|
| GET | `/api/v1/tags` | ✗ | 取得標籤列表 |

### GET `/api/v1/tags`
```json
// 200 ApiResponse<TagResponse[]>
[{ "id": "uuid", "name": "string", "type": "Season|Style|Feature", "createdAt": "datetime" }]
```

---

## Products

| Method | URL | Auth | 說明 |
|--------|-----|:----:|------|
| GET | `/api/v1/products` | ✗ | 取得商品列表 |
| GET | `/api/v1/products/{productId}` | ✗ | 取得單一商品 |
| GET | `/api/v1/products/search` | ✗ | 搜尋商品 |

### GET `/api/v1/products`
```
// Query Params
?categoryId=uuid&audience=0|1&isActive=bool&page=int&pageSize=int

// 200 ApiResponse<GetProductsResponse>
{
  "items": [{ "id": "uuid", "name": "string", "audience": "string?", "isActive": bool, "categoryId": "uuid", "updatedAt": "datetime" }],
  "totalCount": int, "page": int, "pageSize": int
}
```

### GET `/api/v1/products/{productId}`
```json
// 200 ApiResponse<ProductResponse>
{
  "productId": "uuid", "name": "string", "description": "string?",
  "audience": 0, "categoryId": "uuid", "isActive": false,
  "tags": [{ "tagId": "uuid" }],
  "variants": [{
    "variantId": "uuid", "color": "string",
    "skus": [{ "skuId": "uuid", "sizeId": "uuid?", "price": 0.0, "stock": 0 }],
    "images": [{ "imageId": "uuid", "url": "string", "isPrimary": true, "sortOrder": 0 }]
  }]
}

// 404 ApiResponse  →  product_not_found
```

### GET `/api/v1/products/search`
```
// Query Params
?keyword=string&categoryId=uuid&minPrice=decimal&maxPrice=decimal&isActive=bool&page=int&pageSize=int

// 200 ProductSearchResponse
{
  "page": int, "pageSize": int, "totalCount": int, "totalPages": int,
  "items": [{ "productId": "uuid", "name": "string", "price": 0.0, "primaryImageUrl": "string?" }]
}
```

---

## Cart

| Method | URL | Auth | 說明 |
|--------|-----|:----:|------|
| GET | `/api/v1/cart` | ✓ | 取得購物車 |
| POST | `/api/v1/cart/items` | ✓ | 加入商品 |
| PUT | `/api/v1/cart/items/{skuId}` | ✓ | 更新數量 |
| DELETE | `/api/v1/cart/items/{skuId}` | ✓ | 移除商品 |
| DELETE | `/api/v1/cart` | ✓ | 清空購物車 |

### GET `/api/v1/cart`
```json
// 200 ApiResponse<CartResponse>
{
  "cartId": "uuid?", "userId": "uuid",
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
// 204
```

### PUT `/api/v1/cart/items/{skuId}`
```json
// Request
{ "quantity": int }
// 204
```

### DELETE `/api/v1/cart/items/{skuId}`
```
// 204
```

### DELETE `/api/v1/cart`
```
// 204
```

---

## Orders

| Method | URL | Auth | 說明 |
|--------|-----|:----:|------|
| POST | `/api/v1/orders/logistics/start` | ✓ | 開始物流選擇（ECPay） |
| GET | `/api/v1/orders/logistics/status` | ✓ | 確認物流選擇狀態 |
| POST | `/api/v1/orders` | ✓ | 建立訂單 |
| GET | `/api/v1/orders` | ✓ | 取得訂單列表 |
| GET | `/api/v1/orders/{orderId}` | ✓ | 取得單一訂單 |

### POST `/api/v1/orders/logistics/start`
```json
// Request
{ "receiverName": "string", "receiverPhone": "string", "goodsAmount": decimal }

// 200 text/html  →  ECPay 門市選擇頁，前端整頁導向
```

### GET `/api/v1/orders/logistics/status`
```json
// 200
{
  "isReady": bool,
  "logisticsSubType": "string?",
  "receiverStoreName": "string?",
  "receiverAddress": "string?"
}
```

### POST `/api/v1/orders`
```json
// Request
// paymentMethod: 0=LinePay, 1=PayPal, 2=ECPay
{ "paymentMethod": 0, "receiverName": "string", "receiverPhone": "string" }

// 201 ApiResponse<CreateOrderResponse>
{ "orderId": "uuid", "totalAmount": decimal, "paymentUrl": "string?", "createdAt": "datetime" }

// 400 ApiResponse  →  stock_conflict
```
> 前端收到 `paymentUrl` 後直接導向付款頁。

### GET `/api/v1/orders`
```
// Query Params
?status=string&page=int&pageSize=int

// 200 ApiResponse<GetOrdersResponse>
{
  "items": [{ "orderId": "uuid", "status": "string", "totalAmount": decimal, "createdAt": "datetime" }],
  "totalCount": int, "page": int, "pageSize": int
}
```

### GET `/api/v1/orders/{orderId}`
```json
// 200 ApiResponse<OrderResponse>
{ "orderId": "uuid", "status": "string", "totalAmount": decimal, "createdAt": "datetime", ... }

// 404 ApiResponse  →  order_not_found
```

---

## Payments（Callback，非前端主動呼叫）

| Method | URL | Auth | 說明 |
|--------|-----|:----:|------|
| GET | `/api/v1/payments/linepay/confirm?transactionId=&orderId=` | ✗ | LinePay 付款成功 callback |
| GET | `/api/v1/payments/linepay/cancel?orderId=` | ✗ | LinePay 付款取消 callback |
| GET | `/api/v1/payments/paypal/return?token=&orderId=` | ✗ | PayPal 付款成功 callback |
| GET | `/api/v1/payments/paypal/cancel?orderId=` | ✗ | PayPal 付款取消 callback |

> 付款閘道 redirect 瀏覽器至此，後端處理後再 redirect 至前端：
> - 成功：`{FrontendDomain}/orders/{orderId}?payment=success`
> - 取消/失敗：`{FrontendDomain}/orders/{orderId}?payment=cancel`

---

## ECPay（Callback，非前端主動呼叫）

| Method | URL | Auth | 說明 |
|--------|-----|:----:|------|
| POST | `/api/ecpay/client-reply?token=` | ✗ | ECPay 門市選擇結果 callback |

> 選完門市後瀏覽器被 redirect 至 `{FrontendDomain}/checkout?logistics=done`
> 前端再呼叫 `GET /api/v1/orders/logistics/status` 確認。

---

## Admin - Sizes

| Method | URL | Auth (Admin) | 說明 |
|--------|-----|:------------:|------|
| POST | `/api/v1/admin/sizes` | ✓ | 建立尺寸 |
| PUT | `/api/v1/admin/sizes/{sizeId}` | ✓ | 更新尺寸名稱 |
| DELETE | `/api/v1/admin/sizes/{sizeId}` | ✓ | 刪除尺寸 |

### POST `/api/v1/admin/sizes`
```json
// Request
{ "name": "string", "type": "Clothing|Pants|Shoes|Hat" }

// 201 ApiResponse<SizeResponse>
{ "id": "uuid", "name": "string", "type": "string", "createdAt": "datetime" }
```

### PUT `/api/v1/admin/sizes/{sizeId}`
```json
// Request
{ "name": "string" }
// 204
// 404 ApiResponse  →  size_not_found
```

### DELETE `/api/v1/admin/sizes/{sizeId}`
```
// 204
// 404 ApiResponse  →  size_not_found
```

---

## Admin - Tags

| Method | URL | Auth (Admin) | 說明 |
|--------|-----|:------------:|------|
| POST | `/api/v1/admin/tags` | ✓ | 建立標籤 |
| PUT | `/api/v1/admin/tags/{tagId}` | ✓ | 更新標籤名稱 |
| DELETE | `/api/v1/admin/tags/{tagId}` | ✓ | 刪除標籤 |

### POST `/api/v1/admin/tags`
```json
// Request
{ "name": "string", "type": "Season|Style|Feature" }

// 201 ApiResponse<TagResponse>
{ "id": "uuid", "name": "string", "type": "string", "createdAt": "datetime" }
```

### PUT `/api/v1/admin/tags/{tagId}`
```json
// Request
{ "name": "string" }
// 204
// 404 ApiResponse  →  tag_not_found
```

### DELETE `/api/v1/admin/tags/{tagId}`
```
// 204
// 404 ApiResponse  →  tag_not_found
```

---

## Admin - Categories

| Method | URL | Auth (Admin) | 說明 |
|--------|-----|:------------:|------|
| POST | `/api/v1/admin/categories` | ✓ | 建立分類 |
| PUT | `/api/v1/admin/categories/{categoryId}` | ✓ | 更新分類 |
| DELETE | `/api/v1/admin/categories/{categoryId}` | ✓ | 刪除分類 |

### POST `/api/v1/admin/categories`
```json
// Request
{ "name": "string" }

// 201 ApiResponse<CategoryResponse>
{ "id": "uuid", "name": "string", "createdAt": "datetime" }
```

### PUT `/api/v1/admin/categories/{categoryId}`
```json
// Request
{ "name": "string" }
// 204
// 404 ApiResponse  →  category_not_found
```

### DELETE `/api/v1/admin/categories/{categoryId}`
```
// 204
// 404 ApiResponse  →  category_not_found
```

---

## Admin - Products

| Method | URL | Auth (Admin) | 說明 |
|--------|-----|:------------:|------|
| POST | `/api/v1/admin/products/images` | ✓ | Pre-upload 圖片（multi-file） |
| POST | `/api/v1/admin/products` | ✓ | 建立商品 |
| PUT | `/api/v1/admin/products/{productId}` | ✓ | 更新商品 |
| DELETE | `/api/v1/admin/products/{productId}` | ✓ | 刪除商品 |
| POST | `/api/v1/admin/products/{productId}/publish` | ✓ | 上架 |
| POST | `/api/v1/admin/products/{productId}/unpublish` | ✓ | 下架 |
| POST | `/api/v1/admin/products/{productId}/variants` | ✓ | 新增 Variant |
| POST | `/api/v1/admin/products/{productId}/variants/{variantId}/images` | ✓ | 上傳圖片至 Variant |

### POST `/api/v1/admin/products/images`
```
// Request: multipart/form-data
files: File[]

// 200
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

// 201 ApiResponse<CreateProductResponse>
{ "productId": "uuid", "name": "string", "isActive": false, "createdAt": "datetime" }
```

### PUT `/api/v1/admin/products/{productId}`
```json
// Request（完整替換，diff 由後端計算）
{
  "name": "string", "description": "string?", "audience": 0,
  "categoryId": "uuid", "tagIds": ["uuid"],
  "variants": [{
    "variantId": "uuid?",
    "color": "string",
    "skus": [{ "skuId": "uuid?", "sizeId": "uuid?", "price": decimal, "stock": int }],
    "images": [{ "url": "string", "publicId": "string", "isPrimary": bool, "sortOrder": int }]
  }]
}

// 204
// 404 ApiResponse  →  product_not_found
```

### DELETE `/api/v1/admin/products/{productId}`
```
// 204
// 404 ApiResponse  →  product_not_found
```

### POST `/api/v1/admin/products/{productId}/publish`
```
// 204
// 404 ApiResponse  →  product_not_found
```

### POST `/api/v1/admin/products/{productId}/unpublish`
```
// 204
// 404 ApiResponse  →  product_not_found
```

### POST `/api/v1/admin/products/{productId}/variants`
```json
// Request
{
  "color": "string",
  "skus": [{ "sizeId": "uuid?", "price": decimal, "stock": int }]
}

// 201 ApiResponse<AddProductVariantResponse>
{ "variantId": "uuid" }

// 404 ApiResponse  →  product_not_found
```

### POST `/api/v1/admin/products/{productId}/variants/{variantId}/images`
```
// Request: multipart/form-data
file: File
IsPrimary: bool  (query)
SortOrder: int   (query)

// 200 ApiResponse<UploadProductImageResponse>
{ "imageId": "uuid", "url": "string" }

// 400 ApiResponse
// 404 ApiResponse  →  product_not_found / variant_not_found
```

---

## Admin - Orders

| Method | URL | Auth (Admin) | 說明 |
|--------|-----|:------------:|------|
| GET | `/api/v1/admin/orders` | ✓ | 取得所有訂單 |
| POST | `/api/v1/admin/orders/{orderId}/ship` | ✓ | 出貨 |
| POST | `/api/v1/admin/orders/{orderId}/complete` | ✓ | 完成訂單 |
| POST | `/api/v1/admin/orders/print-label` | ✓ | 列印標籤 |

### GET `/api/v1/admin/orders`
```
// Query Params
?status=string&page=int&pageSize=int

// 200 ApiResponse<GetOrdersResponse>
{
  "items": [{ "orderId": "uuid", "status": "string", "totalAmount": decimal, "createdAt": "datetime" }],
  "totalCount": int, "page": int, "pageSize": int
}
```

### POST `/api/v1/admin/orders/{orderId}/ship`
```
// 200
// 400 / 404
```

### POST `/api/v1/admin/orders/{orderId}/complete`
```
// 204
// 404 ApiResponse  →  order_not_found
```

### POST `/api/v1/admin/orders/print-label`
```json
// Request
{ "logisticsId": "string", "logisticsSubType": "string" }
// 200
```

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
