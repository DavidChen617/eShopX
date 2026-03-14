# eShopX

時尚電商平台，採用 DDD 分層架構設計。

---

## 架構分層

```
eShopX.Api
├── Minimal API Endpoints          ← CoreMesh.Endpoints (IEndpoint / IGroupEndpoint)
├── Error Handling                 ← CoreMesh.Result.AspNetCore (UseCoreMeshHttp)
├── DI Registration
└── appsettings.json

Application
├── Command Handlers               ← CoreMesh.Dispatching (IRequestHandler)
├── Query Handlers                 ← CoreMesh.Dispatching (IRequestHandler)
├── Event Handlers                 ← CoreMesh.Dispatching (INotificationHandler)
├── Validation                     ← CoreMesh.Validation (IValidatable)
├── Mapping                        ← CoreMesh.Mapper (IMapFrom / IMapWith)
├── Result                         ← CoreMesh.Result (Result<T>)
├── DTOs / Requests / Responses
└── Interfaces
    ├── IEmailService
    ├── ILinePayService
    ├── IPayPalService
    ├── IECPayService
    ├── ISearchService
    └── IImageStorage

Domain
├── Aggregates
│   ├── User / UserAuthProvider
│   ├── Order / OrderItem
│   ├── Payment
│   ├── Shipment (CVSShipment / HomeShipment)
│   ├── Product / Variant / Sku / Image / Tag
│   ├── Cart / CartItem
│   └── OutboxEvent
├── Value Objects
│   ├── Address
│   └── Avatar
├── Domain Events                  ← IDomainEvent : INotification (CoreMesh.Dispatching.Abstractions)
│   ├── OrderShippedEvent
│   ├── PaymentPaidEvent / PaymentFailedEvent
│   ├── ShipmentCompletedEvent
│   └── ProductCreatedEvent / ProductUpdatedEvent / ProductDeletedEvent
└── Primitives
    ├── AggregateRoot              ← 持有 DomainEvents list
    ├── Entity
    └── ValueObject

Infrastructure
├── Data
│   ├── AppDbContext (EF Core / PostgreSQL)
│   └── Repositories
├── Cache
│   └── Redis (OTP / LogisticsSession / QueryEmbedding)
├── Search
│   ├── ElasticsearchClient
│   └── SemanticKernel (ITextEmbeddingGenerationService)
├── Payment
│   ├── LinePayService
│   └── PayPalService
├── Logistics
│   └── ECPayService
├── Notification
│   └── EmailService
├── Storage
│   └── CloudinaryService
├── Logging                        ← CoreMesh.Logging (AddFileLogger)
└── BackgroundJobs
    └── Hangfire (OutboxProcessor / SandboxCompleteJob)
```

---

## 依賴方向

```
eShopX.Api → Application → Domain
Infrastructure → Application (implements interfaces)
Infrastructure → Domain
eShopX.Api → Infrastructure (DI)
```

---

## CoreMesh 套件對應

| 套件 | 用途 | 使用位置 |
|---|---|---|
| Dispatching.Abstractions | IRequest / INotification 契約 | Domain (IDomainEvent) |
| Dispatching | Command / Query / Event 調度 | Application / eShopX.Api |
| Validation | 輸入驗證 | Application (Handlers) |
| Mapper | DTO 映射 | Application (Handlers) |
| Result | 結果型別 | Application / eShopX.Api |
| Result.AspNetCore | HTTP 回應轉換 / 全域例外處理 | eShopX.Api |
| Endpoints | Minimal API 端點發現 | eShopX.Api |
| Interception | AOP 攔截（logging / auth） | eShopX.Api / Infrastructure |
| Logging | 檔案 Logger | Infrastructure |
