namespace Hisab.Models
{
  public class User
  {
    public Guid Id { get; set; }

    public string OwnerName { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public Guid BusinessId { get; set; }

    public Business Business { get; set; } = null!;
  }
}
