# 📱 MAUI Frontend - Implementation Roadmap

## Overview
This document provides step-by-step guidance for implementing the .NET MAUI mobile application.

---

## 🎯 Implementation Phases

### Phase 1: Foundation (2-3 hours)
Setup core infrastructure and basic navigation.

### Phase 2: Product Browsing (3-4 hours)
Implement product catalog with filtering and search.

### Phase 3: Cart & Orders (3-4 hours)
Shopping cart and order placement functionality.

### Phase 4: Admin Features (2-3 hours)
Dashboard and management capabilities.

### Phase 5: Offline Support (2-3 hours)
Local database and synchronization.

---

## 📋 Phase 1: Foundation

### Step 1.1: Create Base Classes

#### **BaseViewModel.cs**
```csharp
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ArunayanDairy.ViewModels;

public class BaseViewModel : INotifyPropertyChanged
{
    private bool _isBusy;
    private string _title = string.Empty;
    private string _errorMessage = string.Empty;

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(storage, value))
            return false;

        storage = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
```

### Step 1.2: Create API Configuration

#### **ApiConfig.cs**
```csharp
namespace ArunayanDairy.Configuration;

public static class ApiConfig
{
#if DEBUG
    public const string BaseUrl = "http://localhost:5001/api";
#else
    public const string BaseUrl = "https://your-production-api.com/api";
#endif

    public static class Endpoints
    {
        public const string Auth = "auth";
        public const string Categories = "categories";
        public const string Products = "products";
        public const string Orders = "orders";
    }
}
```

### Step 1.3: Create Domain Models

#### **Category.cs**
```csharp
namespace ArunayanDairy.Models;

public class Category
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}
```

#### **Product.cs**
```csharp
namespace ArunayanDairy.Models;

public class Product
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
    public decimal MinOrderQuantity { get; set; }
    public decimal? MaxOrderQuantity { get; set; }
}
```

#### **Order.cs**
```csharp
namespace ArunayanDairy.Models;

public class Order
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime DeliveryDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public List<OrderItem> Items { get; set; } = new();
}

public class OrderItem
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductUnit { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }
}
```

#### **Cart.cs**
```csharp
namespace ArunayanDairy.Models;

public class CartItem
{
    public Product Product { get; set; } = null!;
    public decimal Quantity { get; set; }
    public decimal Subtotal => Quantity * Product.BasePrice;
}

public class Cart
{
    public List<CartItem> Items { get; set; } = new();
    public decimal TotalAmount => Items.Sum(i => i.Subtotal);
    public int ItemCount => Items.Count;
}
```

---

## 📋 Phase 2: API Services

### Step 2.1: Base HTTP Service

#### **BaseHttpService.cs**
```csharp
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ArunayanDairy.Helpers;

namespace ArunayanDairy.Services;

public class BaseHttpService
{
    protected readonly HttpClient _httpClient;
    protected readonly JsonSerializerOptions _jsonOptions;

    public BaseHttpService()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(Configuration.ApiConfig.BaseUrl)
        };

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    protected async Task<T?> GetAsync<T>(string endpoint)
    {
        await AddAuthHeaderAsync();
        var response = await _httpClient.GetAsync(endpoint);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(_jsonOptions);
    }

    protected async Task<T?> PostAsync<T>(string endpoint, object data)
    {
        await AddAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync(endpoint, data);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(_jsonOptions);
    }

    protected async Task<T?> PutAsync<T>(string endpoint, object data)
    {
        await AddAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync(endpoint, data);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(_jsonOptions);
    }

    protected async Task<bool> DeleteAsync(string endpoint)
    {
        await AddAuthHeaderAsync();
        var response = await _httpClient.DeleteAsync(endpoint);
        return response.IsSuccessStatusCode;
    }

    private async Task AddAuthHeaderAsync()
    {
        var token = await SecureStorageHelper.GetAccessTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
```

### Step 2.2: Product Service

#### **IProductService.cs & ProductService.cs**
```csharp
using ArunayanDairy.Models;

namespace ArunayanDairy.Services;

public interface IProductService
{
    Task<List<Category>> GetCategoriesAsync();
    Task<List<Product>> GetProductsAsync(Guid? categoryId = null);
    Task<List<Product>> GetAvailableProductsAsync(DateTime date, Guid? categoryId = null);
    Task<Product?> GetProductByIdAsync(Guid id);
}

public class ProductService : BaseHttpService, IProductService
{
    public async Task<List<Category>> GetCategoriesAsync()
    {
        return await GetAsync<List<Category>>("categories") ?? new List<Category>();
    }

    public async Task<List<Product>> GetProductsAsync(Guid? categoryId = null)
    {
        var endpoint = "products";
        if (categoryId.HasValue)
            endpoint += $"?categoryId={categoryId.Value}";

        return await GetAsync<List<Product>>(endpoint) ?? new List<Product>();
    }

    public async Task<List<Product>> GetAvailableProductsAsync(DateTime date, Guid? categoryId = null)
    {
        var endpoint = $"products/available/{date:yyyy-MM-dd}";
        if (categoryId.HasValue)
            endpoint += $"?categoryId={categoryId.Value}";

        return await GetAsync<List<Product>>(endpoint) ?? new List<Product>();
    }

    public async Task<Product?> GetProductByIdAsync(Guid id)
    {
        return await GetAsync<Product>($"products/{id}");
    }
}
```

