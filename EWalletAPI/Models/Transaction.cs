namespace EWalletAPI.Models;
public class Transaction
{
    public int Id { get; set; }

    public int? FromWalletId { get; set; }

    public int? ToWalletId { get; set; }

    public decimal Amount { get; set; }

    public string Type { get; set; } = "";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}