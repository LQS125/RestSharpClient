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
        await using var server = await TestApiServer.StartAsync(); 
        using var httpClient = new HttpClient 
        {
            BaseAddress = server.BaseAddress
        };

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

   
    private sealed class TestApiServer : IAsyncDisposable 
    {
        private readonly WebApplication _app;  
        private readonly TestApiServerState _state;    

        private TestApiServer(WebApplication app, Uri baseAddress, TestApiServerState state)
        {
            _app = app;
            BaseAddress = baseAddress;
            _state = state;
        }

        public Uri BaseAddress { get; }

        public string? LastPostRunId => _state.LastPostRunId;

        public static async Task<TestApiServer> StartAsync()
        {
            var port = GetFreeTcpPort();  
            var baseAddress = new Uri($"http://127.0.0.1:{port}");   

            var builder = WebApplication.CreateBuilder();  
            builder.WebHost.UseUrls(baseAddress.ToString());   

            var app = builder.Build();   
            var state = new TestApiServerState();   

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

            await app.StartAsync();    

            return new TestApiServer(app, baseAddress, state); 
        }

        public async ValueTask DisposeAsync()
        {
            await _app.StopAsync();  
            await _app.DisposeAsync();    
        }

        private static int GetFreeTcpPort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);  
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();  
            return port;
        }
    }

    private sealed class TestApiServerState
    {
        public string? LastPostRunId { get; set; }
    }
}
