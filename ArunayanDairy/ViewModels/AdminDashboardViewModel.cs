using System.Collections.ObjectModel;
using System.Windows.Input;
using ArunayanDairy.Models;
using ArunayanDairy.Services;

namespace ArunayanDairy.ViewModels;

public class AdminDashboardViewModel : BaseViewModel
{
    private readonly IOrderService _orderService;
    private int _totalOrders;
    private int _pendingOrders;
    private int _confirmedOrders;
    private decimal _todayRevenue;
    private string _selectedStatusFilter = "All";

    public ObservableCollection<Order> RecentOrders { get; } = new();
    public ObservableCollection<Order> FilteredOrders { get; } = new();
    public ObservableCollection<string> StatusFilters { get; } = new()
    {
        "All", "Pending", "Confirmed", "Processing", "OutForDelivery", "Delivered", "Cancelled"
    };

    public int TotalOrders
    {
        get => _totalOrders;
        set => SetProperty(ref _totalOrders, value);
    }

    public int PendingOrders
    {
        get => _pendingOrders;
        set => SetProperty(ref _pendingOrders, value);
    }

    public int ConfirmedOrders
    {
        get => _confirmedOrders;
        set => SetProperty(ref _confirmedOrders, value);
    }

    public decimal TodayRevenue
    {
        get => _todayRevenue;
        set => SetProperty(ref _todayRevenue, value);
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
    public ICommand UpdateOrderStatusCommand { get; }
    public ICommand ManageProductsCommand { get; }
    public ICommand ManageCategoriesCommand { get; }

    public AdminDashboardViewModel(IOrderService orderService)
    {
        _orderService = orderService;
        Title = "Admin Dashboard";

        LoadCommand = new Command(async () => await LoadDashboardDataAsync());
        RefreshCommand = new Command(async () => await LoadDashboardDataAsync());
        OrderTappedCommand = new Command<Order>(async (order) => await OnOrderTapped(order));
        UpdateOrderStatusCommand = new Command<Order>(async (order) => await UpdateOrderStatus(order));
        ManageProductsCommand = new Command(async () => await ManageProducts());
        ManageCategoriesCommand = new Command(async () => await ManageCategories());
    }

    public async Task InitializeAsync()
    {
        await LoadDashboardDataAsync();
    }

    private async Task LoadDashboardDataAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            ClearError();

            // Get all orders for admin
            var orders = await _orderService.GetMyOrdersAsync();
            
            RecentOrders.Clear();
            foreach (var order in orders.OrderByDescending(o => o.OrderDate).Take(20))
            {
                RecentOrders.Add(order);
            }

            // Calculate statistics
            TotalOrders = orders.Count;
            PendingOrders = orders.Count(o => o.Status == "Pending");
            ConfirmedOrders = orders.Count(o => o.Status == "Confirmed");
            TodayRevenue = orders
                .Where(o => o.OrderDate.Date == DateTime.Today)
                .Sum(o => o.TotalAmount);

            FilterOrders();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading dashboard: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void FilterOrders()
    {
        FilteredOrders.Clear();

        var filtered = SelectedStatusFilter == "All"
            ? RecentOrders
            : RecentOrders.Where(o => o.Status == SelectedStatusFilter);

        foreach (var order in filtered)
        {
            FilteredOrders.Add(order);
        }
    }

    private async Task OnOrderTapped(Order order)
    {
        if (order == null)
            return;

        await Shell.Current.GoToAsync($"AdminOrderDetail?orderId={order.Id}");
    }

    private async Task UpdateOrderStatus(Order order)
    {
        if (order == null)
            return;

        if (Shell.Current?.CurrentPage == null)
            return;

        var statusOptions = new[] { "Pending", "Confirmed", "Processing", "OutForDelivery", "Delivered", "Cancelled" };
        var result = await Shell.Current.CurrentPage.DisplayActionSheet(
            "Update Order Status",
            "Cancel",
            null,
            statusOptions);

        if (string.IsNullOrEmpty(result) || result == "Cancel")
            return;

        // TODO: Implement UpdateOrderStatus in OrderService
        await LoadDashboardDataAsync();
    }

    private async Task ManageProducts()
    {
        await Shell.Current.GoToAsync("ManageProducts");
    }

    private async Task ManageCategories()
    {
        await Shell.Current.GoToAsync("ManageCategories");
    }
}
