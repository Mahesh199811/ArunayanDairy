# Quick Start Guide - MAUI UI

## Project Structure Overview

```
ArunayanDairy/
├── Models/
│   └── DomainModels.cs          # Product, Category, Order, CartItem
├── Services/
│   ├── ProductService.cs         # API: Products & Categories
│   └── OrderService.cs           # API: Orders
├── ViewModels/
│   ├── BaseViewModel.cs          # Base with INotifyPropertyChanged
│   ├── ProductListViewModel.cs   # Product browsing + cart
│   ├── ProductDetailViewModel.cs # Product details
│   ├── CartViewModel.cs          # Shopping cart
│   ├── OrderHistoryViewModel.cs  # Order list
│   └── OrderDetailViewModel.cs   # Order details
└── Views/
    ├── ProductListPage.xaml      # Main products page
    ├── ProductDetailPage.xaml    # Product detail
    ├── CartPage.xaml             # Shopping cart
    ├── OrderHistoryPage.xaml     # Orders list
    └── OrderDetailPage.xaml      # Order detail
```

## Running the Application

### 1. Start Backend API
```bash
cd ArunayanDairy.API
dotnet run --urls "http://localhost:5001"
```

### 2. Build MAUI App
```bash
cd ArunayanDairy
dotnet build
```

### 3. Run MAUI App
- **Visual Studio**: Press F5
- **VS Code**: Use MAUI extension to run
- **CLI**: `dotnet build -t:Run -f net9.0-ios` (or android/maccatalyst)

## Test Credentials

### Admin User
- Email: `admin@arunayan.com`
- Password: `admin123`
- Access: All features + admin endpoints

### Customer User
- Email: `customer@test.com`
- Password: `customer123`
- Access: Customer features only

## API Endpoints Used

### Products
- `GET /api/Categories` - List categories
- `GET /api/Products` - List all products
- `GET /api/Products/available/{date}` - Products by date
- `GET /api/Products/{id}` - Product details

### Orders
- `POST /api/Orders` - Create order
- `GET /api/Orders` - Customer orders
- `GET /api/Orders/{id}` - Order details
- `DELETE /api/Orders/{id}` - Cancel order

## Key Features

### ProductListPage
- Search products
- Filter by category
- Filter by date
- Add to cart
- Cart summary footer
- Navigate to product detail

### ProductDetailPage
- Product image and info
- Quantity selector (+/-)
- Min/max quantity validation
- Real-time subtotal
- Add to cart

### CartPage
- List cart items
- Update quantities
- Remove items
- Select delivery date
- Add order notes
- Place order

### OrderHistoryPage
- List all orders
- Search orders
- Filter by status
- View order details
- Cancel order

### OrderDetailPage
- Order information
- List order items
- Total amount
- Cancel button (Pending/Confirmed)

## Navigation Flow

```
Login → Products (Tab 1)
         ├── Tap Product → ProductDetail → Back
         └── Tap Cart Footer → Cart (Tab 3)

Orders (Tab 2)
         └── Tap Order → OrderDetail → Back

Cart (Tab 3)
         └── Place Order → Orders (Tab 2)

Profile (Tab 4)
         └── MainPage (placeholder)
```

## Dependency Injection

All services, ViewModels, and Views are registered in `MauiProgram.cs`:

```csharp
// Services (Singleton)
builder.Services.AddSingleton<HttpClient>();
builder.Services.AddSingleton<SecureStorageHelper>();
builder.Services.AddSingleton<IProductService, ProductService>();
builder.Services.AddSingleton<IOrderService, OrderService>();

// ViewModels (Singleton for shared state, Transient for details)
builder.Services.AddSingleton<ProductListViewModel>();
builder.Services.AddSingleton<CartViewModel>();
builder.Services.AddTransient<ProductDetailViewModel>();
builder.Services.AddTransient<OrderHistoryViewModel>();
builder.Services.AddTransient<OrderDetailViewModel>();

// Views (Transient)
builder.Services.AddTransient<ProductListPage>();
builder.Services.AddTransient<CartPage>();
// ... etc
```

## Shared State

**Cart Management**: `ProductListViewModel` and `CartViewModel` share the same `ObservableCollection<CartItem>`:
- Add items in ProductList → Reflects in Cart
- Update items in Cart → Reflects in ProductList footer
- Real-time synchronization

## Common Issues & Solutions

### Products not loading
**Issue**: Empty product list
**Solution**: 
- Ensure API is running on localhost:5001
- Check JWT token in SecureStorage (login first)
- Verify network permissions

### Cart not persisting
**Issue**: Cart clears between pages
**Solution**: 
- Ensure ProductListViewModel is registered as Singleton
- Ensure CartViewModel is registered as Singleton
- Check both use the same ObservableCollection

### Navigation errors
**Issue**: Navigation doesn't work
**Solution**:
- Check routes registered in AppShell.xaml.cs
- Verify QueryProperty names match
- Use correct route syntax

### Images not showing
**Issue**: Product images don't display
**Solution**:
- Check ImageUrl in product data
- Use TargetNullValue for fallback
- Verify image URLs are accessible

## Build Status

✅ **All platforms build successfully**
- iOS: ✅ 7.3s
- Android: ✅ 39.4s
- macOS Catalyst: ✅ 8.5s

Warnings are non-critical (nullable references, XAML optimizations)

## File Count Summary

**Created**: 20 new files
- 1 Models file
- 2 Services
- 6 ViewModels
- 10 Views (5 XAML + 5 code-behind)
- 2 Converters
- 2 Documentation files

**Updated**: 5 files
- App.xaml
- MauiProgram.cs
- AppShell.xaml + .cs
- LoginViewModel.cs

## MVVM Pattern

All pages follow consistent MVVM:
1. **Model**: Domain models in DomainModels.cs
2. **View**: XAML pages with data binding
3. **ViewModel**: Business logic with ICommand and INotifyPropertyChanged

```
View (XAML)
  ↕ Data Binding
ViewModel (Properties, Commands)
  ↕ Service calls
Service (HTTP Client)
  ↕ HTTP Requests
API (ASP.NET Core)
```

## Testing Checklist

- [ ] Login with test credentials
- [ ] View products list
- [ ] Filter by category
- [ ] Search products
- [ ] View product detail
- [ ] Add to cart
- [ ] View cart
- [ ] Update quantities
- [ ] Place order
- [ ] View order history
- [ ] View order details
- [ ] Cancel order

## Next Development Steps

1. **Offline Support**: Implement SQLite local cache
2. **Image Caching**: Cache product images
3. **Profile Page**: Complete profile functionality
4. **Admin Dashboard**: Admin-only order management
5. **Push Notifications**: Order status updates

## Resources

- **API Documentation**: http://localhost:5001/swagger
- **Implementation Guide**: docs/MAUI_UI_IMPLEMENTATION.md
- **Summary**: docs/UI_IMPLEMENTATION_SUMMARY.md

---

**Status**: ✅ Complete and ready for testing
**Build**: ✅ Successful on all platforms
**Architecture**: ✅ Clean MVVM with proper DI
