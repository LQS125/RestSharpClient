namespace RestSharpClientV2.Options
{
    /// <summary>
    /// REST API 客户端配置选项。
    /// </summary>
    public sealed class RestApiOptions
    {
        /// <summary>
        /// 服务端基础地址。
        /// </summary>
        public string BaseAddress { get; set; } = string.Empty;

        /// <summary>
        /// 请求超时时间（秒）；默认 30 秒。
        /// </summary>
        public int TimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// 是否启用身份验证；默认 true。
        /// </summary>
        public bool EnableAuthorization { get; set; } = true;

        /// <summary>
        /// 是否启用 HTTP 日志记录；默认 true。
        /// </summary>
        public bool EnableLogging { get; set; } = true;
    }
}
