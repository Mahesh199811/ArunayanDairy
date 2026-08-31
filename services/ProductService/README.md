# ProductService

Catalog and inventory for dairy products. ASP.NET Core on .NET 8, EF Core InMemory.

Order Service calls this API to read products and reduce stock after an order is created.

## What is implemented

- Product model (`Id`, `Name`, `Description`, `Price`, `Unit`, `AvailableQuantity`, `AvailableDate`, `CreatedAt`)
- List, get by id, create, reduce stock
- Swagger UI in Development
- `Dockerfile` and `.dockerignore`

## Project layout

```
ProductService/
├── ProductService.sln
├── README.md
└── ProductService.Api/
    ├── Controllers/ProductsController.cs
    ├── Data/ProductDbContext.cs
    ├── DTOs/
    │   ├── CreateProductRequest.cs
    │   └── ReduceStockRequest.cs
    ├── Models/Product.cs
    ├── Properties/launchSettings.json
    ├── Program.cs
    ├── appsettings.json
    ├── Dockerfile
    └── .dockerignore
```

## Run with Docker Compose (preferred)

From the repository root:

```bash
docker compose up --build
```

| | |
|---|---|
| API | http://localhost:5001 |
| Swagger | http://localhost:5001/swagger |

Inside Compose, other containers reach this service at `http://product-service:8080`.

## Run with `dotnet`

```bash
cd services/ProductService/ProductService.Api
dotnet restore
dotnet run --launch-profile http
```

| | |
|---|---|
| API | http://localhost:5296 |
| Swagger | http://localhost:5296/swagger |

## Endpoints

| Method | Path | Description |
|---|---|---|
| GET | `/api/products` | List products |
| GET | `/api/products/{id}` | Get one product |
| POST | `/api/products` | Create a product |
| POST | `/api/products/{id}/reduce-stock` | Decrement `availableQuantity` |

### POST `/api/products`

```json
{
  "name": "Fresh Cow Milk",
  "description": "Fresh farm milk",
  "price": 60,
  "unit": "Liter",
  "availableQuantity": 100,
  "availableDate": "2026-08-31T00:00:00Z"
}
```

### POST `/api/products/{id}/reduce-stock`

```json
{
  "quantity": 2
}
```

## Notes

- In-memory store is empty after every restart. Recreate products before placing orders.
- Price must be greater than zero. Available quantity cannot be negative. Reduce-stock fails if quantity exceeds stock.
