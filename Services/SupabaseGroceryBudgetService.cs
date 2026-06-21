using SmartSpendHome.Models;
using System.Text.Json.Serialization;

namespace SmartSpendHome.Services;

public class SupabaseGroceryBudgetService : IGroceryBudgetService
{
    private readonly ISupabaseApiService _supabaseApi;
    private readonly ISupabaseAuthService _authService;

    public SupabaseGroceryBudgetService(
        ISupabaseApiService supabaseApi,
        ISupabaseAuthService authService)
    {
        _supabaseApi = supabaseApi;
        _authService = authService;
    }

    public async Task<GroceryBudgetSummary> GetSummaryAsync()
    {
        var budgetMonth = await GetOrCreateCurrentBudgetMonthAsync();
        var receipts = await GetReceiptsAsync();

        var totalSpent = receipts.Sum(x => x.TotalAmount);

        return new GroceryBudgetSummary
        {
            MonthlyBudget = budgetMonth.BudgetAmount,
            TotalSpent = totalSpent,
            ReceiptCount = receipts.Count
        };
    }

    public async Task<decimal> GetMonthlyBudgetAsync()
    {
        var budgetMonth = await GetOrCreateCurrentBudgetMonthAsync();

        return budgetMonth.BudgetAmount;
    }

    public async Task SetMonthlyBudgetAsync(decimal amount)
    {
        if (amount < 0)
            amount = 0;

        var userId = await GetCurrentUserIdAsync();
        var budgetMonth = await GetOrCreateCurrentBudgetMonthAsync();

        await _supabaseApi.UpdateAsync(
            $"grocery_budget_months?id=eq.{budgetMonth.Id}&user_id=eq.{userId}",
            new GroceryBudgetMonthUpdate
            {
                BudgetAmount = amount
            });

        // Verify that Supabase really updated the row.
        var updatedRows = await _supabaseApi.GetListAsync<GroceryBudgetMonth>(
            $"grocery_budget_months?select=*&id=eq.{budgetMonth.Id}&user_id=eq.{userId}&limit=1");

        var updatedMonth = updatedRows.FirstOrDefault();

        if (updatedMonth is null)
            throw new InvalidOperationException("Monthly budget row was not found after update.");

        if (updatedMonth.BudgetAmount != amount)
        {
            throw new InvalidOperationException(
                $"Monthly budget update failed. Expected {amount}, but Supabase returned {updatedMonth.BudgetAmount}.");
        }
    }

    public async Task<List<GroceryReceipt>> GetReceiptsAsync()
    {
        var budgetMonth = await GetOrCreateCurrentBudgetMonthAsync();

        var receipts = await _supabaseApi.GetListAsync<GroceryReceipt>(
            $"grocery_receipts?select=*&budget_month_id=eq.{budgetMonth.Id}&order=receipt_date.desc,created_at.desc");

        if (receipts.Count == 0)
            return receipts;

        var receiptIds = receipts.Select(x => x.Id).ToList();
        var idFilter = string.Join(",", receiptIds);

        var items = await _supabaseApi.GetListAsync<GroceryReceiptItem>(
            $"grocery_receipt_items?select=*&receipt_id=in.({idFilter})&order=created_at.asc");

        foreach (var receipt in receipts)
        {
            receipt.Items = items
                .Where(x => x.ReceiptId == receipt.Id)
                .OrderBy(x => x.CreatedAt)
                .ToList();
        }

        return receipts;
    }

