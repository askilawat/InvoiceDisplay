namespace BuggyApp.Models;

public class InvoiceItem
{
    public int InvoiceItemID { get; set; }
    public int InvoiceID { get; set; }
    public int ItemID { get; set; }
    public int Quantity { get; set; } = 1;
    public Invoice? Invoice { get; set; }
    public Item? Item { get; set; }
}