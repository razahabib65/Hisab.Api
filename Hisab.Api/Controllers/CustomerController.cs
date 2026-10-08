using Hisab.Data;
using Hisab.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hisab.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CustomerController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CustomerController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/customer
        [HttpGet]
        public async Task<IActionResult> GetCustomers()
        {
            var businessIdClaim = User.FindFirst("BusinessId")?.Value;

            if (string.IsNullOrWhiteSpace(businessIdClaim))
            {
                return Unauthorized(new
                {
                    message = "Business ID not found in token."
                });
            }

            if (!Guid.TryParse(businessIdClaim, out var businessId))
            {
                return Unauthorized(new
                {
                    message = "Invalid Business ID."
                });
            }

            var customers = await _context.Customers
                .Where(c => c.BusinessId == businessId)
                .ToListAsync();

            return Ok(customers);
        }


        // GET: api/customer/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomer(Guid id)
        {
            var businessIdClaim = User.FindFirst("BusinessId")?.Value;

            if (string.IsNullOrWhiteSpace(businessIdClaim))
            {
                return Unauthorized(new
                {
                    message = "Business ID not found in token."
                });
            }

            if (!Guid.TryParse(businessIdClaim, out var businessId))
            {
                return Unauthorized(new
                {
                    message = "Invalid Business ID."
                });
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c =>
                    c.Id == id &&
                    c.BusinessId == businessId
                );

            if (customer == null)
            {
                return NotFound(new
                {
                    message = "Customer not found."
                });
            }

            var transactions = await _context.Transactions
                .Where(t => t.CustomerId == customer.Id)
                .OrderByDescending(t => t.Date)
                .ToListAsync();

            var totalUdhaar = transactions
                .Where(t => t.Type == "Udhaar")
                .Sum(t => t.Amount);

            var totalReceived = transactions
                .Where(t => t.Type == "Payment")
                .Sum(t => t.Amount);

            var payable = totalUdhaar - totalReceived;

            return Ok(new
            {
                id = customer.Id,
                name = customer.Name,
                phone = customer.Phone,
                address = customer.Address,
                notes = customer.Notes,

                totalUdhaar,
                totalReceived,
                payable,

                transactions = transactions.Select(t => new
                {
                    id = t.Id,
                    amount = t.Amount,
                    type = t.Type,
                    description = t.Description,
                    date = t.Date
                })
            });
        }


        // POST: api/customer
        [HttpPost]
        public async Task<IActionResult> AddCustomer(Customer customer)
        {
            var businessIdClaim = User.FindFirst("BusinessId")?.Value;

            if (string.IsNullOrWhiteSpace(businessIdClaim))
            {
                return Unauthorized(new
                {
                    message = "Business ID not found in token."
                });
            }

            if (!Guid.TryParse(businessIdClaim, out var businessId))
            {
                return Unauthorized(new
                {
                    message = "Invalid Business ID."
                });
            }

            customer.Id = Guid.NewGuid();
            customer.BusinessId = businessId;

            _context.Customers.Add(customer);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Customer added successfully.",
                customerId = customer.Id
            });
        }


        // PUT: api/customer/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(
            Guid id,
            Customer updatedCustomer)
        {
            var businessIdClaim = User.FindFirst("BusinessId")?.Value;

            if (string.IsNullOrWhiteSpace(businessIdClaim))
            {
                return Unauthorized(new
                {
                    message = "Business ID not found in token."
                });
            }

            if (!Guid.TryParse(businessIdClaim, out var businessId))
            {
                return Unauthorized(new
                {
                    message = "Invalid Business ID."
                });
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c =>
                    c.Id == id &&
                    c.BusinessId == businessId
                );

            if (customer == null)
            {
                return NotFound(new
                {
                    message = "Customer not found."
                });
            }

            customer.Name = updatedCustomer.Name;
            customer.Phone = updatedCustomer.Phone;
            customer.Address = updatedCustomer.Address;
            customer.Notes = updatedCustomer.Notes;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Customer updated successfully."
            });
        }


        // DELETE: api/customer/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(Guid id)
        {
            var businessIdClaim = User.FindFirst("BusinessId")?.Value;

            if (string.IsNullOrWhiteSpace(businessIdClaim))
            {
                return Unauthorized(new
                {
                    message = "Business ID not found in token."
                });
            }

            if (!Guid.TryParse(businessIdClaim, out var businessId))
            {
                return Unauthorized(new
                {
                    message = "Invalid Business ID."
                });
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c =>
                    c.Id == id &&
                    c.BusinessId == businessId
                );

            if (customer == null)
            {
                return NotFound(new
                {
                    message = "Customer not found."
                });
            }

            _context.Customers.Remove(customer);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Customer deleted successfully."
            });
        }


        // POST: api/customer/{id}/transaction
        [HttpPost("{id}/transaction")]
        public async Task<IActionResult> AddTransaction(
            Guid id,
            [FromBody] TransactionRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new
                    {
                        message = "Transaction data is required."
                    });
                }

                var businessIdClaim = User.FindFirst("BusinessId")?.Value;

                if (string.IsNullOrWhiteSpace(businessIdClaim))
                {
                    return Unauthorized(new
                    {
                        message = "Business ID not found in token."
                    });
                }

                if (!Guid.TryParse(businessIdClaim, out var businessId))
                {
                    return Unauthorized(new
                    {
                        message = "Invalid Business ID."
                    });
                }

                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c =>
                        c.Id == id &&
                        c.BusinessId == businessId
                    );

                if (customer == null)
                {
                    return NotFound(new
                    {
                        message = "Customer not found."
                    });
                }

                if (request.Amount <= 0)
                {
                    return BadRequest(new
                    {
                        message = "Amount must be greater than 0."
                    });
                }

                if (request.Type != "Udhaar" &&
                    request.Type != "Payment")
                {
                    return BadRequest(new
                    {
                        message = "Type must be Udhaar or Payment."
                    });
                }

                var transaction = new Transaction
                {
                    Id = Guid.NewGuid(),
                    CustomerId = customer.Id,
                    Amount = request.Amount,
                    Type = request.Type,
                    Description = request.Description,
                    Date = DateTime.UtcNow
                };

                _context.Transactions.Add(transaction);

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = request.Type == "Udhaar"
                        ? "Udhaar added successfully."
                        : "Payment received successfully.",

                    transactionId = transaction.Id,
                    amount = transaction.Amount,
                    type = transaction.Type,
                    date = transaction.Date
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("========================================");
                Console.WriteLine("TRANSACTION ERROR");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("========================================");

                return StatusCode(500, new
                {
                    message = "Failed to save transaction.",
                    error = ex.Message
                });
            }
        }
    }
}