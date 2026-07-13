using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NewLife.JT1078.Client.Models;
using NewLife.JT1078.Client.Services;
using NewLife.JT1078.Protocols;

namespace NewLife.JT1078.Client.ViewModels;

/// <summary>TCP 直连测试 ViewModel</summary>
public partial class TcpTestViewModel : ObservableObject
{
    private readonly ConnectionConfig _config;
    private TcpClientService? _tcpClient;

    /// <summary>实例化 TCP 直连测试 ViewModel</summary>
    public TcpTestViewModel(ConnectionConfig config)
    {
        _config = config;
    }

    #region 属性
    /// <summary>是否已连接</summary>
    [ObservableProperty]
    private Boolean _isConnected;

    /// <summary>状态信息</summary>
    [ObservableProperty]
    private String _status = "就绪";

    /// <summary>SIM 卡号</summary>
    [ObservableProperty]
    private String _sim = "138001380000";

    /// <summary>逻辑通道号</summary>
    [ObservableProperty]
    private Byte _channel = 1;

    /// <summary>AVType 索引</summary>
    [ObservableProperty]
    private Int32 _selectedAvType = 4; // H264

    /// <summary>DataType 索引 0=I帧,1=P帧,2=B帧,3=音频,4=透传</summary>
    [ObservableProperty]
    private Int32 _selectedDataType;

    /// <summary>Payload 十六进制数据</summary>
    [ObservableProperty]
    private String _hexPayload = "";

    /// <summary>是否模拟发送</summary>
    [ObservableProperty]
    private Boolean _isSimulating;

    /// <summary>接收消息列表</summary>
    public ObservableCollection<MessageLogEntry> Messages { get; } = [];

    /// <summary>解码后的协议指令消息列表</summary>
    public ObservableCollection<DecodedMessage> DecodedMessages { get; } = [];

    /// <summary>当前选中的解码消息</summary>
    [ObservableProperty]
    private DecodedMessage? _selectedDecodedMessage;

    /// <summary>当前选中的消息日志条目</summary>
    [ObservableProperty]
    private MessageLogEntry? _selectedMessage;

    partial void OnSelectedMessageChanged(MessageLogEntry? value)
    {
        SelectedDecodedMessage = value?.DecodedMessage;
    }

    /// <summary>可用 AVTypes</summary>
    public String[] AVTypes { get; } = ["G711A", "G711U", "ADPCMA", "AAC", "H264", "H265", "MP3", "Transparent"];

    /// <summary>可用 DataTypes</summary>
    public String[] DataTypes { get; } = ["Video I帧", "Video P帧", "Video B帧", "音频帧", "透传数据"];
    #endregion

    /// <summary>连接</summary>
    [RelayCommand]
    private async Task Connect()
    {
        if (IsConnected) return;

        try
        {
            Status = "⏳ 连接中...";
            _tcpClient = new TcpClientService();
            _tcpClient.OnMessageReceived += OnMessageReceived;
            _tcpClient.OnConnectionChanged += OnConnectionChanged;
            _tcpClient.OnError += OnError;

            await _tcpClient.ConnectAsync(_config.Host, _config.TcpPort);
        }
        catch (Exception ex)
        {
            Status = $"❌ 连接失败: {ex.Message}";
        }
    }

    /// <summary>断开</summary>
    [RelayCommand]
    private async Task Disconnect()
    {
        if (_tcpClient != null)
        {
            await _tcpClient.DisconnectAsync();
            _tcpClient.Dispose();
            _tcpClient = null;
        }
        IsConnected = false;
        IsSimulating = false;
        Status = "已断开";
    }

    /// <summary>发送消息</summary>
    [RelayCommand]
    private async Task SendMessage()
    {
        if (_tcpClient == null || !IsConnected)
        {
            Status = "❌ 未连接到服务器";
            return;
        }

        try
        {
            var avTypeStr = AVTypes[SelectedAvType];
            Enum.TryParse<AVTypes>(avTypeStr, out var avType);

            var dataType = SelectedDataType switch
            {
                0 => Protocols.DataTypes.VideoIFrame,
                1 => Protocols.DataTypes.VideoPFrame,
                2 => Protocols.DataTypes.VideoBFrame,
                3 => Protocols.DataTypes.AudioFrame,
                _ => Protocols.DataTypes.Transparent
            };

            var payload = String.IsNullOrEmpty(HexPayload) ? null : HexPayload.ToHex();

            var msg = MessageBuilder.Build(
                avType, dataType, PacketFlags.Alone,
                Sim, Channel,
                payload: payload
            );

            await _tcpClient.SendAsync(msg);

            var pk = msg.ToPacket();
            Messages.Insert(0, new MessageLogEntry
            {
                Direction = "→ 发送",
                Summary = $"{avType} {dataType} Seq={msg.Sequence} SIM={Sim} CH={Channel}",
                HexData = BitConverter.ToString(pk.GetSpan().ToArray(), 0, Math.Min(64, (Int32)pk.Total)),
                MessageType = "JTMessage",
            });
        }
        catch (Exception ex)
        {
            Status = $"❌ 发送失败: {ex.Message}";
        }
    }

