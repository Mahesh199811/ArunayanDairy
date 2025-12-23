using System.Collections.ObjectModel;
using System.Windows.Input;
using ArunayanDairy.Models;
using ArunayanDairy.Services;

namespace ArunayanDairy.ViewModels;

public class ManageProductsViewModel : BaseViewModel
{
    private readonly IProductService _productService;
    private ObservableCollection<Product> _products = new();
    private ObservableCollection<Category> _categories = new();
    private Product? _selectedProduct;
    private bool _isEditing;

    public ManageProductsViewModel(IProductService productService)
    {
        _productService = productService;
        
        LoadDataCommand = new Command(async () => await LoadDataAsync());
        AddProductCommand = new Command(OnAddProduct);
        EditProductCommand = new Command<Product>(OnEditProduct);
        DeleteProductCommand = new Command<Product>(async (p) => await DeleteProductAsync(p));
        SaveProductCommand = new Command(async () => await SaveProductAsync(), () => CanSaveProduct());
        CancelEditCommand = new Command(OnCancelEdit);
    }

    public ObservableCollection<Product> Products
    {
        get => _products;
        set => SetProperty(ref _products, value);
    }

    public ObservableCollection<Category> Categories
    {
        get => _categories;
        set => SetProperty(ref _categories, value);
    }

    public Product? SelectedProduct
    {
        get => _selectedProduct;
        set
        {
            SetProperty(ref _selectedProduct, value);
            ((Command)SaveProductCommand).ChangeCanExecute();
        }
    }

    public bool IsEditing
    {
        get => _isEditing;
        set => SetProperty(ref _isEditing, value);
    }

    public ICommand LoadDataCommand { get; }
    public ICommand AddProductCommand { get; }
    public ICommand EditProductCommand { get; }
    public ICommand DeleteProductCommand { get; }
    public ICommand SaveProductCommand { get; }
    public ICommand CancelEditCommand { get; }

    private async Task LoadDataAsync()
    {
        if (IsBusy)
            return;

        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var categories = await _productService.GetCategoriesAsync();
            Categories = new ObservableCollection<Category>(categories);

            var products = await _productService.GetProductsAsync();
            Products = new ObservableCollection<Product>(products);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading data: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void OnAddProduct()
    {
        SelectedProduct = new Product
        {
            Id = Guid.NewGuid(),
            Name = string.Empty,
            Description = string.Empty,
            SKU = string.Empty,
            Unit = "Liter",
            BasePrice = 0,
            MinOrderQuantity = 1,
            MaxOrderQuantity = 10,
            IsActive = true,
            CategoryId = Categories.FirstOrDefault()?.Id ?? Guid.Empty
        };
        IsEditing = true;
    }

    private void OnEditProduct(Product? product)
    {
        if (product == null)
            return;

        // Create a copy to edit
        SelectedProduct = new Product
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            SKU = product.SKU,
            Unit = product.Unit,
            BasePrice = product.BasePrice,
            MinOrderQuantity = product.MinOrderQuantity,
            MaxOrderQuantity = product.MaxOrderQuantity,
            IsActive = product.IsActive,
            CategoryId = product.CategoryId,
            ImageUrl = product.ImageUrl
        };
        IsEditing = true;
    }

    private async Task DeleteProductAsync(Product? product)
    {
        if (product == null)
            return;

        bool confirm = await Shell.Current.DisplayAlert(
            "Delete Product",
            $"Are you sure you want to delete '{product.Name}'?",
            "Delete",
            "Cancel");

        if (!confirm)
            return;

        IsBusy = true;
        try
        {
            // TODO: Implement delete API endpoint
            // await _productService.DeleteProductAsync(product.Id);
            
            Products.Remove(product);
            await Shell.Current.DisplayAlert("Success", "Product deleted successfully", "OK");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error deleting product: {ex.Message}";
            await Shell.Current.DisplayAlert("Error", ErrorMessage, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SaveProductAsync()
    {
        if (SelectedProduct == null)
            return;

        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            // TODO: Implement create/update API endpoints
            // if (Products.Any(p => p.Id == SelectedProduct.Id))
            //     await _productService.UpdateProductAsync(SelectedProduct);
            // else
            //     await _productService.CreateProductAsync(SelectedProduct);

            var existingProduct = Products.FirstOrDefault(p => p.Id == SelectedProduct.Id);
            if (existingProduct != null)
            {
                // Update existing
                var index = Products.IndexOf(existingProduct);
                Products[index] = SelectedProduct;
            }
            else
            {
                // Add new
                Products.Add(SelectedProduct);
            }

            await Shell.Current.DisplayAlert("Success", "Product saved successfully", "OK");
            IsEditing = false;
            SelectedProduct = null;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error saving product: {ex.Message}";
            await Shell.Current.DisplayAlert("Error", ErrorMessage, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void OnCancelEdit()
    {
        IsEditing = false;
        SelectedProduct = null;
        ErrorMessage = string.Empty;
    }

    private bool CanSaveProduct()
    {
        return SelectedProduct != null
            && !string.IsNullOrWhiteSpace(SelectedProduct.Name)
            && !string.IsNullOrWhiteSpace(SelectedProduct.SKU)
            && SelectedProduct.BasePrice > 0
            && SelectedProduct.CategoryId != Guid.Empty;
    }
}
