# 🏗️ Arunayan Dairy - Production Architecture

## Overview

A production-ready, scalable dairy product management system built with .NET MAUI and ASP.NET Core Web API following enterprise-grade best practices.

## System Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                         CLIENT LAYER                             │
│  .NET MAUI Mobile Application (iOS, Android, macOS, Windows)    │
│                                                                   │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐          │
│  │    Views     │  │  ViewModels  │  │   Services   │          │
│  │    (XAML)    │◄─┤    (MVVM)    │◄─┤  (API Calls) │          │
│  └──────────────┘  └──────────────┘  └──────┬───────┘          │
│                                               │                   │
│  ┌──────────────────────────────────────────▼────────┐          │
│  │        Local SQLite Database (Offline Cache)      │          │
│  └───────────────────────────────────────────────────┘          │
└───────────────────────────────┬─────────────────────────────────┘
                                │
                        HTTPS/JSON (JWT)
                                │
┌───────────────────────────────▼─────────────────────────────────┐
│                         API LAYER                                │
│              ASP.NET Core 9 Web API (Backend)                    │
│                                                                   │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐          │
│  │ Controllers  │─►│   Services   │─►│ Repositories │          │
│  │  (REST API)  │  │ (Business)   │  │ (Data Access)│          │
│  └──────────────┘  └──────────────┘  └──────┬───────┘          │
│         │                                     │                   │
│  ┌──────▼──────────────────────────────────▼────────┐          │
│  │       Authentication & Authorization              │          │
│  │   JWT Tokens | Role-Based Access Control         │          │
│  └───────────────────────────────────────────────────┘          │
└───────────────────────────────┬─────────────────────────────────┘
                                │
                         EF Core 9.0
                                │
┌───────────────────────────────▼─────────────────────────────────┐
│                      DATA LAYER                                  │
│          PostgreSQL (Production) / SQLite (Development)          │
│                                                                   │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐             │
│  │    Users    │  │  Products   │  │   Orders    │             │
│  │   (Auth)    │  │ (Catalog)   │  │ (Transactions)            │
│  └─────────────┘  └─────────────┘  └─────────────┘             │
└─────────────────────────────────────────────────────────────────┘
```

---

## 🧱 Architectural Layers

### 1. **Presentation Layer** (.NET MAUI)
- **Views (XAML)**: UI components with no business logic
- **ViewModels**: Data binding, commands, state management
- **Converters**: Value transformations for data binding
- **Navigation**: Shell-based routing with deep linking

### 2. **Application Layer** (MAUI Services)
- **API Services**: HTTP communication with backend
- **Local Storage Services**: SQLite for offline support
- **Authentication Service**: Token management & secure storage
- **Sync Service**: Background synchronization

### 3. **API Layer** (ASP.NET Core)
- **Controllers**: HTTP endpoints, request validation
- **Services**: Business logic, authorization rules
- **Repositories**: Data access abstraction
- **Middleware**: Error handling, logging, authentication

### 4. **Data Layer**
- **Entity Framework Core**: ORM with migrations
- **PostgreSQL/SQLite**: Relational database
- **DbContext**: Database connection and configuration

---

## 📊 Database Schema

### Core Entities

#### **Users** (Authentication)
```
Users
├── Id (Guid, PK)
├── Name (string, 100)
├── Email (string, 255, Unique Index)
├── PasswordHash (string)
├── Role (enum: "user" | "admin")
├── RefreshToken (string?)
├── RefreshTokenExpiryTime (DateTime?)
├── CreatedAt (DateTime)
└── UpdatedAt (DateTime)
```

#### **Categories**
```
Categories
├── Id (Guid, PK)
├── Name (string, 100, Required)
├── Description (string?)
├── DisplayOrder (int)
├── IsActive (bool)
└── Products (ICollection<Product>)
```

#### **Products** (Catalog)
```
Products
├── Id (Guid, PK)
├── CategoryId (Guid, FK)
├── Name (string, 200, Required)
├── Description (string?)
├── SKU (string, 50, Unique)
├── Unit (enum: Liter, KG, Piece)
├── BasePrice (decimal, 18,2)
├── ImageUrl (string?)
├── IsActive (bool)
├── MinOrderQuantity (decimal)
├── MaxOrderQuantity (decimal?)
├── CreatedAt (DateTime)
├── UpdatedAt (DateTime)
├── Category (Category)
├── Availabilities (ICollection<ProductAvailability>)
└── OrderItems (ICollection<OrderItem>)
```

#### **ProductAvailability** (Date-based Stock)
```
ProductAvailability
├── Id (Guid, PK)
├── ProductId (Guid, FK)
├── AvailableDate (DateTime, Index)
├── StockQuantity (decimal)
├── PriceOverride (decimal?, 18,2)
├── IsAvailable (bool)
├── Product (Product)
└── UNIQUE INDEX (ProductId, AvailableDate)
```

#### **Orders** (Transactions)
```
Orders
├── Id (Guid, PK)
├── OrderNumber (string, 20, Unique)
├── CustomerId (Guid, FK → Users.Id)
├── OrderDate (DateTime, Index)
├── DeliveryDate (DateTime, Index)
├── Status (enum)
├── TotalAmount (decimal, 18,2)
├── Notes (string?)
├── CreatedAt (DateTime)
├── UpdatedAt (DateTime)
├── Customer (User)
└── OrderItems (ICollection<OrderItem>)