### Step 2.3: Order Service

#### **IOrderService.cs & OrderService.cs**
```csharp
using ArunayanDairy.Models;

namespace ArunayanDairy.Services;

public interface IOrderService
{
    Task<Order> CreateOrderAsync(DateTime deliveryDate, List<CartItem> items, string? notes);
    Task<List<Order>> GetMyOrdersAsync();
    Task<Order?> GetOrderByIdAsync(Guid id);
    Task<bool> CancelOrderAsync(Guid id);
}

public class OrderService : BaseHttpService, IOrderService
{
    public async Task<Order> CreateOrderAsync(DateTime deliveryDate, List<CartItem> items, string? notes)
    {
        var request = new
        {
            DeliveryDate = deliveryDate,
            Notes = notes,
            Items = items.Select(i => new
            {
                ProductId = i.Product.Id,
                Quantity = i.Quantity
            }).ToList()
        };

        var order = await PostAsync<Order>("orders", request);
        return order ?? throw new Exception("Failed to create order");
    }

    public async Task<List<Order>> GetMyOrdersAsync()
    {
        return await GetAsync<List<Order>>("orders") ?? new List<Order>();
    }

    public async Task<Order?> GetOrderByIdAsync(Guid id)
    {
        return await GetAsync<Order>($"orders/{id}");
    }

    public async Task<bool> CancelOrderAsync(Guid id)
    {
        return await DeleteAsync($"orders/{id}");
    }
}
```

---

## 📋 Phase 3: ViewModels

### Step 3.1: Product List ViewModel

#### **ProductListViewModel.cs**
```csharp
using System.Collections.ObjectModel;
using System.Windows.Input;
using ArunayanDairy.Models;
using ArunayanDairy.Services;

namespace ArunayanDairy.ViewModels;

public class ProductListViewModel : BaseViewModel
{
    private readonly IProductService _productService;
    private DateTime _selectedDate = DateTime.Today.AddDays(1);
    private Category? _selectedCategory;

    public ObservableCollection<Product> Products { get; } = new();
    public ObservableCollection<Category> Categories { get; } = new();

    public DateTime SelectedDate
    {
        get => _selectedDate;
        set
        {
            if (SetProperty(ref _selectedDate, value))
            {
                _ = LoadProductsAsync();
            }
        }
    }

    public Category? SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            if (SetProperty(ref _selectedCategory, value))
            {
                _ = LoadProductsAsync();
            }
        }
    }

    public ICommand LoadProductsCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand ProductTappedCommand { get; }

    public ProductListViewModel(IProductService productService)
    {
        _productService = productService;
        Title = "Available Products";

        LoadProductsCommand = new Command(async () => await LoadProductsAsync());
        RefreshCommand = new Command(async () => await RefreshAsync());
        ProductTappedCommand = new Command<Product>(async (product) => await OnProductTapped(product));

        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        await LoadCategoriesAsync();
        await LoadProductsAsync();
    }

    private async Task LoadCategoriesAsync()
    {
        try
        {
            IsBusy = true;
            var categories = await _productService.GetCategoriesAsync();

            Categories.Clear();
            foreach (var category in categories.Where(c => c.IsActive))
            {
                Categories.Add(category);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading categories: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadProductsAsync()
    {
        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            var products = await _productService.GetAvailableProductsAsync(
                SelectedDate,
                SelectedCategory?.Id);

            Products.Clear();
            foreach (var product in products)
            {
                Products.Add(product);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading products: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RefreshAsync()
    {
        await LoadProductsAsync();
    }

    private async Task OnProductTapped(Product product)
    {
        await Shell.Current.GoToAsync($"productdetail?id={product.Id}");
    }
}
```

### Step 3.2: Cart ViewModel

