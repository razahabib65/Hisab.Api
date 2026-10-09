using Hisab.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hisab.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TransactionReportController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<TransactionReportController> _logger;


    public TransactionReportController(
        AppDbContext context,
        ILogger<TransactionReportController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetReport(
            [FromQuery] DateOnly fromDate,
            [FromQuery] DateOnly toDate,
            [FromQuery] Guid? customerId = null)
        {
            try
            {
                var businessIdClaim = User.FindFirst("BusinessId")?.Value;

                if (!Guid.TryParse(businessIdClaim, out var businessId))
                {
                    return Unauthorized(new
                    {
                        message = "Valid Business ID not found in token."
                    });
                }

                if (fromDate == default ||
                    toDate == default ||
                    fromDate > toDate)
                {
                    return BadRequest(new
                    {
                        message = "Please provide a valid date range."
                    });
                }

                var startDate = fromDate.ToDateTime(TimeOnly.MinValue);
                var endDateExclusive = toDate.AddDays(1)
                    .ToDateTime(TimeOnly.MinValue);

                var query = _context.Transactions
                    .AsNoTracking()
                    .Where(t =>
                        t.Customer != null &&
                        t.Customer.BusinessId == businessId &&
                        t.Date >= startDate &&
                        t.Date < endDateExclusive);

                if (customerId.HasValue)
                {
                    query = query.Where(
                        t => t.CustomerId == customerId.Value);
                }

                var totalReceived = await query
                    .Where(t => t.Type == "Payment")
                    .SumAsync(t => (decimal?)t.Amount) ?? 0m;

                var totalUdhaarGiven = await query
                    .Where(t => t.Type == "Udhaar")
                    .SumAsync(t => (decimal?)t.Amount) ?? 0m;

                var transactions = await query
                    .OrderByDescending(t => t.Date)
                    .Select(t => new
                    {
                        transactionId = t.Id,
                        customerId = t.CustomerId,
                        customerName = t.Customer!.Name,
                        phone = t.Customer.Phone,
                        address = t.Customer.Address,
                        amount = t.Amount,
                        type = t.Type,
                        description = t.Description,
                        date = t.Date
                    })
                    .ToListAsync();

                return Ok(new
                {
                    fromDate,
                    toDate,
                    totalReceived,
                    totalUdhaarGiven,
                    totalTransactions = transactions.Count,
                    transactions
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Transaction report failed for {FromDate} to {ToDate}",
                    fromDate,
                    toDate);

                return Problem(
                    title: "Failed to generate transaction report.",
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }
    }


}
