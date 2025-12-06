namespace BuggyApp.Models;

public class Item
{
    public int ItemID { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public List<InvoiceItem> InvoiceItems { get; set; } = new();
}

