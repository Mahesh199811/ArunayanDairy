# Services

ASP.NET Core (.NET 8) microservices for ArunayanDairy. Each service is its own solution with a `*.Api` project that includes a `Dockerfile`.

```
services/
├── README.md
├── UserService/
│   ├── UserService.sln
│   ├── README.md
│   └── UserService.Api/
├── ProductService/
│   ├── ProductService.sln
│   ├── README.md
│   └── ProductService.Api/
└── OrderService/
    ├── OrderService.sln
    ├── README.md
    └── OrderService.Api/
```

Compose build contexts are `./services/<Name>/<Name>.Api` (see root `docker-compose.yml`).

| Service | Role | Compose | `dotnet` http profile |
|---|---|---|---|
| UserService | Register / login, JWT | localhost:5080 | localhost:5051 |
| ProductService | Catalog and stock | localhost:5001 | localhost:5296 |
| OrderService | Orders; HTTP to Product | localhost:5002 | localhost:5275 |

UserService uses MySQL (`arunayandairy_users`). ProductService and OrderService still use EF Core InMemory.
