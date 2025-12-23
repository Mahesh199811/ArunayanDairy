using ArunayanDairy.ViewModels;

namespace ArunayanDairy.Views;

public partial class CartPage : ContentPage
{
    private readonly CartViewModel _viewModel;

    public CartPage(CartViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }
}
