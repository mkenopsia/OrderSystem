using System.Security.Claims;
using AuthService.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(Services.AuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        var ok = await authService.RegisterAsync(req.Email, req.Password);
        return ok ? Ok(new { Message = "User created" }) : BadRequest("Email exists");
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest req)
    {
        var token = authService.LoginAsync(req.Email, req.Password);
        return token == null ? Unauthorized("Invalid credentials") : Ok(new { Token = token });
    }
    
    [HttpGet("verify")]
    [AllowAnonymous]
    public IActionResult VerifyToken()
    {
        var authHeader = Request.Headers["Authorization"].FirstOrDefault();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return Unauthorized();

        var token = authHeader["Bearer ".Length..].Trim();
    
        try
        {
            var principal = authService.ValidateToken(token);
            if (principal == null || !principal.Identity?.IsAuthenticated == true) 
                return Unauthorized();
            
            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userEmail = principal.FindFirst(ClaimTypes.Email)?.Value;
            
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Missing user identifier in token");
            
            Response.Headers.Append("X-User-Id", userId);
            if (!string.IsNullOrEmpty(userEmail))
            {
                Response.Headers.Append("X-User-Email", userEmail);
            }
            
            return Ok(new { 
                UserId = userId,
                Email = userEmail 
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Token validation failed: {ex.Message}");
            return Unauthorized();
        }
    }
}