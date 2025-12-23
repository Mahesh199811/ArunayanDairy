using ArunayanDairy.ViewModels;

namespace ArunayanDairy.Views;

public partial class AdminOrderDetailPage : ContentPage
{
    private readonly AdminOrderDetailViewModel _viewModel;

    public AdminOrderDetailPage(AdminOrderDetailViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }
}
