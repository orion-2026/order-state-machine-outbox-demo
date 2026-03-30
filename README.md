# Order State Machine + Outbox Demo (.NET 10)

A .NET 10 WebAPI implementation of the order status flow architecture lesson.

## What this demo shows
- Order service as source of truth
- Explicit state machine for allowed transitions
- Application/service layer centralizing business rules
- Outbox pattern for domain event recording
- Easy local run with SQLite
- Docker Compose option with PostgreSQL

## Order lifecycle
```text
PendingPayment -> Paid -> Fulfilling -> Shipped -> Completed
PendingPayment -> Cancelled
Paid -> Refunded
Fulfilling -> Cancelled
Shipped -> Refunded
```

## API surface
- `GET /health`
- `POST /api/orders`
- `GET /api/orders`
- `GET /api/orders/{id}`
- `GET /api/orders/{id}/actions`
- `POST /api/orders/{id}/transitions`
- `GET /api/outbox`
- `POST /api/outbox/publish`

## Environment variables
- `PORT=8080`
- `DATABASE_PROVIDER=Sqlite | Postgres`
- `DATABASE_CONNECTION_STRING=...`

## Run locally
```bash
cp .env.example .env 2>/dev/null || true
export PATH="/home/ubuntu/.local/share/dotnet:$PATH"
PORT=8080 ASPNETCORE_URLS=http://localhost:8080 dotnet restore
PORT=8080 ASPNETCORE_URLS=http://localhost:8080 dotnet run --no-launch-profile
```

Default local DB:
- `order-demo.db`

## Run with Docker + PostgreSQL
```bash
docker compose up --build
```

Default URLs:
- API: `http://localhost:8080`
- PostgreSQL: `localhost:5432`

## Example flow
### Create order
```bash
curl -X POST http://localhost:8080/api/orders \
  -H "Content-Type: application/json" \
  -d '{"customerId":"cust-001","productSku":"sku-demo-001","quantity":2}'
```

### Check allowed actions
```bash
curl http://localhost:8080/api/orders/{orderId}/actions
```

### Transition state
```bash
curl -X POST http://localhost:8080/api/orders/{orderId}/transitions \
  -H "Content-Type: application/json" \
  -d '{"action":"pay","reason":"Payment callback received"}'
```

### Read outbox
```bash
curl http://localhost:8080/api/outbox
```

## Notes
- Uses `EnsureCreated()` for demo simplicity
- Returns enum values as strings for easier cross-language comparison
- Keeps the project intentionally small and teaching-oriented

## Good next upgrades
- Replace `EnsureCreated()` with EF Core migrations
- Add background outbox publisher
- Add inbox / dedup handling
- Add integration tests
