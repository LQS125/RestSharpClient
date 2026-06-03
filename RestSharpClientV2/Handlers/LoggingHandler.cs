using Microsoft.Extensions.Logging;

namespace RestSharpClientV2.Handlers
{
    /// <summary>
    /// 记录出站 HTTP 请求与响应信息的日志消息处理器。
    /// </summary>
    public sealed class LoggingHandler(ILogger<LoggingHandler> logger) : DelegatingHandler
    {
        /// <inheritdoc />
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            logger.LogInformation($"HTTP Request: {request.Method} {request.RequestUri}");

            var response = await base.SendAsync(request, cancellationToken);

            logger.LogInformation($"HTTP Response: {(int)response.StatusCode} {request.RequestUri}");

            return response;
        }
    }
}
