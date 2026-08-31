# ArunayanDairy

A dairy platform with a React frontend and three ASP.NET Core microservices. Each backend service is containerized. Docker Compose starts User, Product, and Order together on one network.

## Repository structure

```
ArunayanDairy/
├── docker-compose.yml
├── README.md
├── frontend/
│   ├── src/
│   └── README.md
├── services/
│   ├── UserService/
│   │   ├── UserService.sln
│   │   └── UserService.Api/          # Dockerfile, API
│   ├── ProductService/
│   │   ├── ProductService.sln
│   │   └── ProductService.Api/
│   └── OrderService/
│       ├── OrderService.sln
│       └── OrderService.Api/
├── docker/                           # reserved (Compose lives at repo root)
├── lambda/                           # reserved for serverless handlers
└── docs/
```

## Architecture (local)

```
                    Docker Compose
                         |
              arunayandairy-network
                         |
        +----------------+----------------+----------------+
        |                |                |                |
        v                v                v                v
     MySQL         UserService     ProductService    OrderService
  localhost:3306  localhost:5080   localhost:5001    localhost:5002
        |                |                |
        |                |                | EF Core
        +----------------+----------------+
                         |                |
            arunayandairy_users    arunayandairy_products
                                         |
                                         | http://product-service:8080
                                         v
                                   ProductService
```

User Service persists to MySQL (`arunayandairy_users`). Product Service persists to MySQL (`arunayandairy_products`). Order still uses EF Core InMemory.

The React app is not in Compose yet. Run it on the host and point it at the published ports (`frontend/src/services/api.ts`).

| Surface | Host URL | Notes |
|---|---|---|
| User Service | http://localhost:5080 | Swagger: `/swagger` |
| Product Service | http://localhost:5001 | Swagger: `/swagger` |
| Order Service | http://localhost:5002 | Swagger: `/swagger` |
| MySQL | localhost:3306 | volume `mysql-data` |
| Frontend | http://localhost:5173 | Vite; not containerized |

Port **5080** is used for User Service on this Mac because **5000** is taken by AirPlay Receiver.

## Run the backends with Compose

From the repository root:

```bash
docker compose up --build
```

Stop:

```bash
docker compose down
```

Order Service talks to Product Service with Compose DNS: `http://product-service:8080/` (`Services:ProductService` in Order Service `appsettings.json`).

User and Product talk to MySQL with Compose DNS: `Server=mysql` (`ConnectionStrings__DefaultConnection`). Local `dotnet run` uses `Server=localhost` in each service `appsettings.json`.

User and Product rows survive service restarts. They are stored in the `mysql-data` volume. Order data is still in-memory.

## Run a single service with `dotnet`

Each service has an `http` launch profile (Swagger on). Ports differ from Compose:

| Service | `dotnet run` | Compose host port |
|---|---|---|
| User | http://localhost:5051 | 5080 |
| Product | http://localhost:5296 | 5001 |
| Order | http://localhost:5275 | 5002 |

See `services/*/README.md` for details.

## Frontend

```bash
cd frontend
npm install
npm run build
npm run start
```

Use `npm run start` (`vite preview` on 5173) for demos. `npm run dev` can hit Vite HMR issues in this app.

## AWS / platform (planned)

VPC, EC2, ECS, ALB, Auto Scaling, Route 53, CloudFront, API Gateway, Lambda, RDS, S3, CloudWatch, Security Groups, ECR.

## Technologies

- .NET 8 / ASP.NET Core Web API
- React + TypeScript + Vite
- EF Core + Pomelo MySQL (User, Product); InMemory (Order)
- Docker / Docker Compose
- GitHub
