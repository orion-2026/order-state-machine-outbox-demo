# Order State Machine + Outbox Demo

A small .NET 10 WebAPI project based on the architecture lesson for **order status flow** in an e-commerce system.

## What this demo shows

- **Order service is the source of truth**
- **Explicit state machine** controls allowed transitions
- **Application service** centralizes business rules
- **Outbox pattern** records domain events after successful state changes
- **SQLite for easy local run**
- **PostgreSQL via Docker Compose** for a more realistic setup

## Scope

This is a teaching / demo project, not a production-ready system.

The goal is to make the architecture easy to understand and easy to run locally.

## Order lifecycle in this sample

```text
PendingPayment -> Paid -> Fulfilling -> Shipped -> Completed
PendingPayment -> Cancelled
Paid -> Refunded
Fulfilling -> Cancelled
Shipped -> Refunded
```

## Endpoints

### Health
- `GET /health`

### Orders
- `POST /api/orders`
- `GET /api/orders`
- `GET /api/orders/{id}`
- `GET /api/orders/{id}/actions`
- `POST /api/orders/{id}/transitions`

### Outbox
- `GET /api/outbox`
- `POST /api/outbox/publish`

## Option A: run locally with SQLite

### Requirements
- .NET 10 SDK

### Run
```bash
export PATH="/home/ubuntu/.local/share/dotnet:$PATH"
cp .env.example .env 2>/dev/null || true
dotnet restore
dotnet run
```

Default local DB:
- `order-demo.db`

The app auto-creates its schema on startup using `EnsureCreated()`.

## Option B: run with Docker + PostgreSQL

### Requirements
- Docker
- Docker Compose

### Run
```bash
docker compose up --build
```

API URL:
- `http://localhost:8080`

PostgreSQL:
- host: `localhost`
- port: `5432`
- db: `orders_demo`
- user: `postgres`
- password: `postgres`

## Example flow

### 1. Create order
```bash
curl -X POST http://localhost:5238/api/orders \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "cust-001",
    "productSku": "sku-demo-001",
    "quantity": 2
  }'
```

### 2. Check allowed actions
```bash
curl http://localhost:5238/api/orders/{orderId}/actions
```

### 3. Move state
```bash
curl -X POST http://localhost:5238/api/orders/{orderId}/transitions \
  -H "Content-Type: application/json" \
  -d '{
    "action": "pay",
    "reason": "Payment callback received"
  }'
```

### 4. Read outbox
```bash
curl http://localhost:5238/api/outbox
```

### 5. Publish pending outbox events
```bash
curl -X POST http://localhost:5238/api/outbox/publish
```

## Why this matches the architecture lesson

This sample directly maps to the lesson's recommendations:

- Do not let multiple services update order status freely
- Use explicit transitions instead of scattered if/else rules
- Treat events as business facts
- Record events through an outbox instead of pretending DB write + message publish is magically atomic

## What is still simplified

- Uses `EnsureCreated()` instead of full migrations
- No background outbox publisher yet
- No inbox / dedup table yet
- No authentication / authorization
- No optimistic concurrency handling at DB level yet

## Good next upgrades

- Add EF Core migrations
- Add background worker for outbox publishing
- Add dedup / inbox table for callback idempotency
- Add integration tests
- Split domain/application/infrastructure into separate projects
