namespace NewLife.JT1078.Server;

/// <summary>JT1078 流媒体授权接口。控制 HTTP/WS 直播端点的访问权限</summary>
public interface IJT1078Authorization
{
    /// <summary>验证请求是否授权</summary>
    /// <param name="sim">SIM 卡号</param>
    /// <param name="token">Token 令牌</param>
    /// <param name="ip">客户端 IP</param>
    /// <returns>是否授权通过</returns>
    Boolean Authorize(String sim, String token, String ip);
}

/// <summary>默认授权实现（允许所有请求）</summary>
public class JT1078AuthorizationDefault : IJT1078Authorization
{
    /// <summary>默认允许所有请求</summary>
    public Boolean Authorize(String sim, String token, String ip) => true;
}