Status Enum: Pending, Confirmed, Processing, OutForDelivery, Delivered, Cancelled
```

#### **OrderItems**
```
OrderItems
├── Id (Guid, PK)
├── OrderId (Guid, FK)
├── ProductId (Guid, FK)
├── Quantity (decimal)
├── UnitPrice (decimal, 18,2)
├── Subtotal (decimal, 18,2, Computed)
├── Order (Order)
└── Product (Product)
```

### Indexes for Performance
- `Users.Email` (Unique)
- `Products.SKU` (Unique)
- `ProductAvailability.(ProductId, AvailableDate)` (Unique Composite)
- `Orders.CustomerId` (Non-clustered)
- `Orders.OrderDate` (Non-clustered)
- `Orders.DeliveryDate` (Non-clustered)
- `Orders.OrderNumber` (Unique)

---

## 🔐 Security Architecture

### Authentication Flow
1. **Login**: User credentials → Server validates → JWT + Refresh Token returned
2. **Token Storage**: Secure storage (Keychain/Keystore)
3. **API Calls**: Access token in `Authorization: Bearer {token}` header
4. **Token Refresh**: Auto-refresh before expiry using refresh token
5. **Logout**: Clear local tokens + invalidate server refresh token

### Authorization
- **Role-Based Access Control (RBAC)**
  - `user`: Browse products, place orders, view own orders
  - `admin`: All user permissions + manage products, view all orders, update order status

### Security Measures
- PBKDF2 password hashing (100,000 iterations, SHA256)
- JWT tokens with short expiry (Access: 15 min, Refresh: 7 days)
- HTTPS enforcement in production
- CORS configuration
- Input validation on both client and server
- SQL injection prevention (EF Core parameterized queries)

---

## 🔄 Data Flow Patterns

### Read Flow (Customer Views Products)
```
User Taps Products
    ↓
ViewModel.LoadProductsCommand
    ↓
ProductService.GetAvailableProductsAsync(date)
    ↓
Check Local SQLite Cache (if offline)
    ↓ (if online)
HttpClient → GET /api/products?date={date}
    ↓
AuthService injects JWT token
    ↓
API ProductController.GetProducts()
    ↓
ProductService.GetAvailableProducts()
    ↓
Repository queries DB with EF Core
    ↓
Map Entity → DTO → JSON Response
    ↓
MAUI receives response
    ↓
Update local cache
    ↓
ObservableCollection updated
    ↓
UI refreshes via data binding
```

### Write Flow (Customer Places Order)
```
User Confirms Order
    ↓
ViewModel.PlaceOrderCommand
    ↓
Validate cart items & quantities
    ↓
OrderService.CreateOrderAsync(orderRequest)
    ↓
POST /api/orders with JWT
    ↓
API OrderController.CreateOrder()
    ↓
[Authorize] middleware validates token
    ↓
Extract UserId from JWT claims
    ↓
OrderService.CreateOrderAsync()
    ↓
Begin database transaction
    ↓
Create Order entity
    ↓
Create OrderItems
    ↓
Update ProductAvailability stock
    ↓
Commit transaction
    ↓
Return OrderDto
    ↓
MAUI updates local SQLite
    ↓
