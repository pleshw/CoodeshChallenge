# Developer Evaluation Project

`READ CAREFULLY`

## Use Case
**You are a developer on the DeveloperStore team. Now we need to implement the API prototypes.**

As we work with `DDD`, to reference entities from other domains, we use the `External Identities` pattern with denormalization of entity descriptions.

Therefore, you will write an API (complete CRUD) that handles sales records. The API needs to be able to inform:

* Sale number
* Date when the sale was made
* Customer
* Total sale amount
* Branch where the sale was made
* Products
* Quantities
* Unit prices
* Discounts
* Total amount for each item
* Cancelled/Not Cancelled

It's not mandatory, but it would be a differential to build code for publishing events of:
* SaleCreated
* SaleModified
* SaleCancelled
* ItemCancelled

If you write the code, **it's not required** to actually publish to any Message Broker. You can log a message in the application log or however you find most convenient.

### Business Rules

* Purchases above 4 identical items have a 10% discount
* Purchases between 10 and 20 identical items have a 20% discount
* It's not possible to sell above 20 identical items
* Purchases below 4 items cannot have a discount

These business rules define quantity-based discounting tiers and limitations:

1. Discount Tiers:
   - 4+ items: 10% discount
   - 10-20 items: 20% discount

2. Restrictions:
   - Maximum limit: 20 items per product
   - No discounts allowed for quantities below 4 items

## Getting Started

### Option A: one-command full stack (recommended)

