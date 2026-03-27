using System.Net.Http.Json;
using System.Text.Json;
using BusinessLogic.Interfaces;
using Domain.DTOs;

namespace BusinessLogic.Services;

public class ReportsService : IReportsService
{
    private const string BaseUrl = "https://api.searchads.apple.com/api/v5";
    private readonly IAppleSearchAdsCredentialService _credentialService;
    private readonly IHttpClientFactory _httpClientFactory;

    public ReportsService(in IAppleSearchAdsCredentialService credentialService, IHttpClientFactory httpClientFactory)
    {
        _credentialService = credentialService;
        _httpClientFactory = httpClientFactory;
    }
    
    private async Task<string?> GetAccessTokenAsync(Guid userId, CancellationToken ct)
    {
        var result = await _credentialService.GetOrCreateAccessToken(userId, ct);
        return result?.AccessToken;
    }

    private HttpClient CreateClient(string bearerToken)
    {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {bearerToken}");
        return client;
    }
    
    public async Task<CampaignReportResponseDto?> GetCampaignReportAsync(Guid userId, CampaignReportRequestDto request, CancellationToken ct = default)
    {
        var token = await GetAccessTokenAsync(userId, ct);
        if (token == null) return null;

        using var client = CreateClient(token!);
        var response = await client.PostAsJsonAsync($"{BaseUrl}/reports/campaigns", request, ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        Console.WriteLine("Response body: " + body);
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync(ct);
        return JsonSerializer.Deserialize<CampaignReportResponseDto>(json);
    }
}