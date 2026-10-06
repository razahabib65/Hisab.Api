namespace Hisab.Models
{
  public class Customer
  {
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string? Address { get; set; }

    public string? Notes { get; set; }

    public Guid BusinessId { get; set; }

    public Business? Business { get; set; }
  }

  public class TransactionRequest
  {
    public decimal Amount { get; set; }

    public string Type { get; set; } = string.Empty;

    public string? Description { get; set; }
  }
}
