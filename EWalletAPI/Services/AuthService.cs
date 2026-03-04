using EWalletAPI.Data;
using EWalletAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EWalletAPI.Services;

public class AuthService
{
    private readonly AppDbContext _context;

    public AuthService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> ValidateUser(string email, string password)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Email == email);

        if (user == null)
            return null;

        bool valid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

        if (!valid)
            return null;

        return user;
    }
}