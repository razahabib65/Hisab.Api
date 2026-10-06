namespace Hisab.Models
{
  public class Business
  {
    public Guid Id { get; set; }

    public string BusinessName { get; set; } = string.Empty;

    public string BusinessType { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string? Logo { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
  }
}
