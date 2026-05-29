using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ApiClient;


/// <summary>
/// 拿到token的接口和实现,这样就可以将tokenprovider注入到apiclientbase的构造函数中，来拿到从program中获取的token
/// </summary>
// 负责获取登录后 Token 的接口
public interface IApiTokenProvider
{
    Task<string?> GetTokenAsync(CancellationToken cancellationToken = default);
}

// 简易实现接口，直接返回构造函数中传入的静态令牌。替换为从配置、缓存、登录结果或其他令牌服务获取令牌的实现。
public sealed class StaticApiTokenProvider : IApiTokenProvider
{
    private readonly string? _token;

    public StaticApiTokenProvider(string? token)
    {
        _token = token;
    }

    // 实现接口方法
    public Task<string?> GetTokenAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_token);
    }
}

/// <summary>
/// 基础 API 客户端，支持 GET、POST 及可选的 Bearer 令牌请求。
/// 依赖注入，符合依赖倒置原则。
/// </summary>
public abstract class ApiClientBase
{
    protected readonly HttpClient _httpClient; // 发送请求
    protected readonly JsonSerializerOptions _jsonOptions; // JSON序列化配置

    private readonly IApiTokenProvider? _tokenProvider;  // 可选

    // 定义自动实现的属性，{ get; set; }可以在外部读取和更改，MyApiClient 继承了 ApiClientBase，可以通过 apiClient.RequiresToken = false;来设置
    public bool RequiresToken { get; set; } = true;

    protected ApiClientBase(HttpClient httpClient, IApiTokenProvider? tokenProvider = null)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _tokenProvider = tokenProvider;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,  // 反序列化( JSON -> c# )时忽略字段名大小写
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase    // 序列化时使用小驼峰命名
        };
    }

    /// <summary>
    /// 发送 GET 请求，并将响应反序列化为对象。
    /// 代码复用：重载方法的委托调用
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
        CancellationToken cancellationToken = default)  // 是否需要 token
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);  // 创建GET请求，using：自动释放
        await AddTokenAsync(request, requiresToken, cancellationToken);   

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken); // 把接口返回的内容读成字符串
        return JsonSerializer.Deserialize<T>(json, _jsonOptions);// Deserialize<T>:反序列化为想要的类型，GetAsync<UserDto>(url);
    }

    /// <summary>
    /// Sends a POST request with a JSON body and deserializes the response.
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
    /// Sends a POST request and only checks the response status code.
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
    /// 把传入的对象，变成一个标准的 JSON 请求（Http 请求体）
    /// </summary>
    private HttpRequestMessage CreateJsonRequest(HttpMethod method, string url, object data)
    {
        // 对象初始化器 语法
        return new HttpRequestMessage(method, url)  // 创建空的请求，还没有 Body（请求体）
        {
            Content = new StringContent(  // StringContent 需要三个参数
                JsonSerializer.Serialize(data, _jsonOptions),  
                Encoding.UTF8,
                "application/json")
        };
    }

    /// <summary>
    /// 令牌注入辅助方法
    /// 给 HTTP 请求自动加上 Bearer Token 身份验证
    /// requiresToken 参数决定请求是否需要令牌。
    /// </summary>
    private async Task AddTokenAsync(
        HttpRequestMessage request,
        bool requiresToken,
        CancellationToken cancellationToken)
    {
        if (!requiresToken)  // 不需要 token ，直接返回
        {
            return;
        }

        if (_tokenProvider is null)
        {
            throw new InvalidOperationException("This API request requires a token, but no token provider was configured.");
        }

        var token = await _tokenProvider.GetTokenAsync(cancellationToken);  // 获取 token
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException("This API request requires a token, but the token is empty.");
        }
        // 把 Token 加到请求头里
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token); 
    }
}
