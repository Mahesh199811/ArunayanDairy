# MAUI UI Implementation Guide

## Overview
Complete UI implementation with proper MVVM pattern for the Arunayan Dairy mobile application. All pages follow clean architecture principles with proper separation of concerns.

## Project Structure

```
ArunayanDairy/
├── Models/
│   ├── DomainModels.cs          # Product, Category, Order, CartItem models
│   ├── LoginRequest.cs
│   ├── SignupRequest.cs
│   └── LoginResponse.cs
├── Services/
│   ├── IProductService.cs       # Product API interface
│   ├── ProductService.cs        # HTTP client for products
│   ├── IOrderService.cs         # Order API interface
│   ├── OrderService.cs          # HTTP client for orders
│   ├── IAuthService.cs
│   └── AuthService.cs
├── ViewModels/
│   ├── BaseViewModel.cs         # Base class with INotifyPropertyChanged
│   ├── ProductListViewModel.cs  # Products browsing, filtering, cart
│   ├── ProductDetailViewModel.cs # Product details, quantity selection
│   ├── CartViewModel.cs         # Shopping cart management
│   ├── OrderHistoryViewModel.cs # Order list with filters
│   ├── OrderDetailViewModel.cs  # Order details view
│   ├── LoginViewModel.cs
│   └── SignupViewModel.cs
├── Views/
│   ├── ProductListPage.xaml     # Products grid with filters
│   ├── ProductDetailPage.xaml   # Product details page
│   ├── CartPage.xaml            # Shopping cart
│   ├── OrderHistoryPage.xaml   # Orders list
│   ├── OrderDetailPage.xaml    # Order details
│   ├── LoginPage.xaml
│   └── SignupPage.xaml
├── Converters/
│   ├── StringToBoolConverter.cs
│   ├── InvertedBoolConverter.cs
│   ├── IntToBoolConverter.cs
│   └── NullToBoolConverter.cs
└── Helpers/
    └── SecureStorageHelper.cs
```

## Implemented Pages

### 1. ProductListPage
**Purpose:** Main products browsing page with filtering and cart functionality

**Features:**
- Search bar for product name/description
- Category dropdown filter
- Date picker for availability
- Product cards with image, name, price, category
- Add to cart button on each product
- Cart summary footer with item count and total
- Pull-to-refresh support
- Empty state message

**ViewModel:** `ProductListViewModel`
- ObservableCollection<Product> for products
- Category and date filtering
- Search text filtering
- Cart items management
- Commands: LoadCommand, RefreshCommand, ProductTappedCommand, AddToCartCommand, ViewCartCommand

**Navigation:**
- Tap product → Navigate to ProductDetail
- Tap cart footer → Navigate to Cart

### 2. ProductDetailPage
**Purpose:** Display product details and quantity selection

**Features:**
- Large product image
- Product name, category, description
- Price display with unit
- SKU and unit information cards
- Min/max order quantity info
- Quantity selector with +/- buttons
- Real-time subtotal calculation
- Add to cart button (enabled based on quantity rules)

**ViewModel:** `ProductDetailViewModel`
- Query parameter: productId
- Product details loading
- Quantity management with min/max validation
- Commands: IncreaseQuantityCommand, DecreaseQuantityCommand, AddToCartCommand

**Navigation:**
- Back button → Return to ProductList

### 3. CartPage
**Purpose:** Shopping cart management and checkout

**Features:**
- Cart items list with product images
- Quantity controls (+/-) for each item
- Remove item button
- Delivery date picker
- Optional notes editor
- Order summary (total items, total amount)
- Clear cart button
- Place order button

**ViewModel:** `CartViewModel`
- Shared CartItems collection with ProductListViewModel
- Delivery date selection (tomorrow to 30 days)
- Order placement with validation
- Commands: IncreaseQuantityCommand, DecreaseQuantityCommand, RemoveItemCommand, PlaceOrderCommand, ClearCartCommand

**Navigation:**
- After order placed → Navigate to Orders tab
- Empty cart → Shows empty state message

### 4. OrderHistoryPage
**Purpose:** View all customer orders with filtering

**Features:**
- Search bar (order number, product name)
- Status filter dropdown (All, Pending, Confirmed, Processing, Delivered, Cancelled)
- Order cards with:
  - Order number
  - Status badge (color-coded)
  - Order date and delivery date
  - Item count and total amount
- Pull-to-refresh
- Empty state message

**ViewModel:** `OrderHistoryViewModel`
- ObservableCollection<Order> with filtering
- Search and status filter logic
- Commands: LoadCommand, RefreshCommand, OrderTappedCommand, CancelOrderCommand

**Navigation:**
- Tap order → Navigate to OrderDetail

### 5. OrderDetailPage
**Purpose:** View complete order details

