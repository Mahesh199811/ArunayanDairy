# Arunayan Dairy API Design (Phase 1)

This document defines bounded contexts and the initial API contracts to guide backend implementation. Focus is on Auth, Products, and Orders per scope; Users and Notifications domains are identified for future expansion.

## Bounded Contexts

### Auth
- Responsibility: Identity, credentials, session lifecycle (access/refresh tokens).
- Aggregates: Credential (email, password), Session (tokens), Role membership.
- Events: UserSignedUp, UserLoggedIn, TokensRefreshed, UserLoggedOut.

### Users
- Responsibility: User profile, roles/permissions, contact details.
- Aggregates: User (id, name, email, role), Preferences.
- Events: UserProfileUpdated, UserRoleChanged.

### Products
- Responsibility: Catalog management (CRUD), pricing, stock, categorization.
- Aggregates: Product (id, name, description, price, currency, sku, stock, images, categoryId), Category.
- Events: ProductCreated, ProductUpdated, ProductDeactivated, StockAdjusted.

### Orders
- Responsibility: Order creation, status progression, totals, fulfillment.
- Aggregates: Order (id, userId, items, totals, status), OrderItem, Address.
- Events: OrderPlaced, OrderConfirmed, OrderShipped, OrderDelivered, OrderCancelled.

### Notifications
- Responsibility: User-facing messages for domain events (orders, promotions).
- Aggregates: Notification (id, channel, message, status), Subscription.
- Events: NotificationSent, NotificationFailed.

## API Standards
- Base Path: `/api`
- Versioning: Header `X-API-Version: 1` (optional); path versioning can be added later.
- Auth: JWT Bearer in `Authorization: Bearer <token>`.
- Roles: `user`, `admin` (admin required for product mutations).
- Content Type: `application/json` for both request and response.
- IDs: UUIDv4 strings for primary identifiers.
- Pagination: `page` (default 1), `pageSize` (default 20), returns `pageInfo` with `page`, `pageSize`, `totalItems`, `totalPages`.
- Errors: Consistent envelope `{ code: string, message: string, details?: object }`.
- Timestamps: ISO-8601 UTC strings (e.g., `2025-12-20T12:34:56Z`).

## Contracts (Initial Scope)

### Auth APIs

POST `/api/auth/signup`
- Request: `{ name, email, password }`
- Responses:
  - 201: `{ user, tokens: { accessToken, expiresIn, refreshToken } }`
  - 400: invalid request
  - 409: email already exists
  - 422: validation errors

POST `/api/auth/login`
- Request: `{ email, password }`
- Responses:
  - 200: `{ user, tokens }`
  - 401: invalid credentials
  - 422: validation errors

POST `/api/auth/refresh`
- Request: `{ refreshToken }`
- Responses:
  - 200: `{ tokens }`
  - 401: invalid/expired refresh token

### Product APIs

GET `/api/products`
- Query: `page`, `pageSize`, `q`, `sort`, `minPrice`, `maxPrice`, `inStock`
- Responses:
  - 200: `{ items: Product[], pageInfo }`

POST `/api/products` (admin)
- Request: `ProductCreate`
- Responses:
  - 201: `Product`
  - 401/403: unauthorized/forbidden
  - 422: validation errors

PUT `/api/products/{id}` (admin)
- Request: `ProductUpdate`
- Responses:
  - 200: `Product`
  - 401/403: unauthorized/forbidden
  - 404: not found
  - 422: validation errors

### Order APIs

POST `/api/orders`
- Auth: user required
- Request: `{ items: [{ productId, quantity }], shippingAddress, paymentMethod, notes? }`
- Responses:
  - 201: `Order`
  - 401: unauthorized
  - 422: validation errors

GET `/api/orders/user/{userId}`
- Auth: user or admin; userId must match caller unless admin
- Responses:
  - 200: `Order[]`
  - 401/403: unauthorized/forbidden

## Core Schemas (Summary)

### User
```
{
  id: string,
  name: string,
  email: string,
  role: "user" | "admin",
  createdAt: string,
  updatedAt: string
}
```

### Product
```
{
  id: string,
  name: string,
  description?: string,
  price: number,
  currency: string,
  sku?: string,
  stock: number,
  images?: string[],
  categoryId?: string,
  isActive: boolean,
  createdAt: string,
  updatedAt: string
}
```

### Order
```
{
  id: string,
  userId: string,
  items: [{ productId: string, quantity: number, unitPrice: number, lineTotal: number }],
  totals: { subtotal: number, tax: number, shipping: number, total: number },
  status: "pending" | "confirmed" | "shipped" | "delivered" | "cancelled",
  paymentStatus: "unpaid" | "paid" | "refunded",
  shippingAddress: { line1: string, line2?: string, city: string, state: string, postalCode: string, country: string },
  createdAt: string,
  updatedAt: string
}
```

### Tokens
```
{
  accessToken: string,
  expiresIn: number,
  refreshToken: string
}
```

## Notes & Next Steps
- Users and Notifications endpoints to be defined in Phase 2.
- Payment processor integration and webhook flows are out of Phase 1 scope.
- See `openapi.yaml` for full machine-readable specs.
