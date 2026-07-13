namespace NewLife.JT1078.Client.Models;

/// <summary>设备信息</summary>
public class DeviceInfo
{
    /// <summary>设备标识</summary>
    public String DeviceKey { get; set; } = "";

    /// <summary>远程地址</summary>
    public String? Remote { get; set; }

    /// <summary>最后活跃时间</summary>
    public DateTime LastActive { get; set; }

    /// <summary>连接类型</summary>
    public String Type { get; set; } = "TCP";
}
