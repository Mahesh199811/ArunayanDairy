# Arunayan Dairy API

.NET 9 Web API with JWT authentication for Arunayan Dairy mobile application.

## Features

✅ **JWT Authentication** - Secure token-based authentication with access & refresh tokens  
✅ **Password Hashing** - PBKDF2 with salt (100,000 iterations)  
✅ **Swagger/OpenAPI** - Interactive API documentation with Bearer auth support  
✅ **CORS Enabled** - Ready for cross-origin requests from MAUI app  
✅ **Role-Based Access** - User and Admin roles  

## Architecture

```
ArunayanDairy.API/
├── Controllers/
│   └── AuthController.cs          # Auth endpoints (login, signup, refresh)
├── Services/
│   ├── IAuthService.cs            # Service interface
│   └── AuthService.cs             # Business logic with in-memory store
├── Security/
│   ├── JwtTokenGenerator.cs       # JWT access & refresh token generation
│   └── PasswordHasher.cs          # PBKDF2 password hashing
├── Models/
│   ├── User.cs                    # User entity
│   ├── LoginDto.cs                # Login request
│   ├── SignupDto.cs               # Signup request
│   ├── RefreshTokenDto.cs         # Token refresh request
│   ├── AuthResponse.cs            # Auth response with user & tokens
│   └── ApiError.cs                # Standard error response
├── Program.cs                     # App configuration & DI setup
└── appsettings.json               # Configuration (JWT secret, issuer, etc.)
```

## API Endpoints

### Authentication

#### POST `/api/auth/login`
Login with email and password.

**Request:**
```json
{
  "email": "admin@arunayan.com",
  "password": "admin123"
}
```

**Response (200):**
```json
{
  "user": {
    "id": "uuid",
    "name": "Admin User",
    "email": "admin@arunayan.com",
    "role": "admin",
    "createdAt": "2025-12-21T05:30:00Z",
    "updatedAt": "2025-12-21T05:30:00Z"
  },
  "tokens": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresIn": 3600,
    "refreshToken": "base64-encoded-random-token"
  }
}
```

**Errors:**
- `401` - Invalid credentials
- `422` - Validation error

---

#### POST `/api/auth/signup`
Create a new user account.

**Request:**
```json
{
  "name": "John Doe",
  "email": "john@example.com",
  "password": "securepass123"
}
```

**Response (201):**
```json
{
  "user": { ... },
  "tokens": { ... }
}
```

**Errors:**
- `409` - Email already exists
- `422` - Validation error (name min 2 chars, password min 8 chars)

---

#### POST `/api/auth/refresh`
Refresh access token using refresh token.

**Request:**
```json
{
  "refreshToken": "base64-encoded-refresh-token"
}
```

**Response (200):**
```json
{
  "accessToken": "new-jwt-token",
  "expiresIn": 3600,
  "refreshToken": "new-refresh-token"
}
```

**Errors:**
- `401` - Invalid or expired refresh token

## Configuration

### JWT Settings (`appsettings.json`)
```json
{
  "Jwt": {
    "Secret": "YourSecretKeyHere-MustBe32CharsOrMore!",
    "Issuer": "ArunayanDairyAPI",
    "Audience": "ArunayanDairyMAUI",
    "AccessTokenExpiryMinutes": "15",
    "RefreshTokenExpiryDays": "7"
  }
}
```

**Important:** 
- Change `Secret` in production (use secure random string)
- Access token: 15 minutes (configurable)
- Refresh token: 7 days (configurable)

### Development Settings (`appsettings.Development.json`)
- Longer token expiry (60 min access, 30 day refresh)
- More verbose logging

## Getting Started

### Prerequisites
- .NET 9 SDK

### Run the API

```bash
cd ArunayanDairy.API

# Restore dependencies
dotnet restore

# Build
dotnet build

# Run on port 5000
dotnet run --urls "http://localhost:5000"

# Or run on a different port
dotnet run --urls "http://localhost:8080"
```

The API will be available at:
- **API Base:** `http://localhost:5000`
- **Swagger UI:** `http://localhost:5000/swagger`

### Test the API

#### Using Swagger UI
1. Open `http://localhost:5000/swagger`
2. Expand `/api/auth/login`
3. Click "Try it out"
4. Use test credentials:
   - Email: `admin@arunayan.com`
   - Password: `admin123`
