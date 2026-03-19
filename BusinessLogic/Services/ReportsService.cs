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

                var (revenue, userCount, trialsCount, payingUserCount) = await GetRevenueAndUserCountsForCampaignInRangeAsync(campaignId.Value, startUtc, endUtc, ct);
                row.Revenue = (decimal)revenue;
                row.TrialsCount = trialsCount;
                row.Arpu = row?.Total?.TotalInstalls > 0 ? (decimal)revenue / row.Total.TotalInstalls : 0;

                row.Trial2PaidConversionRate = trialsCount > 0 ? (double)payingUserCount / trialsCount * 100.0 : 0;
                row.Install2TrialConversionRate = row.Total?.TotalInstalls > 0 ? (double)trialsCount / row.Total.TotalInstalls.Value * 100.0 : 0;
                row.Install2PaidConversionRate = row.Total?.TotalInstalls > 0 ? (double)payingUserCount / row.Total.TotalInstalls.Value * 100.0 : 0;

                var localSpendAmount = ParseAmount(row.Total?.LocalSpend?.Amount);
                if (localSpendAmount.HasValue && localSpendAmount.Value > 0)
                {
                    row.Roas = row.Revenue / localSpendAmount.Value;
                    if (payingUserCount > 0)
                    {
                        row.Cac = localSpendAmount.Value / payingUserCount;
                    }
                    else
                    {
                        row.Cac = 0;
                    }
                    row.CostPerTrial = trialsCount > 0 ? localSpendAmount.Value / trialsCount : 0;
                }
                else
                {
                    row.Roas = 0;
                    row.CostPerTrial = 0;
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

    private async Task<(double Revenue, int UserCount, int TrialsCount, int PayingUserCount)> GetRevenueAndUserCountsForCampaignInRangeAsync(long campaignId, DateTime startUtc, DateTime endUtc, CancellationToken ct)
    {
        var query = _db.AppUsers
            .AsNoTracking()
            .Where(u => u.CampaignId == campaignId && u.InstallDate >= startUtc && u.InstallDate <= endUtc);

        var revenue = await query.SumAsync(u => u.TotalRevenue, ct);
        var userCount = await query.CountAsync(ct);
        var trialsCount = await query.CountAsync(u => u.HasTrial, ct);
        var payingUserCount = await query.CountAsync(u => u.TotalRevenue > 0, ct);
        return (revenue, userCount, trialsCount, payingUserCount);
    }

    private static decimal? ParseAmount(string? amount)
    {
        if (string.IsNullOrWhiteSpace(amount))
            return null;
        return decimal.TryParse(amount, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : null;
    }
}
