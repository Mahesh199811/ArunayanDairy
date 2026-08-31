# OrderService

Handles dairy orders for ArunayanDairy. An order can include multiple products and stores the unit price at purchase time so later catalog changes do not rewrite history.

When creating an order, this service calls Product Service:

1. `GET /api/products/{id}` — availability, date, and quantity
2. `POST /api/products/{id}/reduce-stock` — decrement leftover stock after the order is saved

## Project layout

```
OrderService/
├── OrderService.sln
├── README.md
└── OrderService.Api/
    ├── Controllers/OrdersController.cs
    ├── Data/OrderDbContext.cs
    ├── DTOs/
    │   ├── CreateOrderRequest.cs
    │   └── CreateOrderItemRequest.cs
    ├── Models/
    │   ├── Order.cs
    │   └── OrderItem.cs
    ├── Services/ProductServiceClient.cs
    ├── Properties/launchSettings.json
    ├── Program.cs
    ├── appsettings.json
    ├── Dockerfile
    └── .dockerignore
```

## Endpoints

| Method | Path | Description |
|---|---|---|
| POST | `/api/orders` | Create an order |
| GET | `/api/orders/{id}` | Get one order with items |
| GET | `/api/orders/user/{userId}` | List a customer's orders |

## Run with Docker Compose (preferred)

Product Service starts with Compose. Order uses Compose DNS, not localhost.

From the repository root:

```bash
docker compose up --build
```

| | |
|---|---|
| API | http://localhost:5002 |
| Swagger | http://localhost:5002/swagger |

Product Service base URL is `Services:ProductService` in `appsettings.json`:

```text
http://product-service:8080/
```

## Run with `dotnet`

Product Service must be running first. Point `Services:ProductService` at that instance (for local `dotnet`, typically `http://localhost:5296/`).

```bash
cd services/OrderService/OrderService.Api
dotnet run --launch-profile http
```

| | |
|---|---|
| API | http://localhost:5275 |
| Swagger | http://localhost:5275/swagger |

## Create an order

```json
{
  "userId": "<user-guid>",
  "scheduledDate": "2026-08-31T00:00:00Z",
  "items": [
    {
      "productId": "<product-guid>",
      "quantity": 2
    }
  ]
}
```

`scheduledDate` cannot be in the past and must match the product `availableDate`. Quantity must be greater than zero and not exceed available stock. Example: 2 L of milk at ₹60 → `totalAmount` 120, status `Pending`.

## Notes

- In-memory store is empty after every restart.
- Do not put environment-specific Product URLs in source for AWS yet; Compose uses the service name `product-service`.
