# eShopX.Api | v1

> 版本：`1.0.0`　所有端點皆使用版本前綴 `/api/v1/`。
> 需要 JWT 的端點請在 Header 帶上 `Authorization: Bearer <token>`。

## Response Envelope

所有 JSON response 統一包在 `ApiResponse<T>` 中：

```json
// 成功
{ "isSuccess": true, "code": "ok", "data": { ... } }

// 失敗
{ "isSuccess": false, "code": "error_code",
  "problem": { "status": 400, "title": "error_code", "detail": "..." } }
```

---

## Auth

| Method | URL | Auth |
|--------|-----|:----:|
| `POST` | `/api/v1/auth/google` | ✗ |
| `POST` | `/api/v1/auth/line` | ✗ |
| `POST` | `/api/v1/auth/login` | ✗ |
| `POST` | `/api/v1/auth/logout` | ✗ |
| `POST` | `/api/v1/auth/refresh` | ✗ |
| `POST` | `/api/v1/auth/register` | ✗ |
| `POST` | `/api/v1/auth/send-otp` | ✗ |

---

### `POST` `/api/v1/auth/google`

**Request**

```json
{
  "code": "string",
  "codeVerifier": "string",
  "state": "string"
}
```

**Responses**

- **200** `ApiResponse<GoogleAuthResponse>`
  ```json
  {
    "accessToken": "string",
    "refreshToken": "string",
    "userId": "string",
    "name": "string",
    "expiresAt": "string",
    "googleSub": "string",
    "email": "string",
    "picture": "string"
  }
  ```

---

### `POST` `/api/v1/auth/line`

**Request**

```json
{
  "code": "string",
  "codeVerifier": "string",
  "nonce": "string"
}
```

**Responses**

- **200** `ApiResponse<LineAuthResponse>`
  ```json
  {
    "accessToken": "string",
    "refreshToken": "string",
    "userId": "string",
    "name": "string",
    "expiresAt": "string",
    "lineSub": "string",
    "email": "string",
    "avatarUrl": "string"
  }
  ```

---

### `POST` `/api/v1/auth/login`

**Request**

```json
{
  "email": "string",
  "password": "string"
}
```

**Responses**

- **200** `ApiResponse<LoginResponse>`
  ```json
  {
    "accessToken": "string",
    "refreshToken": "string",
    "userId": "string",
    "name": "string",
    "expiresAt": "string"
  }
  ```
- **400** `ApiResponse` Bad Request

---

### `POST` `/api/v1/auth/logout`

**Request**

```json
{
  "refreshToken": "string"
}
```

**Responses**

- **204** No Content

---

### `POST` `/api/v1/auth/refresh`

**Request**

```json
{
  "refreshToken": "string"
}
```

**Responses**

- **200** `ApiResponse<RefreshTokenResponse>`
  ```json
  {
    "accessToken": "string",
    "refreshToken": "string",
    "expiresAt": "string"
  }
  ```
- **400** `ApiResponse` Bad Request
- **404** `ApiResponse` Not Found

---

### `POST` `/api/v1/auth/register`

**Request**

```json
{
  "name": "string",
  "email": "string",
  "password": "string",
  "otp": "string"
}
```

**Responses**

- **201** `ApiResponse<RegisterUserResponse>`
  ```json
  {
    "userId": "string",
    "email": "string",
    "createdAt": "string"
  }
  ```
- **400** `ApiResponse` Bad Request

---

### `POST` `/api/v1/auth/send-otp`

**Request**

```json
{
  "email": "string"
}
```

**Responses**

- **200** `ApiResponse` OK
- **400** `ApiResponse` Bad Request

---
## Users

| Method | URL | Auth |
|--------|-----|:----:|
| `GET` | `/api/v1/users/me` | ✓ |
| `POST` | `/api/v1/users/me/avatar` | ✓ |

---

### `GET` `/api/v1/users/me`
> 需要 `Bearer` Token

**Responses**

