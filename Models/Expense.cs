namespace SmartSpendHome.Models;

public class Expense
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Date { get; set; } = DateTime.Today;
    public decimal Amount { get; set; }
    public string Category { get; set; } = "";
    public string PaymentMethod { get; set; } = "";
    public string Store { get; set; } = "";
    public string Note { get; set; } = "";
    public bool IsEssential { get; set; }
    public bool HasReceipt { get; set; }
    public bool IsFromShopping { get; set; }
    public List<Guid> ShoppingItemIds { get; set; } = new();
}
