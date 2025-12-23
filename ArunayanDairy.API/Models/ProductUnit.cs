namespace ArunayanDairy.API.Models;

/// <summary>
/// Units of measurement for dairy products
/// </summary>
public enum ProductUnit
{
    Liter,      // For milk, buttermilk, etc.
    Kilogram,   // For paneer, butter, etc.
    Piece,      // For individual items
    Milliliter, // For small quantities
    Gram        // For small quantities
}