**Features:**
- Order header with number and status badge
- Order date and delivery date
- Optional notes display
- Order items list (product name, quantity, unit price, subtotal)
- Total amount summary
- Cancel order button (visible only for Pending/Confirmed status)

**ViewModel:** `OrderDetailViewModel`
- Query parameter: orderId
- Order details loading
- Cancel order functionality
- Commands: CancelOrderCommand

**Navigation:**
- After cancel → Navigate back to OrderHistory

## MVVM Pattern Implementation

### BaseViewModel
All ViewModels inherit from `BaseViewModel` which provides:
- INotifyPropertyChanged implementation
- IsBusy property with IsNotBusy inverse
- Title property for page titles
- ErrorMessage property for error display
- SetProperty helper method
- ClearError helper method

### Command Pattern
All user actions use ICommand:
- Button clicks
- List item taps
- Pull-to-refresh
- Navigation

### Data Binding
- Two-way binding for user inputs (SearchBar, Picker, DatePicker, Editor)
- One-way binding for display (Labels, Images)
- Collection binding for lists (CollectionView)
- MultiBinding for formatted strings (price with unit, subtotals)

### Converters
- `StringToBoolConverter`: Shows elements when string is not empty
- `InvertedBoolConverter`: Inverts boolean (e.g., IsNotBusy)
- `IntToBoolConverter`: Shows cart footer when count > 0
- `NullToBoolConverter`: Shows elements when value is not null

## Service Layer

### ProductService
HTTP client wrapper for product endpoints:
- `GetCategoriesAsync()`: Get all active categories
- `GetProductsAsync(categoryId)`: Get products by category
- `GetAvailableProductsAsync(date, categoryId)`: Get products available on date
- `GetProductByIdAsync(id)`: Get product details

Base URL: `http://localhost:5001/api` (iOS) or `http://10.0.2.2:5001/api` (Android)

### OrderService
HTTP client wrapper for order endpoints:
- `CreateOrderAsync(request)`: Place new order
- `GetMyOrdersAsync(fromDate, toDate)`: Get customer orders
- `GetOrderByIdAsync(orderId)`: Get order details
- `CancelOrderAsync(orderId)`: Cancel order

All requests include JWT Bearer token from SecureStorage.

## Navigation Structure

### Shell Navigation (AppShell.xaml)
```xml
TabBar:
├── Products (ProductListPage) - Default tab
├── Orders (OrderHistoryPage)
├── Cart (CartPage)
└── Profile (MainPage)

Routes:
├── ProductDetail (modal)
├── OrderDetail (modal)
└── Cart (can also navigate via route)
```

### Navigation Flow
1. **Login** → Products tab (default landing)
2. **Products** → ProductDetail (tap product)
3. **ProductDetail** → Back to Products
4. **Products** → Cart (tap cart footer)
5. **Cart** → Orders tab (after order placed)
6. **Orders** → OrderDetail (tap order)
7. **OrderDetail** → Back to Orders

## Dependency Injection

### MauiProgram.cs Registration
```csharp
// Services
builder.Services.AddSingleton<HttpClient>();
builder.Services.AddSingleton<IAuthService, AuthService>();
builder.Services.AddSingleton<IProductService, ProductService>();
builder.Services.AddSingleton<IOrderService, OrderService>();

// ViewModels
builder.Services.AddSingleton<ProductListViewModel>(); // Shared cart state
builder.Services.AddTransient<ProductDetailViewModel>();
builder.Services.AddSingleton<CartViewModel>(); // Shared cart state
builder.Services.AddTransient<OrderHistoryViewModel>();
builder.Services.AddTransient<OrderDetailViewModel>();

// Views
builder.Services.AddTransient<ProductListPage>();
builder.Services.AddTransient<ProductDetailPage>();
builder.Services.AddTransient<CartPage>();
builder.Services.AddTransient<OrderHistoryPage>();
builder.Services.AddTransient<OrderDetailPage>();
```

**Singleton vs Transient:**
- Singleton: ProductListViewModel, CartViewModel (shared cart state)
- Transient: Detail pages (new instance each navigation)

## State Management

### Shared Cart State
`ProductListViewModel` and `CartViewModel` share the same `ObservableCollection<CartItem>`:
- ProductListViewModel adds items to cart
- CartViewModel manages cart (update quantities, remove items, checkout)
- Cart count displayed in ProductList footer
- Changes automatically reflect in both ViewModels

### Property Change Notifications
All properties that affect UI use `OnPropertyChanged()`:
- IsBusy affects loading indicators
- ErrorMessage affects error frame visibility
- Collection changes update CollectionView
- Computed properties (CartTotal, CartItemCount) notify on collection changes

## UI Components

### Product Card
- 80x80 image (AspectFill)
- Name (16pt Bold)
- Category name (12pt Gray)
- Price with unit (14pt Blue Bold)
- Min order quantity (12pt Gray)
- Add to cart button (+)

