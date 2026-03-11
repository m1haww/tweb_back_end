using System.Security.Claims;
using BusinessLogic.Interfaces;
using Domain.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/campaign/reports")]
[Authorize]
public class CampaignReportsController : ControllerBase
{
    private readonly IKeywordService _keywordService;
    private readonly ICampaignService _campaignService;

    public CampaignReportsController(IKeywordService keywordService, ICampaignService campaignService)
    {
        _keywordService = keywordService;
        _campaignService = campaignService;
    }

    private Guid? GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }

    [HttpPost("{campaignId:long}/reports/keywords")]
    public async Task<IActionResult> GetKeywordReport(long campaignId, [FromBody] KeywordReportRequestDto request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized(new { message = "User is not authenticated." });

        var report = await _keywordService.GetKeywordReportAsync(campaignId, userId.Value, request, ct);
        if (report == null)
            return BadRequest(new { message = "Failed to fetch keyword report. Check Apple Search Ads credentials and request body." });

        return Ok(report);
    }

    [HttpPost("get-all")]
    public async Task<IActionResult> GetCampaignReportList([FromBody] CampaignReportRequestDto request, CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized(new { message = "User is not authenticated." });

        var report = await _campaignService.GetCampaignReportAsync(userId.Value, request, ct);
        if (report == null)
            return BadRequest(new { message = "Failed to fetch campaign report. Check Apple Search Ads credentials and request body." });

        return Ok(report);
    }
}
