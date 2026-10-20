using Microsoft.AspNetCore.Mvc;
using NewLife.Data;
using NewLife.JT1078.Models;
using NewLife.JT1078.Protocols;
using NewLife.JT1078.Server;
using NewLife.Log;

namespace NewLife.JT1078.Web.Controllers;

/// <summary>设备命令 API。集群环境下自动路由到目标设备所在的服务器</summary>
[ApiController]
[Route("[controller]")]
public class CommandController : ControllerBase
{
    #region 属性
    private JT1078Server Server { get; }
    private DeviceSessionManager DeviceManager { get; }

    /// <summary>实例化命令控制器</summary>
    public CommandController(JT1078Server server)
    {
        Server = server;
        DeviceManager = server.DeviceManager;
    }
    #endregion

    #region 信令下发
    /// <summary>向终端设备发送下行信令</summary>
    /// <param name="request">命令请求</param>
    /// <returns>命令发送结果</returns>
    [HttpPost("Send")]
    public IActionResult Send([FromBody] CommandRequest request)
    {
        if (request == null) return BadRequest(L.T("Request must not be null."));

        if (DeviceManager == null)
            throw new InvalidOperationException(L.T("Device command bus is not initialized."));

        // 构造 JT1078 RTP 报文
        var data = request.Data?.ToHex();
        var payload = data != null ? (IPacket)new ArrayPacket(data) : null;
        var result = DeviceManager.Publish(
            request.SIM,
            request.Channel,
            request.AVType,
            request.DataType,
            payload
        );

        return Ok(new { success = result, message = result ? L.T("Command sent.") : L.T("Device is offline; the command has been broadcast to the cluster.") });
    }

    /// <summary>向终端设备发送下行信令（异步）</summary>
    [HttpPost("SendAsync")]
    public async Task<IActionResult> SendAsync([FromBody] CommandRequest request, CancellationToken cancellationToken)
    {
        if (request == null) return BadRequest(L.T("Request must not be null."));

        if (DeviceManager == null)
            throw new InvalidOperationException(L.T("Device command bus is not initialized."));

        var data = request.Data?.ToHex();
        var payload = data != null ? (IPacket)new ArrayPacket(data) : null;
        var result = await DeviceManager.PublishAsync(
            request.SIM,
            request.Channel,
            request.AVType,
            request.DataType,
            payload,
            cancellationToken
        ).ConfigureAwait(false);

        return Ok(new { success = result, message = result ? L.T("Command sent.") : L.T("Device is offline; the command has been broadcast to the cluster.") });
    }

    /// <summary>列出本地已连接的设备</summary>
    [HttpGet("Devices")]
    public IActionResult GetDevices()
    {
        if (DeviceManager == null)
            return Ok(Array.Empty<Object>());

        var sessions = DeviceManager.GetSessions();
        var list = new List<Object>();
        foreach (var s in sessions)
        {
            if (s is JT1078Session tcp)
                list.Add(new { tcp.DeviceKey, tcp.Remote, tcp.LastActive, Type = "TCP" });
            else if (s is JT1078UdpSession udp)
                list.Add(new { DeviceKey = udp.DeviceKey, Remote = udp.RemoteEndPoint?.ToString(), udp.LastActive, Type = "UDP" });
        }

        return Ok(list);
    }
    #endregion
}

/// <summary>命令请求参数</summary>
public class CommandRequest
{
    /// <summary>SIM 卡号</summary>
    public String SIM { get; set; }

    /// <summary>逻辑通道号</summary>
    public Int32 Channel { get; set; }

    /// <summary>音视频编码类型</summary>
    public AVTypes AVType { get; set; }

    /// <summary>数据类型（下行信令）</summary>
    public DataTypes DataType { get; set; }

    /// <summary>负载数据（十六进制字符串）</summary>
    public String Data { get; set; }
}


