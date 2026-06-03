namespace RestSharpClientV2.Abstractions
{
    /// <summary>
    /// REST API 客户端接口。
    /// 提供基于 HTTP 的通用 CRUD 操作抽象，用于与远程 REST 服务交互。
    /// 实现类负责序列化、反序列化、错误处理及日志记录等横切关注点。
    /// </summary>
    public interface IRestApiClient
    {
        /// <summary>
        /// 发送 HTTP GET 请求。
        /// </summary>
        /// <typeparam name="TResponse">期望的响应数据类型。</typeparam>
        /// <param name="url">请求地址（相对路径）。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>包含响应数据的 API 结果；失败时包含错误信息。</returns>
        Task<ApiResult<TResponse>> GetAsync<TResponse>(
            string url,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 发送 HTTP POST 请求。
        /// </summary>
        /// <typeparam name="TRequest">请求体数据类型。</typeparam>
        /// <typeparam name="TResponse">期望的响应数据类型。</typeparam>
        /// <param name="url">请求地址（相对路径）。</param>
        /// <param name="request">请求体数据。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>包含响应数据的 API 结果；失败时包含错误信息。</returns>
        Task<ApiResult<TResponse>> PostAsync<TRequest, TResponse>(
            string url,
            TRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 发送 HTTP PUT 请求。
        /// </summary>
        /// <typeparam name="TRequest">请求体数据类型。</typeparam>
        /// <typeparam name="TResponse">期望的响应数据类型。</typeparam>
        /// <param name="url">请求地址（相对路径）。</param>
        /// <param name="request">请求体数据。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>包含响应数据的 API 结果；失败时包含错误信息。</returns>
        Task<ApiResult<TResponse>> PutAsync<TRequest, TResponse>(
            string url,
            TRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 发送 HTTP DELETE 请求。
        /// </summary>
        /// <typeparam name="TResponse">期望的响应数据类型。</typeparam>
        /// <param name="url">请求地址（相对路径）。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>包含响应数据的 API 结果；失败时包含错误信息。</returns>
        Task<ApiResult<TResponse>> DeleteAsync<TResponse>(
            string url,
            CancellationToken cancellationToken = default);
    }
}
