using Microsoft.AspNetCore.Mvc;
using EWalletAPI.Data;
using EWalletAPI.Models;
using EWalletAPI.DTOs;
using EWalletAPI.Services;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EWalletAPI.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly AuthService _authService;
    private readonly IConfiguration _config;

    public AuthController(AppDbContext context, AuthService authService, IConfiguration config)
    {
        _context = context;
        _authService = authService;
        _config = config;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var existingUser = _context.Users
            .FirstOrDefault(x => x.Email == request.Email);

        if (existingUser != null)
        {
            return BadRequest("Email already registered");
        }

        var user = new User
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();


        // AUTO CREATE WALLET
        var wallet = new Wallet
        {
            UserId = user.Id,
            Balance = 0
        };

        _context.Wallets.Add(wallet);
        await _context.SaveChangesAsync();


        return Ok("User registered");
    }
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _authService.ValidateUser(request.Email, request.Password);

        if (user == null)
        {
            return Unauthorized("Invalid email or password");
        }

        var token = GenerateJwtToken(user);

        return Ok(new
        {
            token = token
        });
    }
    private string GenerateJwtToken(Models.User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email)
        };

        var jwtKey = _config["Jwt:Key"] ?? throw new Exception("JWT key not configured");
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey)
        );

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"] ?? throw new Exception("JWT issuer not configured"),
            audience: _config["Jwt:Audience"] ?? throw new Exception("JWT audience not configured"),
            claims: claims,
            expires: DateTime.Now.AddHours(2),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}