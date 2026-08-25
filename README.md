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

### Prerequisites

* [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
* [Docker](https://www.docker.com/) (for PostgreSQL and RabbitMQ — no local install of either needed)
* The [`dotnet-ef`](https://learn.microsoft.com/en-us/ef/core/cli/dotnet) global tool: `dotnet tool install --global dotnet-ef`

### 1. Start the database and message broker

```bash
cd template/backend
docker compose up -d ambev.developerevaluation.database ambev.developerevaluation.messagebroker
```

This starts:

* PostgreSQL 13 on `localhost:5432` (database `developer_evaluation`, user `developer`, password `ev@luAt10n` — already set in `docker-compose.yml` and matching `src/Ambev.DeveloperEvaluation.WebApi/appsettings.json`'s `ConnectionStrings:DefaultConnection`)
* RabbitMQ 3 (management plugin) on `localhost:5672` (AMQP, same `developer`/`ev@luAt10n` credentials, matching `ConnectionStrings:RabbitMq`) with its management UI at [http://localhost:15672](http://localhost:15672)

Nothing to configure — the defaults just work.

### 2. Apply database migrations

```bash
cd template/backend/src/Ambev.DeveloperEvaluation.WebApi
dotnet ef database update --project ../Ambev.DeveloperEvaluation.ORM/Ambev.DeveloperEvaluation.ORM.csproj --startup-project Ambev.DeveloperEvaluation.WebApi.csproj --context DefaultContext
```

This creates the `Users`, `Sales` and `SaleItems` tables.

### 3. Run the API

```bash
cd template/backend/src/Ambev.DeveloperEvaluation.WebApi
dotnet run
```

The API starts on the URL printed in the console (see `Properties/launchSettings.json`; typically `http://localhost:5298` or similar). Swagger UI is available at `/swagger` in the Development environment.

> **Note:** this repo targets `net8.0`. If your machine only has a newer .NET runtime installed (no .NET 8 runtime), prefix the `dotnet ef`/`dotnet run` commands above with `DOTNET_ROLL_FORWARD=LatestMajor` (PowerShell: `$env:DOTNET_ROLL_FORWARD="LatestMajor"`) to let it run on the newer runtime instead of installing .NET 8 separately.

### Alternative: full Docker Compose stack

`docker-compose.yml` also defines an `ambev.developerevaluation.webapi` service. It builds and runs, but has not been fully wired for one-command startup (its connection string still needs to target the `ambev.developerevaluation.database` service hostname instead of `localhost` when running inside the Docker network) — running the API locally with `dotnet run` against the dockerized database, as described above, is the supported path for now.

## Running Tests

```bash
cd template/backend
dotnet test tests/Ambev.DeveloperEvaluation.Unit/Ambev.DeveloperEvaluation.Unit.csproj
```

Runs the xUnit unit test suite (entity behavior, validators, and command handlers for both `User` and `Sale`, using NSubstitute for mocking and Bogus for test data generation). No database or running API is required for these tests.

## API Documentation

* [General API](/.doc/general-api.md) — response envelope, error shape, and the pagination/ordering query contract
* [Sales API](/.doc/sales-api.md) — the full Sales CRUD (this is the actual deliverable for this project)

A Postman/Insomnia collection is not included; the endpoint documentation above with example requests/responses is intended to cover manual testing.

## Implementation Status

**Implemented:**

* Full Sales CRUD (create, get by id, paginated/filtered list, update, delete) plus dedicated cancel-sale, cancel-item, and reactivate-sale actions
* External Identities pattern for Customer/Branch/Product (denormalized id + name, no cross-domain foreign keys)
* Quantity-based discount business rules, enforced both by request validation and as a domain invariant on the `Sale` aggregate (see [Sale.cs](/template/backend/src/Ambev.DeveloperEvaluation.Domain/Entities/Sale.cs))
* `SaleCreated` / `SaleModified` / `SaleCancelled` / `ItemCancelled` / `SaleReactivated` events, published to RabbitMQ via Rebus and consumed by dedicated handlers that write to the application log
* All `/api/Sales` endpoints require a JWT (`[Authorize]`); Swagger's **Authorize** button is wired up with a Bearer scheme, so the padlock appears on every protected endpoint and you can paste a token obtained from `POST /api/Auth`
* Standardized `{ type, error, detail }` error responses for not-found, business-rule-violation, and authentication-failure cases
* Unit tests covering the discount tiers, cancellation behavior, validators, and command handlers

**Known limitations / what I'd do with more time:**

* The full multi-container `docker compose up` flow (API + Postgres together) isn't wired end-to-end yet — see the note under Getting Started
* List filtering supports pagination, ordering, cancelled/customer/branch/date-range filters, but not the full generic wildcard/`_min`/`_max` query contract described in [General API](/.doc/general-api.md)
* Only unit tests are included; the `tests/Ambev.DeveloperEvaluation.Integration` and `tests/Ambev.DeveloperEvaluation.Functional` projects are still empty scaffolding
* MongoDB and Redis services are defined in `docker-compose.yml` but unused — PostgreSQL via EF Core was the chosen data store
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
