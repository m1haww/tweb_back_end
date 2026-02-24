using System.Security.Claims;
using Groz_Backend.DTOs;
using Groz_Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Groz_Backend.Controllers;

/// <summary>
/// Rutele pentru autentificare: register, login, me.
/// Base: /api/auth
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// POST /api/auth/register - Înregistrare utilizator nou.
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        if (request == null)
            return BadRequest("Date invalide.");

        var response = await _authService.Register(request, ct);
        if (response == null)
            return BadRequest("Există deja un cont cu acest email.");

        return Ok(response);
    }

    /// <summary>
    /// POST /api/auth/login - Login; returnează token JWT și user.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        if (request == null)
            return BadRequest("Date invalide.");

        var response = await _authService.Login(request, ct);
        if (response == null)
            return Unauthorized("Email sau parolă incorectă.");

        return Ok(response);
    }

    /// <summary>
    /// GET /api/auth/me - Utilizatorul curent (necesită header: Authorization: Bearer &lt;token&gt;).
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var user = await _authService.GetUserById(userId, ct);
        if (user == null)
            return NotFound("Utilizator negăsit.");

        return Ok(new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name
        });
    }
}