- **200** `ApiResponse<GetMeResponse>`
  ```json
  {
    "id": "string",
    "name": "string",
    "email": "string",
    "avatarUrl": "string",
    "roles": [
      "string"
    ],
    "isLocalUser": true
  }
  ```
- **404** `ApiResponse` Not Found

---

### `POST` `/api/v1/users/me/avatar`
> 需要 `Bearer` Token

**Request**

```
// multipart/form-data
file: file (required)
```

**Responses**

- **200** `ApiResponse<UpdateUserAvatarResponse>`
  ```json
  {
    "url": "string"
  }
  ```
- **400** `ApiResponse` Bad Request
- **404** `ApiResponse` Not Found

---
## Categories

| Method | URL | Auth |
|--------|-----|:----:|
| `GET` | `/api/v1/categories` | ✗ |

---

### `GET` `/api/v1/categories`

**Responses**

- **200** `ApiResponse<CategoryResponse[]>` OK

---
## Sizes

| Method | URL | Auth |
|--------|-----|:----:|
| `GET` | `/api/v1/sizes` | ✗ |

---

### `GET` `/api/v1/sizes`

| 參數 | 位置 | 類型 | 必填 | 說明 |
|------|------|------|:----:|------|
| `type` | query | string |  |  |

**Responses**

- **200** `ApiResponse<SizeResponse[]>` OK

---
## Tags

| Method | URL | Auth |
|--------|-----|:----:|
| `GET` | `/api/v1/tags` | ✗ |

---

### `GET` `/api/v1/tags`

**Responses**

- **200** `ApiResponse<TagResponse[]>` OK

---
## Products

| Method | URL | Auth |
|--------|-----|:----:|
| `GET` | `/api/v1/products/{productId}` | ✗ |
| `GET` | `/api/v1/products` | ✗ |
| `GET` | `/api/v1/products/search` | ✗ |

---

### `GET` `/api/v1/products/{productId}`

| 參數 | 位置 | 類型 | 必填 | 說明 |
|------|------|------|:----:|------|
| `productId` | path | string | ✓ |  |

**Responses**

- **200** `ApiResponse<ProductResponse>`
  ```json
  {
    "id": "string",
    "name": "string",
    "description": "string",
    "audience": "string",
    "isActive": true,
    "categoryId": "string",
    "createdAt": "string",
    "updatedAt": "string",
    "variants": [
      {
        "id": "string",
        "color": "string",
        "skus": [
          {
            "id": "string",
            "sizeId": "string",
            "price": 0.0,
            "stock": 0
          }
        ],
        "images": [
          {
            "id": "string",
            "url": "string",
            "isPrimary": true,
            "sortOrder": 0
          }
        ]
      }
    ],
    "tagIds": [
      "string"
    ]
  }
  ```
- **404** `ApiResponse` Not Found

---

### `GET` `/api/v1/products`

| 參數 | 位置 | 類型 | 必填 | 說明 |
|------|------|------|:----:|------|
| `categoryId` | query | string |  |  |
| `audience` | query | string |  |  |
| `isActive` | query | boolean |  |  |
| `page` | query | ['integer', 'string'] | ✓ |  |
| `pageSize` | query | ['integer', 'string'] | ✓ |  |

**Responses**

- **200** `ApiResponse<GetProductsResponse>`
  ```json
  {
    "items": [
      {
        "id": "string",
        "name": "string",
        "audience": "string",
        "isActive": true,
        "categoryId": "string",
        "updatedAt": "string",
        "primaryImageUrl": "string",
        "startingPrice": 0.0,
        "colors": [
          "string"
        ]
      }
    ],
    "totalCount": 0,
    "page": 0,
    "pageSize": 0
  }
  ```

---

### `GET` `/api/v1/products/search`

| 參數 | 位置 | 類型 | 必填 | 說明 |
|------|------|------|:----:|------|
| `keyword` | query | string |  |  |
| `categoryId` | query | string |  |  |
| `minPrice` | query | ['number', 'string'] |  |  |
| `maxPrice` | query | ['number', 'string'] |  |  |
| `isActive` | query | boolean |  |  |
| `audience` | query | string |  |  |
| `page` | query | ['integer', 'string'] | ✓ |  |
| `pageSize` | query | ['integer', 'string'] | ✓ |  |

