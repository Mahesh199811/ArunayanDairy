# UserService

Handles user accounts and authentication for ArunayanDairy.

The API lives in `UserService.Api` (ASP.NET Core on .NET 8). Passwords are hashed with BCrypt. Login issues a JWT that other services can validate.

## What is implemented

- User model (`Id`, `FullName`, `Email`, `PasswordHash`, `CreatedAt`)
- Register and login DTOs
- EF Core + Pomelo MySQL (`UserDbContext`, database `arunayandairy_users`)
- JWT token generation (`AuthService`)
- `POST /api/auth/register` and `POST /api/auth/login`
- JWT Bearer authentication wired in `Program.cs`
- Swagger UI in Development
- `Dockerfile` and `.dockerignore`

There are no protected user/profile endpoints yet. JWT middleware is ready for them.

## Project layout

```
UserService/
├── UserService.sln
├── README.md
└── UserService.Api/
    ├── Controllers/AuthController.cs
    ├── Data/UserDbContext.cs
    ├── Migrations/
    ├── DTOs/
    │   ├── LoginRequest.cs
    │   └── RegisterRequest.cs
    ├── Models/User.cs
    ├── Services/AuthService.cs
    ├── Properties/launchSettings.json
    ├── Program.cs
    ├── appsettings.json
    ├── Dockerfile
    └── .dockerignore
```

## Packages

- `BCrypt.Net-Next` — password hashing
- `Pomelo.EntityFrameworkCore.MySql` — MySQL
- `Microsoft.EntityFrameworkCore.Design` — EF migrations
- `Microsoft.AspNetCore.Authentication.JwtBearer` — JWT validation
- `Swashbuckle.AspNetCore` — Swagger

## Run with Docker Compose (preferred)

From the repository root:

```bash
docker compose up --build
```

| | |
|---|---|
| API | http://localhost:5080 |
| Swagger | http://localhost:5080/swagger |

Host port is **5080** because macOS AirPlay often occupies **5000**. Inside the container the app listens on **8080**.

Compose sets `ConnectionStrings__DefaultConnection` to `Server=mysql;...Database=arunayandairy_users`. MySQL must be up first (`depends_on: mysql`). Apply schema from the host once:

```bash
docker compose up -d mysql
cd services/UserService/UserService.Api
dotnet ef database update
```

## Run with `dotnet`

```bash
cd services/UserService/UserService.Api
dotnet restore
dotnet run --launch-profile http
```

Use `--launch-profile http` so the app starts in Development. Without that profile, Swagger is not enabled. Start Compose MySQL first; `appsettings.json` uses `Server=localhost;Port=3306`.

| | |
|---|---|
| API | http://localhost:5051 |
| Swagger | http://localhost:5051/swagger |

## Endpoints

### POST `/api/auth/register`

Creates a user. Email must be unique.

**Request**

```json
{
  "fullName": "John Doe",
  "email": "john@example.com",
  "password": "Password123!"
}
```

**Success (200)**

```json
{
  "id": "<guid>",
  "fullName": "John Doe",
  "email": "john@example.com"
}
```

**Error:** `400` if the email is already registered.

### POST `/api/auth/login`

Returns a JWT (valid for 1 hour).

**Request**

```json
{
  "email": "john@example.com",
  "password": "Password123!"
}
```

**Success (200)**

```json
{
  "token": "<jwt>",
  "user": {
    "id": "<guid>",
    "fullName": "John Doe",
    "email": "john@example.com"
  }
}
```

**Error:** `401` for unknown email or wrong password.

JWT claims include `sub` (user id), `email`, and name. Issuer is `ArunayanDairy.UserService`; audience is `ArunayanDairy`.

## Test with curl

Replace the host/port if you are using Compose (`5080`) instead of `dotnet` (`5051`).

```bash
curl -X POST "http://localhost:5080/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "fullName": "John Doe",
    "email": "john@example.com",
    "password": "Password123!"
  }'

curl -X POST "http://localhost:5080/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john@example.com",
    "password": "Password123!"
  }'
```

When protected routes exist, send the token as:

```bash
curl -H "Authorization: Bearer <token>" "http://localhost:5080/api/..."
```

## Notes

- Users persist in MySQL (`Users` table). Restarting User Service does not wipe accounts. Removing the `mysql-data` volume does.
- `root` / `rootpassword` is local-only. Do not use it in AWS.
- JWT settings are in `appsettings.json` (`Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience`). The key is for local development only; replace it before any real deployment.
- HTTPS redirection may warn locally because the `http` profile has no HTTPS port.
