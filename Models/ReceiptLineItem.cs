namespace SmartSpendHome.Models;

public class ReceiptLineItem
{
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public bool IsMatchedToShoppingList { get; set; }
    public Guid? ShoppingItemId { get; set; }
}