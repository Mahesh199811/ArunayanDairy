# Docker

This folder is reserved for extra container assets (env templates, cache). MySQL runs from the root `docker-compose.yml`.

**Compose and service images live elsewhere:**

```
ArunayanDairy/
├── docker-compose.yml                 # orchestrates the three APIs
├── docker/                            # this folder
└── services/
    ├── UserService/UserService.Api/Dockerfile
    ├── ProductService/ProductService.Api/Dockerfile
    └── OrderService/OrderService.Api/Dockerfile
```

From the repository root:

```bash
docker compose up --build
docker compose ps
docker compose down
```

| Compose service | Container name | Host → container |
|---|---|---|
| mysql | arunayandairy-mysql | 3306 → 3306 |
| user-service | arunayandairy-user-service | 5080 → 8080 |
| product-service | arunayandairy-product-service | 5001 → 8080 |
| order-service | arunayandairy-order-service | 5002 → 8080 |

They share the Compose network `arunayandairy-network`. Order Service calls Product Service at `http://product-service:8080`. User Service calls MySQL at `mysql:3306`. Data lives in volume `mysql-data`.

The frontend is not composed yet. Product and Order still use in-memory stores.
