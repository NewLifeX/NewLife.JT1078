namespace NewLife.JT1078.Server;

/// <summary>JT1078 UDP 会话。代表一个 UDP 设备连接</summary>
public class JT1078UdpSession
{
    /// <summary>设备标识。SIM_Channel</summary>
    public String DeviceKey { get; set; }

    /// <summary>SIM 卡号</summary>
    public String SIM { get; set; }

    /// <summary>逻辑通道号</summary>
    public Int32 Channel { get; set; }

    /// <summary>远程端点</summary>
    public System.Net.EndPoint RemoteEndPoint { get; set; }

    /// <summary>最后活跃时间</summary>
    public DateTime LastActive { get; set; } = DateTime.Now;
}