### Order Card
- Order number (18pt Bold)
- Status badge (colored frame)
- Order date and delivery date
- Item count and total amount (18pt Green Bold)
- Tap to view details

### Cart Item Card
- 80x80 product image
- Product name and price
- Remove button (X)
- Quantity controls (+/-)
- Subtotal display

### Empty States
- Products: "No products available / Try changing filters"
- Cart: "Your cart is empty / Add products to get started"
- Orders: "No orders found / Place your first order!"

## Error Handling

### Error Display
All pages have error frame at top:
- Red background (#FFE6E6)
- Error message in red text
- Visible only when ErrorMessage is not empty
- Automatically hidden when ClearError() called

### Error Sources
- Network errors (HTTP failures)
- API errors (4xx, 5xx responses)
- Validation errors (min/max quantity)
- Authentication errors (token expired)

### Loading States
- IsBusy shows ActivityIndicator
- IsBusy disables buttons
- Pull-to-refresh uses IsBusy
- Loading overlay on detail pages

## Testing Checklist

### Products Page
- [ ] Load products on page appear
- [ ] Filter by category works
- [ ] Filter by date works
- [ ] Search filters correctly
- [ ] Add to cart increases count
- [ ] Cart footer shows correct total
- [ ] Navigate to product detail
- [ ] Navigate to cart
- [ ] Pull-to-refresh reloads data

### Product Detail Page
- [ ] Product loads with correct data
- [ ] Image displays properly
- [ ] Quantity increases/decreases
- [ ] Min/max quantity enforced
- [ ] Subtotal calculates correctly
- [ ] Add to cart button enabled/disabled correctly
- [ ] Success message shown
- [ ] Navigate back works

### Cart Page
- [ ] Cart items display correctly
- [ ] Increase/decrease quantity works
- [ ] Remove item works
- [ ] Delivery date picker works
- [ ] Notes field works
- [ ] Total calculates correctly
- [ ] Place order creates order
- [ ] Clear cart empties cart
- [ ] Empty state displays when empty

### Order History Page
- [ ] Orders load on appear
- [ ] Search filters correctly
- [ ] Status filter works
- [ ] Order cards display correctly
- [ ] Status badges color-coded
- [ ] Navigate to order detail
- [ ] Pull-to-refresh works

### Order Detail Page
- [ ] Order loads with correct data
- [ ] Order items display correctly
- [ ] Total amount correct
- [ ] Cancel button visible for Pending/Confirmed
- [ ] Cancel order works
- [ ] Navigate back works

## Next Steps

### Pending Features
1. **Offline Support**: SQLite local cache for products and orders
2. **Image Caching**: Cache product images locally
3. **Favorites**: Save favorite products
4. **Order Tracking**: Real-time order status updates
5. **Push Notifications**: Order status notifications
6. **Admin Dashboard**: Admin-only order management
7. **Profile Page**: User profile with logout
8. **Settings**: App settings and preferences

### Performance Optimizations
1. Lazy loading for long product lists
2. Image compression and caching
3. Debounce search input
4. Cache API responses
5. Optimize CollectionView rendering

### Accessibility
1. Screen reader support
2. High contrast mode
3. Font scaling support
4. Keyboard navigation
5. Voice commands

## API Endpoints Used

### Products
- `GET /api/Categories` - Get all categories
- `GET /api/Products` - Get all products
- `GET /api/Products/available/{date}` - Get available products by date
- `GET /api/Products/{id}` - Get product details

### Orders
- `POST /api/Orders` - Create order
- `GET /api/Orders` - Get customer orders
- `GET /api/Orders/{id}` - Get order details
- `DELETE /api/Orders/{id}` - Cancel order

All endpoints require JWT Bearer token in Authorization header.

## Troubleshooting

### Products not loading
- Check API server is running on localhost:5001
- Verify JWT token in SecureStorage
- Check network permissions in AndroidManifest.xml / Info.plist

### Cart state not persisting
- ProductListViewModel and CartViewModel must be registered as Singleton
- Check dependency injection in MauiProgram.cs

### Navigation not working
- Verify routes registered in AppShell.xaml.cs
- Check Shell.Current.GoToAsync calls
- Ensure QueryProperty names match parameters

### Images not displaying
- Check ImageUrl is valid
- Use TargetNullValue for missing images
- Verify network permissions

## Conclusion

The UI implementation provides a complete, production-ready mobile application with:
- ✅ Clean MVVM architecture
- ✅ Proper dependency injection
- ✅ Comprehensive error handling
- ✅ Responsive UI with loading states
- ✅ Shared cart state management
- ✅ Tab-based navigation
- ✅ Modal detail pages
- ✅ Pull-to-refresh support
- ✅ Empty states
- ✅ Real-time computed properties
- ✅ Type-safe data binding

All pages follow consistent patterns and are ready for testing and deployment.
