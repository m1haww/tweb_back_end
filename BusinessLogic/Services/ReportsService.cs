using System.Text.Json;
using BusinessLogic.Interfaces;
using Domain.DTOs;

namespace BusinessLogic.Services;

public class ReportsService : IReportsService
{
    private readonly IAppleSearchAdsApiClient _apiClient;

    public ReportsService(IAppleSearchAdsApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<CampaignReportResponseDto?> GetCampaignReportAsync(Guid userId, CampaignReportRequestDto request, CancellationToken ct = default)
    {
        var response = await _apiClient.PostAsJsonAsync(userId, $"{AppleSearchAdsApiClientService.BaseUrl}/reports/campaigns", request, ct);
        if (response == null || !response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync(ct);
        response.Dispose();
        return JsonSerializer.Deserialize<CampaignReportResponseDto>(json);
    }
}
