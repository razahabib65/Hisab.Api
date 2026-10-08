
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


        // =========================================
        // GET: api/Dashboard
        // =========================================

        [HttpGet]
        public async Task<IActionResult> GetDashboard()
        {
            // Get BusinessId from JWT
            var businessIdClaim =
                User.FindFirst("BusinessId")?.Value;

            if (string.IsNullOrWhiteSpace(businessIdClaim))
            {
                return Unauthorized(new
                {
                    message = "Business ID not found in token."
                });
            }

            if (!Guid.TryParse(
                businessIdClaim,
                out var businessId))
            {
                return Unauthorized(new
                {
                    message = "Invalid Business ID."
                });
            }


            // =========================================
            // TOTAL CUSTOMERS
            // =========================================

            var totalCustomers =
                await _context.Customers
                    .CountAsync(c =>
                        c.BusinessId == businessId);


            // =========================================
            // CUSTOMER IDS
            // =========================================

            var customerIds =
                await _context.Customers
                    .Where(c =>
                        c.BusinessId == businessId)
                    .Select(c => c.Id)
                    .ToListAsync();


            // =========================================
            // ALL TRANSACTIONS
            // =========================================

            var transactions =
                await _context.Transactions
                    .Where(t =>
                        customerIds.Contains(
                            t.CustomerId))
                    .ToListAsync();


            // =========================================
            // TOTAL UDHAAR
            // =========================================

            var totalUdhaar =
                transactions
                    .Where(t =>
                        t.Type == "Udhaar")
                    .Sum(t => t.Amount);


            // =========================================
            // TOTAL RECEIVED
            // =========================================

            var totalReceived =
                transactions
                    .Where(t =>
                        t.Type == "Payment")
                    .Sum(t => t.Amount);


            // =========================================
            // TOTAL OUTSTANDING
            // =========================================

            var totalOutstanding =
                totalUdhaar - totalReceived;


            // =========================================
            // RECENT TRANSACTIONS
            // =========================================

            var recentTransactions =
                await _context.Transactions
                    .Where(t =>
                        customerIds.Contains(
                            t.CustomerId))
                    .OrderByDescending(t => t.Date)
                    .Take(10)
                    .Select(t => new
                    {
                        id = t.Id,

                        customerId =
                            t.CustomerId,

                        customerName =
                            t.Customer != null
                                ? t.Customer.Name
                                : "Unknown Customer",

                        amount =
                            t.Amount,

                        type =
                            t.Type,

                        description =
                            t.Description,

                        date =
                            t.Date
                    })
                    .ToListAsync();


            // =========================================
            // FINAL RESPONSE
            // =========================================

            return Ok(new
            {
                totalCustomers,

                totalUdhaar,

                totalReceived,

                totalOutstanding,

                recentTransactions
            });
        }
    }
}

