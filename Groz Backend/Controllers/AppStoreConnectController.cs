using System.Security.Claims;
using Groz_Backend.DTOs;
using Groz_Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Groz_Backend.Controllers;

/// <summary>
/// API pentru salvare și citire credențiale App Store Connect (per user logat).
/// Base: /api/AppStoreConnect
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AppStoreConnectController : ControllerBase
{
    private readonly Data.AppDbContext _db;

    public AppStoreConnectController(Data.AppDbContext db)
    {
        _db = db;
    }

    private Guid? GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }

    /// <summary>
    /// GET /api/AppStoreConnect - Verifică dacă userul are credențiale salvate. Nu returnează PrivateKey.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetStatus(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var cred = await _db.AppStoreConnectCredentials
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == userId, ct);

        if (cred == null)
            return Ok(new AppStoreConnectStatusResponse { Configured = false });

        var suffix = cred.KeyId.Length >= 4
            ? "..." + cred.KeyId[^4..]
            : null;

        return Ok(new AppStoreConnectStatusResponse
        {
            Configured = true,
            KeyIdSuffix = suffix
        });
    }

    /// <summary>
    /// PUT /api/AppStoreConnect - Salvează sau actualizează credențialele (Key ID, Issuer ID, .p8).
    /// </summary>
    [HttpPut]
    public async Task<IActionResult> Save([FromBody] SaveAppStoreConnectRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();
        if (request == null) return BadRequest(new { message = "Invalid data." });

        var keyId = request.KeyId.Trim();
        var issuerId = request.IssuerId.Trim();
        var privateKey = request.PrivateKey.Trim();
        if (string.IsNullOrEmpty(keyId) || string.IsNullOrEmpty(issuerId) || string.IsNullOrEmpty(privateKey))
            return BadRequest(new { message = "Key ID, Issuer ID and private key are required." });

        var existing = await _db.AppStoreConnectCredentials
            .FirstOrDefaultAsync(c => c.UserId == userId, ct);

        if (existing != null)
        {
            existing.KeyId = keyId;
            existing.IssuerId = issuerId;
            existing.PrivateKey = privateKey;
        }
        else
        {
            _db.AppStoreConnectCredentials.Add(new AppStoreConnectCredential
            {
                Id = Guid.NewGuid(),
                UserId = userId.Value,
                KeyId = keyId,
                IssuerId = issuerId,
                PrivateKey = privateKey
            });
        }

        await _db.SaveChangesAsync(ct);
        return Ok(new AppStoreConnectStatusResponse { Configured = true, KeyIdSuffix = keyId.Length >= 4 ? "..." + keyId[^4..] : null });
    }

    /// <summary>
    /// DELETE /api/AppStoreConnect - Șterge credențialele salvate pentru userul curent.
    /// </summary>
    [HttpDelete]
    public async Task<IActionResult> Delete(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var cred = await _db.AppStoreConnectCredentials.FirstOrDefaultAsync(c => c.UserId == userId, ct);
        if (cred != null)
        {
            _db.AppStoreConnectCredentials.Remove(cred);
            await _db.SaveChangesAsync(ct);
        }

        return Ok(new AppStoreConnectStatusResponse { Configured = false });
    }
}
