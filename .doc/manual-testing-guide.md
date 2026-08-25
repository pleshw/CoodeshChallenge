[Back to README](../README.md)

## Manual Testing Guide

Step-by-step walkthrough for exercising the whole Sales API by hand, either
with `curl` (copy-paste ready below) or Swagger UI (equivalent noted at each
step). Assumes the stack is already running — see
[Getting Started](../README.md#getting-started) if not; either `docker
compose up -d` (the whole stack, API included) or the bare-metal `dotnet
run` path both leave the API on `http://localhost:5119`, which every example
below assumes.

Prefer clicking through Postman instead? Import
[`postman/Ambev.DeveloperEvaluation.postman_collection.json`](./postman/Ambev.DeveloperEvaluation.postman_collection.json) —
it's the same walkthrough below as a ready-to-run collection (login/token
handling included), also runnable headlessly via `newman run`.

### 0. Open Swagger UI (optional, but the easiest way to follow along)

Go to `http://localhost:5119/swagger`. Every endpoint below has a
matching entry there, groupable by controller (`Sales`, `Users`, `Auth`).

### 1. Register a user

```bash
curl -X POST http://localhost:5119/api/Users -H "Content-Type: application/json" -d '{
  "username": "testuser",
  "email": "testuser@example.com",
  "password": "Coodesh@2026",
  "phone": "+5581999999999",
  "status": 1,
  "role": 1
}'
```

`status: 1` = `Active`, `role: 1` = `Customer` (`2` = `Manager`, `3` = `Admin`
— see [UserRole.cs](/template/backend/src/Ambev.DeveloperEvaluation.Domain/Enums/UserRole.cs)).
The password must be 8+ chars with upper, lower, digit and special char, or
`CreateUser` returns `400 ValidationError`.

In Swagger: `POST /api/Users` → **Try it out** → paste the body above → **Execute**.

### 2. Log in to get a JWT

```bash
curl -X POST http://localhost:5119/api/Auth -H "Content-Type: application/json" -d '{
  "email": "testuser@example.com",
  "password": "Coodesh@2026"
}'
```

Copy the `token` field from the response — every Sales call from here on
needs it.

### 3. Authorize

**Swagger UI**: click **Authorize** (top right, padlock icon), paste the raw
token (no `Bearer ` prefix — Swagger adds it), click **Authorize** then
**Close**. Every Sales operation now shows a closed padlock and sends the
header automatically.

**curl**: add `-H "Authorization: Bearer <token>"` to every command below.
For convenience, export it once:

```bash
export TOKEN="<paste your token here>"
```

**Sanity check** — confirm auth is actually enforced:

```bash
curl -i http://localhost:5119/api/Sales                              # expect 401, no header
curl -i http://localhost:5119/api/Sales -H "Authorization: Bearer $TOKEN"   # expect 200
```

### 4. Create sales — one per discount tier

```bash
# 3 units -> 0% discount
curl -X POST http://localhost:5119/api/Sales -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" -d '{
  "saleDate": "2026-08-25T10:00:00Z",
  "customerId": "11111111-1111-1111-1111-111111111111",
  "customerName": "Acme Corp",
  "branchId": "22222222-2222-2222-2222-222222222222",
  "branchName": "Downtown Branch",
  "items": [{ "productId": "33333333-3333-3333-3333-333333333333", "productName": "Beer Can 350ml", "unitPrice": 5.00, "quantity": 3 }]
}'

# 4 units -> 10% discount (same body, quantity: 4)
# 15 units -> 20% discount (same body, quantity: 15)

# 21 units -> rejected, expect 400 BusinessRuleViolation
curl -i -X POST http://localhost:5119/api/Sales -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" -d '{
  "saleDate": "2026-08-25T10:00:00Z",
  "customerId": "11111111-1111-1111-1111-111111111111",
  "customerName": "Acme Corp",
  "branchId": "22222222-2222-2222-2222-222222222222",
  "branchName": "Downtown Branch",
  "items": [{ "productId": "33333333-3333-3333-3333-333333333333", "productName": "Beer Can 350ml", "unitPrice": 5.00, "quantity": 21 }]
}'
```

