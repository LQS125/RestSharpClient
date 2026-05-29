using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ApiClient;

public class MyApiClient : ApiClientBase
{
    // : base(httpClient, tokenProvider)调用父类 ApiClientBase 的构造函数,把接收到的两个参数直接传给父类
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
