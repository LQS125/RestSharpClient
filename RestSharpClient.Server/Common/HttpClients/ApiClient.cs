using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ApiClient;

public class MyApiClient : ApiClientBase
{
    public MyApiClient(HttpClient httpClient, IApiTokenProvider? tokenProvider = null)
        : base(httpClient, tokenProvider)
    {
    }

    public Task<UserDto?> GetUserAsync(
        int id,
        bool requiresToken = true,
        CancellationToken cancellationToken = default)
    {
        return GetAsync<UserDto>($"/users/{id}", requiresToken, cancellationToken);
    }

    public Task CreateUserAsync(
        UserCreateDto dto,
        bool requiresToken = true,
        CancellationToken cancellationToken = default)
    {
        return PostAsync("/users", dto, requiresToken, cancellationToken);
    }
}
