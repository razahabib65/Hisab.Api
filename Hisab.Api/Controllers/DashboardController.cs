using Hisab.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hisab.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  [Authorize]
  public class DashboardController : ControllerBase
  {
    private readonly AppDbContext _context;

    public DashboardController(AppDbContext context)
    {
      _context = context;
    }

    // GET: api/dashboard
    [HttpGet]
    public async Task<IActionResult> GetDashboard()
    {
      var businessId = Guid.Parse(
          User.FindFirst("BusinessId")!.Value
      );

      // Total customers of current business
      var totalCustomers = await _context.Customers
          .CountAsync(c => c.BusinessId == businessId);

      // Get customer IDs of current business
      var customerIds = await _context.Customers
          .Where(c => c.BusinessId == businessId)
          .Select(c => c.Id)
          .ToListAsync();

      // Get transactions of current business customers
      var transactions = await _context.Transactions
          .Where(t => customerIds.Contains(t.CustomerId))
          .ToListAsync();

      var totalUdhaar = transactions
          .Where(t => t.Type == "Udhaar")
          .Sum(t => t.Amount);

      var totalReceived = transactions
          .Where(t => t.Type == "Payment")
          .Sum(t => t.Amount);

      var totalOutstanding = totalUdhaar - totalReceived;

      return Ok(new
      {
        totalCustomers,
        totalUdhaar,
        totalReceived,
        totalOutstanding
      });
    }
  }
}
