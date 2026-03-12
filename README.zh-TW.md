# eShopX

eShopX 是一個以電商場景為核心的全端專案，包含 Angular 前端、ASP.NET Core 後端，以及 Docker、Kubernetes、Terraform 與 GitHub Actions 的部署流程。專案重點不只是 CRUD，而是把訂單、付款、搜尋、快取、訊息佇列與雲端部署整合成一套可實際運作的系統。

- 採用 **洋蔥式 + Clean Architecture / DDD 風格分層**
- 後端以 **Use Case / CQRS** 為核心組織程式
- 使用 **PostgreSQL + EF Core** 作為主要資料儲存
- 使用 **Redis** 做首頁與查詢快取
- 使用 **Kafka + Outbox Pattern** 做非同步事件傳遞
- 使用 **Elasticsearch** 做商品搜尋與索引同步
- 整合 **Google Login、LINE Login、LINE Pay、PayPal、Cloudinary**
- 提供 **Docker Compose 本機啟動** 與 **Kubernetes 雲端部署**

## 核心架構

### 1. Vertical Slice Architecture

專案不是把 Controller、Service、Repository 全部分散在水平分層，而是以功能切片來組織。每個 Use Case 會包含自己的：

- Command / Query
- Handler
- Validator
- Mapping Profile

例如：

- 訂單建立：`src/ApplicationCore/UseCases/Orders/CreateOrder`
- 商品查詢：`src/ApplicationCore/UseCases/Products/GetProducts`
- 評價建立：`src/ApplicationCore/UseCases/Orders/CreateReview`

這樣的好處是功能聚合度高，改需求時比較容易定位影響範圍。

### 2. DDD / Clean Architecture 風格

專案大致分成以下幾層：

- `src/ApplicationCore`
  - Domain Entity
  - Interface
  - Use Case
- `src/Infrastructure`
  - EF Core
  - Repository 實作
  - 第三方服務整合
  - Kafka / Redis / Elasticsearch
- `src/eShopX.Api`
  - Minimal API Endpoints
  - DI 與應用啟動設定
- `src/eShopX.Common`
  - 共用例外處理
  - Mediator
  - Mapping
  - Logging 與工具類別

`ApplicationCore` 不直接依賴外部實作，而是透過介面與 `Infrastructure` 解耦。

### 3. CQRS / Use Case 模式

讀寫分離是這個專案很明顯的設計方式：

- Query 專注讀取
- Command 專注狀態變更

例如：

- `GetProductsQuery`
- `CreateOrderCommand`
- `CreatePaidOrderFromCartCommand`

這讓交易流程、讀取流程與背景任務邏輯比較容易拆開。

### 4. Repository + Unit of Work

資料存取透過 Repository 抽象包裝：

- `IReadRepository<T>`
- `IRepository<T>`
- `IUnitOfWork`

實作在：

- `src/Infrastructure/Data/Repositories/RepositoryBase.cs`
- `src/Infrastructure/Data/EfUnitOfWork.cs`

訂單與付款建立等複合流程，會透過 `ExecuteInTransactionAsync(...)` 包住 transaction 與 execution strategy。

### 5. Outbox Pattern + Kafka

這個專案有實作相對完整的 Outbox 流程。

大致流程：

1. 業務資料先寫入 DB
2. 對應事件寫入 `OutboxEvents`
3. `OutboxPublisherHostedService` 定時掃描待發布事件
4. 發送到 Kafka topic
5. `OutboxConsumerHostedService` 消費 topic
6. 後續處理，例如商品搜尋索引同步

這樣可以降低「資料已寫入 DB，但事件未成功送出」的不一致問題。

相關位置：

- `src/Infrastructure/Messaging/OutboxPublisherHostedService.cs`
- `src/Infrastructure/Messaging/OutboxConsumerHostedService.cs`
- `src/Infrastructure/Messaging/Products/ProductIndexOutboxEventHandler.cs`

### 6. 搜尋與快取

#### Elasticsearch

用於商品搜尋與索引同步：

- 商品資料變更後透過事件更新索引
- 搜尋服務有 fallback 到 DB 的設計

#### Redis

主要用在首頁與部分查詢結果快取，例如：

- banner
- categories
- recommend products
- homepage reviews

## 技術棧

### Backend

- .NET 10
- ASP.NET Core Minimal API
- Entity Framework Core
- PostgreSQL
- Redis
- Kafka
- Elasticsearch
- 自製 Mediator
- 自製 Mapping / Logging / Exception Handling

### Frontend

- Angular
- RxJS
- Tailwind CSS
- ng-zorro / Ant Design icons

### 第三方整合

- Google OAuth
- LINE Login
- LINE Pay
- PayPal
- Cloudinary
- Gmail SMTP

### DevOps / Infra

- Docker / Docker Compose
- Kubernetes
- ingress-nginx
- cert-manager + Let's Encrypt
- Terraform
- GitHub Actions

## 目錄結構

```text
.
├── docker.compose.yaml
├── infra
│   ├── k8s
│   └── terraform
├── src
│   ├── ApplicationCore
│   ├── Infrastructure
│   ├── eShopX.Api
│   ├── eShopX.Common
│   └── eShopX.Web
└── tests
```

## 本機啟動

### 1. 啟動基礎服務

```bash
docker compose -f docker.compose.yaml up -d
```

預設會啟動：

- PostgreSQL
- Redis
- Kafka
- Elasticsearch
- API
- Web

### 2. 前端本機開發

```bash
cd src/eShopX.Web
pnpm install
pnpm start
```

### 3. 後端本機開發

```bash
dotnet run --project src/eShopX.Api/
```

## 部署方式

### Docker Compose

適合本機或單機 VM 測試：

```bash
docker compose -f docker.compose.yaml up -d --build
```

### Kubernetes

Kubernetes manifest 放在：

- `infra/k8s`

常見資源包含：

- PostgreSQL
- Redis
- Kafka
- Elasticsearch
- API / Web Deployment
- Ingress
- cert-manager ClusterIssuer

### Terraform

AWS 基礎設施在：

- `infra/terraform`

主要用途：

- EC2
- Security Group
- K8s 節點基礎網路設定

## CI/CD 流程

GitHub Actions workflow 會做四件事：

1. 產生 EF migration SQL
2. Build 並 push API / Web Docker image
3. 在 self-hosted runner 上執行資料庫 migration
4. 套用 Kubernetes manifests

workflow 檔案位置：

- `.github/workflows/build-push.yml`

## Git Hooks

若要啟用 repo 內版本化的 Git hooks：

```bash
git config core.hooksPath hooks
```
