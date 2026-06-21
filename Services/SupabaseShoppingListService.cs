using SmartSpendHome.Models;
using System.Text.Json.Serialization;

namespace SmartSpendHome.Services;

public class SupabaseShoppingListService : IShoppingListService
{
    private readonly ISupabaseApiService _supabaseApi;
    private readonly ISupabaseAuthService _authService;

    public SupabaseShoppingListService(
        ISupabaseApiService supabaseApi,
        ISupabaseAuthService authService)
    {
        _supabaseApi = supabaseApi;
        _authService = authService;
    }

    public async Task<List<ShoppingProduct>> GetPendingProductsAsync()
    {
        return await _supabaseApi.GetListAsync<ShoppingProduct>(
            "shopping_products?select=*&is_pending=eq.true&order=needed_since.asc");
    }

    public async Task<List<ShoppingProduct>> GetAllProductsAsync()
    {
        return await _supabaseApi.GetListAsync<ShoppingProduct>(
            "shopping_products?select=*&order=name.asc");
    }

    public async Task AddProductAsync(
        string name,
        string? defaultQuantity = null,
        string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return;

        var cleanName = name.Trim();
        var userId = await _authService.GetUserIdAsync();

        if (userId is null || userId == Guid.Empty)
            throw new InvalidOperationException("You are not logged in. Please login first.");

        var allProducts = await GetAllProductsAsync();

        var existing = allProducts.FirstOrDefault(x =>
            x.Name.Equals(cleanName, StringComparison.OrdinalIgnoreCase));

        if (existing is not null)
        {
            await MarkAsNeededAsync(existing.Id);
            return;
        }

        var insert = new ShoppingProductInsert
        {
            UserId = userId.Value,
            Name = cleanName,
            Notes = notes,
            DefaultQuantity = defaultQuantity,
            IsPending = true,
            NeededSince = DateTime.UtcNow
        };

        await _supabaseApi.InsertAsync<ShoppingProductInsert, ShoppingProduct>(
            "shopping_products",
            insert);
    }

    public async Task MarkAsPurchasedAsync(Guid productId)
    {
        var update = new ShoppingProductUpdate
        {
            IsPending = false,
            LastPurchasedAt = DateTime.UtcNow
        };

        await _supabaseApi.UpdateAsync(
            $"shopping_products?id=eq.{productId}",
            update);
    }

    public async Task MarkAsNeededAsync(Guid productId)
    {
        var update = new ShoppingProductUpdate
        {
            IsPending = true,
            NeededSince = DateTime.UtcNow
        };

        await _supabaseApi.UpdateAsync(
            $"shopping_products?id=eq.{productId}",
            update);
    }

    public async Task DeleteProductAsync(Guid productId)
    {
        await _supabaseApi.DeleteAsync(
            $"shopping_products?id=eq.{productId}");
    }

    private class ShoppingProductInsert
    {
        [JsonPropertyName("user_id")]
        public Guid UserId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("notes")]
        public string? Notes { get; set; }

        [JsonPropertyName("default_quantity")]
        public string? DefaultQuantity { get; set; }

        [JsonPropertyName("is_pending")]
        public bool IsPending { get; set; }

        [JsonPropertyName("needed_since")]
        public DateTime? NeededSince { get; set; }
    }

    private class ShoppingProductUpdate
    {
        [JsonPropertyName("is_pending")]
        public bool? IsPending { get; set; }

        [JsonPropertyName("needed_since")]
        public DateTime? NeededSince { get; set; }

        [JsonPropertyName("last_purchased_at")]
        public DateTime? LastPurchasedAt { get; set; }
    }
}
