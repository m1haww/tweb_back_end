namespace BusinessLogic.Interfaces;

public interface IAppleSearchAdsApiClient
{
    Task<HttpResponseMessage?> GetAsync(Guid userId, string requestUri, CancellationToken ct = default);

    Task<HttpResponseMessage?> PostAsJsonAsync<T>(Guid userId, string requestUri, T value, CancellationToken ct = default);

    Task<HttpResponseMessage?> PutAsJsonAsync<T>(Guid userId, string requestUri, T value, CancellationToken ct = default);

    Task<HttpResponseMessage?> DeleteAsync(Guid userId, string requestUri, CancellationToken ct = default);
}
