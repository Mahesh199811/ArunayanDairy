using Microsoft.Extensions.Logging;
using ArunayanDairy.Services;
using ArunayanDairy.ViewModels;
using ArunayanDairy.Views;
using ArunayanDairy.Helpers;

namespace ArunayanDairy;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// Register Services
		builder.Services.AddSingleton<SecureStorageHelper>();
		builder.Services.AddSingleton<HttpClient>();
		builder.Services.AddSingleton<IAuthService, AuthService>();

		// Register ViewModels
		builder.Services.AddTransient<LoginViewModel>();
		builder.Services.AddTransient<SignupViewModel>();

		// Register Views
		builder.Services.AddTransient<LoginPage>();
		builder.Services.AddTransient<SignupPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