**Responses**

- **200** `ApiResponse<ProductSearchResponse>`
  ```json
  {
    "page": 0,
    "pageSize": 0,
    "totalCount": 0,
    "totalPages": 0,
    "items": [
      {
        "productId": "string",
        "categoryId": "string",
        "name": "string",
        "description": "string",
        "price": 0.0,
        "stockQuantity": 0,
        "isActive": true,
        "primaryImageUrl": "string"
      }
    ]
  }
  ```

---
## Cart

| Method | URL | Auth |
|--------|-----|:----:|
| `POST` | `/api/v1/cart/items` | ✓ |
| `GET` | `/api/v1/cart` | ✓ |
| `DELETE` | `/api/v1/cart` | ✓ |
| `PUT` | `/api/v1/cart/items/{skuId}` | ✓ |
| `DELETE` | `/api/v1/cart/items/{skuId}` | ✓ |

---

### `POST` `/api/v1/cart/items`
> 需要 `Bearer` Token

**Request**

```json
{
  "skuId": "string",
  "quantity": 0
}
```

**Responses**

- **204** No Content

---

### `GET` `/api/v1/cart`
> 需要 `Bearer` Token

**Responses**

- **200** `ApiResponse<CartResponse>`
  ```json
  {
    "cartId": "string",
    "userId": "string",
    "items": [
      {
        "itemId": "string",
        "skuId": "string",
        "quantity": 0,
        "productName": "string",
        "color": "string",
        "sizeName": "string",
        "unitPrice": 0.0,
        "primaryImageUrl": "string"
      }
    ]
  }
  ```

---

### `DELETE` `/api/v1/cart`
> 需要 `Bearer` Token

**Responses**

- **204** No Content

---

### `PUT` `/api/v1/cart/items/{skuId}`
> 需要 `Bearer` Token

| 參數 | 位置 | 類型 | 必填 | 說明 |
|------|------|------|:----:|------|
| `skuId` | path | string | ✓ |  |

**Request**

```json
{
  "quantity": 0
}
```

**Responses**

- **204** No Content

---

### `DELETE` `/api/v1/cart/items/{skuId}`
> 需要 `Bearer` Token

| 參數 | 位置 | 類型 | 必填 | 說明 |
|------|------|------|:----:|------|
| `skuId` | path | string | ✓ |  |

**Responses**

- **204** No Content

---
## Orders

| Method | URL | Auth |
|--------|-----|:----:|
| `GET` | `/api/v1/orders` | ✓ |
| `POST` | `/api/v1/orders` | ✓ |
| `GET` | `/api/v1/orders/logistics/status` | ✓ |
| `GET` | `/api/v1/orders/{orderId}` | ✓ |
| `POST` | `/api/v1/orders/logistics/start` | ✓ |

---

### `GET` `/api/v1/orders`
> 需要 `Bearer` Token

| 參數 | 位置 | 類型 | 必填 | 說明 |
|------|------|------|:----:|------|
| `page` | query | ['integer', 'string'] |  |  |
| `pageSize` | query | ['integer', 'string'] |  |  |
| `status` | query | string |  |  |

**Responses**

- **200** `ApiResponse<GetOrdersResponse>`
  ```json
  {
    "items": [
      {
        "orderId": "string",
        "userId": "string",
        "status": "string",
        "totalAmount": 0.0,
        "createdAt": "string",
        "shipment": {
          "logisticsSubType": "string",
          "logisticsId": "string",
          "receiverName": "string",
          "receiverPhone": "string",
          "storeName": "string",
          "address": "string"
        }
      }
    ],
    "totalCount": 0,
    "page": 0,
    "pageSize": 0
  }
  ```

---

### `POST` `/api/v1/orders`
> 需要 `Bearer` Token

