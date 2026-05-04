using Blazored.LocalStorage;
using SmartSpendHome.Models;

namespace SmartSpendHome.Services;

public class ExpenseService
{
    private const string StorageKey = "expenses";
    private readonly ILocalStorageService _localStorage;

    public ExpenseService(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public async Task<List<Expense>> GetAllAsync()
    {
        return await _localStorage.GetItemAsync<List<Expense>>(StorageKey) ?? new List<Expense>();
    }

    public async Task AddAsync(Expense expense)
    {
        var expenses = await GetAllAsync();
        expenses.Add(expense);
        await _localStorage.SetItemAsync(StorageKey, expenses);
    }

    public async Task DeleteAsync(Guid id)
    {
        var expenses = await GetAllAsync();
        expenses = expenses.Where(x => x.Id != id).ToList();
        await _localStorage.SetItemAsync(StorageKey, expenses);
    }
}