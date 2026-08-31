# ProductService

Catalog and inventory for dairy products. ASP.NET Core on .NET 8. Data is stored in MySQL (`arunayandairy_products`).

Order Service calls this API to read products and reduce stock after an order is created.

## What is implemented

- Product model (`Id`, `Name`, `Description`, `Price`, `Unit`, `AvailableQuantity`, `AvailableDate`, `CreatedAt`)
- List, get by id, create, reduce stock
- Duplicate name + description rejected
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
    ├── Migrations/
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

## Packages

- `Pomelo.EntityFrameworkCore.MySql` — MySQL
- `Microsoft.EntityFrameworkCore.Design` — EF migrations
- `Swashbuckle.AspNetCore` — Swagger

## Run with Docker Compose (preferred)

From the repository root:

```bash
docker compose up --build
```

| | |
|---|---|
| API | http://localhost:5001 |
| Swagger | http://localhost:5001/swagger |

Inside Compose, other containers reach this service at `http://product-service:8080`. Compose sets `ConnectionStrings__DefaultConnection` to `Server=mysql;...Database=arunayandairy_products`.

Apply schema from the host once (MySQL must be running):

```bash
docker compose up -d mysql
cd services/ProductService/ProductService.Api
dotnet ef database update
```

## Run with `dotnet`

Start Compose MySQL first. `appsettings.json` uses `Server=localhost;Port=3306`.

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

**Error:** `400` if name and description already exist on another product.

### POST `/api/products/{id}/reduce-stock`

```json
{
  "quantity": 2
}
```

## Notes

- Products persist in MySQL. Restarting Product Service does not wipe the catalog. Removing the `mysql-data` volume does.
- In-memory products from earlier steps are not copied into MySQL. Create products again after this migration.
- Price must be greater than zero. Available quantity cannot be negative. Reduce-stock fails if quantity exceeds stock.
- Create is rejected (`400`) if another product already has the same name and description (case-insensitive).
- `root` / `rootpassword` is local-only. Do not use it in AWS.
