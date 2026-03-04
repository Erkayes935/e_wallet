namespace EWalletAPI.Models;
public class Wallet
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public User? User { get; set; }

    public decimal Balance { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<Transaction> Transactions { get; set; } = new();
}