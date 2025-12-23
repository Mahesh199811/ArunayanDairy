using ArunayanDairy.ViewModels;

namespace ArunayanDairy.Views;

public partial class ManageProductsPage : ContentPage
{
    public ManageProductsPage(ManageProductsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        if (BindingContext is ManageProductsViewModel viewModel)
        {
            viewModel.LoadDataCommand.Execute(null);
        }
    }
}
