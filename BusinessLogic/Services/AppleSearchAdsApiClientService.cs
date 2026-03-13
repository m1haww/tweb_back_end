using System.Net.Http.Json;
using BusinessLogic.Interfaces;

namespace BusinessLogic.Services;

public class AppleSearchAdsApiClientService : IAppleSearchAdsApiClient
{
    public const string BaseUrl = "https://api.searchads.apple.com/api/v5";

    private readonly IAppleSearchAdsCredentialService _credentialService;
    private readonly IHttpClientFactory _httpClientFactory;

    public AppleSearchAdsApiClientService(
        IAppleSearchAdsCredentialService credentialService,
        IHttpClientFactory httpClientFactory)
    {
        _credentialService = credentialService;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<HttpResponseMessage?> GetAsync(Guid userId, string requestUri, CancellationToken ct = default)
    {
        var tokenResult = await _credentialService.GetOrCreateAccessToken(userId, ct);
        if (tokenResult?.AccessToken == null || tokenResult.OrgId == null)
            return null;

        using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        AddHeaders(request, tokenResult.AccessToken, tokenResult.OrgId.Value);

        var client = _httpClientFactory.CreateClient();
        return await client.SendAsync(request, ct);
    }

    public async Task<HttpResponseMessage?> PostAsJsonAsync<T>(Guid userId, string requestUri, T value, CancellationToken ct = default)
    {
        var tokenResult = await _credentialService.GetOrCreateAccessToken(userId, ct);
        if (tokenResult?.AccessToken == null || tokenResult.OrgId == null)
            return null;

        using var request = new HttpRequestMessage(HttpMethod.Post, requestUri) { Content = JsonContent.Create(value) };
        AddHeaders(request, tokenResult.AccessToken, tokenResult.OrgId.Value);

        var client = _httpClientFactory.CreateClient();
        return await client.SendAsync(request, ct);
    }

    public async Task<HttpResponseMessage?> PutAsJsonAsync<T>(Guid userId, string requestUri, T value, CancellationToken ct = default)
    {
        var tokenResult = await _credentialService.GetOrCreateAccessToken(userId, ct);
        if (tokenResult?.AccessToken == null || tokenResult.OrgId == null)
            return null;

        using var request = new HttpRequestMessage(HttpMethod.Put, requestUri) { Content = JsonContent.Create(value) };
        AddHeaders(request, tokenResult.AccessToken, tokenResult.OrgId.Value);

        var client = _httpClientFactory.CreateClient();
        return await client.SendAsync(request, ct);
    }

    public async Task<HttpResponseMessage?> DeleteAsync(Guid userId, string requestUri, CancellationToken ct = default)
    {
        var tokenResult = await _credentialService.GetOrCreateAccessToken(userId, ct);
        if (tokenResult?.AccessToken == null || tokenResult.OrgId == null)
            return null;

        using var request = new HttpRequestMessage(HttpMethod.Delete, requestUri);
        AddHeaders(request, tokenResult.AccessToken, tokenResult.OrgId.Value);

        var client = _httpClientFactory.CreateClient();
        return await client.SendAsync(request, ct);
    }

    private static void AddHeaders(HttpRequestMessage request, string accessToken, long orgId)
    {
        request.Headers.Add("Authorization", $"Bearer {accessToken}");
        request.Headers.Add("X-AP-Context", $"orgId={orgId}");
    }
}
