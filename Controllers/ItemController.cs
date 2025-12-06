using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BuggyApp.Data;
using BuggyApp.Models;

namespace BuggyApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ItemController : ControllerBase
    {
        private readonly InvoiceDbContext _context;

        public ItemController(InvoiceDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetAllItems()
        {
            var items = _context.Items.OrderBy(i => i.Name).ToList();
            var result = items.Select(item => new
            {
                itemId = item.ItemID,
                name = item.Name,
                price = (double)item.Price
            }).ToList();

            return Ok(result);
        }

        [HttpPost]
        public IActionResult CreateItem([FromBody] CreateItemRequest request)
        {
            var maxId = _context.Items.Any() ? _context.Items.Max(i => i.ItemID) : 0;
            var newItem = new Item
            {
                ItemID = maxId + 1,
                Name = request.Name,
                Price = request.Price
            };

            _context.Items.Add(newItem);
            _context.SaveChanges();

            return Ok(new { itemId = newItem.ItemID, message = "Item created" });
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteItem(int id)
        {
            var item = _context.Items.Find(id);
            if (item == null)
            {
                return NotFound("Item not found");
            }

            _context.Items.Remove(item);
            _context.SaveChanges();

            return Ok(new { message = "Item deleted" });
        }

        public class CreateItemRequest
        {
            public string Name { get; set; } = "";
            public decimal Price { get; set; }
        }
    }
}

