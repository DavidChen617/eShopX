# eShopX

eShopX is a full-stack e-commerce system built with Angular, ASP.NET Core, PostgreSQL, Redis, Kafka, and Elasticsearch. The project is structured as a modular monolith with strong Vertical Slice and Clean Architecture / DDD-style boundaries, and includes Docker, Kubernetes, Terraform, and GitHub Actions for delivery.

## Architecture

The backend is organized around feature-oriented slices rather than traditional controller/service/repository folders. Each use case keeps its command or query, handler, validator, and mapping logic close together.

### Core Architectural Concepts

- **Vertical Slice Architecture**
  - Feature code is grouped under `src/ApplicationCore/UseCases/`
- **CQRS**
  - Read and write flows are split into dedicated queries and commands
- **Clean Architecture / DDD-style layering**
  - `ApplicationCore` contains business logic and abstractions
  - `Infrastructure` contains EF Core, external services, caching, messaging, and persistence implementations
  - `eShopX.Api` exposes Minimal API endpoints and composition root wiring
- **Repository + Unit of Work**
  - Repositories encapsulate persistence access
  - `EfUnitOfWork` coordinates transactions and execution strategy handling
- **Outbox Pattern**
  - Domain changes are persisted first
  - Background services publish outbox events to Kafka
  - Downstream consumers process those events asynchronously

## Technology Stack

### Backend

- **Framework:** .NET 10
- **API Style:** ASP.NET Core Minimal API
- **ORM:** Entity Framework Core
- **Database:** PostgreSQL
- **Cache:** Redis
- **Messaging:** Kafka
- **Search:** Elasticsearch
- **Cross-cutting:** custom mediator, mapping, logging, exception handling

### Frontend

- **Framework:** Angular
- **Styling:** Tailwind CSS
- **UI / Icons:** ng-zorro / Ant Design icons
- **Reactive layer:** RxJS
- **Production hosting:** Nginx

### External Integrations

- Google Login
- LINE Login
- LINE Pay
- PayPal
- Cloudinary
- Gmail SMTP

### DevOps & Infrastructure

- Docker / Docker Compose
- Kubernetes
- ingress-nginx
- cert-manager + Let's Encrypt
- Terraform
- GitHub Actions

## Project Structure

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

### Layer Responsibilities

- `src/ApplicationCore`
  - entities, interfaces, use cases, commands, queries, validators
- `src/Infrastructure`
  - EF Core context, repositories, cache, messaging, search, third-party integrations
- `src/eShopX.Api`
  - HTTP endpoints, app startup, DI wiring, environment configuration
- `src/eShopX.Common`
  - shared mediator, mapping, logging, exception handling
- `src/eShopX.Web`
  - Angular frontend

## Local Development

### Prerequisites

- Docker / Docker Desktop
- .NET SDK
- Node.js
- pnpm

### Start the full stack with Docker Compose

```bash
docker compose -f docker.compose.yaml up -d
```

This starts the main local dependencies and services, including:

- PostgreSQL
- Redis
- Kafka
- Elasticsearch
- API
- Web

### Frontend development

```bash
cd src/eShopX.Web
pnpm install
pnpm start
```

### Backend development

```bash
dotnet run --project src/eShopX.Api/
```

## Deployment

### Docker Compose

Useful for local and single-VM environments:

```bash
docker compose -f docker.compose.yaml up -d --build
```

### Kubernetes

Kubernetes manifests live under:

- `infra/k8s`

They cover:

- PostgreSQL
- Redis
- Kafka
- Elasticsearch
- API / Web Deployments
- Ingress
- cert-manager ClusterIssuer

### Terraform

AWS infrastructure definitions live under:

- `infra/terraform`

## CI/CD

The GitHub Actions workflow performs four major stages:

1. Generate EF migration SQL
2. Build and push API / Web Docker images
3. Run database migration on the self-hosted runner
4. Apply Kubernetes manifests to the cluster

Workflow file:

- `.github/workflows/build-push.yml`

## Git Hooks

To enable repository-managed hooks:

```bash
git config core.hooksPath hooks
```
