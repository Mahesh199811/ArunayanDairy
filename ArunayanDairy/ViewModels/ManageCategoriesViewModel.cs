using System.Collections.ObjectModel;
using System.Windows.Input;
using ArunayanDairy.Models;
using ArunayanDairy.Services;

namespace ArunayanDairy.ViewModels;

public class ManageCategoriesViewModel : BaseViewModel
{
    private readonly IProductService _productService;
    private ObservableCollection<Category> _categories = new();
    private Category? _selectedCategory;
    private bool _isEditing;

    public ManageCategoriesViewModel(IProductService productService)
    {
        _productService = productService;
        
        LoadCategoriesCommand = new Command(async () => await LoadCategoriesAsync());
        AddCategoryCommand = new Command(OnAddCategory);
        EditCategoryCommand = new Command<Category>(OnEditCategory);
        DeleteCategoryCommand = new Command<Category>(async (c) => await DeleteCategoryAsync(c));
        SaveCategoryCommand = new Command(async () => await SaveCategoryAsync(), () => CanSaveCategory());
        CancelEditCommand = new Command(OnCancelEdit);
    }

    public ObservableCollection<Category> Categories
    {
        get => _categories;
        set => SetProperty(ref _categories, value);
    }

    public Category? SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            SetProperty(ref _selectedCategory, value);
            ((Command)SaveCategoryCommand).ChangeCanExecute();
        }
    }

    public bool IsEditing
    {
        get => _isEditing;
        set => SetProperty(ref _isEditing, value);
    }

    public ICommand LoadCategoriesCommand { get; }
    public ICommand AddCategoryCommand { get; }
    public ICommand EditCategoryCommand { get; }
    public ICommand DeleteCategoryCommand { get; }
    public ICommand SaveCategoryCommand { get; }
    public ICommand CancelEditCommand { get; }

    private async Task LoadCategoriesAsync()
    {
        if (IsBusy)
            return;

        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var categories = await _productService.GetCategoriesAsync();
            Categories = new ObservableCollection<Category>(categories);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading categories: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void OnAddCategory()
    {
        SelectedCategory = new Category
        {
            Id = Guid.NewGuid(),
            Name = string.Empty,
            Description = string.Empty,
            DisplayOrder = Categories.Count + 1,
            IsActive = true
        };
        IsEditing = true;
    }

    private void OnEditCategory(Category? category)
    {
        if (category == null)
            return;

        // Create a copy to edit
        SelectedCategory = new Category
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            DisplayOrder = category.DisplayOrder,
            IsActive = category.IsActive
        };
        IsEditing = true;
    }

    private async Task DeleteCategoryAsync(Category? category)
    {
        if (category == null)
            return;

        bool confirm = await Shell.Current.DisplayAlert(
            "Delete Category",
            $"Are you sure you want to delete '{category.Name}'?",
            "Delete",
            "Cancel");

        if (!confirm)
            return;

        IsBusy = true;
        try
        {
            // TODO: Implement delete API endpoint
            // await _productService.DeleteCategoryAsync(category.Id);
            
            Categories.Remove(category);
            await Shell.Current.DisplayAlert("Success", "Category deleted successfully", "OK");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error deleting category: {ex.Message}";
            await Shell.Current.DisplayAlert("Error", ErrorMessage, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SaveCategoryAsync()
    {
        if (SelectedCategory == null)
            return;

        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            // TODO: Implement create/update API endpoints
            // if (Categories.Any(c => c.Id == SelectedCategory.Id))
            //     await _productService.UpdateCategoryAsync(SelectedCategory);
            // else
            //     await _productService.CreateCategoryAsync(SelectedCategory);

            var existingCategory = Categories.FirstOrDefault(c => c.Id == SelectedCategory.Id);
            if (existingCategory != null)
            {
                // Update existing
                var index = Categories.IndexOf(existingCategory);
                Categories[index] = SelectedCategory;
            }
            else
            {
                // Add new
                Categories.Add(SelectedCategory);
            }

            await Shell.Current.DisplayAlert("Success", "Category saved successfully", "OK");
            IsEditing = false;
            SelectedCategory = null;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error saving category: {ex.Message}";
            await Shell.Current.DisplayAlert("Error", ErrorMessage, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void OnCancelEdit()
    {
        IsEditing = false;
        SelectedCategory = null;
        ErrorMessage = string.Empty;
    }

    private bool CanSaveCategory()
    {
        return SelectedCategory != null
            && !string.IsNullOrWhiteSpace(SelectedCategory.Name);
    }
}
