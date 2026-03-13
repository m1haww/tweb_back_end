using System.Text.Json;
using BusinessLogic.Interfaces;
using Domain.DTOs;

namespace BusinessLogic.Services;

public class CampaignService : ICampaignService
{
    private readonly IAppleSearchAdsApiClient _apiClient;

    public CampaignService(IAppleSearchAdsApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<CampaignDto?> GetByIdAsync(long campaignId, Guid userId, CancellationToken ct = default)
    {
        var response = await _apiClient.GetAsync(userId, $"{AppleSearchAdsApiClientService.BaseUrl}/campaigns/{campaignId}", ct);
        if (response == null || !response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync(ct);
        response.Dispose();
        return DeserializeCampaign(json);
    }

    public async Task<IReadOnlyList<CampaignDto>> GetAllAsync(Guid userId, CancellationToken ct = default)
    {
        var response = await _apiClient.GetAsync(userId, $"{AppleSearchAdsApiClientService.BaseUrl}/campaigns", ct);
        if (response == null || !response.IsSuccessStatusCode)
            return Array.Empty<CampaignDto>();

        var json = await response.Content.ReadAsStringAsync(ct);
        response.Dispose();
        var list = JsonSerializer.Deserialize<CampaignListResponseDto>(json);
        if (list?.Data != null && list.Data.Count > 0)
            return list.Data;
        var array = JsonSerializer.Deserialize<List<CampaignDto>>(json);
        return array ?? new List<CampaignDto>();
    }

    public async Task<CampaignDto?> CreateAsync(CreateCampaignDto dto, Guid userId, CancellationToken ct = default)
    {
        var response = await _apiClient.PostAsJsonAsync(userId, $"{AppleSearchAdsApiClientService.BaseUrl}/campaigns", dto, ct);
        if (response == null || !response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync(ct);
        response.Dispose();
        return DeserializeCampaign(json);
    }

    public async Task<CampaignDto?> UpdateAsync(long campaignId, UpdateCampaignDto dto, Guid userId, CancellationToken ct = default)
    {
        var response = await _apiClient.PutAsJsonAsync(userId, $"{AppleSearchAdsApiClientService.BaseUrl}/campaigns/{campaignId}", dto, ct);
        if (response == null || !response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync(ct);
        response.Dispose();
        return DeserializeCampaign(json);
    }

    public async Task<bool> DeleteAsync(long campaignId, Guid userId, CancellationToken ct = default)
    {
        var response = await _apiClient.DeleteAsync(userId, $"{AppleSearchAdsApiClientService.BaseUrl}/campaigns/{campaignId}", ct);
        if (response == null)
            return false;
        var success = response.IsSuccessStatusCode;
        response.Dispose();
        return success;
    }

    private static CampaignDto? DeserializeCampaign(string json)
    {
        var wrapper = JsonSerializer.Deserialize<CampaignResponseDto>(json);
        if (wrapper?.Data != null)
            return wrapper.Data;
        return JsonSerializer.Deserialize<CampaignDto>(json);
    }
}
