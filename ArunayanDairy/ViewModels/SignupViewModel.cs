using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ArunayanDairy.Models;
using ArunayanDairy.Services;

namespace ArunayanDairy.ViewModels;

/// <summary>
/// ViewModel for the Signup page with name, email, password validation.
/// </summary>
public class SignupViewModel : INotifyPropertyChanged
{
    private readonly IAuthService _authService;
    private string _name = string.Empty;
    private string _email = string.Empty;
    private string _password = string.Empty;
    private string _confirmPassword = string.Empty;
    private bool _isBusy;
    private string _errorMessage = string.Empty;

    public SignupViewModel(IAuthService authService)
    {
        _authService = authService;
        SignupCommand = new Command(async () => await OnSignupAsync(), () => CanSignup());
        NavigateToLoginCommand = new Command(OnNavigateToLogin);
    }

    public string Name
    {
        get => _name;
        set
        {
            if (_name != value)
            {
                _name = value;
                OnPropertyChanged();
                ((Command)SignupCommand).ChangeCanExecute();
            }
        }
    }

    public string Email
    {
        get => _email;
        set
        {
            if (_email != value)
            {
                _email = value;
                OnPropertyChanged();
                ((Command)SignupCommand).ChangeCanExecute();
            }
        }
    }

    public string Password
    {
        get => _password;
        set
        {
            if (_password != value)
            {
                _password = value;
                OnPropertyChanged();
                ((Command)SignupCommand).ChangeCanExecute();
            }
        }
    }

    public string ConfirmPassword
    {
        get => _confirmPassword;
        set
        {
            if (_confirmPassword != value)
            {
                _confirmPassword = value;
                OnPropertyChanged();
                ((Command)SignupCommand).ChangeCanExecute();
            }
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (_isBusy != value)
            {
                _isBusy = value;
                OnPropertyChanged();
                ((Command)SignupCommand).ChangeCanExecute();
            }
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            if (_errorMessage != value)
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }
    }

    public ICommand SignupCommand { get; }
    public ICommand NavigateToLoginCommand { get; }

    private bool CanSignup()
    {
        return !IsBusy 
            && !string.IsNullOrWhiteSpace(Name) 
            && !string.IsNullOrWhiteSpace(Email)
            && !string.IsNullOrWhiteSpace(Password)
            && !string.IsNullOrWhiteSpace(ConfirmPassword)
            && Password.Length >= 8
            && Password == ConfirmPassword;
    }

    private async Task OnSignupAsync()
    {
        if (IsBusy)
            return;

        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            // Validate password match
            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Passwords do not match";
                return;
            }

            // Validate password length
            if (Password.Length < 8)
            {
                ErrorMessage = "Password must be at least 8 characters";
                return;
            }

            // Validate name length
            if (Name.Length < 2)
            {
                ErrorMessage = "Name must be at least 2 characters";
                return;
            }

            var request = new SignupRequest
            {
                Name = Name.Trim(),
                Email = Email.Trim(),
                Password = Password
            };

            var response = await _authService.SignupAsync(request);

            if (response != null)
            {
                // Navigate to main page on successful signup
                await Shell.Current.GoToAsync("///MainPage");
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async void OnNavigateToLogin()
    {
        await Shell.Current.GoToAsync("//LoginPage");
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
