using System.Collections.ObjectModel;
using System.Windows.Input;
using ArunayanDairy.Models;
using ArunayanDairy.Services;

namespace ArunayanDairy.ViewModels;

public class ProductListViewModel : BaseViewModel
{
    private readonly IProductService _productService;
    private readonly IOrderService _orderService;
    
    private Vendor? _selectedVendor;
    private Category? _selectedCategory;
    private DateTime _selectedDate = DateTime.Today;
    private string _searchText = string.Empty;

    public ObservableCollection<Vendor> Vendors { get; } = new();
    public ObservableCollection<Category> Categories { get; } = new();
    public ObservableCollection<Product> Products { get; } = new();
    public ObservableCollection<Product> FilteredProducts { get; } = new();
    public ObservableCollection<CartItem> CartItems { get; } = new();

    public Vendor? SelectedVendor
    {
        get => _selectedVendor;
        set
        {
            SetProperty(ref _selectedVendor, value);
            CartItems.Clear(); // Clear cart when vendor changes
            OnPropertyChanged(nameof(CartItemCount));
            OnPropertyChanged(nameof(CartTotal));
            _ = LoadProductsAsync();
        }
    }

    public Category? SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            SetProperty(ref _selectedCategory, value);
            _ = LoadProductsAsync();
        }
    }

    public DateTime SelectedDate
    {
        get => _selectedDate;
        set
        {
            SetProperty(ref _selectedDate, value);
            _ = LoadProductsAsync();
        }
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            SetProperty(ref _searchText, value);
            FilterProducts();
        }
    }

    public int CartItemCount => CartItems.Sum(c => (int)c.Quantity);
    public decimal CartTotal => CartItems.Sum(c => c.Subtotal);

    public ICommand LoadCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand ProductTappedCommand { get; }
    public ICommand AddToCartCommand { get; }
    public ICommand ViewCartCommand { get; }

    public ProductListViewModel(IProductService productService, IOrderService orderService)
    {
        _productService = productService;
        _orderService = orderService;
        Title = "Products";

        LoadCommand = new Command(async () => await LoadDataAsync());
        RefreshCommand = new Command(async () => await LoadDataAsync());
        ProductTappedCommand = new Command<Product>(async (product) => await OnProductTapped(product));
        AddToCartCommand = new Command<Product>(async (product) => await AddToCart(product));
        ViewCartCommand = new Command(async () => await ViewCart());
    }

    public async Task InitializeAsync()
    {
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            ClearError();

            // Load vendors
            var vendors = await _productService.GetVendorsAsync();
            Vendors.Clear();
            foreach (var vendor in vendors)
            {
                Vendors.Add(vendor);
            }
            
            // Auto-select first vendor if none selected
            if (SelectedVendor == null && Vendors.Count > 0)
            {
                SelectedVendor = Vendors.First();
            }

            // Load categories
            var categories = await _productService.GetCategoriesAsync();
            Categories.Clear();
            Categories.Add(new Category { Id = Guid.Empty, Name = "All Categories" });
            foreach (var category in categories.Where(c => c.IsActive))
            {
                Categories.Add(category);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading categories: {ex.Message}";
        }
    }

    private async Task LoadProductsAsync()
    {
        try
        {
            if (SelectedVendor == null)
                return;

            var categoryId = SelectedCategory?.Id != Guid.Empty ? SelectedCategory?.Id : null;
            var products = await _productService.GetAvailableProductsAsync(SelectedDate, categoryId, SelectedVendor.Id);
            
            Products.Clear();
            foreach (var product in products)
            {
                Products.Add(product);
            }

            FilterProducts();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading products: {ex.Message}";
        }
    }

    private void FilterProducts()
    {
        FilteredProducts.Clear();
        
        var filtered = string.IsNullOrWhiteSpace(SearchText)
            ? Products
            : Products.Where(p => p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                                 p.Description?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true);

        foreach (var product in filtered)
        {
            FilteredProducts.Add(product);
        }
    }

    private async Task OnProductTapped(Product product)
    {
        if (product == null)
            return;

        await Shell.Current.GoToAsync($"ProductDetail?productId={product.Id}");
    }

    private async Task AddToCart(Product product)
    {
        if (product == null)
            return;

        var existingItem = CartItems.FirstOrDefault(c => c.Product.Id == product.Id);
        if (existingItem != null)
        {
            existingItem.Quantity += product.MinOrderQuantity;
        }
        else
        {
            CartItems.Add(new CartItem
            {
                Product = product,
                Quantity = product.MinOrderQuantity,
                EffectivePrice = product.BasePrice
            });
        }

        OnPropertyChanged(nameof(CartItemCount));
        OnPropertyChanged(nameof(CartTotal));

        if (Shell.Current?.CurrentPage != null)
            await Shell.Current.CurrentPage.DisplayAlert("Success", $"{product.Name} added to cart", "OK");
    }

    private async Task ViewCart()
    {
        if (!CartItems.Any())
        {
            if (Shell.Current?.CurrentPage != null)
                await Shell.Current.CurrentPage.DisplayAlert("Cart Empty", "Your cart is empty", "OK");
            return;
        }

        await Shell.Current.GoToAsync("Cart");
    }
}
