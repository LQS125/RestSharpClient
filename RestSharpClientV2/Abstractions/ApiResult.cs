namespace RestSharpClientV2.Abstractions
{
    /// <summary>
    /// REST API 通用结果包装类。
    /// 用于统一封装 HTTP 请求的响应状态、业务数据及错误信息。
    /// </summary>
    public sealed class ApiResult<T>
    {
        /// <summary>
        /// 指示请求是否成功。
        /// </summary>
        public bool IsSuccess { get; init; }

        /// <summary>
        /// HTTP 状态码；仅在请求已到达服务端时有效。
        /// </summary>
        public int? StatusCode { get; init; }

        /// <summary>
        /// 错误信息；仅在 <see cref="IsSuccess"/> 为 false 时有意义。
        /// </summary>
        public string? ErrorMessage { get; init; }

        /// <summary>
        /// 响应业务数据；仅在 <see cref="IsSuccess"/> 为 true 时有效。
        /// </summary>
        public T? Data { get; init; }

        /// <summary>
        /// 创建成功的结果实例。
        /// </summary>
        /// <param name="data">业务数据。</param>
        /// <param name="statusCode">HTTP 状态码。</param>
        /// <returns>包含成功状态的结果实例。</returns>
        public static ApiResult<T> Success(T? data, int? statusCode = null)
        {
            return new ApiResult<T>
            {
                IsSuccess = true,
                Data = data,
                StatusCode = statusCode
            };
        }

        /// <summary>
        /// 创建失败的结果实例。
        /// </summary>
        /// <param name="errorMessage">错误描述。</param>
        /// <param name="statusCode">HTTP 状态码。</param>
        /// <returns>包含失败状态的结果实例。</returns>
        public static ApiResult<T> Failure(string errorMessage, int? statusCode = null)
        {
            return new ApiResult<T>
            {
                IsSuccess = false,
                ErrorMessage = errorMessage,
                StatusCode = statusCode
            };
        }
    }
}