Save the `id` from the 201 response of the first call (3 units) — used below
as `SALE_ID`.

```bash
export SALE_ID="<paste the id from the create response>"
```

### 5. Get the sale by id

```bash
curl http://localhost:5119/api/Sales/$SALE_ID -H "Authorization: Bearer $TOKEN"
```

### 6. List, with pagination / filter / ordering

```bash
curl "http://localhost:5119/api/Sales?_page=1&_size=10" -H "Authorization: Bearer $TOKEN"
curl "http://localhost:5119/api/Sales?customerId=11111111-1111-1111-1111-111111111111" -H "Authorization: Bearer $TOKEN"
curl "http://localhost:5119/api/Sales?_order=totalAmount desc" -H "Authorization: Bearer $TOKEN"
curl "http://localhost:5119/api/Sales?cancelled=false" -H "Authorization: Bearer $TOKEN"
```

See [Sales API](./sales-api.md#get-apisales) for the full query parameter list.

### 7. Update the sale (full item replacement)

```bash
curl -X PUT http://localhost:5119/api/Sales/$SALE_ID -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" -d '{
  "saleDate": "2026-08-25T10:00:00Z",
  "customerId": "11111111-1111-1111-1111-111111111111",
  "customerName": "Acme Corp Updated",
  "branchId": "22222222-2222-2222-2222-222222222222",
  "branchName": "Downtown Branch",
  "items": [{ "productId": "33333333-3333-3333-3333-333333333333", "productName": "Beer Can 350ml", "unitPrice": 5.00, "quantity": 9 }]
}'
```

Confirm the response's `items[0].discount` is `0.10` (9 units) and
`totalAmount` reflects it.

### 8. Cancel the sale

```bash
curl -X POST http://localhost:5119/api/Sales/$SALE_ID/cancel -H "Authorization: Bearer $TOKEN"
curl http://localhost:5119/api/Sales/$SALE_ID -H "Authorization: Bearer $TOKEN"   # isCancelled: true, cancelledAt: <timestamp>
```

### 9. Reactivate it

```bash
curl -X POST http://localhost:5119/api/Sales/$SALE_ID/reactivate -H "Authorization: Bearer $TOKEN"
curl http://localhost:5119/api/Sales/$SALE_ID -H "Authorization: Bearer $TOKEN"   # isCancelled: false, cancelledAt: null

# Reactivating an already-active sale should reject:
curl -i -X POST http://localhost:5119/api/Sales/$SALE_ID/reactivate -H "Authorization: Bearer $TOKEN"   # expect 400 BusinessRuleViolation
```

### 10. Cancel a single item

```bash
# grab the item id from step 5's response, or re-GET the sale
export ITEM_ID="<paste an item id>"
curl -X POST http://localhost:5119/api/Sales/$SALE_ID/items/$ITEM_ID/cancel -H "Authorization: Bearer $TOKEN"
```

`totalAmount` on the sale should drop to exclude that item.

### 11. Delete the sale (hard delete)

```bash
curl -X DELETE http://localhost:5119/api/Sales/$SALE_ID -H "Authorization: Bearer $TOKEN"
curl -i http://localhost:5119/api/Sales/$SALE_ID -H "Authorization: Bearer $TOKEN"   # expect 404 ResourceNotFound
```

### 12. Verify the domain events fired for real

Every write above (`create`, `update`, `cancel`, `reactivate`, `cancel item`)
publishes an event through RabbitMQ, not just an in-process log call — see
[Sales API → Domain Events](./sales-api.md#domain-events).

- **Application log**: check the console running `dotnet run`, or
  `template/backend/src/Ambev.DeveloperEvaluation.WebApi/logs/log-*.txt`, for
  lines like `SaleCreated: sale ... created at ...`.
- **RabbitMQ management UI**: `http://localhost:15672`
  (`developer` / `ev@luAt10n`) → **Queues** → `sales-events` → the
  `publish`/`deliver`/`ack` counters under **Message rates** should each be
  at least as high as the number of writes you made above.

<br>
<div style="display: flex; justify-content: space-between;">
  <a href="./sales-api.md">Previous: Sales API</a>
  <a href="../README.md">Back to README</a>
</div>
