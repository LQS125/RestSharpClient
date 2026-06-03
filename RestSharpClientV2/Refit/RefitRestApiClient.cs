using Refit;
using RestSharpClientV2.Abstractions;

namespace RestSharpClientV2.Refit
{
    public sealed class RefitRestApiClient(IGenericRefitApi api) : IRestApiClient
    {
        /// <inheritdoc />
        public async Task<ApiResult<TResponse>> GetAsync<TResponse>(
            string url,
            CancellationToken cancellationToken = default)
        {
            return await ExecuteAsync(() =>
                api.GetAsync<TResponse>(NormalizeUrl(url), cancellationToken));
        }

        /// <inheritdoc />
        public async Task<ApiResult<TResponse>> PostAsync<TRequest, TResponse>(
            string url,
            TRequest request,
            CancellationToken cancellationToken = default)
        {
            return await ExecuteAsync(() =>
                api.PostAsync<TRequest, TResponse>(
                    NormalizeUrl(url),
                    request,
                    cancellationToken));
        }

        /// <inheritdoc />
        public async Task<ApiResult<TResponse>> PutAsync<TRequest, TResponse>(
            string url,
            TRequest request,
            CancellationToken cancellationToken = default)
        {
            return await ExecuteAsync(() =>
                api.PutAsync<TRequest, TResponse>(
                    NormalizeUrl(url),
                    request,
                    cancellationToken));
        }

        /// <inheritdoc />
        public async Task<ApiResult<TResponse>> DeleteAsync<TResponse>(
            string url,
            CancellationToken cancellationToken = default)
        {
            return await ExecuteAsync(() =>
                api.DeleteAsync<TResponse>(
                    NormalizeUrl(url),
                    cancellationToken));
        }

        /// <summary>
        /// 执行 Refit API 调用并统一包装为 <see cref="ApiResult{TResponse}"/>。
        /// </summary>
        /// <typeparam name="TResponse">期望的响应数据类型。</typeparam>
        /// <param name="action">Refit API 调用委托。</param>
        /// <returns>成功时包含响应数据，失败时包含错误信息的 API 结果。</returns>
        private static async Task<ApiResult<TResponse>> ExecuteAsync<TResponse>(
            Func<Task<ApiResponse<TResponse>>> action)
        {
            try
            {
                var response = await action();

                if (response.IsSuccessStatusCode)
                {
                    return ApiResult<TResponse>.Success(
                        response.Content,
                        (int)response.StatusCode);
                }

                return ApiResult<TResponse>.Failure(
                    response.Error?.Content
                    ?? response.Error?.Message
                    ?? "HTTP request failed.",
                    (int)response.StatusCode);
            }
            catch (ApiException ex)
            {
                return ApiResult<TResponse>.Failure(
                    ex.Content ?? ex.Message,
                    (int)ex.StatusCode);
            }
            catch (TaskCanceledException ex)
            {
                return ApiResult<TResponse>.Failure(
                    $"Request timeout or canceled: {ex.Message}");
            }
            catch (Exception ex)
            {
                return ApiResult<TResponse>.Failure(ex.Message);
            }
        }

        /// <summary>
        /// 规范化 URL，去除开头的斜杠，避免与 Refit 路径模板冲突。
        /// </summary>
        /// <param name="url">原始 URL。</param>
        /// <returns>去除前导斜杠后的 URL。</returns>
        private static string NormalizeUrl(string url)
        {
            return url.TrimStart('/');
        }
    }
}