    public async Task AddReceiptAsync(
        string storeName,
        decimal totalAmount,
        DateTime receiptDate,
        string? ocrText = null,
        List<ParsedReceiptItem>? detectedItems = null)
    {
        if (totalAmount <= 0)
            return;

        var userId = await GetCurrentUserIdAsync();
        var budgetMonth = await GetOrCreateCurrentBudgetMonthAsync();

        var receiptInsert = new GroceryReceiptInsert
        {
            UserId = userId,
            BudgetMonthId = budgetMonth.Id,
            StoreName = string.IsNullOrWhiteSpace(storeName)
                ? "Unknown Store"
                : storeName.Trim(),
            TotalAmount = totalAmount,
            ReceiptDate = receiptDate.Date,
            OcrText = ocrText
        };

        var insertedReceipts = await _supabaseApi.InsertAsync<GroceryReceiptInsert, GroceryReceipt>(
            "grocery_receipts",
            receiptInsert);

        var insertedReceipt = insertedReceipts.FirstOrDefault();

        if (insertedReceipt is null)
            throw new InvalidOperationException("Could not create receipt in Supabase.");

        var validItems = detectedItems?
            .Where(x => !string.IsNullOrWhiteSpace(x.ItemName))
            .Select(x => new GroceryReceiptItemInsert
            {
                ReceiptId = insertedReceipt.Id,
                ItemName = x.ItemName.Trim(),
                Price = x.Price
            })
            .ToList() ?? new List<GroceryReceiptItemInsert>();

        if (validItems.Count > 0)
        {
            await _supabaseApi.InsertAsync<List<GroceryReceiptItemInsert>, GroceryReceiptItem>(
                "grocery_receipt_items",
                validItems);
        }
    }

    public async Task DeleteReceiptAsync(Guid receiptId)
    {
        await _supabaseApi.DeleteAsync(
            $"grocery_receipts?id=eq.{receiptId}");
    }

    private async Task<GroceryBudgetMonth> GetOrCreateCurrentBudgetMonthAsync()
    {
        var userId = await GetCurrentUserIdAsync();
        var monthStart = GetCurrentMonthStart();
        var monthStartText = monthStart.ToString("yyyy-MM-dd");

        var existingMonths = await _supabaseApi.GetListAsync<GroceryBudgetMonth>(
            $"grocery_budget_months?select=*&user_id=eq.{userId}&month_start=eq.{monthStartText}&limit=1");

        var existingMonth = existingMonths.FirstOrDefault();

        if (existingMonth is not null)
            return existingMonth;

        var insertedMonths = await _supabaseApi.InsertAsync<GroceryBudgetMonthInsert, GroceryBudgetMonth>(
            "grocery_budget_months",
            new GroceryBudgetMonthInsert
            {
                UserId = userId,
                MonthStart = monthStart,
                BudgetAmount = 700
            });

        var insertedMonth = insertedMonths.FirstOrDefault();

        if (insertedMonth is null)
            throw new InvalidOperationException("Could not create the current grocery budget month.");

        return insertedMonth;
    }

    private async Task<Guid> GetCurrentUserIdAsync()
    {
        var userId = await _authService.GetUserIdAsync();

        if (userId is null || userId == Guid.Empty)
            throw new InvalidOperationException("You are not logged in. Please login first.");

        return userId.Value;
    }

    private DateTime GetCurrentMonthStart()
    {
        var today = DateTime.Today;

        return new DateTime(today.Year, today.Month, 1);
    }

    private class GroceryBudgetMonthInsert
    {
        [JsonPropertyName("user_id")]
        public Guid UserId { get; set; }

        [JsonPropertyName("month_start")]
        public DateTime MonthStart { get; set; }

        [JsonPropertyName("budget_amount")]
        public decimal BudgetAmount { get; set; }
    }

    private class GroceryBudgetMonthUpdate
    {
        [JsonPropertyName("budget_amount")]
        public decimal BudgetAmount { get; set; }
    }

    private class GroceryReceiptInsert
    {
        [JsonPropertyName("user_id")]
        public Guid UserId { get; set; }

        [JsonPropertyName("budget_month_id")]
        public Guid BudgetMonthId { get; set; }

        [JsonPropertyName("store_name")]
        public string StoreName { get; set; } = string.Empty;

        [JsonPropertyName("receipt_date")]
        public DateTime ReceiptDate { get; set; }

        [JsonPropertyName("total_amount")]
        public decimal TotalAmount { get; set; }

        [JsonPropertyName("ocr_text")]
        public string? OcrText { get; set; }
    }

    private class GroceryReceiptItemInsert
    {
        [JsonPropertyName("receipt_id")]
        public Guid ReceiptId { get; set; }

        [JsonPropertyName("item_name")]
        public string ItemName { get; set; } = string.Empty;

        [JsonPropertyName("quantity")]
        public decimal? Quantity { get; set; }

        [JsonPropertyName("price")]
        public decimal? Price { get; set; }
    }
}