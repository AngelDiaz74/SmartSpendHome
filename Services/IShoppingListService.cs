using SmartSpendHome.Models;

namespace SmartSpendHome.Services;

public interface IShoppingListService
{
    Task<List<ShoppingProduct>> GetPendingProductsAsync();
    Task<List<ShoppingProduct>> GetAllProductsAsync();

    Task AddProductAsync(string name, string? defaultQuantity = null, string? notes = null);
    Task MarkAsPurchasedAsync(Guid productId);
    Task MarkAsNeededAsync(Guid productId);
    Task DeleteProductAsync(Guid productId);
}
