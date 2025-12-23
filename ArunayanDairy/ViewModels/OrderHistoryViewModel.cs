using System.Collections.ObjectModel;
using System.Windows.Input;
using ArunayanDairy.Models;
using ArunayanDairy.Services;

namespace ArunayanDairy.ViewModels;

public class OrderHistoryViewModel : BaseViewModel
{
    private readonly IOrderService _orderService;
    private string _searchText = string.Empty;
    private string _selectedStatusFilter = "All";

    public ObservableCollection<Order> Orders { get; } = new();
    public ObservableCollection<Order> FilteredOrders { get; } = new();
    public ObservableCollection<string> StatusFilters { get; } = new()
    {
        "All", "Pending", "Confirmed", "Processing", "OutForDelivery", "Delivered", "Cancelled"
    };

    public string SearchText
    {
        get => _searchText;
        set
        {
            SetProperty(ref _searchText, value);
            FilterOrders();
        }
    }

    public string SelectedStatusFilter
    {
        get => _selectedStatusFilter;
        set
        {
            SetProperty(ref _selectedStatusFilter, value);
            FilterOrders();
        }
    }

    public ICommand LoadCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand OrderTappedCommand { get; }
    public ICommand CancelOrderCommand { get; }

    public OrderHistoryViewModel(IOrderService orderService)
    {
        _orderService = orderService;
        Title = "Order History";

        LoadCommand = new Command(async () => await LoadOrdersAsync());
        RefreshCommand = new Command(async () => await LoadOrdersAsync());
        OrderTappedCommand = new Command<Order>(async (order) => await OnOrderTapped(order));
        CancelOrderCommand = new Command<Order>(async (order) => await CancelOrder(order));
    }

    public async Task InitializeAsync()
    {
        await LoadOrdersAsync();
    }

    private async Task LoadOrdersAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            ClearError();

            var orders = await _orderService.GetMyOrdersAsync();
            
            Orders.Clear();
            foreach (var order in orders.OrderByDescending(o => o.OrderDate))
            {
                Orders.Add(order);
            }

            FilterOrders();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading orders: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void FilterOrders()
    {
        FilteredOrders.Clear();

        var filtered = Orders.AsEnumerable();

        // Filter by status
        if (SelectedStatusFilter != "All")
        {
            filtered = filtered.Where(o => o.Status == SelectedStatusFilter);
        }

        // Filter by search text
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            filtered = filtered.Where(o =>
                o.OrderNumber.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                o.Items.Any(i => i.ProductName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)));
        }

        foreach (var order in filtered)
        {
            FilteredOrders.Add(order);
        }
    }

    private async Task OnOrderTapped(Order order)
    {
        if (order == null)
            return;

        await Shell.Current.GoToAsync($"OrderDetail?orderId={order.Id}");
    }

    private async Task CancelOrder(Order order)
    {
        if (order == null || order.Status == "Cancelled" || order.Status == "Delivered")
            return;

        if (Shell.Current?.CurrentPage == null)
            return;

        var confirm = await Shell.Current.CurrentPage.DisplayAlert(
            "Cancel Order",
            $"Are you sure you want to cancel order {order.OrderNumber}?",
            "Yes", "No");

        if (!confirm)
            return;

        try
        {
            IsBusy = true;
            var success = await _orderService.CancelOrderAsync(order.Id);

            if (success)
            {
                if (Shell.Current?.CurrentPage != null)
                    await Shell.Current.CurrentPage.DisplayAlert("Success", "Order cancelled successfully", "OK");
                await LoadOrdersAsync();
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
