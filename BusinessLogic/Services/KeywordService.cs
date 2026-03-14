using System.Text.Json;
using BusinessLogic.Interfaces;
using Domain.DTOs;

namespace BusinessLogic.Services;

public class KeywordService : IKeywordService
{
    private readonly IAppleSearchAdsApiClient _apiClient;

    public KeywordService(IAppleSearchAdsApiClient apiClient)
    {
        _apiClient = apiClient;
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
        if (response == null || !response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync(ct);
        response.Dispose();
        var report = JsonSerializer.Deserialize<KeywordReportResponseDto>(json);
        if (report?.Data?.ReportingDataResponse?.Row == null)
            return report;

        foreach (var row in report.Data.ReportingDataResponse.Row)
        {
            FillRowDisplayFields(row);
        }

        return report;
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
