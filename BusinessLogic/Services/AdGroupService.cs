using System.Text.Json;
using BusinessLogic.Interfaces;
using Domain.DTOs;

namespace BusinessLogic.Services;

public class AdGroupService : IAdGroupService
{
    private readonly IAppleSearchAdsApiClient _apiClient;

    public AdGroupService(IAppleSearchAdsApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<AdGroupDto?> GetByIdAsync(long campaignId, long adGroupId, Guid userId, CancellationToken ct = default)
    {
        var response = await _apiClient.GetAsync(userId, $"{AppleSearchAdsApiClientService.BaseUrl}/campaigns/{campaignId}/adgroups/{adGroupId}", ct);
        if (response == null || !response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync(ct);
        response.Dispose();
        return DeserializeAdGroup(json);
    }

    public async Task<IReadOnlyList<AdGroupDto>> GetAllAsync(long campaignId, Guid userId, int? limit = null, int? offset = null, CancellationToken ct = default)
    {
        var url = $"{AppleSearchAdsApiClientService.BaseUrl}/campaigns/{campaignId}/adgroups";
        if (limit.HasValue || offset.HasValue)
        {
            var query = new List<string>();
            if (limit.HasValue) query.Add($"limit={limit.Value}");
            if (offset.HasValue) query.Add($"offset={offset.Value}");
            url += "?" + string.Join("&", query);
        }

        var response = await _apiClient.GetAsync(userId, url, ct);
        if (response == null || !response.IsSuccessStatusCode)
            return Array.Empty<AdGroupDto>();

        var json = await response.Content.ReadAsStringAsync(ct);
        response.Dispose();
        var list = JsonSerializer.Deserialize<AdGroupListResponseDto>(json);
        if (list?.Data != null && list.Data.Count > 0)
            return list.Data;
        var array = JsonSerializer.Deserialize<List<AdGroupDto>>(json);
        return array ?? new List<AdGroupDto>();
    }

    public async Task<AdGroupDto?> CreateAsync(long campaignId, CreateAdGroupDto dto, Guid userId, CancellationToken ct = default)
    {
        var response = await _apiClient.PostAsJsonAsync(userId, $"{AppleSearchAdsApiClientService.BaseUrl}/campaigns/{campaignId}/adgroups", dto, ct);
        if (response == null || !response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync(ct);
        response.Dispose();
        return DeserializeAdGroup(json);
    }

    public async Task<AdGroupDto?> UpdateAsync(long campaignId, long adGroupId, UpdateAdGroupDto dto, Guid userId, CancellationToken ct = default)
    {
        var response = await _apiClient.PutAsJsonAsync(userId, $"{AppleSearchAdsApiClientService.BaseUrl}/campaigns/{campaignId}/adgroups/{adGroupId}", dto, ct);
        if (response == null || !response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync(ct);
        response.Dispose();
        return DeserializeAdGroup(json);
    }

    public async Task<bool> DeleteAsync(long campaignId, long adGroupId, Guid userId, CancellationToken ct = default)
    {
        var response = await _apiClient.DeleteAsync(userId, $"{AppleSearchAdsApiClientService.BaseUrl}/campaigns/{campaignId}/adgroups/{adGroupId}", ct);
        if (response == null)
            return false;
        var success = response.IsSuccessStatusCode;
        response.Dispose();
        return success;
    }

    private static AdGroupDto? DeserializeAdGroup(string json)
    {
        var wrapper = JsonSerializer.Deserialize<AdGroupResponseDto>(json);
        if (wrapper?.Data != null)
            return wrapper.Data;
        return JsonSerializer.Deserialize<AdGroupDto>(json);
    }
}
