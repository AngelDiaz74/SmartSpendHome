using Blazored.LocalStorage;
using SmartSpendHome.Models;

namespace SmartSpendHome.Services;

public class IncomeService
{
    private const string StorageKey = "incomes";
    private readonly ILocalStorageService _localStorage;

    public IncomeService(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public async Task<List<Income>> GetAllAsync()
    {
        return await _localStorage.GetItemAsync<List<Income>>(StorageKey) ?? new List<Income>();
    }

    public async Task AddAsync(Income income)
    {
        var incomes = await GetAllAsync();
        incomes.Add(income);
        await _localStorage.SetItemAsync(StorageKey, incomes);
    }

    public async Task DeleteAsync(Guid id)
    {
        var incomes = await GetAllAsync();
        incomes = incomes.Where(x => x.Id != id).ToList();
        await _localStorage.SetItemAsync(StorageKey, incomes);
    }
}