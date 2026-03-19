using System.Globalization;
using System.Text.Json;
using BusinessLogic.Database;
using BusinessLogic.Interfaces;
using Domain.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogic.Services;

public class KeywordService : IKeywordService
{
    private readonly IAppleSearchAdsApiClient _apiClient;
    private readonly AppDbContext _db;

    public KeywordService(IAppleSearchAdsApiClient apiClient, AppDbContext db)
    {
        _apiClient = apiClient;
        _db = db;
    }

    public async Task<IReadOnlyList<KeywordDto>> GetAllAsync(long campaignId, long adGroupId, Guid userId, int? limit = null, int? offset = null, CancellationToken ct = default)
    {
        var url = $"{AppleSearchAdsApiClientService.BaseUrl}/campaigns/{campaignId}/adgroups/{adGroupId}/targetingkeywords";
        if (limit.HasValue || offset.HasValue)
        {
            var query = new List<string>();
            if (limit.HasValue) query.Add($"limit={limit.Value}");
            if (offset.HasValue) query.Add($"offset={offset.Value}");
            url += "?" + string.Join("&", query);
        }

        var response = await _apiClient.GetAsync(userId, url, ct);
        if (response == null || !response.IsSuccessStatusCode)
            return Array.Empty<KeywordDto>();

        var json = await response.Content.ReadAsStringAsync(ct);
        response.Dispose();
        var list = JsonSerializer.Deserialize<KeywordListResponseDto>(json);
        if (list?.Data != null && list.Data.Count > 0)
            return list.Data;
        var array = JsonSerializer.Deserialize<List<KeywordDto>>(json);
        return array ?? new List<KeywordDto>();
    }

    public async Task<KeywordReportResponseDto?> GetKeywordReportAsync(long campaignId, Guid userId, KeywordReportRequestDto request, CancellationToken ct = default)
    {
        var response = await _apiClient.PostAsJsonAsync(userId, $"{AppleSearchAdsApiClientService.BaseUrl}/reports/campaigns/{campaignId}/keywords", request, ct);
        
        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync(ct);
        response.Dispose();
        var report = JsonSerializer.Deserialize<KeywordReportResponseDto>(json);
        if (report?.Data?.ReportingDataResponse?.Row == null)
            return report;

        foreach (var row in report.Data.ReportingDataResponse.Row)
            FillRowDisplayFields(row);

        if (TryParseReportDateRange(request.StartTime, request.EndTime, out var startUtc, out var endUtc))
        {
            foreach (var row in report.Data.ReportingDataResponse.Row)
            {
                var keywordId = row.Metadata?.KeywordId;
                var adGroupId = row.Metadata?.AdGroupId;
                if (!keywordId.HasValue || !adGroupId.HasValue)
                    continue;

                var (revenue, userCount, trialsCount, payingUserCount) = await GetRevenueAndUserCountsForKeywordInRangeAsync(campaignId, keywordId.Value, adGroupId.Value, startUtc, endUtc, ct);
                row.Revenue = (decimal)revenue;
                row.TrialsCount = trialsCount;
                row.Arpu = userCount > 0 ? (decimal)revenue / userCount : 0;

                row.Trial2PaidConversionRate = trialsCount > 0 ? (double)payingUserCount / trialsCount * 100.0 : 0;
                row.Install2TrialConversionRate = row.TotalInstalls > 0 ? (double)trialsCount / row.TotalInstalls * 100.0 : 0;
                row.Install2PaidConversionRate = row.TotalInstalls > 0 ? (double)payingUserCount / row.TotalInstalls * 100.0 : 0;

                var localSpendAmount = ParseAmount(row.Total?.LocalSpend?.Amount);
                if (localSpendAmount.HasValue && localSpendAmount.Value > 0)
                {
                    row.Roas = row.Revenue / localSpendAmount.Value;
                    row.Cac = payingUserCount > 0 ? localSpendAmount.Value / payingUserCount : 0;
                    row.CostPerTrial = trialsCount > 0 ? localSpendAmount.Value / trialsCount : 0;
                }
                else
                {
                    row.Roas = 0;
                    row.Cac = 0;
                    row.CostPerTrial = 0;
                }
            }
        }

        return report;
    }

    private static bool TryParseReportDateRange(string? startTime, string? endTime, out DateTime startUtc, out DateTime endUtc)
    {
        startUtc = default;
        endUtc = default;
        if (string.IsNullOrWhiteSpace(startTime) || string.IsNullOrWhiteSpace(endTime))
            return false;
        if (!DateTime.TryParse(startTime, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out startUtc))
            return false;
        if (!DateTime.TryParse(endTime, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out endUtc))
            return false;
        startUtc = startUtc.Date;
        endUtc = endUtc.Date.AddDays(1).AddTicks(-1);
        return true;
    }

    private async Task<(double Revenue, int UserCount, int TrialsCount, int PayingUserCount)> GetRevenueAndUserCountsForKeywordInRangeAsync(long campaignId, long keywordId, long adGroupId, DateTime startUtc, DateTime endUtc, CancellationToken ct)
    {
        var query = _db.AppUsers
            .AsNoTracking()
            .Where(u => u.CampaignId == campaignId && u.KeywordId == keywordId && u.AdGroupId == adGroupId
                && u.InstallDate >= startUtc && u.InstallDate <= endUtc);

        var revenue = await query.SumAsync(u => u.TotalRevenue, ct);
        var userCount = await query.CountAsync(ct);
        var trialsCount = await query.CountAsync(u => u.HasTrial, ct);
        var payingUserCount = await query.CountAsync(u => u.TotalRevenue > 0, ct);
        return (revenue, userCount, trialsCount, payingUserCount);
    }

    private static decimal? ParseAmount(string? amount)
    {
        if (string.IsNullOrWhiteSpace(amount)) return null;
        return decimal.TryParse(amount, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : null;
    }

    private static void FillRowDisplayFields(KeywordReportRowDto row)
    {
        var m = row.Metadata;
        var t = row.Total;
        if (m != null)
        {
            row.Status = m.KeywordDisplayStatus;
            row.Country = m.CountriesOrRegions != null && m.CountriesOrRegions.Count > 0
                ? string.Join(", ", m.CountriesOrRegions)
                : null;
            row.Keyword = m.Keyword;
            row.MatchType = m.MatchType;
            row.AdGroupName = m.AdGroupName;
            row.CampaignId = m.CampaignId;
            row.KeywordId = m.KeywordId;
            row.BidAmount = m.BidAmount;
        }
        if (t != null)
        {
            row.AvgCpt = t.AvgCpt;
            row.LocalSpend = t.LocalSpend;
            row.Taps = t.Taps;
            row.Impressions = t.Impressions;
            row.Ttr = t.Ttr;
            row.TapInstalls = t.TapInstalls;
            row.TapInstallCpi = t.TapInstallCpi;
            row.TotalAvgCpi = t.TotalAvgCpi;
            row.TotalInstallRate = t.TotalInstallRate;
            row.TapInstallRate = t.TapInstallRate;
            row.AvgCpm = t.AvgCpm;
            row.TotalInstalls = t.TotalInstalls;
        }
    }
}
