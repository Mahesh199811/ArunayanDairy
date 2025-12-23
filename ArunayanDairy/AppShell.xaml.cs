using ArunayanDairy.Views;

namespace ArunayanDairy;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		// Register routes for navigation
		Routing.RegisterRoute("ProductDetail", typeof(ProductDetailPage));
		Routing.RegisterRoute("OrderDetail", typeof(OrderDetailPage));
		Routing.RegisterRoute("Cart", typeof(CartPage));
		Routing.RegisterRoute("AdminOrderDetail", typeof(AdminOrderDetailPage));
	}
}
