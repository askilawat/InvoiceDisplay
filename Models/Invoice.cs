namespace BuggyApp.Models;

public class Invoice
{
    public int InvoiceID { get; set; }
    public int CustomerID { get; set; }
    public DateTime Date { get; set; }
    public decimal Total { get; set; }
    public Customer? Customer { get; set; }
    public List<InvoiceItem> Items { get; set; } = new();
}