#### **CartViewModel.cs**
```csharp
using System.Collections.ObjectModel;
using System.Windows.Input;
using ArunayanDairy.Models;
using ArunayanDairy.Services;

namespace ArunayanDairy.ViewModels;

public class CartViewModel : BaseViewModel
{
    private readonly IOrderService _orderService;
    private DateTime _deliveryDate = DateTime.Today.AddDays(1);
    private string _notes = string.Empty;

    public ObservableCollection<CartItem> Items { get; } = new();

    public decimal TotalAmount => Items.Sum(i => i.Subtotal);
    public int ItemCount => Items.Count;

    public DateTime DeliveryDate
    {
        get => _deliveryDate;
        set => SetProperty(ref _deliveryDate, value);
    }

    public string Notes
    {
        get => _notes;
        set => SetProperty(ref _notes, value);
    }

    public ICommand RemoveItemCommand { get; }
    public ICommand UpdateQuantityCommand { get; }
    public ICommand PlaceOrderCommand { get; }
    public ICommand ClearCartCommand { get; }

    public CartViewModel(IOrderService orderService)
    {
        _orderService = orderService;
        Title = "Shopping Cart";

        RemoveItemCommand = new Command<CartItem>(RemoveItem);
        UpdateQuantityCommand = new Command<CartItem>(UpdateQuantity);
        PlaceOrderCommand = new Command(async () => await PlaceOrderAsync());
        ClearCartCommand = new Command(ClearCart);
    }

    public void AddItem(Product product, decimal quantity)
    {
        var existingItem = Items.FirstOrDefault(i => i.Product.Id == product.Id);

        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
        }
        else
        {
            Items.Add(new CartItem
            {
                Product = product,
                Quantity = quantity
            });
        }

        OnPropertyChanged(nameof(TotalAmount));
        OnPropertyChanged(nameof(ItemCount));
    }

    private void RemoveItem(CartItem item)
    {
        Items.Remove(item);
        OnPropertyChanged(nameof(TotalAmount));
        OnPropertyChanged(nameof(ItemCount));
    }

    private void UpdateQuantity(CartItem item)
    {
        OnPropertyChanged(nameof(TotalAmount));
    }

    private async Task PlaceOrderAsync()
    {
        if (IsBusy) return;

        if (!Items.Any())
        {
            await Shell.Current.DisplayAlert("Empty Cart", "Please add items to cart", "OK");
            return;
        }

        try
        {
            IsBusy = true;
            var order = await _orderService.CreateOrderAsync(DeliveryDate, Items.ToList(), Notes);

            await Shell.Current.DisplayAlert(
                "Success",
                $"Order {order.OrderNumber} placed successfully!",
                "OK");

            ClearCart();
            await Shell.Current.GoToAsync("//orders");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to place order: {ex.Message}";
            await Shell.Current.DisplayAlert("Error", ErrorMessage, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ClearCart()
    {
        Items.Clear();
        Notes = string.Empty;
        OnPropertyChanged(nameof(TotalAmount));
        OnPropertyChanged(nameof(ItemCount));
    }
}
```

---

## 📋 Phase 4: XAML Views

### Step 4.1: Product List Page

