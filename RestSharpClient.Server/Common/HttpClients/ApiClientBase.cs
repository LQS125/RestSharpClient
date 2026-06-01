using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ApiClient;


/// <summary>
/// 拿到token的接口和实现
/// </summary>
public interface IApiTokenProvider
{
    Task<string?> GetTokenAsync(CancellationToken cancellationToken = default);
}

public sealed class StaticApiTokenProvider : IApiTokenProvider
{
    private readonly string? _token;

    public StaticApiTokenProvider(string? token)
    {
        _token = token;
    }

    public Task<string?> GetTokenAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_token);
    }
}

/// <summary>
/// 基础 API 客户端，支持 GET、POST 及可选的 Bearer 令牌请求。
/// </summary>
public abstract class ApiClientBase
{
    protected readonly HttpClient _httpClient; 
    protected readonly JsonSerializerOptions _jsonOptions; 

    private readonly IApiTokenProvider? _tokenProvider;  

    public bool RequiresToken { get; set; } = true;

    protected ApiClientBase(HttpClient httpClient, IApiTokenProvider? tokenProvider = null)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _tokenProvider = tokenProvider;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,  
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase   
        };
    }

    /// <summary>
    /// 发送 GET 请求，并将响应反序列化为对象。
    /// </summary>
    protected Task<T?> GetAsync<T>(
        string url,
        CancellationToken cancellationToken = default)
    {
        return GetAsync<T>(url, RequiresToken, cancellationToken);
    }

    protected async Task<T?> GetAsync<T>(
        string url,
        bool requiresToken,
        CancellationToken cancellationToken = default) 
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url); 
        await AddTokenAsync(request, requiresToken, cancellationToken);   

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<T>(json, _jsonOptions);
    }

    /// <summary>
    /// 发送带 JSON 正文的 POST 请求，并将响应反序列化为对象。
    /// </summary>
    protected Task<T?> PostAsync<T>(
        string url,
        object data,
        CancellationToken cancellationToken = default)
    {
        return PostAsync<T>(url, data, RequiresToken, cancellationToken);
    }

    protected async Task<T?> PostAsync<T>(
        string url,
        object data,
        bool requiresToken,
        CancellationToken cancellationToken = default)
    {
        using var request = CreateJsonRequest(HttpMethod.Post, url, data);
        await AddTokenAsync(request, requiresToken, cancellationToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken); 
        return JsonSerializer.Deserialize<T>(responseJson, _jsonOptions); 
    }

    /// <summary>
    /// 发送 POST 请求，仅检查响应状态码（不反序列化）。
    /// </summary>
    protected Task PostAsync(
        string url,
        object data,
        CancellationToken cancellationToken = default)
    {
        return PostAsync(url, data, RequiresToken, cancellationToken);
    }

    protected async Task PostAsync(
        string url,
        object data,
        bool requiresToken,
        CancellationToken cancellationToken = default)
    {
        using var request = CreateJsonRequest(HttpMethod.Post, url, data);
        await AddTokenAsync(request, requiresToken, cancellationToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// 构造一个携带 JSON 请求体的 HttpRequestMessage
    /// </summary>
    private HttpRequestMessage CreateJsonRequest(HttpMethod method, string url, object data)
    {
        return new HttpRequestMessage(method, url) 
        {
            Content = new StringContent(  
                JsonSerializer.Serialize(data, _jsonOptions),  
                Encoding.UTF8,
                "application/json")
        };
    }

    /// <summary>
    /// 令牌注入辅助方法
    /// </summary>
    private async Task AddTokenAsync(
        HttpRequestMessage request,
        bool requiresToken,
        CancellationToken cancellationToken)
    {
        if (!requiresToken) 
        {
            return;
        }

        if (_tokenProvider is null)
        {
            throw new InvalidOperationException("This API request requires a token, but no token provider was configured.");
        }

        var token = await _tokenProvider.GetTokenAsync(cancellationToken); 
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException("This API request requires a token, but the token is empty.");
        }
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token); 
    }
}
