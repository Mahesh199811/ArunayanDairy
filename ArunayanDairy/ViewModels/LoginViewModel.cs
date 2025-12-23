using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ArunayanDairy.Models;
using ArunayanDairy.Services;

namespace ArunayanDairy.ViewModels;

/// <summary>
/// ViewModel for the Login page with email/password validation and authentication.
/// </summary>
public class LoginViewModel : INotifyPropertyChanged
{
    private readonly IAuthService _authService;
    private string _email = string.Empty;
    private string _password = string.Empty;
    private bool _isBusy;
    private string _errorMessage = string.Empty;

    public LoginViewModel(IAuthService authService)
    {
        _authService = authService;
        LoginCommand = new Command(async () => await OnLoginAsync(), () => CanLogin());
        NavigateToSignupCommand = new Command(OnNavigateToSignup);
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
                ((Command)LoginCommand).ChangeCanExecute();
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
                ((Command)LoginCommand).ChangeCanExecute();
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
                ((Command)LoginCommand).ChangeCanExecute();
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

    public ICommand LoginCommand { get; }
    public ICommand NavigateToSignupCommand { get; }

    private bool CanLogin()
    {
        return !IsBusy && !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password);
    }

    private async Task OnLoginAsync()
    {
        if (IsBusy)
            return;

        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var request = new LoginRequest
            {
                Email = Email.Trim(),
                Password = Password
            };

            var response = await _authService.LoginAsync(request);

            if (response != null)
            {
                // Navigate based on user role
                if (response.User.Role.Equals("admin", StringComparison.OrdinalIgnoreCase))
                {
                    await Shell.Current.GoToAsync("//AdminDashboard");
                }
                else
                {
                    await Shell.Current.GoToAsync("//Products");
                }
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

    private async void OnNavigateToSignup()
    {
        await Shell.Current.GoToAsync("//SignupPage");
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
