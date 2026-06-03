namespace RestSharpClientV2.Abstractions
{
    /// <summary>
    /// 访问令牌提供者接口。
    /// 负责异步获取用于 REST API 身份验证的 Bearer Token。
    /// </summary>
    public interface ITokenProvider
    {
        /// <summary>
        /// 异步获取访问令牌。
        /// </summary>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>有效的访问令牌；若未登录或令牌不可用则返回 null。</returns>
        Task<string?> GetTokenAsync(CancellationToken cancellationToken = default);
    }
}
