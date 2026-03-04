namespace EWalletAPI.DTOs;

public class TransferRequest
{
    public int ToUserId { get; set; }

    public decimal Amount { get; set; }
}