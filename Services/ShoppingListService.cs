using Blazored.LocalStorage;
using SmartSpendHome.Models;

namespace SmartSpendHome.Services;

public class ShoppingListService : IShoppingListService
{
    private const string StorageKey = "smartspendhome_shopping_products";

    private readonly ILocalStorageService _localStorage;

    public ShoppingListService(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public async Task<List<ShoppingProduct>> GetPendingProductsAsync()
    {
        var products = await GetProductsFromStorageAsync();

        return products
            .Where(x => x.IsPending)
            .OrderBy(x => x.NeededSince)
            .ToList();
    }

    public async Task<List<ShoppingProduct>> GetAllProductsAsync()
    {
        var products = await GetProductsFromStorageAsync();

        return products
            .OrderBy(x => x.Name)
            .ToList();
    }

    public async Task AddProductAsync(string name, string? defaultQuantity = null, string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return;

        var products = await GetProductsFromStorageAsync();

        var cleanName = name.Trim();

        var existing = products.FirstOrDefault(x =>
            x.Name.Equals(cleanName, StringComparison.OrdinalIgnoreCase));

        if (existing is not null)
        {
            existing.IsPending = true;
            existing.NeededSince = DateTime.UtcNow;
            existing.UpdatedAt = DateTime.UtcNow;

            await SaveProductsToStorageAsync(products);
            return;
        }

        products.Add(new ShoppingProduct
        {
            Id = Guid.NewGuid(),
            Name = cleanName,
            DefaultQuantity = defaultQuantity,
            Notes = notes,
            IsPending = true,
            NeededSince = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        await SaveProductsToStorageAsync(products);
    }

    public async Task MarkAsPurchasedAsync(Guid productId)
    {
        var products = await GetProductsFromStorageAsync();

        var product = products.FirstOrDefault(x => x.Id == productId);

        if (product is not null)
        {
            product.IsPending = false;
            product.LastPurchasedAt = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;

            await SaveProductsToStorageAsync(products);
        }
    }

    public async Task MarkAsNeededAsync(Guid productId)
    {
        var products = await GetProductsFromStorageAsync();

        var product = products.FirstOrDefault(x => x.Id == productId);

        if (product is not null)
        {
            product.IsPending = true;
            product.NeededSince = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;

            await SaveProductsToStorageAsync(products);
        }
    }

    public async Task DeleteProductAsync(Guid productId)
    {
        var products = await GetProductsFromStorageAsync();

        var product = products.FirstOrDefault(x => x.Id == productId);

        if (product is not null)
        {
            products.Remove(product);
            await SaveProductsToStorageAsync(products);
        }
    }

    private async Task<List<ShoppingProduct>> GetProductsFromStorageAsync()
    {
        var products = await _localStorage.GetItemAsync<List<ShoppingProduct>>(StorageKey);

        return products ?? new List<ShoppingProduct>();
    }

    private async Task SaveProductsToStorageAsync(List<ShoppingProduct> products)
    {
        await _localStorage.SetItemAsync(StorageKey, products);
    }
}