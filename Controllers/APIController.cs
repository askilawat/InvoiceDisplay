using Microsoft.AspNetCore.Mvc;

namespace BuggyApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DataController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetData()
        {
            string result = "Data fetched successfully";
            if(result != null && result.Length > 0)
            {
                return Ok(new { message = result });
            }
            return BadRequest("No data");
        }
    }
}
