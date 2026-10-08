namespace Hisab.DTOS
{
    public class TransactionRequest
    {
        public decimal Amount { get; set; }

        public string Type { get; set; } = string.Empty;

        public string? Description { get; set; }
    }
}