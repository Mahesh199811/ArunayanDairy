# OrderService

Handles dairy orders for ArunayanDairy. An order can include multiple products and stores the unit price at purchase time so later catalog changes do not rewrite history.

When creating an order, this service calls Product Service (`GET /api/products/{id}`) to check availability, date, and quantity. It does **not** decrement stock yet.

## Endpoints

| Method | Path | Description |
|---|---|---|
| POST | `/api/orders` | Create an order |
| GET | `/api/orders/{id}` | Get one order with items |
| GET | `/api/orders/user/{userId}` | List a customer's orders |

## Run locally

Product Service must be running first (default `http://localhost:5296`).

```bash
cd services/OrderService/OrderService.Api
dotnet run --launch-profile http
```

| | |
|---|---|
| API | http://localhost:5275 |
| Swagger | http://localhost:5275/swagger |

Product Service base URL is `Services:ProductService` in `appsettings.json`.

## Create an order

```json
{
  "userId": "<user-guid>",
  "scheduledDate": "2026-08-29T00:00:00Z",
  "items": [
    {
      "productId": "<product-guid>",
      "quantity": 2
    }
  ]
}
```

`scheduledDate` must match the product `availableDate`. Quantity must be greater than zero and not exceed available stock. Example: 2 L of milk at ₹60 → `totalAmount` 120, status `Pending`.