**Request**

```json
{
  "paymentMethod": "LinePay" | "PayPal",
  "receiverName": "string",
  "receiverPhone": "string"
}
```

**Responses**

- **201** `ApiResponse<CreateOrderResponse>`
  ```json
  {
    "orderId": "string",
    "totalAmount": 0.0,
    "paymentUrl": "string",
    "createdAt": "string",
    "logisticsSubType": "string",
    "storeName": "string",
    "address": "string"
  }
  ```
- **400** `ApiResponse` Bad Request

---

### `GET` `/api/v1/orders/logistics/status`
> 需要 `Bearer` Token

**Responses**

- **200** `ApiResponse<LogisticsStatusDto>`
  ```json
  {
    "isReady": true,
    "logisticsSubType": "string",
    "storeName": "string",
    "address": "string"
  }
  ```

---

### `GET` `/api/v1/orders/{orderId}`
> 需要 `Bearer` Token

| 參數 | 位置 | 類型 | 必填 | 說明 |
|------|------|------|:----:|------|
| `orderId` | path | string | ✓ |  |

**Responses**

- **200** `ApiResponse<OrderResponse>`
  ```json
  {
    "orderId": "string",
    "userId": "string",
    "status": "string",
    "totalAmount": 0.0,
    "createdAt": "string",
    "items": [
      {
        "itemId": "string",
        "skuId": "string",
        "productName": "string",
        "color": "string",
        "size": "string",
        "unitPrice": 0.0,
        "quantity": 0,
        "totalPrice": 0.0
      }
    ]
  }
  ```
- **404** `ApiResponse` Not Found

---

### `POST` `/api/v1/orders/logistics/start`
> 需要 `Bearer` Token

**Request**

```json
{
  "receiverName": "string",
  "receiverPhone": "string",
  "goodsAmount": 0.0
}
```

**Responses**

- **200** `ApiResponse<string>` OK

---
## Payments

| Method | URL | Auth |
|--------|-----|:----:|
| `GET` | `/api/v1/payments/linepay/cancel` | ✗ |
| `GET` | `/api/v1/payments/linepay/confirm` | ✗ |
| `GET` | `/api/v1/payments/paypal/cancel` | ✗ |
| `GET` | `/api/v1/payments/paypal/return` | ✗ |

---

### `GET` `/api/v1/payments/linepay/cancel`

| 參數 | 位置 | 類型 | 必填 | 說明 |
|------|------|------|:----:|------|
| `orderId` | query | string | ✓ |  |

**Responses**

- **200** OK

---

### `GET` `/api/v1/payments/linepay/confirm`

| 參數 | 位置 | 類型 | 必填 | 說明 |
|------|------|------|:----:|------|
| `transactionId` | query | ['integer', 'string'] | ✓ |  |
| `orderId` | query | string | ✓ |  |

**Responses**

- **200** OK

---

### `GET` `/api/v1/payments/paypal/cancel`

| 參數 | 位置 | 類型 | 必填 | 說明 |
|------|------|------|:----:|------|
| `orderId` | query | string | ✓ |  |

**Responses**

- **200** OK

---

### `GET` `/api/v1/payments/paypal/return`

| 參數 | 位置 | 類型 | 必填 | 說明 |
|------|------|------|:----:|------|
| `token` | query | string | ✓ |  |
| `orderId` | query | string | ✓ |  |

**Responses**

- **200** OK

---
## ECPay

| Method | URL | Auth |
|--------|-----|:----:|
| `POST` | `/api/ecpay/client-reply` | ✗ |

---

### `POST` `/api/ecpay/client-reply`

| 參數 | 位置 | 類型 | 必填 | 說明 |
|------|------|------|:----:|------|
| `token` | query | string | ✓ |  |

**Request**

```
// multipart/form-data
ResultData: string (required)
```

**Responses**

- **200** OK

---
## Admin - Categories

