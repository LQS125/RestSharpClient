using System.Net;
using System.Net.Sockets;
using ApiClient;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RestSharpClient.Tests;

[TestClass]
public sealed class MyApiClientIntegrationTests
{
    [TestMethod]
    public async Task GetAsync_CallsRealHttpEndpoint()
    {
        await using var server = await TestApiServer.StartAsync(); // 启动临时测试服务器
        // 创建 HttpClient 并配置基地址,后续请求只需提供相对路径（如 /integration-get）。
        using var httpClient = new HttpClient // using 确保 HttpClient 被正确释放。
        {
            BaseAddress = server.BaseAddress
        };

        // 实例化被测客户端
        var apiClient = new MyApiClient(httpClient)
        {
            RequiresToken = false
        };

        var result = await apiClient.GetAsync("/integration-get");

        Assert.IsNotNull(result);
        Assert.AreEqual(100, result.Id);
        Assert.AreEqual("integration-get-ok", result.RunId);
    }

    [TestMethod]
    public async Task PostAsync_CallsRealHttpEndpoint()
    {
        await using var server = await TestApiServer.StartAsync();
        using var httpClient = new HttpClient
        {
            BaseAddress = server.BaseAddress
        };

        var apiClient = new MyApiClient(httpClient)
        {
            RequiresToken = false
        };

        await apiClient.PostAsync("/integration-post", new PostDto { RunId = "integration-post-ok" });

        Assert.AreEqual("integration-post-ok", server.LastPostRunId);
    }

    // 临时 ASP.NET Core 服务器：在本地动态绑定一个空闲端口，启动真实的 Web 应用程序，注册测试专用的 API 端点，并记录请求状态
    private sealed class TestApiServer : IAsyncDisposable // sealed:不能被继承。IAsyncDisposable：提供异步释放资源的能力，用于停止并销毁服务器。
    {
        private readonly WebApplication _app;   // 实际运行的 ASP.NET Core 应用程序实例。
        private readonly TestApiServerState _state;    // 用于保存服务器端接收到的数据

        private TestApiServer(WebApplication app, Uri baseAddress, TestApiServerState state)
        {
            _app = app;
            BaseAddress = baseAddress;
            _state = state;
        }

        public Uri BaseAddress { get; }

        public string? LastPostRunId => _state.LastPostRunId;

        // 静态工厂方法 :  封装对象的创建过程，并返回一个该类型（或其子类型）的实例,对外提供一个清晰的获取实例的入口。
        // 静态工厂方法 = 静态方法 + 返回实例。解决了构造函数的一些局限性（命名、多态、缓存、异步等）。
        public static async Task<TestApiServer> StartAsync()
        {
            var port = GetFreeTcpPort();  // 获取空闲端口
            var baseAddress = new Uri($"http://127.0.0.1:{port}");   // 使用 127.0.0.1（localhost），不依赖外部网络

            var builder = WebApplication.CreateBuilder();   // 创建 Web 应用构建器
            builder.WebHost.UseUrls(baseAddress.ToString());   // 指定监听地址

            var app = builder.Build();     // 构建应用
            var state = new TestApiServerState();   // 创建状态存储

            // 注册测试专用路由
            app.MapGet("/integration-get", () =>
                Results.Json(new GetDto
                {
                    Id = 100,
                    RunId = "integration-get-ok"
                }));

            app.MapPost("/integration-post", (PostDto dto) =>
            {
                state.LastPostRunId = dto.RunId;
                return Results.Ok();
            });

            await app.StartAsync();    // 启动服务器

            return new TestApiServer(app, baseAddress, state);  // 返回实例
        }

        // 异步释放
        public async ValueTask DisposeAsync()
        {
            await _app.StopAsync();   // 停止 Web 应用程序 
            await _app.DisposeAsync();    // 释放资源
        }

        // 端口分配辅助方法
        private static int GetFreeTcpPort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);  // 使用 TcpListener 在 0 端口上监听，操作系统会自动分配一个空闲端口。
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();   // 立即停止监听，返回该端口号。这样端口被释放，但短时间内不会被其他进程占用
            return port;
        }
    }

    private sealed class TestApiServerState
    {
        public string? LastPostRunId { get; set; }
    }
}
