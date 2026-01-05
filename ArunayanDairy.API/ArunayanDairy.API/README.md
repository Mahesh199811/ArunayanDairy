# Arunayan Dairy API

A complete REST API for dairy management system built with ASP.NET Core 9.0, featuring JWT authentication, DynamoDB integration, and Swagger documentation.

## 🚀 Features

- **Authentication & Authorization**: JWT-based authentication with role-based access control (Admin/Customer)
- **User Management**: User registration, login, and profile management
- **Product Management**: CRUD operations for dairy products with category filtering
- **Order Management**: Complete order lifecycle management with status tracking
- **Database**: AWS DynamoDB integration for scalable data storage
- **API Documentation**: Interactive Swagger UI for API testing
- **Exception Handling**: Global exception middleware for consistent error responses
- **Password Security**: BCrypt for secure password hashing

## 📁 Project Structure

```
ArunayanDairy.API/
├── Controllers/           # API endpoints
│   ├── AuthController.cs
│   ├── UsersController.cs
│   ├── ProductsController.cs
│   └── OrdersController.cs
├── Services/              # Business logic layer
│   ├── Interfaces/
│   └── Implementation classes
├── DTOs/                  # Data transfer objects
│   ├── Auth/
│   ├── Users/
│   ├── Products/
│   └── Orders/
├── Models/                # Domain models
│   ├── User.cs
│   ├── Product.cs
│   └── Order.cs
├── Repositories/          # Data access layer
│   ├── Interfaces/
│   └── DynamoDb/
├── Infrastructure/        # Core infrastructure
│   ├── DynamoDbContext.cs
│   └── JwtTokenGenerator.cs
├── Middleware/
│   └── ExceptionMiddleware.cs
└── Program.cs
```

## 🛠️ Technology Stack

- **.NET 9.0**
- **ASP.NET Core Web API**
- **AWS SDK for DynamoDB**
- **JWT Bearer Authentication**
- **BCrypt.Net** for password hashing
- **Swashbuckle (Swagger)** for API documentation

## 📦 NuGet Packages

- AWSSDK.DynamoDBv2
- BCrypt.Net-Next
- Microsoft.AspNetCore.Authentication.JwtBearer
- Swashbuckle.AspNetCore

## ⚙️ Configuration

Update `appsettings.json` with your configuration:

```json
{
  "Jwt": {
    "Secret": "YourSuperSecretKeyForJWTTokenGeneration12345678",
    "Issuer": "ArunayanDairyAPI",
    "Audience": "ArunayanDairyClients",
    "ExpirationHours": "24"
  },
  "DynamoDB": {
    "ServiceUrl": "http://localhost:8000"
  }
}
```

## 🚀 Getting Started

1. **Prerequisites**
   - .NET 9.0 SDK
   - AWS DynamoDB Local (for development) or AWS account

2. **Install DynamoDB Local** (Optional for local development)
   ```bash
   docker run -p 8000:8000 amazon/dynamodb-local
   ```

3. **Restore packages**
   ```bash
   dotnet restore
   ```

4. **Build the project**
   ```bash
   dotnet build
   ```

5. **Run the application**
   ```bash
   dotnet run
   ```

6. **Access Swagger UI**
   
   Open your browser and navigate to: `http://localhost:5291`

## 📚 API Endpoints

### Authentication
- `POST /api/auth/login` - User login
- `POST /api/auth/signup` - User registration

### Users
- `GET /api/users` - Get all users (Admin only)
- `GET /api/users/{id}` - Get user by ID
- `DELETE /api/users/{id}` - Delete user (Admin only)

### Products
- `GET /api/products` - Get all products
- `GET /api/products/{id}` - Get product by ID
- `GET /api/products/category/{category}` - Get products by category
- `POST /api/products` - Create product (Admin only)
- `PUT /api/products/{id}` - Update product (Admin only)
- `DELETE /api/products/{id}` - Delete product (Admin only)

### Orders
- `GET /api/orders` - Get all orders (Admin only)
- `GET /api/orders/{id}` - Get order by ID
- `GET /api/orders/my-orders` - Get current user's orders
- `POST /api/orders` - Create new order
- `PATCH /api/orders/{id}/status` - Update order status (Admin only)
- `DELETE /api/orders/{id}` - Delete order (Admin only)

## 🔐 Authentication

To use protected endpoints:

1. Register a new user or login using `/api/auth/signup` or `/api/auth/login`
2. Copy the JWT token from the response
3. In Swagger UI, click the "Authorize" button
4. Enter: `Bearer {your-token}`
5. Click "Authorize"

## 📝 Sample Requests

### Register User
```json
POST /api/auth/signup
{
  "email": "customer@example.com",
  "password": "password123",
  "fullName": "John Doe",
  "phoneNumber": "+1234567890",
  "address": "123 Main St, City"
}
```

### Create Product
```json
POST /api/products
{
  "name": "Fresh Milk",
  "description": "Pure cow milk",
  "price": 50.00,
  "unit": "Liter",
  "stockQuantity": 100,
  "category": "Milk",
  "imageUrl": "https://example.com/milk.jpg",
  "isAvailable": true
}
```

### Create Order
```json
POST /api/orders
{
  "items": [
    {
      "productId": "product-id-here",
      "quantity": 2
    }
  ],
  "deliveryAddress": "123 Main St, City",
  "paymentMethod": "Cash",
  "deliveryDate": "2026-01-10T10:00:00Z"
}
```

## 🔒 Security Features

- Password hashing with BCrypt
- JWT token-based authentication
- Role-based authorization (Admin/Customer)
- CORS configuration
- HTTPS redirection

## 🐛 Error Handling

The API uses a global exception middleware that:
- Logs all exceptions
- Returns consistent error responses
- Shows detailed errors in development mode
- Hides sensitive information in production

## 📄 License

This project is licensed under the MIT License.

## 👨‍💻 Developer

Arunayan Dairy API - Developed for dairy management operations