| Method | URL | Auth |
|--------|-----|:----:|
| `POST` | `/api/v1/admin/categories` | ✓ |
| `PUT` | `/api/v1/admin/categories/{categoryId}` | ✓ |
| `DELETE` | `/api/v1/admin/categories/{categoryId}` | ✓ |

---

### `POST` `/api/v1/admin/categories`
> 需要 `Admin` Token

**Request**

```json
{
  "name": "string"
}
```

**Responses**

- **201** `ApiResponse<CategoryResponse>`
  ```json
  {
    "id": "string",
    "name": "string",
    "createdAt": "string"
  }
  ```

---

### `PUT` `/api/v1/admin/categories/{categoryId}`
> 需要 `Admin` Token

| 參數 | 位置 | 類型 | 必填 | 說明 |
|------|------|------|:----:|------|
| `categoryId` | path | string | ✓ |  |

**Request**

```json
{
  "name": "string"
}
```

**Responses**

- **204** No Content
- **404** `ApiResponse` Not Found

---

### `DELETE` `/api/v1/admin/categories/{categoryId}`
> 需要 `Admin` Token

| 參數 | 位置 | 類型 | 必填 | 說明 |
|------|------|------|:----:|------|
| `categoryId` | path | string | ✓ |  |

**Responses**

- **204** No Content
- **404** `ApiResponse` Not Found

---
## Admin - Sizes

| Method | URL | Auth |
|--------|-----|:----:|
| `POST` | `/api/v1/admin/sizes` | ✓ |
| `PUT` | `/api/v1/admin/sizes/{sizeId}` | ✓ |
| `DELETE` | `/api/v1/admin/sizes/{sizeId}` | ✓ |

---

### `POST` `/api/v1/admin/sizes`
> 需要 `Admin` Token

**Request**

```json
{
  "name": "string",
  "type": "string"
}
```

**Responses**

- **201** `ApiResponse<SizeResponse>`
  ```json
  {
    "id": "string",
    "name": "string",
    "type": "string",
    "createdAt": "string"
  }
  ```

---

### `PUT` `/api/v1/admin/sizes/{sizeId}`
> 需要 `Admin` Token

| 參數 | 位置 | 類型 | 必填 | 說明 |
|------|------|------|:----:|------|
| `sizeId` | path | string | ✓ |  |

**Request**

```json
{
  "name": "string"
}
```

**Responses**

- **204** No Content
- **404** `ApiResponse` Not Found

---

### `DELETE` `/api/v1/admin/sizes/{sizeId}`
> 需要 `Admin` Token

| 參數 | 位置 | 類型 | 必填 | 說明 |
|------|------|------|:----:|------|
| `sizeId` | path | string | ✓ |  |

**Responses**

- **204** No Content
- **404** `ApiResponse` Not Found

---
## Admin - Tags

| Method | URL | Auth |
|--------|-----|:----:|
| `POST` | `/api/v1/admin/tags` | ✓ |
| `PUT` | `/api/v1/admin/tags/{tagId}` | ✓ |
| `DELETE` | `/api/v1/admin/tags/{tagId}` | ✓ |

---

### `POST` `/api/v1/admin/tags`
> 需要 `Admin` Token

**Request**

```json
{
  "name": "string",
  "type": "Season" | "Style" | "Feature"
}
```

**Responses**

- **201** `ApiResponse<TagResponse>`
  ```json
  {
    "id": "string",
    "name": "string",
    "type": "string",
    "createdAt": "string"
  }
  ```

---

### `PUT` `/api/v1/admin/tags/{tagId}`
> 需要 `Admin` Token

| 參數 | 位置 | 類型 | 必填 | 說明 |
|------|------|------|:----:|------|
| `tagId` | path | string | ✓ |  |

**Request**

```json
{
  "name": "string"
}
```

**Responses**

- **204** No Content
- **404** `ApiResponse` Not Found

---

### `DELETE` `/api/v1/admin/tags/{tagId}`
> 需要 `Admin` Token

| 參數 | 位置 | 類型 | 必填 | 說明 |
|------|------|------|:----:|------|
| `tagId` | path | string | ✓ |  |

