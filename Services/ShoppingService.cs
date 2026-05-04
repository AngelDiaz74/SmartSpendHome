using Blazored.LocalStorage;
using SmartSpendHome.Models;

namespace SmartSpendHome.Services;

public class ShoppingService
{
    private const string StorageKey = "shopping_items";
    private readonly ILocalStorageService _localStorage;

    public ShoppingService(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public async Task<List<ShoppingItem>> GetAllAsync()
    {
        return await _localStorage.GetItemAsync<List<ShoppingItem>>(StorageKey) ?? new List<ShoppingItem>();
    }

    public async Task SaveAllAsync(List<ShoppingItem> items)
    {
        await _localStorage.SetItemAsync(StorageKey, items);
    }

    public async Task AddAsync(ShoppingItem item)
    {
        var items = await GetAllAsync();
        items.Add(item);
        await SaveAllAsync(items);
    }

    public async Task DeleteAsync(Guid id)
    {
        var items = await GetAllAsync();
        items = items.Where(x => x.Id != id).ToList();
        await SaveAllAsync(items);
    }
}
