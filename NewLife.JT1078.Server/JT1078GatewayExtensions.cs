using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace NewLife.JT1078.Server;

/// <summary>JT1078 网关构建器。提供流式注册 TCP/UDP/HTTP 等服务</summary>
public interface IJT1078GatewayBuilder
{
    /// <summary>服务集合</summary>
    IServiceCollection Services { get; }
}

/// <summary>JT1078 网关构建器扩展方法</summary>
public static class JT1078GatewayExtensions
{
    /// <summary>添加 JT1078 网关核心服务</summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">配置</param>
    /// <returns>网关构建器</returns>
    public static IJT1078GatewayBuilder AddJT1078Gateway(this IServiceCollection services, Object configuration = null)
    {
        // 注册核心服务
        services.TryAddSingleton<JT1078Server>();
        services.TryAddSingleton<SessionManager>();
        services.TryAddSingleton<IJT1078Authorization, JT1078AuthorizationDefault>();

        // 注册设备命令总线
        services.AddSingleton<DeviceSessionManager>();

        // 注册托管服务
        services.AddHostedService<JT1078HostedService>();

        return new JT1078GatewayBuilderDefault(services);
    }

    /// <summary>启用 TCP 服务</summary>
    public static IJT1078GatewayBuilder AddTcp(this IJT1078GatewayBuilder builder)
    {
        // TCP 服务已默认启用，此方法为显式声明
        return builder;
    }

    /// <summary>启用 HTTP 服务（默认授权）</summary>
    public static IJT1078GatewayBuilder AddHttp(this IJT1078GatewayBuilder builder)
    {
        // HTTP 服务通过 ASP.NET Core 自动注册
        return builder;
    }

    /// <summary>启用 HTTP 服务（自定义授权）</summary>
    public static IJT1078GatewayBuilder AddHttp<TAuthorization>(this IJT1078GatewayBuilder builder)
        where TAuthorization : class, IJT1078Authorization
    {
        builder.Services.Replace(ServiceDescriptor.Singleton<IJT1078Authorization, TAuthorization>());
        return builder;
    }

    /// <summary>设置自定义授权实现</summary>
    public static IJT1078GatewayBuilder SetAuthorization<TAuthorization>(this IJT1078GatewayBuilder builder)
        where TAuthorization : class, IJT1078Authorization
    {
        builder.Services.Replace(ServiceDescriptor.Singleton<IJT1078Authorization, TAuthorization>());
        return builder;
    }
}

/// <summary>JT1078 网关构建器默认实现</summary>
internal class JT1078GatewayBuilderDefault : IJT1078GatewayBuilder
{
    public IServiceCollection Services { get; }

    public JT1078GatewayBuilderDefault(IServiceCollection services)
    {
        Services = services;
    }
}