**Responses**

- **204** No Content
- **404** `ApiResponse` Not Found

---
## Admin - Products

| Method | URL | Auth |
|--------|-----|:----:|
| `POST` | `/api/v1/admin/products/{productId}/variants` | ✓ |
| `POST` | `/api/v1/admin/products` | ✓ |
| `PUT` | `/api/v1/admin/products/{productId}` | ✓ |
| `DELETE` | `/api/v1/admin/products/{productId}` | ✓ |
| `POST` | `/api/v1/admin/products/{productId}/publish` | ✓ |
| `POST` | `/api/v1/admin/products/reindex` | ✓ |
| `POST` | `/api/v1/admin/products/{productId}/unpublish` | ✓ |
| `POST` | `/api/v1/admin/products/{productId}/variants/{variantId}/images` | ✓ |
| `POST` | `/api/v1/admin/products/images` | ✓ |

---

### `POST` `/api/v1/admin/products/{productId}/variants`
> 需要 `Admin` Token

| 參數 | 位置 | 類型 | 必填 | 說明 |
|------|------|------|:----:|------|
| `productId` | path | string | ✓ |  |

**Request**

```json
{
  "color": "string",
  "skus": [
    {
      "sizeId": "string",
      "price": 0.0,
      "stock": 0
    }
  ]
}
```

**Responses**

- **201** `ApiResponse<AddProductVariantResponse>`
  ```json
  {
    "variantId": "string",
    "color": "string"
  }
  ```
- **404** `ApiResponse` Not Found

---

### `POST` `/api/v1/admin/products`
> 需要 `Admin` Token

**Request**

```json
{
  "name": "string",
  "description": "string",
  "audience": "Men" | "Women",
  "categoryId": "string",
  "tagIds": [
    "string"
  ],
  "variants": [
    {
      "color": "string",
      "skus": [
        {
          "sizeId": "string",
          "price": 0.0,
          "stock": 0
        }
      ],
      "images": [
        {
          "url": "string",
          "publicId": "string",
          "isPrimary": true,
          "sortOrder": 0
        }
      ]
    }
  ]
}
```

**Responses**

- **201** `ApiResponse<CreateProductResponse>`
  ```json
  {
    "productId": "string",
    "name": "string",
    "isActive": true,
    "createdAt": "string"
  }
  ```

---

### `PUT` `/api/v1/admin/products/{productId}`
> 需要 `Admin` Token

| 參數 | 位置 | 類型 | 必填 | 說明 |
|------|------|------|:----:|------|
| `productId` | path | string | ✓ |  |

**Request**

```json
{
  "name": "string",
  "description": "string",
  "audience": "Men" | "Women",
  "categoryId": "string",
  "tagIds": [
    "string"
  ],
  "variants": [
    {
      "variantId": "string",
      "color": "string",
      "skus": [
        {
          "skuId": "string",
          "sizeId": "string",
          "price": 0.0,
          "stock": 0
        }
      ],
      "images": [
        {
          "url": "string",
          "publicId": "string",
          "isPrimary": true,
          "sortOrder": 0
        }
      ]
    }
  ]
}
```

**Responses**

- **204** No Content
- **404** `ApiResponse` Not Found

---

### `DELETE` `/api/v1/admin/products/{productId}`
> 需要 `Admin` Token

| 參數 | 位置 | 類型 | 必填 | 說明 |
|------|------|------|:----:|------|
| `productId` | path | string | ✓ |  |

**Responses**

- **204** No Content
- **404** `ApiResponse` Not Found

---

### `POST` `/api/v1/admin/products/{productId}/publish`
> 需要 `Admin` Token

| 參數 | 位置 | 類型 | 必填 | 說明 |
|------|------|------|:----:|------|
| `productId` | path | string | ✓ |  |

**Responses**

- **204** No Content
- **404** `ApiResponse` Not Found

---

### `POST` `/api/v1/admin/products/reindex`
> 需要 `Admin` Token

