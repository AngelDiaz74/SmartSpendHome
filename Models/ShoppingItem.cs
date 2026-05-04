namespace SmartSpendHome.Models;

public class ShoppingItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public string PreferredStore { get; set; } = "";
    public decimal EstimatedPrice { get; set; }
    public bool NeedToBuy { get; set; }
    public bool Bought { get; set; }
}