using System.Windows.Input;
using ArunayanDairy.Models;
using ArunayanDairy.Services;

namespace ArunayanDairy.ViewModels;

[QueryProperty(nameof(OrderId), "orderId")]
public class AdminOrderDetailViewModel : BaseViewModel
{
    private readonly IOrderService _orderService;
    private Order? _order;

    public Order? Order
    {
        get => _order;
        set => SetProperty(ref _order, value);
    }

    public string OrderId { get; set; } = string.Empty;

    public ICommand LoadCommand { get; }
    public ICommand UpdateStatusCommand { get; }

    public AdminOrderDetailViewModel(IOrderService orderService)
    {
        _orderService = orderService;
        Title = "Order Management";

        LoadCommand = new Command(async () => await LoadOrderAsync());
        UpdateStatusCommand = new Command(async () => await UpdateOrderStatusAsync());
    }

    public async Task InitializeAsync()
    {
        await LoadOrderAsync();
    }

    private async Task LoadOrderAsync()
    {
        if (IsBusy || string.IsNullOrEmpty(OrderId))
            return;

        try
        {
            IsBusy = true;
            ClearError();

            if (Guid.TryParse(OrderId, out var id))
            {
                Order = await _orderService.GetOrderByIdAsync(id);
                
                if (Order != null)
                {
                    Title = $"Order {Order.OrderNumber}";
                }
                else
                {
                    ErrorMessage = "Order not found";
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading order: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task UpdateOrderStatusAsync()
    {
        if (Order == null || Shell.Current?.CurrentPage == null)
            return;

        var statusOptions = new[] { "Pending", "Confirmed", "Processing", "OutForDelivery", "Delivered", "Cancelled" };
        var result = await Shell.Current.CurrentPage.DisplayActionSheet(
            "Update Order Status",
            "Cancel",
            null,
            statusOptions);

        if (string.IsNullOrEmpty(result) || result == "Cancel")
            return;

        try
        {
            IsBusy = true;
            // TODO: Implement UpdateOrderStatus API call
            Order.Status = result;
            
            if (Shell.Current?.CurrentPage != null)
                await Shell.Current.CurrentPage.DisplayAlert("Success", 
                    $"Order status updated to {result}", "OK");
            
            await LoadOrderAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error updating status: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