Requires only [Docker](https://www.docker.com/).

```bash
cd template/backend
docker compose up -d
```

This builds the API image, starts PostgreSQL and RabbitMQ, waits for both to
report healthy (`depends_on: condition: service_healthy`), then starts the
API — which applies any pending EF Core migrations itself on boot (gated by
the `RUN_MIGRATIONS_ON_STARTUP` env var, set only for this containerized
path) before serving traffic. No separate migration step, no manual
`localhost` vs. container-hostname connection-string juggling.

* API: [http://localhost:5119](http://localhost:5119) (Swagger at `/swagger`)
* RabbitMQ management UI: [http://localhost:15672](http://localhost:15672) (`developer`/`ev@luAt10n`)
* PostgreSQL: `localhost:5432` (`developer_evaluation` / `developer` / `ev@luAt10n`)

Rebuild after changing code: `docker compose up -d --build`. Tear everything
down (keeping data): `docker compose down`; including the database volume:
`docker compose down -v`.

### Option B: run the API on bare metal (faster edit/run loop while coding)

Skips the Docker image rebuild step, at the cost of a couple more manual commands.

**Prerequisites:** [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0), Docker (for Postgres/RabbitMQ only), the [`dotnet-ef`](https://learn.microsoft.com/en-us/ef/core/cli/dotnet) global tool (`dotnet tool install --global dotnet-ef`).

#### 1. Start the database and message broker

```bash
cd template/backend
docker compose up -d ambev.developerevaluation.database ambev.developerevaluation.messagebroker
```

Nothing to configure — the defaults in `docker-compose.yml` already match `src/Ambev.DeveloperEvaluation.WebApi/appsettings.json`'s `ConnectionStrings`.

#### 2. Apply database migrations

```bash
cd template/backend/src/Ambev.DeveloperEvaluation.WebApi
dotnet ef database update --project ../Ambev.DeveloperEvaluation.ORM/Ambev.DeveloperEvaluation.ORM.csproj --startup-project Ambev.DeveloperEvaluation.WebApi.csproj --context DefaultContext
```

This creates the `Users`, `Sales` and `SaleItems` tables. (`RUN_MIGRATIONS_ON_STARTUP` from Option A is not set here on purpose, so this step stays explicit for local development — see `Program.cs`.)

#### 3. Run the API

```bash
cd template/backend/src/Ambev.DeveloperEvaluation.WebApi
dotnet run
```

The API starts on the URL printed in the console (see `Properties/launchSettings.json`; typically `http://localhost:5298` or similar). Swagger UI is available at `/swagger` in the Development environment.

> **Note:** this repo targets `net8.0`. If your machine only has a newer .NET runtime installed (no .NET 8 runtime), prefix the `dotnet ef`/`dotnet run` commands above with `DOTNET_ROLL_FORWARD=LatestMajor` (PowerShell: `$env:DOTNET_ROLL_FORWARD="LatestMajor"`) to let it run on the newer runtime instead of installing .NET 8 separately.

## Running Tests

```bash
cd template/backend
dotnet test tests/Ambev.DeveloperEvaluation.Unit/Ambev.DeveloperEvaluation.Unit.csproj
```

Runs the xUnit unit test suite (entity behavior, validators, and command handlers for both `User` and `Sale`, using NSubstitute for mocking and Bogus for test data generation). No database or running API is required for these tests.

## API Documentation

* [General API](/.doc/general-api.md) — response envelope, error shape, and the pagination/ordering query contract
* [Sales API](/.doc/sales-api.md) — the full Sales CRUD (this is the actual deliverable for this project)
* [Manual Testing Guide](/.doc/manual-testing-guide.md) — step-by-step walkthrough (register, login, exercise every Sales endpoint, verify events) using curl or Swagger UI
* [Postman Collection](/.doc/postman/Ambev.DeveloperEvaluation.postman_collection.json) — importable collection mirroring the Manual Testing Guide (register → login, with the JWT captured automatically → all four discount-tier creates → get/list/update → cancel/reactivate → cancel item → delete). Verified end-to-end with `newman run` against a live instance; re-runnable without manual cleanup (each run registers a fresh, timestamp-suffixed email)

## Implementation Status

**Implemented:**

* Full Sales CRUD (create, get by id, paginated/filtered list, update, delete) plus dedicated cancel-sale, cancel-item, and reactivate-sale actions
* External Identities pattern for Customer/Branch/Product (denormalized id + name, no cross-domain foreign keys)
* Quantity-based discount business rules, enforced both by request validation and as a domain invariant on the `Sale` aggregate (see [Sale.cs](/template/backend/src/Ambev.DeveloperEvaluation.Domain/Entities/Sale.cs))
* `SaleCreated` / `SaleModified` / `SaleCancelled` / `ItemCancelled` / `SaleReactivated` events, published to RabbitMQ via Rebus and consumed by dedicated handlers that write to the application log
* All `/api/Sales` endpoints require a JWT (`[Authorize]`); Swagger's **Authorize** button is wired up with a Bearer scheme, so the padlock appears on every protected endpoint and you can paste a token obtained from `POST /api/Auth`
  * This was a deliberate decision: the Users/Auth feature (and its `Role` enum) already existed in the template, but nothing consumed the JWT it issued — no endpoint checked it. Rather than leave a working login system fully disconnected, I wired it into the Sales endpoints, the actual deliverable of this project. Note it only requires *any* authenticated user today — it doesn't yet restrict by `Role` (e.g. only `Manager`/`Admin` can cancel a sale); that would be a natural next step but wasn't required for this challenge
* Standardized `{ type, error, detail }` error responses for not-found, business-rule-violation, and authentication-failure cases
* Real dependency health checks: `GET /health/ready` actually opens a connection to Postgres and to RabbitMQ (not a hardcoded "always healthy" placeholder) and returns `503` if either is unreachable; `GET /health/live` stays dependency-free on purpose, since a liveness probe shouldn't fail just because a downstream service is down — see [PostgresHealthCheck.cs](/template/backend/src/Ambev.DeveloperEvaluation.WebApi/HealthChecks/PostgresHealthCheck.cs) / [RabbitMqHealthCheck.cs](/template/backend/src/Ambev.DeveloperEvaluation.WebApi/HealthChecks/RabbitMqHealthCheck.cs)
* One-command full stack: `docker compose up -d` builds the API image, waits for Postgres and RabbitMQ to be healthy (`depends_on: condition: service_healthy`), applies pending EF Core migrations on boot, and serves the API — verified from a completely fresh `docker compose down -v` state with the full Postman collection passing end-to-end against the containerized instance
* Redis-backed cache for `GET /api/Sales` (the paginated list endpoint), using generation-based invalidation: every cache entry's key embeds a generation number, and any Sale write (create/update/cancel/reactivate/cancel-item/delete) just increments that one counter — instantly invalidating every previously cached page/filter/order combination without enumerating or deleting individual keys. Fails open: if Redis is unreachable the endpoint still serves correctly straight from Postgres, just uncached (verified by stopping the `cache` container mid-session) — see [ISalesListCache.cs](/template/backend/src/Ambev.DeveloperEvaluation.Application/Sales/Common/ISalesListCache.cs) / [RedisSalesListCache.cs](/template/backend/src/Ambev.DeveloperEvaluation.IoC/Caching/RedisSalesListCache.cs)
* A few pre-existing bugs found via manual/Postman testing were fixed along the way (all in the `Users` feature, none in `Sales`): `POST /api/Users` and `GET /api/Users/{id}` were both missing AutoMapper configuration for the user's `name` (and `GetUser` was missing its `Result → Response` mapping entirely, throwing on every call); `POST /api/Users` with a duplicate email threw an unhandled `500` with an exposed stack trace instead of a clean `409 Conflict`
* Unit tests covering the discount tiers, cancellation behavior, validators, and command handlers

**Known limitations / what I'd do with more time:**

* List filtering supports pagination, ordering, cancelled/customer/branch/date-range filters, but not the full generic wildcard/`_min`/`_max` query contract described in [General API](/.doc/general-api.md)
* Only unit tests are included; the `tests/Ambev.DeveloperEvaluation.Integration` and `tests/Ambev.DeveloperEvaluation.Functional` projects are still empty scaffolding
* MongoDB is defined in `docker-compose.yml` but unused — PostgreSQL via EF Core was the chosen data store. Redis *is* now used, for the Sales list cache (see above)
* The Redis cache's worst case (Redis completely unreachable) adds real latency to `GET /api/Sales` — around 2s in testing, from four sequential Redis round-trips per request each hitting a 300ms timeout before falling back to Postgres — instead of failing instantly. Acceptable for this challenge's scope since Redis is always started by `docker compose up`, but a batched/pipelined read or a short in-process generation cache would remove it with more time
* RabbitMQ is deployed with default/no-op durability tuning (single node, no persistence policy beyond RabbitMQ's own defaults) — fine for local dev and for this challenge's scope, not production-hardened
* The `Sales` endpoints' JSON responses were fixed to avoid a double-wrapping bug in `BaseController`'s response helpers; the same fix hasn't been applied to the pre-existing `Users`/`Auth` endpoints, which still return a nested `data.data` envelope

## Overview
This section provides a high-level overview of the project and the various skills and competencies it aims to assess for developer candidates. 

See [Overview](/.doc/overview.md)

## Tech Stack
This section lists the key technologies used in the project, including the backend, testing, frontend, and database components. 

See [Tech Stack](/.doc/tech-stack.md)

## Frameworks
This section outlines the frameworks and libraries that are leveraged in the project to enhance development productivity and maintainability. 

See [Frameworks](/.doc/frameworks.md)

<!-- 
## API Structure
This section includes links to the detailed documentation for the different API resources:
- [API General](./docs/general-api.md)
- [Products API](/.doc/products-api.md)
- [Carts API](/.doc/carts-api.md)
- [Users API](/.doc/users-api.md)
- [Auth API](/.doc/auth-api.md)
-->

## Project Structure
This section describes the overall structure and organization of the project files and directories. 

See [Project Structure](/.doc/project-structure.md)
