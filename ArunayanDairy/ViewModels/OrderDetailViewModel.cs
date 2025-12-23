using System.Windows.Input;
using ArunayanDairy.Models;
using ArunayanDairy.Services;

namespace ArunayanDairy.ViewModels;

[QueryProperty(nameof(OrderId), "orderId")]
public class OrderDetailViewModel : BaseViewModel
{
    private readonly IOrderService _orderService;
    private Order? _order;

    public Order? Order
    {
        get => _order;
        set => SetProperty(ref _order, value);
    }

    public string OrderId { get; set; } = string.Empty;

    public bool CanCancel => Order?.Status == "Pending" || Order?.Status == "Confirmed";

    public ICommand LoadCommand { get; }
    public ICommand CancelOrderCommand { get; }

    public OrderDetailViewModel(IOrderService orderService)
    {
        _orderService = orderService;
        Title = "Order Details";

        LoadCommand = new Command(async () => await LoadOrderAsync());
        CancelOrderCommand = new Command(async () => await CancelOrderAsync(), () => CanCancel);
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
                    OnPropertyChanged(nameof(CanCancel));
                    ((Command)CancelOrderCommand).ChangeCanExecute();
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

    private async Task CancelOrderAsync()
    {
        if (Order == null || !CanCancel)
            return;

        if (Shell.Current?.CurrentPage == null)
            return;

        var confirm = await Shell.Current.CurrentPage.DisplayAlert(
            "Cancel Order",
            $"Are you sure you want to cancel this order?",
            "Yes", "No");

        if (!confirm)
            return;

        try
        {
            IsBusy = true;
            var success = await _orderService.CancelOrderAsync(Order.Id);

            if (success)
            {
                if (Shell.Current?.CurrentPage != null)
                    await Shell.Current.CurrentPage.DisplayAlert("Success", "Order cancelled successfully", "OK");
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                ErrorMessage = "Failed to cancel order. Please try again.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error cancelling order: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
