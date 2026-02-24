using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Groz_Backend.Data;
using Groz_Backend.DTOs;
using Groz_Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Groz_Backend.Services;


/// Logica de autentificare: înregistrare, login, generare JWT.
/// Folosește PostgreSQL prin AppDbContext.

public class AuthService
{
    private readonly IConfiguration _config;
    private readonly AppDbContext _db;

    public AuthService(IConfiguration config, AppDbContext db)
    {
        _config = config;
        _db = db;
    }

    public async Task<AuthResponse?> Register(RegisterRequest request, CancellationToken ct = default)
    {
        var emailNorm = request.Email.Trim();
        if (await _db.Users.AnyAsync(u => u.Email == emailNorm, ct))
            return null;

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = emailNorm,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Name = request.Name?.Trim(),
            CreatedAt = DateTime.UtcNow
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);
        return BuildAuthResponse(user);
    }

    public async Task<AuthResponse?> Login(LoginRequest request, CancellationToken ct = default)
    {
        var emailNorm = request.Email.Trim();
        var user = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == emailNorm, ct);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return null;

        return BuildAuthResponse(user);
    }

    public async Task<User?> GetUserById(Guid id, CancellationToken ct = default)
    {
        return await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    private AuthResponse BuildAuthResponse(User user)
    {
        var token = GenerateJwt(user);
        var expiresAt = DateTime.UtcNow.AddHours(24);

        return new AuthResponse
        {
            Token = token,
            ExpiresAt = expiresAt,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name
            }
        };
    }

    private string GenerateJwt(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key lipsește din config")));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
