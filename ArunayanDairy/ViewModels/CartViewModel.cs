using System.Collections.ObjectModel;
using System.Windows.Input;
using ArunayanDairy.Models;
using ArunayanDairy.Services;

namespace ArunayanDairy.ViewModels;

public class CartViewModel : BaseViewModel
{
    private readonly IOrderService _orderService;
    private readonly ProductListViewModel _productListViewModel;
    private DateTime _deliveryDate = DateTime.Today.AddDays(1);
    private string _notes = string.Empty;

    public ObservableCollection<CartItem> CartItems { get; }

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

    public DateTime MinimumDate => DateTime.Today.AddDays(1);
    public DateTime MaximumDate => DateTime.Today.AddDays(30);

    public decimal TotalAmount => CartItems.Sum(c => c.Subtotal);
    public int TotalItems => CartItems.Sum(c => (int)c.Quantity);

    public bool HasItems => CartItems.Any();

    public ICommand IncreaseQuantityCommand { get; }
    public ICommand DecreaseQuantityCommand { get; }
    public ICommand RemoveItemCommand { get; }
    public ICommand PlaceOrderCommand { get; }
    public ICommand ClearCartCommand { get; }

    public CartViewModel(IOrderService orderService, ProductListViewModel productListViewModel)
    {
        _orderService = orderService;
        _productListViewModel = productListViewModel;
        CartItems = productListViewModel.CartItems;
        Title = "Shopping Cart";

        IncreaseQuantityCommand = new Command<CartItem>(IncreaseQuantity);
        DecreaseQuantityCommand = new Command<CartItem>(DecreaseQuantity);
        RemoveItemCommand = new Command<CartItem>(RemoveItem);
        PlaceOrderCommand = new Command(async () => await PlaceOrderAsync(), () => HasItems);
        ClearCartCommand = new Command(ClearCart);

        CartItems.CollectionChanged += (s, e) => UpdateTotals();
    }

    private void IncreaseQuantity(CartItem item)
    {
        if (item?.Product == null)
            return;

        var newQuantity = item.Quantity + item.Product.MinOrderQuantity;
        if (!item.Product.MaxOrderQuantity.HasValue || newQuantity <= item.Product.MaxOrderQuantity)
        {
            item.Quantity = newQuantity;
            UpdateTotals();
        }
    }

    private void DecreaseQuantity(CartItem item)
    {
        if (item?.Product == null)
            return;

        var newQuantity = item.Quantity - item.Product.MinOrderQuantity;
        if (newQuantity >= item.Product.MinOrderQuantity)
        {
            item.Quantity = newQuantity;
            UpdateTotals();
        }
    }

    private void RemoveItem(CartItem item)
    {
        if (item == null)
            return;

        CartItems.Remove(item);
        UpdateTotals();
    }

    private void ClearCart()
    {
        CartItems.Clear();
        UpdateTotals();
    }

    private void UpdateTotals()
    {
        OnPropertyChanged(nameof(TotalAmount));
        OnPropertyChanged(nameof(TotalItems));
        OnPropertyChanged(nameof(HasItems));
        ((Command)PlaceOrderCommand).ChangeCanExecute();
    }

    private async Task PlaceOrderAsync()
    {
        if (IsBusy || !HasItems)
            return;

        try
        {
            IsBusy = true;
            ClearError();

            // Get vendorId from first cart item (all items should be from same vendor)
            var vendorId = CartItems.FirstOrDefault()?.Product?.VendorId ?? Guid.Empty;
            
            if (vendorId == Guid.Empty)
            {
                ErrorMessage = "Invalid vendor. Please try again.";
                return;
            }

            var request = new CreateOrderRequest
            {
                VendorId = vendorId,
                DeliveryDate = DeliveryDate,
                Notes = Notes,
                Items = CartItems.Select(c => new CreateOrderItemRequest
                {
                    ProductId = c.Product.Id,
                    Quantity = c.Quantity
                }).ToList()
            };

            var order = await _orderService.CreateOrderAsync(request);

            if (order != null)
            {
                ClearCart();
                if (Shell.Current?.CurrentPage != null)
                    await Shell.Current.CurrentPage.DisplayAlert("Success", 
                        $"Order {order.OrderNumber} placed successfully!\nDelivery Date: {order.DeliveryDate:MMM dd, yyyy}", 
                        "OK");
                await Shell.Current.GoToAsync("//Orders");
            }
            else
            {
                ErrorMessage = "Failed to place order. Please try again.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error placing order: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
