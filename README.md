# eShopX

eShopX is a modern, scalable e-commerce platform engineered with a focus on clean architecture, performance, and cloud-native principles. It features a robust .NET backend employing Vertical Slice Architecture and a responsive Angular frontend, all streamlined for deployment using Docker and Kubernetes.

## Architecture

The project follows a **Vertical Slice Architecture**, moving away from traditional horizontal layering. This approach groups code by feature (e.g., "Add to Cart", "Checkout") rather than technical concern (Controller, Service, Repository), resulting in highly cohesive and maintainable codebases.

### Key Architectural Patterns:
*   **Vertical Slices:** Business logic is encapsulated in `UseCases` (e.g., `src/ApplicationCore/UseCases/`), containing everything needed to perform a specific action.
*   **CQRS (Command Query Responsibility Segregation):** Separation of read and write operations for optimized performance and scalability.
*   **Modular Endpoints:** API endpoints are defined close to their feature logic, adhering to the Minimal API paradigm for reduced boilerplate.
*   **Domain-Driven Design (DDD):** Core business entities and logic reside in `ApplicationCore`, isolated from external concerns.

## Technology Stack

### Backend (.NET)
*   **Framework:** .NET 10
*   **Data Access:** Entity Framework Core
*   **Database:** PostgreSQL
*   **Caching:** Redis
*   **Communication:** Mediator (In-process messaging), Custom Result Pattern
*   **Utilities:** Custom high-performance Logging (`ChannelLogDispatcher`)

### Frontend (Web)
*   **Framework:** Angular
*   **Styling:** Tailwind CSS
*   **State Management:** RxJS
*   **Server:** Nginx (for production serving)

### DevOps & Infrastructure
*   **Containerization:** Docker & Docker Compose
*   **Orchestration:** Kubernetes (K8s)
*   **IaC (Infrastructure as Code):** Terraform
*   **CI/CD:** GitHub Actions

## Project Structure

```text
/src
├── ApplicationCore   # Domain Entities, Interfaces, and Use Cases (Vertical Slices)
├── Infrastructure    # DB Context (EF Core), External Services, Migrations
├── eShopX.Api        # API Entry point, Endpoint configurations, DI setup
├── eShopX.Common     # Shared utilities, Exceptions, Custom Logging, Validator
└── eShopX.Web        # Angular Frontend Application
/k8s                  # Kubernetes manifest files for deployment
/terraform            # Terraform scripts for infrastructure provisioning
```

## Getting Started

### Prerequisites
*   Docker Desktop
*   .NET SDK
*   Node.js & pnpm/npm

### Running Locally (Docker Compose)
The easiest way to start the entire stack (Database, Redis, API, Web) is via Docker Compose:

```bash
docker-compose up -d
```

## Git Hooks

To enable the repository hooks, point Git to the versioned hooks directory:

```bash
git config core.hooksPath hooks
```
