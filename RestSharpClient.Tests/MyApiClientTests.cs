using System.Net;
using System.Text;
using ApiClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// 该程序集中的各个测试方法可以并行执行。
[assembly: Parallelize(Scope = ExecutionScope.MethodLevel)]

namespace RestSharpClient.Tests;

/// <summary>
/// HttpClient:客户端，用于发送 HTTP 请求并接收响应。
/// HttpRequestMessage:表示一个HTTP 请求.包含方法（GET/POST）、URL、Headers、Body 等。
/// HttpResponseMessage:表示一个HTTP 响应，包含状态码、Headers、Body 等。
/// HttpMessageHandler:抽象基类，负责将 HttpRequestMessage 经过管道处理后变成 HttpResponseMessage。HttpClient 内部依赖它。
/// </summary>
[TestClass]
public sealed class MyApiClientTests
{
    [TestMethod]
    public async Task GetAsync_ReturnsDto()
    {
        // 创建假的 HTTP 处理器，模拟一个假的后端接口,返回固定的JSON
        // new FakeHttpMessageHandler(...)
        //_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = ... }:Lambda 表达式，定义了一个匿名函数
        //  _ 是给 FakeHttpMessageHandler构造函数中传入的 send， new HttpResponseMessage就是 返回的内容
        // Lambda 表达式:(参数列表) => 表达式  或  (参数列表) => { 语句块 }:给它一个输入，它做点处理，然后输出一个结果
        var handler = new FakeHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)  // HttpStatusCode.OK = 200
            {
                Content = new StringContent("""{"id":1,"runId":"get-ok"}""", Encoding.UTF8, "application/json")
            });

        //  创建 ApiClient，隔离外部网络依赖
        var apiClient = CreateClient(handler);   // 传入 FakeHttpMessageHandler，返回一个 MyApiClient
        apiClient.RequiresToken = false;

        var result = await apiClient.GetAsync("/test-get");

        // 断言（检查结果对不对）
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Id);
        Assert.AreEqual("get-ok", result.RunId);
        Assert.AreEqual(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.IsNull(handler.LastRequest?.Headers.Authorization);
    }

    [TestMethod]
    public async Task PostAsync_SendsJsonBody()
    {
        var handler = new FakeHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK));

        var apiClient = CreateClient(handler);
        apiClient.RequiresToken = false;

        await apiClient.PostAsync("/test-post", new PostDto { RunId = "post-ok" });

        Assert.AreEqual(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.AreEqual("application/json", handler.LastRequest?.Content?.Headers.ContentType?.MediaType);

        var body = await handler.ReadLastRequestBodyAsync();
        Assert.AreEqual("""{"runId":"post-ok"}""", body);
        Assert.IsNull(handler.LastRequest?.Headers.Authorization);
    }

    // 组装并返回一个可用于测试的 MyApiClient 实例
    // 根据传入的 HttpMessageHandler,创建 HttpClient(发请求工具),给 HttpClient 设一个假的基础地址
    // 用这个 HttpClient 实例化 MyApiClient，并返回该实例。
    private static MyApiClient CreateClient(HttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://example.test")
        };

        return new MyApiClient(httpClient);
    }

    // 重写了 HttpMessageHandler  中的SendAsync请求/响应循环 记录请求信息，并返回自定义的响应 HttpResponseMessage。
    private sealed class FakeHttpMessageHandler : HttpMessageHandler  // sealed：不能被继承。
    {
        // _send 是一个方法（委托），调用它时传入 HttpRequestMessage（请求），它会返回一个 HttpResponseMessage（响应）。
        // Func<HttpRequestMessage, HttpResponseMessage> = 输入请求，输出响应 的方法
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _send;
        private string? _lastRequestBody;

        public FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> send)
        {
            _send = send;
        }

        // 记录最后一次请求
        public HttpRequestMessage? LastRequest { get; private set; }

        // 读取最后一次请求的内容,可以拿到 POST 发送的 JSON 内容
        public Task<string?> ReadLastRequestBodyAsync()
        {
            return Task.FromResult(_lastRequestBody);
        }

        // 发送 HTTP 请求，并得到返回结果
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,  // 要发送的请求
            CancellationToken cancellationToken)
        {
            // 1. 保存这次请求，方便测试查看
            LastRequest = request;

            // 2. 如果有请求体（比如POST的JSON），保存下来
            // cancellationToken：取消异步执行的参数
            if (request.Content is not null)
            {
                _lastRequestBody = await request.Content.ReadAsStringAsync(cancellationToken);
            }

            // 3. 调用你传入的委托，返回【假响应】
            return _send(request);
        }
    }
}
