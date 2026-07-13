namespace NewLife.JT1078.Client.Models;

/// <summary>服务器连接配置</summary>
public class ConnectionConfig
{
    /// <summary>服务器主机地址</summary>
    public String Host { get; set; } = "127.0.0.1";

    /// <summary>HTTP/WebSocket 端口（Web 项目端口）</summary>
    public Int32 HttpPort { get; set; } = 5000;

    /// <summary>TCP 端口（JT1078 服务端端口）</summary>
    public Int32 TcpPort { get; set; } = 1078;

    /// <summary>UDP 端口</summary>
    public Int32 UdpPort { get; set; } = 1078;

    /// <summary>认证 Token</summary>
    public String? Token { get; set; } = "demo";

    /// <summary>HTTP 基础地址</summary>
    public String HttpBaseUrl => $"http://{Host}:{HttpPort}";

    /// <summary>WebSocket 基础地址</summary>
    public String WsBaseUrl => $"ws://{Host}:{HttpPort}";

    /// <summary>TCP 地址</summary>
    public String TcpAddress => $"{Host}:{TcpPort}";

    /// <summary>UDP 地址</summary>
    public String UdpAddress => $"{Host}:{UdpPort}";
}
