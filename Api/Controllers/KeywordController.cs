using System.Security.Claims;
using BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/campaigns/{campaignId:long}/adgroups/{adGroupId:long}/targetingkeywords")]
[Authorize]
public class KeywordController : ControllerBase
{
    private readonly IKeywordService _keywordService;

    public KeywordController(IKeywordService keywordService)
    {
        _keywordService = keywordService;
    }

    private Guid? GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(long campaignId, long adGroupId, [FromQuery] int? limit, [FromQuery] int? offset, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized(new { message = "User is not authenticated." });

        var keywords = await _keywordService.GetAllAsync(campaignId, adGroupId, userId.Value, limit, offset, ct);
        return Ok(keywords);
    }
}
