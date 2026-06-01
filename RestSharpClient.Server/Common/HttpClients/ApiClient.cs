using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ApiClient;

public class MyApiClient : ApiClientBase
{
    public MyApiClient(HttpClient httpClient, IApiTokenProvider? tokenProvider = null): base(httpClient, tokenProvider)
    {
    }

    public Task<GetDto?> GetAsync(
        string url,
        CancellationToken cancellationToken = default)
    {
        return GetAsync<GetDto>(url, cancellationToken);
    }

    public Task<GetDto?> GetAsync(
        string url,
        bool requiresToken,
        CancellationToken cancellationToken = default)
    {
        return GetAsync<GetDto>(url, requiresToken, cancellationToken);
    }

    public Task PostAsync(
        string url,
        PostDto dto,
        CancellationToken cancellationToken = default)
    {
        return base.PostAsync(url, dto, cancellationToken);
    }

    public Task PostAsync(
        string url,
        PostDto dto,
        bool requiresToken,
        CancellationToken cancellationToken = default)
    {
        return base.PostAsync(url, dto, requiresToken, cancellationToken);
    }
}