**Responses**

- **200** `ApiResponse<ReindexProductsResult>`
  ```json
  {
    "totalCount": 0,
    "indexed": 0,
    "failed": 0
  }
  ```

---

### `POST` `/api/v1/admin/products/{productId}/unpublish`
> 需要 `Admin` Token

| 參數 | 位置 | 類型 | 必填 | 說明 |
|------|------|------|:----:|------|
| `productId` | path | string | ✓ |  |

**Responses**

- **204** No Content
- **404** `ApiResponse` Not Found

---

### `POST` `/api/v1/admin/products/{productId}/variants/{variantId}/images`
> 需要 `Admin` Token

| 參數 | 位置 | 類型 | 必填 | 說明 |
|------|------|------|:----:|------|
| `productId` | path | string | ✓ |  |
| `variantId` | path | string | ✓ |  |
| `IsPrimary` | query | boolean |  |  |
| `SortOrder` | query | ['integer', 'string'] |  |  |

**Request**

```
// multipart/form-data
file: file (required)
```

**Responses**

- **200** `ApiResponse<UploadProductImageResponse>`
  ```json
  {
    "imageId": "string",
    "url": "string"
  }
  ```
- **400** `ApiResponse` Bad Request
- **404** `ApiResponse` Not Found

---

### `POST` `/api/v1/admin/products/images`
> 需要 `Admin` Token

**Request**

```
// multipart/form-data
files: file (required)
```

**Responses**

- **200** `ApiResponse<TempImageUploadResult[]>` OK
- **400** `ApiResponse` Bad Request

---
## Admin

| Method | URL | Auth |
|--------|-----|:----:|
| `GET` | `/api/v1/admin/orders` | ✓ |
| `POST` | `/api/v1/admin/orders/{orderId}/complete` | ✓ |
| `POST` | `/api/v1/admin/orders/print-label` | ✓ |
| `POST` | `/api/v1/admin/orders/{orderId}/ship` | ✓ |

---

### `GET` `/api/v1/admin/orders`
> 需要 `Admin` Token

| 參數 | 位置 | 類型 | 必填 | 說明 |
|------|------|------|:----:|------|
| `page` | query | ['integer', 'string'] |  |  |
| `pageSize` | query | ['integer', 'string'] |  |  |
| `status` | query | string |  |  |

**Responses**

- **200** `ApiResponse<GetOrdersResponse>`
  ```json
  {
    "items": [
      {
        "orderId": "string",
        "userId": "string",
        "status": "string",
        "totalAmount": 0.0,
        "createdAt": "string",
        "shipment": {
          "logisticsSubType": "string",
          "logisticsId": "string",
          "receiverName": "string",
          "receiverPhone": "string",
          "storeName": "string",
          "address": "string"
        }
      }
    ],
    "totalCount": 0,
    "page": 0,
    "pageSize": 0
  }
  ```

---

### `POST` `/api/v1/admin/orders/{orderId}/complete`
> 需要 `Admin` Token

| 參數 | 位置 | 類型 | 必填 | 說明 |
|------|------|------|:----:|------|
| `orderId` | path | string | ✓ |  |

**Responses**

- **204** No Content
- **404** `ApiResponse` Not Found

---

### `POST` `/api/v1/admin/orders/print-label`
> 需要 `Admin` Token

**Request**

```json
{
  "logisticsId": "string",
  "logisticsSubType": "string"
}
```

**Responses**

- **200** OK

---

### `POST` `/api/v1/admin/orders/{orderId}/ship`
> 需要 `Admin` Token

| 參數 | 位置 | 類型 | 必填 | 說明 |
|------|------|------|:----:|------|
| `orderId` | path | string | ✓ |  |

**Responses**

- **200** `ApiResponse<ShipOrderResponse>`
  ```json
  {
    "logisticsId": "string",
    "logisticsSubType": "string"
  }
  ```
- **400** `ApiResponse` Bad Request
- **404** `ApiResponse` Not Found

---