Navigate to Order Confirmation
```

---

## 🎯 Design Patterns Used

### 1. **MVVM (Model-View-ViewModel)**
- Complete separation of UI and business logic
- Two-way data binding
- INotifyPropertyChanged for reactive updates
- Commands for user actions

### 2. **Repository Pattern**
- Abstract data access layer
- Testable business logic
- Easy database switching
- Generic repository for common operations

### 3. **Unit of Work**
- Transaction management
- Consistent save operations across repositories
- Rollback support for complex operations

### 4. **Dependency Injection**
- Constructor injection throughout
- Service lifetime management
- Testability and loose coupling

### 5. **DTO Pattern**
- Separate API contracts from domain models
- Data validation at API boundary
- Version compatibility
- Security (don't expose internal models)

### 6. **Factory Pattern**
- ViewModel creation
- HTTP client configuration
- Database context initialization

### 7. **Strategy Pattern**
- Multiple payment methods (future)
- Different delivery options
- Pricing strategies

---

## 🚀 Scalability Considerations

### Current Architecture (Phase 1)
- Single-region deployment
- Monolithic API
- Direct database connection
- In-memory caching

### Future Enhancements (Phase 2+)

#### **Microservices Architecture**
```
API Gateway (AWS API Gateway)
    ↓
┌────────────┬────────────┬────────────┐
│   Auth     │  Product   │   Order    │
│  Service   │  Service   │  Service   │
└────────────┴────────────┴────────────┘
```

#### **Caching Layer**
- Redis for distributed caching
- Cache product catalog
- Session management
- Reduce database load

#### **Message Queue**
- AWS SQS/SNS for async processing
- Order confirmation emails
- Inventory updates
- Analytics events

#### **CDN for Images**
- AWS S3 + CloudFront
- Product images
- User avatars
- Static assets

#### **Database Optimization**
- Read replicas for reporting
- Database sharding by region
- Materialized views for analytics
- Connection pooling

#### **Monitoring & Observability**
- Application Insights / CloudWatch
- Distributed tracing
- Performance metrics
- Error tracking (Sentry)

#### **CI/CD Pipeline**
- GitHub Actions
- Automated testing
- Staging environment
- Blue-green deployments

---

## 📱 Offline-First Strategy

### Local SQLite Schema
```
LocalProducts (mirrors Products)
LocalOrders (pending sync)
LocalOrderItems
SyncQueue (tracks pending operations)
```

### Sync Process
1. **App Start**: Check connectivity
2. **Online**: Fetch latest data, sync pending changes
3. **Offline**: Work with local cache
4. **Queue Operations**: Store create/update operations
5. **Reconnect**: Push queued operations to server
6. **Conflict Resolution**: Last-write-wins with timestamp

---

## 🧪 Testing Strategy

### Unit Tests
- ViewModels (command execution, validation)
- Services (business logic)
- Repositories (data access)
- Converters (value transformations)

### Integration Tests
- API endpoints
- Database operations
- Authentication flows

### UI Tests
- Critical user journeys
- Cross-platform compatibility
- Accessibility compliance

---

## 🔧 Configuration Management

### Environment-Specific Settings

**Development**
- SQLite database
- Localhost API (http://localhost:5001)
- Verbose logging
- Swagger enabled

**Staging**
- PostgreSQL
- Staging API URL
- Limited logging
- Test data

**Production**
- AWS RDS PostgreSQL
- Production API (HTTPS only)
- Error logging only
- Real payment integration

---

## 📝 API Versioning

### URL-Based Versioning
```
/api/v1/products
/api/v1/orders
/api/v2/products (future breaking changes)
```

### Header-Based Versioning (Alternative)
```
GET /api/products
Accept: application/vnd.arunayan.v1+json
```

---

## 🌐 Localization & Globalization

### Current: English (en-US)
### Future Support:
- Hindi (hi-IN)
- Marathi (mr-IN)
- Regional date/time formats
- Currency formatting (₹ INR)

---

## 📊 Performance Targets

| Metric | Target |
|--------|--------|
| API Response Time | < 200ms (p95) |
| App Cold Start | < 3s |
| Product List Load | < 1s |
| Order Placement | < 2s |
| Offline Access | Instant |
| Database Query | < 50ms |

---

## 🔒 Compliance & Privacy

- GDPR-ready data handling
- User data export capability
- Right to be forgotten (delete account)
- Data encryption at rest and in transit
- PCI DSS for payment data (future)

---

This architecture provides a solid foundation for a scalable, maintainable, and production-ready dairy management application while allowing for future enhancements and growth.
