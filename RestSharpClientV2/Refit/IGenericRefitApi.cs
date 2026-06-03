using Refit;
using RestSharpClientV2.Abstractions;

namespace RestSharpClientV2.Refit
{
    /// <summary>
    /// 基于 Refit 的通用 REST API 接口定义。
    /// 提供与 <see cref="IRestApiClient"/> 对应的底层 HTTP 调用契约。
    /// </summary>
    public interface IGenericRefitApi
    {
        /// <summary>
        /// 发送 HTTP GET 请求。
        /// </summary>
        /// <typeparam name="TResponse">期望的响应数据类型。</typeparam>
        /// <param name="url">请求地址（相对路径）。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>包含响应数据的 Refit API 响应。</returns>
        [Get("/{**url}")]
        Task<ApiResponse<TResponse>> GetAsync<TResponse>(
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
        /// <returns>包含响应数据的 Refit API 响应。</returns>
        [Post("/{**url}")]
        Task<ApiResponse<TResponse>> PostAsync<TRequest, TResponse>(
            string url,
            [Body] TRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 发送 HTTP PUT 请求。
        /// </summary>
        /// <typeparam name="TRequest">请求体数据类型。</typeparam>
        /// <typeparam name="TResponse">期望的响应数据类型。</typeparam>
        /// <param name="url">请求地址（相对路径）。</param>
        /// <param name="request">请求体数据。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>包含响应数据的 Refit API 响应。</returns>
        [Put("/{**url}")]
        Task<ApiResponse<TResponse>> PutAsync<TRequest, TResponse>(
            string url,
            [Body] TRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 发送 HTTP DELETE 请求。
        /// </summary>
        /// <typeparam name="TResponse">期望的响应数据类型。</typeparam>
        /// <param name="url">请求地址（相对路径）。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>包含响应数据的 Refit API 响应。</returns>
        [Delete("/{**url}")]
        Task<ApiResponse<TResponse>> DeleteAsync<TResponse>(
            string url,
            CancellationToken cancellationToken = default);
    }
}