#### **ProductListPage.xaml**
```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:vm="clr-namespace:ArunayanDairy.ViewModels"
             xmlns:models="clr-namespace:ArunayanDairy.Models"
             x:Class="ArunayanDairy.Views.ProductListPage"
             x:DataType="vm:ProductListViewModel"
             Title="{Binding Title}">

    <Grid RowDefinitions="Auto,*" Padding="10">
        
        <!-- Filters -->
        <VerticalStackLayout Grid.Row="0" Spacing="10" Padding="0,0,0,10">
            <Label Text="Select Delivery Date:" FontAttributes="Bold"/>
            <DatePicker Date="{Binding SelectedDate}"
                       MinimumDate="{x:Static System:DateTime.Today}"
                       Format="D"/>
            
            <Label Text="Filter by Category:" FontAttributes="Bold"/>
            <Picker ItemsSource="{Binding Categories}"
                   SelectedItem="{Binding SelectedCategory}"
                   ItemDisplayBinding="{Binding Name}"
                   Title="All Categories"/>
        </VerticalStackLayout>

        <!-- Products List -->
        <RefreshView Grid.Row="1"
                    IsRefreshing="{Binding IsBusy}"
                    Command="{Binding RefreshCommand}">
            <CollectionView ItemsSource="{Binding Products}">
                <CollectionView.EmptyView>
                    <ContentView>
                        <VerticalStackLayout Padding="20" VerticalOptions="Center">
                            <Label Text="No products available"
                                  HorizontalOptions="Center"
                                  FontSize="18"
                                  TextColor="Gray"/>
                        </VerticalStackLayout>
                    </ContentView>
                </CollectionView.EmptyView>

                <CollectionView.ItemTemplate>
                    <DataTemplate x:DataType="models:Product">
                        <Frame Padding="10" Margin="5" CornerRadius="10"
                              HasShadow="True">
                            <Frame.GestureRecognizers>
                                <TapGestureRecognizer 
                                    Command="{Binding Source={RelativeSource AncestorType={x:Type vm:ProductListViewModel}}, Path=ProductTappedCommand}"
                                    CommandParameter="{Binding .}"/>
                            </Frame.GestureRecognizers>

                            <Grid ColumnDefinitions="*,Auto">
                                <VerticalStackLayout Grid.Column="0">
                                    <Label Text="{Binding Name}"
                                          FontSize="18"
                                          FontAttributes="Bold"/>
                                    <Label Text="{Binding CategoryName}"
                                          FontSize="14"
                                          TextColor="Gray"/>
                                    <Label Text="{Binding Description}"
                                          FontSize="14"
                                          MaxLines="2"/>
                                </VerticalStackLayout>

                                <VerticalStackLayout Grid.Column="1" 
                                                    VerticalOptions="Center">
                                    <Label Text="{Binding BasePrice, StringFormat='₹{0:N2}'}"
                                          FontSize="20"
                                          FontAttributes="Bold"
                                          HorizontalOptions="End"/>
                                    <Label Text="{Binding Unit, StringFormat='per {0}'}"
                                          FontSize="12"
                                          TextColor="Gray"
                                          HorizontalOptions="End"/>
                                </VerticalStackLayout>
                            </Grid>
                        </Frame>
                    </DataTemplate>
                </CollectionView.ItemTemplate>
            </CollectionView>
        </RefreshView>

        <!-- Loading Indicator -->
        <ActivityIndicator Grid.RowSpan="2"
                         IsRunning="{Binding IsBusy}"
                         IsVisible="{Binding IsBusy}"
                         Color="{StaticResource Primary}"
                         VerticalOptions="Center"
                         HorizontalOptions="Center"/>

        <!-- Error Message -->
        <Label Grid.Row="1"
              Text="{Binding ErrorMessage}"
              IsVisible="{Binding ErrorMessage, Converter={StaticResource StringToBoolConverter}}"
              TextColor="Red"
              HorizontalOptions="Center"
              VerticalOptions="Center"/>
    </Grid>

</ContentPage>
```

---

## 📋 Phase 5: Shell Navigation

### Step 5.1: Update AppShell

#### **AppShell.xaml**
```xml
<?xml version="1.0" encoding="UTF-8" ?>
<Shell
    x:Class="ArunayanDairy.AppShell"
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    xmlns:views="clr-namespace:ArunayanDairy.Views"
    Title="Arunayan Dairy">

    <FlyoutItem Title="Products" Icon="shopping_cart.png">
        <ShellContent ContentTemplate="{DataTemplate views:ProductListPage}" />
    </FlyoutItem>

    <FlyoutItem Title="Cart" Icon="cart.png">
        <ShellContent ContentTemplate="{DataTemplate views:CartPage}" />
    </FlyoutItem>

    <FlyoutItem Title="My Orders" Icon="orders.png">
        <ShellContent ContentTemplate="{DataTemplate views:OrderHistoryPage}" />
    </FlyoutItem>

    <FlyoutItem Title="Profile" Icon="profile.png">
        <ShellContent ContentTemplate="{DataTemplate views:ProfilePage}" />
    </FlyoutItem>

</Shell>
```

---

## 🛠️ Service Registration

### MauiProgram.cs
```csharp
builder.Services.AddSingleton<IProductService, ProductService>();
builder.Services.AddSingleton<IOrderService, OrderService>();
builder.Services.AddSingleton<CartViewModel>();

builder.Services.AddTransient<ProductListViewModel>();
builder.Services.AddTransient<ProductListPage>();

builder.Services.AddTransient<CartPage>();
builder.Services.AddTransient<OrderHistoryViewModel>();
builder.Services.AddTransient<OrderHistoryPage>();
```

---

## ✅ Testing Checklist

- [ ] Login with test credentials
- [ ] Browse products by date
- [ ] Filter by category
- [ ] View product details
- [ ] Add items to cart
- [ ] Modify cart quantities
- [ ] Place order
- [ ] View order history
- [ ] Cancel order
- [ ] Admin: view dashboard
- [ ] Admin: manage products
- [ ] Admin: fulfill orders

---

## 🚀 Next Steps

1. Implement remaining ViewModels (OrderHistory, ProductDetail)
2. Create remaining XAML pages
3. Add offline SQLite support
4. Implement background sync
5. Add push notifications
6. Implement image upload
7. Add unit tests
8. Prepare for deployment

---

**This provides a solid foundation for building the complete MAUI application!**
