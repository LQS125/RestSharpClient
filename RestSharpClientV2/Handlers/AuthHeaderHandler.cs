using System.Net.Http.Headers;
using RestSharpClientV2.Abstractions;

namespace RestSharpClientV2.Handlers
{
    /// <summary>
    /// 为出站 HTTP 请求自动附加 Authorization 头的消息处理器。
    /// 从 <see cref="ITokenProvider"/> 获取 Bearer Token，并在 Token 非空时注入请求头。
    /// </summary>
    public sealed class AuthHeaderHandler(ITokenProvider tokenProvider) : DelegatingHandler
    {
        /// <inheritdoc />
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var token = await tokenProvider.GetTokenAsync(cancellationToken);

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
