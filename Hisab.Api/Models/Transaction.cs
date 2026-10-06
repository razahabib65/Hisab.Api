namespace Hisab.Models
{
  public class Transaction
  {
    public Guid Id { get; set; }

    public Guid CustomerId { get; set; }

    public decimal Amount { get; set; }

    public string Type { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime Date { get; set; }

    public Customer? Customer { get; set; }
  }
}
