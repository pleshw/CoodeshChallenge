[Back to README](../README.md)

### Sales

All Sale endpoints live under `/api/Sales`. Every response is wrapped in the
standard envelope (`success`, `message`, `errors`), and unhandled errors use
the `{ type, error, detail }` shape described in [General API](./general-api.md).

Money fields are `decimal`. `discount` is the applied percentage (`0`, `0.10`
or `0.20`) as decided by the [business rules](../README.md#business-rules).
Customer, Branch and Product are referenced using the **External Identities**
pattern: only their `id` and a denormalized `name` are stored on the Sale —
there is no foreign key into a Customer/Branch/Product table, since those
domains don't exist in this service.

#### POST /api/Sales
- Description: Creates a new sale. The server generates the `saleNumber` and
  applies the quantity-based discount tier to every item.
- Request Body:
  ```json
  {
    "saleDate": "2026-08-25T10:00:00Z",
    "customerId": "11111111-1111-1111-1111-111111111111",
    "customerName": "Acme Corp",
    "branchId": "22222222-2222-2222-2222-222222222222",
    "branchName": "Downtown Branch",
    "items": [
      { "productId": "33333333-3333-3333-3333-333333333333", "productName": "Beer Can 350ml", "unitPrice": 5.00, "quantity": 4 }
    ]
  }
  ```
- Response `201 Created`:
  ```json
  {
    "data": {
      "id": "guid",
      "saleNumber": "SALE-20260825143617-4968FED3",
      "saleDate": "2026-08-25T10:00:00Z",
      "customerId": "guid",
      "customerName": "Acme Corp",
      "branchId": "guid",
      "branchName": "Downtown Branch",
      "totalAmount": 18.00,
      "isCancelled": false,
      "items": [
        {
          "id": "guid",
          "productId": "guid",
          "productName": "Beer Can 350ml",
          "quantity": 4,
          "unitPrice": 5.00,
          "discount": 0.10,
          "totalAmount": 18.00,
          "isCancelled": false
        }
      ]
    },
    "success": true,
    "message": "Sale created successfully",
    "errors": []
  }
  ```
- Errors: `400` if a field fails validation, or if any item's quantity is
  outside `1-20` (see business rules).

#### GET /api/Sales/{id}
- Description: Retrieves a single sale by its ID, including all items.
- Response `200 OK`: same shape as the `data` object above.
- Errors: `404 ResourceNotFound` if the sale doesn't exist.

#### GET /api/Sales
- Description: Retrieves a paginated, filtered, ordered list of sales.
- Query Parameters:
  - `_page` (optional, default `1`)
  - `_size` (optional, default `10`)
  - `_order` (optional): `"saleDate"` or `"totalAmount"`, optionally followed
    by `"asc"`/`"desc"` (default `saleDate asc`), e.g. `_order=totalAmount desc`
  - `cancelled` (optional, bool): filter by cancellation status
  - `customerId` / `branchId` (optional, guid)
  - `startDate` / `endDate` (optional, date range on `saleDate`)
- Response `200 OK`:
  ```json
  {
    "currentPage": 1,
    "totalPages": 1,
    "totalCount": 2,
    "data": [ { "...": "same shape as GET /api/Sales/{id}" } ],
    "success": true,
    "message": "",
    "errors": []
  }
  ```

#### PUT /api/Sales/{id}
- Description: Updates a sale's header fields and **fully replaces** its item
  list (not a partial patch) — discounts are recalculated for the new items.
- Request Body: same shape as `POST /api/Sales` (without `saleNumber`, which
  never changes).
- Response `200 OK`: same shape as `POST /api/Sales`'s response, with
  `message: "Sale updated successfully"`.
- Errors: `404 ResourceNotFound` if the sale doesn't exist.

#### DELETE /api/Sales/{id}
- Description: Permanently deletes a sale and its items (hard delete). For
  the "Cancelled" business status instead, use `POST /api/Sales/{id}/cancel`.
- Response `200 OK`: `{ "success": true, "message": "Sale deleted successfully", "errors": [] }`
- Errors: `404 ResourceNotFound` if the sale doesn't exist.

#### POST /api/Sales/{id}/cancel
- Description: Cancels the whole sale (`isCancelled = true`). Publishes a
  `SaleCancelled` event to the application log. Does **not** cancel individual
  items — cancelling an item is a separate action.
- Response `200 OK`:
  ```json
  { "data": { "id": "guid", "saleNumber": "string", "isCancelled": true }, "success": true, "message": "Sale cancelled successfully", "errors": [] }
  ```
- Errors: `404 ResourceNotFound` if the sale doesn't exist.

#### POST /api/Sales/{id}/items/{itemId}/cancel
- Description: Cancels a single item within a sale. The sale's `totalAmount`
  is recalculated to exclude it. Publishes an `ItemCancelled` event to the
  application log.
- Response `200 OK`:
  ```json
  { "data": { "saleId": "guid", "itemId": "guid", "isCancelled": true, "saleTotalAmount": 0.00 }, "success": true, "message": "Sale item cancelled successfully", "errors": [] }
  ```
- Errors: `404 ResourceNotFound` if the sale or the item doesn't exist.

### Domain Events

`SaleCreated`, `SaleModified`, `SaleCancelled` and `ItemCancelled` are
published internally (via MediatR) after each corresponding write succeeds.
No message broker is used — each event is written to the application log
(see `Application/Events/` and `Application/Events/Handlers/`).

<br>
<div style="display: flex; justify-content: space-between;">
  <a href="./general-api.md">Previous: General API</a>
  <a href="../README.md">Back to README</a>
</div>
