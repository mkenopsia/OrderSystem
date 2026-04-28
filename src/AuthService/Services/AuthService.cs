using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthService.Domain;
using AuthService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Services;

public class AuthService(AuthDbContext db, IConfiguration config)
{
    public async Task<bool> RegisterAsync(string email, string password)
    {
        if (await db.Users.AnyAsync(u => u.Email == email)) return false;
        
        db.Users.Add(new User 
        { 
            Email = email, 
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password) 
        });
        await db.SaveChangesAsync();
        return true;
    }

    public string? LoginAsync(string email, string password)
    {
        var user = db.Users.FirstOrDefault(u => u.Email == email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)) return null;

        return GenerateJwtToken(user);
    }

    private string GenerateJwtToken(User user)
    {
        var secret = config["JWT__Secret"] ?? throw new Exception("JWT__Secret missing");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email)
        };

        var token = new JwtSecurityToken(
            issuer: config["JWT__Issuer"],
            audience: config["JWT__Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    public ClaimsPrincipal? ValidateToken(string token)
    {
        var secret = config["JWT__Secret"] ?? throw new Exception("JWT__Secret missing");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

        var tokenParams = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = config["JWT__Issuer"],
            ValidAudience = config["JWT__Audience"],
            IssuerSigningKey = key
        };

        try
        {
            var handler = new JwtSecurityTokenHandler();
            return handler.ValidateToken(token, tokenParams, out _);
        }
        catch
        {
            return null;
        }
    }
}