5. Copy the `accessToken` from response
6. Click "Authorize" button at top
7. Enter: `Bearer <paste-token-here>`
8. Now you can test authenticated endpoints

#### Using cURL

**Login:**
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@arunayan.com","password":"admin123"}'
```

**Signup:**
```bash
curl -X POST http://localhost:5000/api/auth/signup \
  -H "Content-Type: application/json" \
  -d '{"name":"Test User","email":"test@example.com","password":"testpass123"}'
```

**Refresh Token:**
```bash
curl -X POST http://localhost:5000/api/auth/refresh \
  -H "Content-Type: application/json" \
  -d '{"refreshToken":"your-refresh-token-here"}'
```

## Security Features

### Password Hashing
- Algorithm: PBKDF2-SHA256
- Iterations: 100,000
- Salt: 128-bit random per password
- Output: 256-bit hash

### JWT Tokens
- Algorithm: HS256 (HMAC-SHA256)
- Claims: `sub` (userId), `email`, `name`, `role`, `jti`, `iat`
- Issuer/Audience validation enabled
- Clock skew: 0 (strict expiry)

### Refresh Tokens
- 64-byte cryptographically secure random value
- Base64 encoded
- Stored with user record
- Expiry tracked separately from JWT

## Data Storage

**Current:** In-memory `List<User>` (for demo)  
**Production:** Replace with:
- Entity Framework Core + SQL Server/PostgreSQL
- Or MongoDB for NoSQL approach

### Seeded Test Account
```
Email: admin@arunayan.com
Password: admin123
Role: admin
```

## CORS Configuration

Currently allows **all origins** for development. In production:

```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://yourapp.com")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});
```

## MAUI App Configuration

Update `AuthService.cs` in MAUI project:

```csharp
// Android Emulator
private const string BaseUrl = "http://10.0.2.2:5000";

// iOS Simulator
private const string BaseUrl = "http://localhost:5000";

// Physical Device
private const string BaseUrl = "http://192.168.1.100:5000"; // Your machine's IP
```

## Authentication Flow

```
1. User enters email + password in MAUI app
2. MAUI sends POST /api/auth/login
3. API validates credentials (email lookup + password verify)
4. API generates JWT access token (15 min) + refresh token (7 days)
5. API returns user info + tokens
6. MAUI stores tokens in SecureStorage (Keychain/Keystore)
7. All future MAUI requests include: Authorization: Bearer <access-token>
8. When access token expires, MAUI sends POST /api/auth/refresh
9. API validates refresh token and issues new tokens
```

## Error Responses

All errors follow consistent format:

```json
{
  "code": "ERROR_CODE",
  "message": "Human-readable error message",
  "details": { /* optional additional info */ }
}
```

**Common Error Codes:**
- `VALIDATION_ERROR` - Request validation failed
- `INVALID_CREDENTIALS` - Wrong email/password
- `EMAIL_ALREADY_EXISTS` - Signup with existing email
- `INVALID_REFRESH_TOKEN` - Refresh token invalid/expired
- `INTERNAL_ERROR` - Server error

## Next Steps

### Phase 2: Products API
- [ ] Add `Product` entity and DTOs
- [ ] Implement `IProductService` and `ProductService`
- [ ] Create `ProductsController` with CRUD endpoints
- [ ] Add `[Authorize(Roles = "admin")]` for mutations

### Phase 3: Orders API
- [ ] Add `Order` and `OrderItem` entities
- [ ] Implement order creation and status management
- [ ] Add user-order association

### Phase 4: Database
- [ ] Add Entity Framework Core
- [ ] Create DbContext with Users, Products, Orders
- [ ] Add migrations
- [ ] Replace in-memory storage

### Phase 5: Production Readiness
- [ ] Add rate limiting
- [ ] Implement refresh token rotation
- [ ] Add logging (Serilog)
- [ ] Configure HTTPS
- [ ] Environment-specific CORS
- [ ] Health check endpoints

## Troubleshooting

### Port Already in Use
```bash
# Find process using port 5000
lsof -i :5000

# Kill the process
kill -9 <PID>

# Or use a different port
dotnet run --urls "http://localhost:8080"
```

### CORS Errors from MAUI
Ensure API is running and CORS is enabled (it is by default).

### JWT Validation Fails
- Check JWT secret matches in `appsettings.json`
- Verify issuer/audience match configuration
- Token may be expired (15 min default)

## License
© 2025 Arunayan Dairy
