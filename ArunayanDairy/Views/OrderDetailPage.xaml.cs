using ArunayanDairy.ViewModels;

namespace ArunayanDairy.Views;

public partial class OrderDetailPage : ContentPage
{
    private readonly OrderDetailViewModel _viewModel;

    public OrderDetailPage(OrderDetailViewModel viewModel)
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
