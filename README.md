# Order State Machine + Outbox Demo

A small .NET 10 WebAPI project based on the architecture lesson for **order status flow** in an e-commerce system.

## What this demo shows

This project turns today's architecture topic into a minimal working sample:

- **Order service is the source of truth**
- **Explicit state machine** controls allowed transitions
- **Application service** centralizes business rules
- **Outbox pattern** records domain events after successful state changes
- **Idempotency-friendly direction** via versioning and explicit transition rules

## Scope

This is a **teaching / demo project**, not a production-ready system.

It intentionally keeps persistence simple by using an in-memory store so the core architecture is easy to read.

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

## Run

```bash
export PATH="/home/ubuntu/.local/share/dotnet:$PATH"
dotnet run
```

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

- **Do not let multiple services update order status freely**
- **Use explicit transitions instead of scattered if/else rules**
- **Treat events as business facts**
- **Record events through an outbox instead of pretending DB write + message publish is magically atomic**

## Next upgrades if you want a real project

- Replace in-memory store with PostgreSQL
- Add optimistic concurrency checks at persistence level
- Add inbox / dedup table for external callback idempotency
- Add background worker for outbox publishing
- Split application, domain, and infrastructure into separate projects
- Add integration tests
