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
		builder.Services.AddSingleton<IProductService, ProductService>();
		builder.Services.AddSingleton<IOrderService, OrderService>();

        // Register ViewModels
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<SignupViewModel>();
        builder.Services.AddSingleton<ProductListViewModel>();
        builder.Services.AddTransient<ProductDetailViewModel>();
        builder.Services.AddSingleton<CartViewModel>();
        builder.Services.AddTransient<OrderHistoryViewModel>();
        builder.Services.AddTransient<OrderDetailViewModel>();
        builder.Services.AddTransient<AdminDashboardViewModel>();
        builder.Services.AddTransient<AdminOrderDetailViewModel>();

        // Register Views
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<SignupPage>();
        builder.Services.AddTransient<ProductListPage>();
        builder.Services.AddTransient<ProductDetailPage>();
        builder.Services.AddTransient<CartPage>();
        builder.Services.AddTransient<OrderHistoryPage>();
        builder.Services.AddTransient<OrderDetailPage>();
        builder.Services.AddTransient<AdminDashboardPage>();
        builder.Services.AddTransient<AdminOrderDetailPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
