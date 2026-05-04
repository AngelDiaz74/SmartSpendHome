namespace SmartSpendHome.Models;

public class Income
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Date { get; set; } = DateTime.Today;
    public decimal Amount { get; set; }
    public string Source { get; set; } = "";
    public string Note { get; set; } = "";
}
