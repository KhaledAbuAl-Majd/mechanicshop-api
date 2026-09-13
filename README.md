# MechanicShop API

[![CI/CD Pipeline](https://github.com/KhaledAbuAl-Majd/mechanicshop-api/actions/workflows/ci-cd.yml/badge.svg)](https://github.com/KhaledAbuAl-Majd/mechanicshop-api/actions/workflows/ci-cd.yml)
![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)
![Docker](https://img.shields.io/badge/Docker-Containerized-2496ED?logo=docker)
![Azure Container Apps](https://img.shields.io/badge/Azure-Container%20Apps-0089D6?logo=microsoftazure)
![Redis](https://img.shields.io/badge/Redis-Upstash-DC382D?logo=redis)
![Tests](https://img.shields.io/badge/Tests-624%20Passed-brightgreen?logo=checkmarx)

**MechanicShop** is a backend REST API for managing an automotive repair shop's day-to-day operations: scheduling, work orders, customers and vehicles, repair tasks, labor assignment, and invoicing.

The project is primarily a study and demonstration of backend architecture rather than a full product: **Clean Architecture**, **SOLID principles**, **CQRS with MediatR** (including custom pipeline behaviors), tactical **Domain-Driven Design**, the **Result pattern**, the **Transactional Outbox pattern**, **Idempotency pattern**, **Testing**, and a full CI/CD pipeline to the cloud.

---

## Table of Contents

- [Live Endpoints and API Documentation](#live-endpoints-and-api-documentation)
- [Demo Credentials](#demo-credentials)
- [Architecture and Engineering Decisions](#architecture-and-engineering-decisions)
- [Testing Strategy](#testing-strategy)
- [Infrastructure and CI/CD Pipeline](#infrastructure-and-cicd-pipeline)
- [Local Development Setup](#local-development-setup)
- [Tech Stack Summary](#tech-stack-summary)

---

## Live Endpoints and API Documentation

The API is deployed on **Azure Container Apps** with a custom domain and managed SSL.

- **Scalar API Reference:** https://mechanicshop-api.khaledabualmajd.dev/scalar
- **Swagger UI:** https://mechanicshop-api.khaledabualmajd.dev/swagger
- **OpenAPI Specification (JSON):** https://mechanicshop-api.khaledabualmajd.dev/openapi/v1.json
- **Prometheus Metrics:** https://mechanicshop-api.khaledabualmajd.dev/metrics

---

## Demo Credentials

There is no public registration endpoint. Two accounts are seeded on startup for demo purposes, one per role. The password is the same as the email:

| Role | Email | Password | Access |
|---|---|---|---|
| **Manager** | `manager@test` | `manager@test` | Full access: work orders, invoices, customer management, assigning labor |
| **Labor / Mechanic** | `labor@test` | `labor@test` | View assigned work orders, update assigned repair tasks |

---

## Architecture and Engineering Decisions

### Clean Architecture Layers

```text
src/
├── MechanicShop.Domain          # Entities, domain events, Result pattern, business invariants
├── MechanicShop.Application     # CQRS features (commands/queries + handlers), pipeline behaviors, DTOs
├── MechanicShop.Infrastructure  # EF Core, Identity, token provider, Outbox background service
└── MechanicShop.Api             # Controllers, API versioning, middleware, rate limiting, OpenAPI
```

### Why No Generic Repository Pattern

The application injects `IAppDbContext` directly instead of wrapping EF Core in a repository abstraction.

- EF Core's `DbSet<T>` already behaves like a repository, and `DbContext` already acts as a Unit of Work.
- `IAppDbContext` exposes the `DbSet` properties and `SaveChangesAsync` needed by the Application layer, nothing more.

### Domain-Driven Design and Result Pattern

- Entities such as `WorkOrder`, `Customer`, and `Invoice` have private constructors and are created through static factory methods (e.g. `WorkOrder.Create(...)`), keeping invalid states out of the domain.
- Handlers return a custom `Result<T>` / `Result` with typed `Error` objects (`Validation`, `NotFound`, `Conflict`, `Forbidden`, ...) instead of throwing exceptions for expected business failures.
- This covers most, but not all, of tactical DDD — it's closer to ~85-90% than a textbook implementation.

### CQRS and Pipeline Behaviors

Every feature is split into commands/queries with MediatR handlers. Cross-cutting concerns are handled through pipeline behaviors instead of being repeated in each handler:

- **`ValidationBehaviour`** — runs FluentValidation validators before the handler executes.
- **`CachingBehaviour`** — automatically caches responses for queries marked with `ICachedQuery`.
- **`CacheInvalidationBehaviour`** — evicts the relevant cache entries when a command marked with `IInvalidateCacheCommand` succeeds.

### Transactional Outbox Pattern

Used to avoid the dual-write problem when a database change needs a side effect (like a notification) to happen reliably alongside it:

- `OutboxMessageInterceptor` intercepts `SaveChangesAsync`, converts any raised domain events into `OutboxMessages` rows, and writes them in the same database transaction as the actual change.
- `OutboxProcessorBackgroundService` runs on a timer, reads unprocessed messages, publishes them to their handlers, and marks them as processed, tracking retry counts for failures.

### Idempotent Requests

State-changing endpoints that create a resource (e.g. creating a customer) are decorated with a custom `[Idempotent]` action filter:

- The client sends an `X-Idempotency-Key` header.
- The filter stores the executed response using `HybridCache`, which provides stampede/race-condition protection out of the box — this was the main reason it was chosen over a plain in-memory dictionary.

### Authentication and Authorization

- Built on **ASP.NET Core Identity** (`IdentityUser`), with a custom `TokenProvider` for JWT access and refresh tokens.
- The token provider uses `TimeProvider` instead of `DateTime.UtcNow` directly, so token expiry logic is testable.
- Refresh tokens rotate on use: the old one is revoked, a new one is issued, and multiple devices can hold valid tokens for the same user at the same time.
- Authorization is role-based (`Manager`, `Labor`), with custom policies for finer-grained rules — e.g. a resource-ownership policy/requirement/handler that checks whether the logged-in user is either the resource owner or a manager before allowing access.

### Caching Strategy

- **Hybrid Cache** (`Microsoft.Extensions.Caching.Hybrid`): in-memory L1 combined with Redis (Upstash) as L2.
- **Output Caching** on selected endpoints, with tag-based eviction (`cache.EvictByTagAsync(...)`) instead of time-based expiry alone.

### API Layer

- **RFC 9457 Problem Details** for all error responses, produced by a base `ApiController` that maps domain `Result` errors to the right HTTP status via `.Match(Ok, Problem)`.
- A **global exception handler** catches unhandled exceptions and returns them in the same Problem Details format.
- **API versioning** via URL segment (`/v1/...`), with controllers, requests, and responses organized by version.
- **OpenAPI, Swagger UI, and Scalar** for documentation, including custom document/operation transformers (e.g. to document the idempotency header and JWT bearer scheme).
- **Rate limiting** partitioned by authenticated user ID or IP, with a stricter policy on auth endpoints and a separate concurrency limiter for heavier operations.
- **CORS**, configured from app settings rather than hardcoded.

### Observability

- **Serilog** for structured logging, shipped to **Seq**.
- **OpenTelemetry** tracing and metrics, exported via OTLP and scraped by **Prometheus**, visualized in **Grafana**.

---

## Testing Strategy

```text
624 Total Tests (100% Passed)
├── MechanicShop.Domain.UnitTests              : 178 Tests — domain logic, invariants, Result pattern
├── MechanicShop.Application.UnitTests         :  42 Tests — behaviors, mappers, business policies
├── MechanicShop.Application.SubcutaneousTests : 236 Tests — CQRS handlers against a real DB, no HTTP layer
└── MechanicShop.Api.IntegrationTests          : 168 Tests — full HTTP round trip
```

- **Subcutaneous tests** send MediatR commands/queries directly through a `WebApplicationFactory`, against a real database, skipping HTTP serialization.
- **Integration tests** spin up a real, isolated SQL Server instance per run using `Testcontainers.MsSql`.
- Database state between tests is reset using **Respawn**.

---

## Infrastructure and CI/CD Pipeline

```text
[Push to develop] ──────► [Build & Run 624 Tests]

[PR Merge to main] ─────► [Build & Test] ──► [Docker Build] ──► [Push to GHCR] ──► [Deploy to Azure Container Apps]
```

- **CI:** restores dependencies via central package management, builds in `Release`, and runs the full test suite on every push/PR to `main` and `develop`.
- **CD (main branch only):** builds the Docker image and pushes it to **GitHub Container Registry (GHCR)**, tagged with the commit SHA, then deploys the new revision to **Azure Container Apps**.
- **Production dependencies:**
  - Compute: Azure Container Apps
  - Database: Azure SQL
  - Distributed cache: Redis via [Upstash](https://console.upstash.com/)
  - SSL/Domain: custom domain with an Azure-managed certificate

---

## Local Development Setup

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

### Run with Docker Compose

```bash
git clone https://github.com/KhaledAbuAl-Majd/mechanicshop-api.git
cd mechanicshop-api
docker-compose up -d
```

This starts the API along with SQL Server, Redis, and Prometheus.

### Or run locally via the CLI

Set a local SQL Server connection string in `appsettings.Development.json`, then:

```bash
dotnet restore MechanicShop.slnx
dotnet run --project src/MechanicShop.Api
```

Migrations and data seeding run automatically on startup in development.

---

## Tech Stack Summary

- **Runtime:** .NET 10 (C# 13)
- **Architecture:** Clean Architecture, CQRS, tactical DDD, Outbox pattern, Result pattern
- **Libraries:** MediatR, FluentValidation, Entity Framework Core, QuestPDF
- **Caching:** `Microsoft.Extensions.Caching.Hybrid`, Redis (Upstash), Output Cache
- **Identity and Security:** ASP.NET Core Identity, JWT bearer tokens, custom authorization policies
- **Resilience:** `System.Threading.RateLimiting`, custom idempotency action filter
- **Testing:** xUnit, NSubstitute, `WebApplicationFactory`, `Testcontainers.MsSql`, Respawn
- **Observability:** Serilog, Seq, OpenTelemetry, Prometheus, Grafana
- **DevOps and Cloud:** Docker, GitHub Actions, GHCR, Azure Container Apps, Azure SQL
