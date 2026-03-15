# Order Spec

## 目標

這份文件只定義 `Application` 規格，聚焦：

- API 斷點
- 前後端互動
- Application use case 邊界
- Order / Payment / Logistics 的流程責任

不處理：

- Infrastructure 實作細節
- 第三方 SDK / HTTP 細節
- DB schema

## Flow Overview

整個下單流程不是單一 API，而是由多個斷點組成：

1. 前端發起物流選擇
2. 綠界回打物流選擇結果
3. 前端正式建立訂單
4. 前端跳轉付款頁
5. 金流回打付款結果

---

## Step 1. 開始選擇物流

### API

`POST /orders/logistics-selection`

### 呼叫者

Frontend

### Request

```json
{
  "goodsAmount": 500,
  "goodsName": "測試商品"
}
```

### Response

```json
{
  "html": "<form>...</form>"
}
```

### Application Use Case

`StartLogisticsSelection`

### Application 責任

- 產生 logistics session token
- 暫存 `token -> userId`
- 向物流服務請求選店頁
- 回傳 HTML 給前端

### Frontend 責任

- 接收 HTML
- 導頁到綠界物流選擇頁

---

## Step 2. 完成物流選擇

### API

`POST /ecpay/client-reply?token={token}`

### 呼叫者

ECPay

### Input

- `token`
- `ResultData`

### Application Use Case

`CompleteLogisticsSelection`

### Application 責任

- 驗證 token 是否有效
- 解析物流選擇結果
- 暫存使用者已選物流資料
- 清掉 logistics session
- redirect 前端回 checkout 頁

### 暫存資料語意

已選物流資料至少包含：

- 物流類型
- 物流子類型
- 收件人資訊
- 門市資訊或地址資訊
- 暫時物流編號

### Frontend 責任

- 接收 redirect
- 回到 checkout 頁
- 顯示已選物流資訊

---

## Step 3. 建立訂單

### API

`POST /orders`

### 呼叫者

Frontend

### Request

```json
{
  "paymentMethod": "LinePay"
}
```

### Response

```json
{
  "orderId": "guid",
  "paymentUrl": "https://..."
}
```

### Application Use Case

`CreateOrder`

### Application 責任

- 讀取購物車
- 驗證商品與庫存
- 讀取已選物流資料
- 建立 Order
- 依付款方式呼叫對應付款服務
- 建立 Payment
- 建立 Shipment
- 清空購物車
- 清除暫存物流資料

### Failure Handling

若流程中途失敗，需要由 application 定義補償規則，例如：

- 補回庫存
- 不建立訂單
- 不建立 payment
- 不建立 shipment

---

## Step 4. 跳轉付款頁

### 呼叫者

Frontend

### 行為

前端收到 `paymentUrl` 後，導向第三方付款頁。

---

## Step 5. 付款結果回調

### API

依付款方式分開：

- `GET /api/payments/line/callback/confirm`
- `GET /api/payments/line/callback/cancel`
- `GET /api/payments/paypal/callback/return`
- `GET /api/payments/paypal/callback/cancel`

### 呼叫者

LinePay / PayPal

### Application Use Case

- `MarkPaymentAsPaid`
- `MarkPaymentAsFailed`

### Application 責任

- 驗證付款結果
- 更新 Payment 狀態
- redirect 前端到成功或失敗頁

---

## Application Ports

Application 只定義抽象，不處理第三方細節。

### Repositories

- `ICartRepository`
- `IProductRepository`
- `IOrderRepository`
- `IPaymentRepository`
- `IShipmentRepository`

### External Service Ports

- 物流選擇服務
- 物流建單服務
- LinePay 付款服務
- PayPal 付款服務

### Cross-Cutting Ports

- `ICacher`
- `IUnitOfWork`

---

## Use Case List

- `StartLogisticsSelection`
- `CompleteLogisticsSelection`
- `CreateOrder`
- `CreatePayment`
- `CreateShipment`
- `MarkPaymentAsPaid`
- `MarkPaymentAsFailed`

---

## Core Rules

- 下單流程是多個 API 斷點組成，不是單一 request 完成
- Application 負責流程編排
- Domain 負責狀態與規則
- Infrastructure 負責第三方服務實作
- Application 不直接暴露第三方服務的 request / response model
