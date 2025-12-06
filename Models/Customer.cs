namespace BuggyApp.Models;

public class Customer
{
    public int CustomerID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Contact { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public List<Invoice> Invoices { get; set; } = new();
}