    /// <summary>发送原始数据</summary>
    [RelayCommand]
    private async Task SendRaw()
    {
        if (_tcpClient == null || !IsConnected || String.IsNullOrEmpty(HexPayload)) return;

        try
        {
            var data = HexPayload.ToHex();
            await _tcpClient.SendRawAsync(data);

            Messages.Insert(0, new MessageLogEntry
            {
                Direction = "→ 发送",
                Summary = $"原始数据 {data.Length} 字节",
                HexData = HexPayload,
                MessageType = "Raw",
            });
        }
        catch (Exception ex)
        {
            Status = $"❌ 发送失败: {ex.Message}";
        }
    }

    /// <summary>模拟发送（定时发送测试帧）</summary>
    [RelayCommand]
    private async Task ToggleSimulate()
    {
        if (IsSimulating)
        {
            IsSimulating = false;
            Status = "模拟已停止";
            return;
        }

        if (_tcpClient == null || !IsConnected)
        {
            Status = "❌ 未连接到服务器";
            return;
        }

        IsSimulating = true;
        Status = "▶ 模拟发送中...";

        _ = Task.Run(async () =>
        {
            while (IsSimulating && _tcpClient != null && IsConnected)
            {
                try
                {
                    // 交替发送 I 帧和 P 帧
                    var isIFrame = DateTime.Now.Second % 5 == 0;
                    var msg = isIFrame
                        ? MessageBuilder.BuildVideoIFrame(Sim, Channel)
                        : MessageBuilder.BuildAudioFrame(Sim, Channel);

                    await _tcpClient.SendAsync(msg);

                    Messages.Insert(0, new MessageLogEntry
                    {
                        Direction = "→ 发送",
                        Summary = $"[模拟] {(isIFrame ? "H264 I帧" : "G711A 音频")} Seq={msg.Sequence}",
                        MessageType = "Simulate",
                    });
                }
                catch { break; }

                // 每 100ms 发送一帧（模拟 10fps）
                await Task.Delay(100);
            }

            IsSimulating = false;
            Status = "模拟已结束";
        });
    }

    private void OnMessageReceived(JTMessage msg)
    {
        var summary = $"{msg.Type} {msg.DataType} Seq={msg.Sequence} SIM={msg.SIM} CH={msg.Channel}";
        var hexData = "";
        var msgType = "JTMessage";

        // 尝试解码协议指令
        DecodedMessage? decoded = null;
        if (msg.Payload != null && msg.Payload.Total > 0)
        {
            var payload = msg.Payload.GetSpan().ToArray();
            hexData = BitConverter.ToString(payload, 0, Math.Min(64, payload.Length));

            // 透传数据类型通常包含协议指令
            if (msg.DataType == Protocols.DataTypes.Transparent || payload.Length >= 2)
            {
                decoded = ProtocolDecoder.Decode(payload);
                if (decoded != null && decoded.IsParsed)
                {
                    summary = $"← [{decoded.KindName}] {decoded.Description}";
                    msgType = decoded.KindName;
                }
            }
        }

        Messages.Insert(0, new MessageLogEntry
        {
            Direction = "← 接收",
            Summary = summary,
            HexData = hexData,
            MessageType = msgType,
            DecodedMessage = decoded,
        });

        // 添加到解码消息列表
        if (decoded != null && decoded.IsParsed)
        {
            DecodedMessages.Insert(0, decoded);
            SelectedDecodedMessage = decoded;
        }
    }

    private void OnConnectionChanged(Boolean connected)
    {
        IsConnected = connected;
        Status = connected ? "已连接" : "已断开";
        if (!connected) IsSimulating = false;
    }

    private void OnError(Exception ex)
    {
        Status = $"❌ 错误: {ex.Message}";
        IsConnected = false;
        IsSimulating = false;
    }
}
