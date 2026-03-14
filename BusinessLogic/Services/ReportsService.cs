using System.Globalization;
using System.Text.Json;
using BusinessLogic.Database;
using BusinessLogic.Interfaces;
using Domain.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogic.Services;

public class ReportsService : IReportsService
{
    private readonly IAppleSearchAdsApiClient _apiClient;
    private readonly AppDbContext _db;

    public ReportsService(IAppleSearchAdsApiClient apiClient, AppDbContext db)
    {
        _apiClient = apiClient;
        _db = db;
    }

    public async Task<CampaignReportResponseDto?> GetCampaignReportAsync(Guid userId, CampaignReportRequestDto request, CancellationToken ct = default)
    {
        var response = await _apiClient.PostAsJsonAsync(userId, $"{AppleSearchAdsApiClientService.BaseUrl}/reports/campaigns", request, ct);
        
        if (response == null || !response.IsSuccessStatusCode)
            return null;

        try
        {
            var json = await response.Content.ReadAsStringAsync(ct);
            response.Dispose();
            var report = JsonSerializer.Deserialize<CampaignReportResponseDto>(json);
            if (report?.Data?.ReportingDataResponse?.Row == null)
                return report;
            
            if (!TryParseReportDateRange(request.StartTime, request.EndTime, out var startUtc, out var endUtc))
                return report;
            
            foreach (var row in report.Data.ReportingDataResponse.Row)
            {
                var campaignId = row.Metadata?.CampaignId;
                if (!campaignId.HasValue)
                    continue;

                var (revenue, userCount) = await GetRevenueAndUserCountForCampaignInRangeAsync(campaignId.Value, startUtc, endUtc, ct);
                row.Revenue = (decimal)revenue;
                if (userCount > 0)
                {
                    row.Arpu = (decimal)revenue / userCount;
                }
                else
                {
                    row.Arpu = 0;
                }

                var localSpendAmount = ParseAmount(row.Total?.LocalSpend?.Amount);
                if (localSpendAmount.HasValue && localSpendAmount.Value > 0)
                {
                    row.Roas = row.Revenue / localSpendAmount.Value;
                }
                else
                {
                    row.Roas = 0;
                }
            }

            return report;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error while fetching campaign report:");
            Console.WriteLine(ex.Message);
            return null;
        }
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

    private async Task<(double Revenue, int UserCount)> GetRevenueAndUserCountForCampaignInRangeAsync(long campaignId, DateTime startUtc, DateTime endUtc, CancellationToken ct)
    {
        var query = _db.AppUsers
            .AsNoTracking()
            .Where(u => u.CampaignId == campaignId && u.InstallDate >= startUtc && u.InstallDate <= endUtc);

        var revenue = await query.SumAsync(u => u.TotalRevenue, ct);
        var userCount = await query.CountAsync(ct);
        return (revenue, userCount);
    }

    private static decimal? ParseAmount(string? amount)
    {
        if (string.IsNullOrWhiteSpace(amount))
            return null;
        return decimal.TryParse(amount, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : null;
    }
}
