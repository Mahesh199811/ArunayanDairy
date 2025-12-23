using System.Windows.Input;
using ArunayanDairy.Models;
using ArunayanDairy.Services;

namespace ArunayanDairy.ViewModels;

[QueryProperty(nameof(ProductId), "productId")]
public class ProductDetailViewModel : BaseViewModel
{
    private readonly IProductService _productService;
    private Product? _product;
    private decimal _quantity;
    private decimal _selectedPrice;

    public Product? Product
    {
        get => _product;
        set => SetProperty(ref _product, value);
    }

    public decimal Quantity
    {
        get => _quantity;
        set
        {
            SetProperty(ref _quantity, value);
            OnPropertyChanged(nameof(Subtotal));
            OnPropertyChanged(nameof(CanAddToCart));
        }
    }

    public decimal SelectedPrice
    {
        get => _selectedPrice;
        set
        {
            SetProperty(ref _selectedPrice, value);
            OnPropertyChanged(nameof(Subtotal));
        }
    }

    public decimal Subtotal => Quantity * SelectedPrice;

    public bool CanAddToCart => Product != null && 
                                 Quantity >= Product.MinOrderQuantity && 
                                 (!Product.MaxOrderQuantity.HasValue || Quantity <= Product.MaxOrderQuantity);

    public string ProductId { get; set; } = string.Empty;

    public ICommand LoadCommand { get; }
    public ICommand IncreaseQuantityCommand { get; }
    public ICommand DecreaseQuantityCommand { get; }
    public ICommand AddToCartCommand { get; }

    public ProductDetailViewModel(IProductService productService)
    {
        _productService = productService;
        Title = "Product Details";

        LoadCommand = new Command(async () => await LoadProductAsync());
        IncreaseQuantityCommand = new Command(IncreaseQuantity);
        DecreaseQuantityCommand = new Command(DecreaseQuantity);
        AddToCartCommand = new Command(async () => await AddToCartAsync(), () => CanAddToCart);
    }

    public async Task InitializeAsync()
    {
        await LoadProductAsync();
    }

    private async Task LoadProductAsync()
    {
        if (IsBusy || string.IsNullOrEmpty(ProductId))
            return;

        try
        {
            IsBusy = true;
            ClearError();

            if (Guid.TryParse(ProductId, out var id))
            {
                Product = await _productService.GetProductByIdAsync(id);
                
                if (Product != null)
                {
                    Title = Product.Name;
                    Quantity = Product.MinOrderQuantity;
                    SelectedPrice = Product.BasePrice;
                }
                else
                {
                    ErrorMessage = "Product not found";
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading product: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void IncreaseQuantity()
    {
        if (Product == null)
            return;

        var newQuantity = Quantity + Product.MinOrderQuantity;
        if (!Product.MaxOrderQuantity.HasValue || newQuantity <= Product.MaxOrderQuantity)
        {
            Quantity = newQuantity;
        }
    }

    private void DecreaseQuantity()
    {
        if (Product == null)
            return;

        var newQuantity = Quantity - Product.MinOrderQuantity;
        if (newQuantity >= Product.MinOrderQuantity)
        {
            Quantity = newQuantity;
        }
    }

    private async Task AddToCartAsync()
    {
        if (Product == null || !CanAddToCart)
            return;

        // In a real app, add to cart service
        if (Shell.Current?.CurrentPage != null)
            await Shell.Current.CurrentPage.DisplayAlert("Success", 
                $"Added {Quantity} {Product.Unit} of {Product.Name} to cart", "OK");
        
        await Shell.Current.GoToAsync("..");
    }
}
