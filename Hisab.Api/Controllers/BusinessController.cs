using Hisab.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hisab.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BusinessController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BusinessController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/business
        [HttpGet]
        public async Task<IActionResult> GetBusiness()
        {
            var businessIdClaim = User.FindFirst("BusinessId")?.Value;

            if (string.IsNullOrEmpty(businessIdClaim))
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

            var business = await _context.Businesses
                .Include(b => b.Users)
                .FirstOrDefaultAsync(b => b.Id == businessId);

            if (business == null)
            {
                return NotFound(new
                {
                    message = "Business not found."
                });
            }

            var owner = business.Users.FirstOrDefault();

            return Ok(new
            {
                id = business.Id,
                businessName = business.BusinessName,
                businessType = business.BusinessType,
                address = business.Address,
                logo = business.Logo,

                ownerName = owner?.OwnerName,
                phone = owner?.Phone,
                email = owner?.Email
            });
        }

        // PUT: api/business
        [HttpPut]
        public async Task<IActionResult> UpdateBusiness(
            [FromBody] UpdateBusinessRequest request)
        {
            var businessIdClaim = User.FindFirst("BusinessId")?.Value;

            if (string.IsNullOrEmpty(businessIdClaim))
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

            var business = await _context.Businesses
                .Include(b => b.Users)
                .FirstOrDefaultAsync(b => b.Id == businessId);

            if (business == null)
            {
                return NotFound(new
                {
                    message = "Business not found."
                });
            }

            var owner = business.Users.FirstOrDefault();

            // Business details
            business.BusinessName = request.BusinessName;
            business.BusinessType = request.BusinessType;
            business.Address = request.Address;
            business.Logo = request.Logo;

            // Owner details
            if (owner != null)
            {
                owner.OwnerName = request.OwnerName;
                owner.Phone = request.Phone;
                owner.Email = request.Email;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Business updated successfully."
            });
        }
    }

    public class UpdateBusinessRequest
    {
        public string BusinessName { get; set; } = string.Empty;

        public string BusinessType { get; set; } = string.Empty;

        public string? Address { get; set; }

        public string? Logo { get; set; }

        public string OwnerName { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
    }
}