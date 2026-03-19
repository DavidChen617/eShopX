# eShopX

全端電商系統，後端採 ASP.NET Core Minimal API + DDD 分層，前端採 Angular，基礎設施以 Docker / Kubernetes / Terraform 部署於 AWS，並透過 GitHub Actions 實現 CI/CD。

---

### Hybrid Search（混合搜索）
商品搜索結合 **BM25 關鍵字匹配**與 **kNN 語義向量搜索**，向量由 HuggingFace BGE-M3（1024 維，多語言模型）生成並索引至 Elasticsearch。

純 BM25 的死角：用戶搜「上班不累腳的鞋」，商品描述寫「記憶泡棉、人體工學」→ 零分。
純語義的死角：搜精確型號「Nike Air Max 90」→ 向量相似度不如字面匹配。
Hybrid 同時解決兩個問題；BGE-M3 支援跨語言，英文 query 可直接命中中文商品。

### Outbox Pattern（保證訊息至少一次送達）
Domain 狀態變更（商品上架、付款成功）先寫入同一個 PostgreSQL transaction，再由 Background Service 發布至 Kafka，Consumer 異步處理後續副作用（Elasticsearch 索引同步、寄送確認信）。

不用雙寫，不怕 message queue 寫失敗導致資料不一致；任何 Consumer 失敗皆可重試，不影響主流程。

### 補償事務（Compensating Transaction）
下單流程在呼叫第三方金流前，Order / Payment / Shipment 已寫入 DB 並扣除庫存。若金流回傳失敗，Handler 執行補償事務：還原庫存、刪除訂單、還原購物車，確保不留孤兒資料。

### CQRS + Vertical Slice Architecture
每個 Use Case（`CreateOrder`、`SearchProducts` 等）是一個獨立 slice，包含 Command/Query、Handler、Validator。讀寫路徑分離，互不干擾，新增功能不需修改既有程式碼。

---

## 技術棧

### 後端
| 項目 | 技術 |
|------|------|
| Framework | .NET 10 / ASP.NET Core Minimal API |
| ORM | Entity Framework Core |
| 資料庫 | PostgreSQL |
| 快取 | Redis（Cache-Aside，TTL + 主動失效） |
| 訊息佇列 | Kafka |
| 搜索引擎 | Elasticsearch |
| 語義向量 | HuggingFace BGE-M3（1024 維） |

### 前端
| 項目 | 技術 |
|------|------|
| Framework | Angular |
| Styling | Tailwind CSS |
| UI 元件 | PrimeNG |
| 非同步處理 | RxJS |

### 第三方整合
- **身份驗證：** Google OAuth、LINE OAuth
- **金流：** LINE Pay、PayPal
- **物流：** 綠界科技 ECPay
- **圖片儲存：** Cloudinary
- **通知：** Gmail SMTP

### 基礎設施
| 項目 | 技術 |
|------|------|
| 容器化 | Docker / Docker Compose |
| 編排 | Kubernetes |
| Ingress | ingress-nginx + cert-manager（Let's Encrypt） |
| IaC | Terraform（AWS EC2、VPC、Security Group、IAM） |
| CI/CD | GitHub Actions |

---

## 架構分層

```
eShopX.Api       → Minimal API Endpoints、DI 組裝、Middleware
Application      → Use Cases（CQRS Handler）、Validator、介面定義
Domain           → Aggregates、Value Objects、Domain 例外、Outbox 事件
Infrastructure   → EF Core、Repository、Redis、Kafka、Elasticsearch、第三方整合
eShopX.Web       → Angular 前端
```

依賴方向：`eShopX.Api → Application → Domain`，Infrastructure 實作 Application 定義的介面，Domain 層無任何外部相依。

---

## CI/CD 流程

PR 從 `develop` merge 進 `main` 後自動觸發，依序執行：

1. 產生 EF Core idempotent migration SQL
2. Build & Push API / Web Docker image 至 Docker Hub
3. 在 self-hosted runner 執行 DB migration
4. `kubectl apply` 更新 Kubernetes 集群

---

## 本地開發

**Prerequisites：** Docker、.NET SDK、Node.js、pnpm

```bash
# 啟動全部相依服務（PostgreSQL、Redis、Kafka、Elasticsearch）
docker compose -f docker.compose.yaml up -d

# 前端開發模式
cd src/eShopX.Web
pnpm install
pnpm start

# 後端單獨啟動
dotnet run --project src/eShopX.Api/
```

---

## 專案結構

```
.
├── .github/workflows/   # GitHub Actions CI/CD
├── infra/
│   ├── k8s/             # Kubernetes manifests
│   └── terraform/       # AWS 基礎設施定義
└── src/
    ├── Domain/          # Aggregates、Value Objects、Domain 例外、Outbox 事件
    ├── Application/     # Use Cases、CQRS Handler、Validator、介面定義
    ├── Infrastructure/  # EF Core、快取、訊息、搜索、第三方
    ├── eShopX.Api/      # Minimal API Endpoints、啟動設定
    └── eShopX.Web/      # Angular 前端
```

---

## Git Hooks

```bash
git config core.hooksPath hooks
```
