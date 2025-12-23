using ArunayanDairy.ViewModels;

namespace ArunayanDairy.Views;

public partial class ManageCategoriesPage : ContentPage
{
    public ManageCategoriesPage(ManageCategoriesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        if (BindingContext is ManageCategoriesViewModel viewModel)
        {
            viewModel.LoadCategoriesCommand.Execute(null);
        }
    }
}
