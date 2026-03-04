using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using EWalletAPI.Data;
using EWalletAPI.DTOs;
using EWalletAPI.Models;

namespace EWalletAPI.Controllers;

[ApiController]
[Route("wallet")]
public class WalletController : ControllerBase
{
    private readonly AppDbContext _context;

    public WalletController(AppDbContext context)
    {
        _context = context;
    }
    [Authorize]
    [HttpPost("topup")]
    public async Task<IActionResult> TopUp([FromBody] TopUpRequest request)
    {
        if (request.Amount <= 0)
            return BadRequest("Amount must be greater than zero");

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var wallet = await _context.Wallets
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (wallet == null)
            return NotFound("Wallet not found");

        wallet.Balance += request.Amount;

        var transaction = new Transaction
        {
            ToWalletId = wallet.Id,
            Amount = request.Amount,
            Type = "TopUp"
        };

        _context.Transactions.Add(transaction);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            balance = wallet.Balance
        });
    }
    [Authorize]
    [HttpPost("transfer")]
    public async Task<IActionResult> Transfer([FromBody] TransferRequest request)
    {
        if (request.Amount <= 0)
            return BadRequest("Invalid amount");

        var fromUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var fromWallet = await _context.Wallets
            .FirstOrDefaultAsync(x => x.UserId == fromUserId);

        var toWallet = await _context.Wallets
            .FirstOrDefaultAsync(x => x.UserId == request.ToUserId);

        if (fromWallet == null || toWallet == null)
            return NotFound("Wallet not found");

        if (fromWallet.Balance < request.Amount)
            return BadRequest("Insufficient balance");


        await using var dbTransaction = await _context.Database.BeginTransactionAsync();

        try
        {
            fromWallet.Balance -= request.Amount;
            toWallet.Balance += request.Amount;

            var transaction = new Transaction
            {
                FromWalletId = fromWallet.Id,
                ToWalletId = toWallet.Id,
                Amount = request.Amount,
                Type = "Transfer"
            };

            _context.Transactions.Add(transaction);

            await _context.SaveChangesAsync();

            await dbTransaction.CommitAsync();

            return Ok("Transfer success");
        }
        catch
        {
            await dbTransaction.RollbackAsync();
            return StatusCode(500, "Transfer failed");
        }
    }
    [Authorize]
    [HttpGet("balance")]
    public async Task<IActionResult> GetBalance()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var wallet = await _context.Wallets
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (wallet == null)
            return NotFound("Wallet not found");

        return Ok(new
        {
            balance = wallet.Balance
        });
    }
    [Authorize]
    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var wallet = await _context.Wallets
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (wallet == null)
            return NotFound("Wallet not found");

        var transactions = await _context.Transactions
            .Where(t => t.FromWalletId == wallet.Id || t.ToWalletId == wallet.Id)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new
            {
                t.Id,
                t.Type,
                t.Amount,
                t.CreatedAt
            })
            .ToListAsync();

        return Ok(transactions);
    }
    [Authorize]
    [HttpGet("statement")]
    public async Task<IActionResult> GetStatement()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var wallet = await _context.Wallets
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (wallet == null)
            return NotFound("Wallet not found");

        var transactions = await _context.Transactions
            .Where(t => t.FromWalletId == wallet.Id || t.ToWalletId == wallet.Id)
            .OrderByDescending(t => t.Id)
            .Select(t => new
            {
                type = t.Type,
                amount = t.Amount,
                date = t.CreatedAt,
                fromWallet = t.FromWalletId,
                toWallet = t.ToWalletId
            })
            .ToListAsync();

        return Ok(new
        {
            balance = wallet.Balance,
            transactions
        });
    }
}