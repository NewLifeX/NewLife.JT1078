using NewLife.JT1078.Protocols;
using NewLife.Data;

namespace NewLife.JT1078.Server;

/// <summary>消息宿主接口。向终端设备发送信令</summary>
public interface IMsgHost
{
    /// <summary>向设备发送下行信令</summary>
    Task<Int32> SendJT(AVTypes avtype, DataTypes dataType, Packet data);
}
