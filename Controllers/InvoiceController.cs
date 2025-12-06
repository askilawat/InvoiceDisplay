using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BuggyApp.Data;
using BuggyApp.Models;

namespace BuggyApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvoiceController : ControllerBase
    {
        private readonly InvoiceDbContext _context;

        public InvoiceController(InvoiceDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetAllInvoices()
        {
            var invoices = _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Items)
                    .ThenInclude(ii => ii.Item)
                .OrderByDescending(i => i.InvoiceID)
                .ToList();

            var result = invoices.Select(inv => new
            {
                invoiceId = inv.InvoiceID,
                customerName = inv.Customer != null ? inv.Customer.Name : "Unknown",
                date = inv.Date.ToString("yyyy-MM-dd HH:mm"),
                total = (double)inv.Total
            }).ToList();

            return Ok(result);
        }

        [HttpGet("{id}")]
        public IActionResult GetInvoice(int id)
        {
            var invoice = _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Items)
                    .ThenInclude(ii => ii.Item)
                .FirstOrDefault(i => i.InvoiceID == id);

            if (invoice == null)
            {
                return NotFound("Invoice not found");
            }

            var items = invoice.Items.Select(invItem => new
            {
                itemId = invItem.Item?.ItemID ?? 0,
                name = invItem.Item?.Name ?? "Unknown",
                price = (double)(invItem.Item?.Price ?? 0),
                quantity = invItem.Quantity
            }).ToList();

            var result = new
            {
                invoiceId = invoice.InvoiceID,
                customer = invoice.Customer != null ? new
                {
                    id = invoice.Customer.CustomerID,
                    name = invoice.Customer.Name,
                    contact = invoice.Customer.Contact,
                    address = invoice.Customer.Address
                } : null,
                date = invoice.Date.ToString("yyyy-MM-dd HH:mm"),
                items = items,
                total = (double)invoice.Total
            };

            return Ok(result);
        }

        [HttpPost]
        public IActionResult CreateInvoice([FromBody] CreateInvoiceRequest request)
        {
            Customer? customer;
            
            if (request.CustomerId > 0)
            {
                customer = _context.Customers.Find(request.CustomerId);
                if (customer == null)
                {
                    return NotFound("Customer not found");
                }
            }
            else
            {
                var maxCustId = _context.Customers.Any() ? _context.Customers.Max(c => c.CustomerID) : 0;
                customer = new Customer
                {
                    CustomerID = maxCustId + 1,
                    Name = request.CustomerName,
                    Contact = request.CustomerContact ?? "",
                    Address = request.CustomerAddress ?? ""
                };
                _context.Customers.Add(customer);
                _context.SaveChanges();
            }

            var maxInvId = _context.Invoices.Any() ? _context.Invoices.Max(i => i.InvoiceID) : 0;
            var newInvoice = new Invoice
            {
                InvoiceID = maxInvId + 1,
                CustomerID = customer.CustomerID,
                Date = DateTime.Now,
                Total = 0
            };

            _context.Invoices.Add(newInvoice);
            _context.SaveChanges();

            decimal total = 0;
            if (request.Items != null && request.Items.Count > 0)
            {
                var maxInvItemId = _context.InvoiceItems.Any() ? _context.InvoiceItems.Max(ii => ii.InvoiceItemID) : 0;
                
                foreach (var invoiceItemData in request.Items)
                {
                    var item = _context.Items.Find(invoiceItemData.ItemId);
                    if (item != null)
                    {
                        maxInvItemId++;
                        var quantity = invoiceItemData.Quantity > 0 ? invoiceItemData.Quantity : 1;
                        var invoiceItem = new InvoiceItem
                        {
                            InvoiceItemID = maxInvItemId,
                            InvoiceID = newInvoice.InvoiceID,
                            ItemID = invoiceItemData.ItemId,
                            Quantity = quantity
                        };
                        _context.InvoiceItems.Add(invoiceItem);
                        total += item.Price * quantity;
                    }
                }
                
                newInvoice.Total = total;
                _context.SaveChanges();
            }

            return Ok(new { invoiceId = newInvoice.InvoiceID, message = "Invoice created" });
        }

        [HttpPut("{id}")]
        public IActionResult UpdateInvoice(int id, [FromBody] UpdateInvoiceRequest request)
        {
            var invoice = _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Items)
                .FirstOrDefault(i => i.InvoiceID == id);
            
            if (invoice == null)
            {
                return NotFound("Invoice not found");
            }

            if (invoice.Customer != null)
            {
                invoice.Customer.Name = request.CustomerName;
                invoice.Customer.Contact = request.CustomerContact ?? "";
                invoice.Customer.Address = request.CustomerAddress ?? "";
            }

            if (request.Items != null && request.Items.Count > 0)
            {
                _context.InvoiceItems.RemoveRange(invoice.Items);
                _context.SaveChanges();
                
                var maxInvItemId = _context.InvoiceItems.Any() ? _context.InvoiceItems.Max(ii => ii.InvoiceItemID) : 0;
                decimal total = 0;
                
                foreach (var invoiceItemData in request.Items)
                {
                    var item = _context.Items.Find(invoiceItemData.ItemId);
                    if (item != null)
                    {
                        maxInvItemId++;
                        var quantity = invoiceItemData.Quantity > 0 ? invoiceItemData.Quantity : 1;
                        var invoiceItem = new InvoiceItem
                        {
                            InvoiceItemID = maxInvItemId,
                            InvoiceID = invoice.InvoiceID,
                            ItemID = invoiceItemData.ItemId,
                            Quantity = quantity
                        };
                        _context.InvoiceItems.Add(invoiceItem);
                        total += item.Price * quantity;
                    }
                }
                
                invoice.Total = total;
            }

            _context.SaveChanges();
            return Ok(new { message = "Invoice updated" });
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteInvoice(int id)
        {
            var invoice = _context.Invoices
                .Include(i => i.Items)
                .FirstOrDefault(i => i.InvoiceID == id);
            
            if (invoice == null)
            {
                return NotFound("Invoice not found");
            }

            _context.InvoiceItems.RemoveRange(invoice.Items);
            _context.Invoices.Remove(invoice);
            _context.SaveChanges();

            return Ok(new { message = "Invoice deleted" });
        }

        [HttpDelete("{id}/items/{invoiceItemId}")]
        public IActionResult DeleteInvoiceItem(int id, int invoiceItemId)
        {
            var invoice = _context.Invoices
                .Include(i => i.Items)
                    .ThenInclude(ii => ii.Item)
                .FirstOrDefault(i => i.InvoiceID == id);
            
            var invoiceItem = _context.InvoiceItems
                .FirstOrDefault(ii => ii.InvoiceItemID == invoiceItemId && ii.InvoiceID == id);
            
            if (invoiceItem == null)
            {
                return NotFound("Invoice item not found");
            }

            _context.InvoiceItems.Remove(invoiceItem);
            
            if (invoice != null)
            {
                invoice.Total = invoice.Items
                    .Where(ii => ii.InvoiceItemID != invoiceItemId)
                    .Sum(ii => (ii.Item?.Price ?? 0) * ii.Quantity);
            }
            
            _context.SaveChanges();
            return Ok(new { message = "Item removed from invoice" });
        }

        [HttpGet("customers")]
        public IActionResult GetAllCustomers()
        {
            var customers = _context.Customers.OrderBy(c => c.Name).ToList();
            var result = customers.Select(c => new
            {
                id = c.CustomerID,
                name = c.Name,
                contact = c.Contact,
                address = c.Address
            }).ToList();

            return Ok(result);
        }

        public class CreateInvoiceRequest
        {
            public int CustomerId { get; set; }
            public string CustomerName { get; set; } = "";
            public string? CustomerContact { get; set; }
            public string? CustomerAddress { get; set; }
            public List<InvoiceItemData>? Items { get; set; }
        }

        public class InvoiceItemData
        {
            public int ItemId { get; set; }
            public int Quantity { get; set; } = 1;
        }

        public class UpdateInvoiceRequest
        {
            public string CustomerName { get; set; } = "";
            public string? CustomerContact { get; set; }
            public string? CustomerAddress { get; set; }
            public List<InvoiceItemData>? Items { get; set; }
        }
    }
}
