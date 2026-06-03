using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Refit;
using RestSharpClientV2.Abstractions;
using RestSharpClientV2.Handlers;
using RestSharpClientV2.Options;
using RestSharpClientV2.Refit;

namespace RestSharpClientV2.Extensions
{
    /// <summary>
    /// 提供 <see cref="IServiceCollection"/> 的 REST API 相关扩展方法。
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 注册 REST API 客户端及相关基础设施到依赖注入容器。
        /// </summary>
        /// <param name="services">服务集合。</param>
        /// <param name="configure">用于配置 <see cref="RestApiOptions"/> 的委托。</param>
        /// <returns>返回服务集合，支持链式调用。</returns>
        public static IServiceCollection AddRestApi(
            this IServiceCollection services,
            Action<RestApiOptions> configure)
        {
            services.Configure(configure);

            services.AddTransient<AuthHeaderHandler>();
            services.AddTransient<LoggingHandler>();

            services
                .AddRefitClient<IGenericRefitApi>()
                .ConfigureHttpClient((sp, client) =>
                {
                    var options = sp
                        .GetRequiredService<IOptions<RestApiOptions>>()
                        .Value;

                    client.BaseAddress = new Uri(options.BaseAddress);
                    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
                })
                .AddHttpMessageHandler<AuthHeaderHandler>()
                .AddHttpMessageHandler<LoggingHandler>();

            services.AddScoped<IRestApiClient, RefitRestApiClient>();

            return services;
        }
    }
}
