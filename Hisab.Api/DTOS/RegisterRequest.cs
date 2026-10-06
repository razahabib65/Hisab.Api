using Microsoft.AspNetCore.Http;
namespace Hisab.DTOs

{
  public class RegisterRequest
  {
    // Business Information
    public string BusinessName { get; set; } = string.Empty;
    public string BusinessType { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public IFormFile? Logo { get; set; }

    // Owner Information
    public string OwnerName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    // Password
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
  }
}
