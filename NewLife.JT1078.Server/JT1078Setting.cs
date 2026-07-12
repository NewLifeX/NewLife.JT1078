using NewLife.Configuration;

namespace NewLife.JT1078.Server;

/// <summary>JT1078 服务端配置</summary>
[Config("JT1078")]
public class JT1078Setting : Config<JT1078Setting>
{
    /// <summary>调试模式</summary>
    public Boolean Debug { get; set; }

    /// <summary>监听端口</summary>
    public Int32 Port { get; set; } = 1078;
}
