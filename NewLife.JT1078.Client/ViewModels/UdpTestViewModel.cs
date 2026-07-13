using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NewLife.JT1078.Client.Models;
using NewLife.JT1078.Client.Services;
using NewLife.JT1078.Protocols;

namespace NewLife.JT1078.Client.ViewModels;

/// <summary>UDP 直连测试 ViewModel</summary>
public partial class UdpTestViewModel : ObservableObject
{
    private readonly ConnectionConfig _config;
    private UdpClientService? _udpClient;

    /// <summary>实例化 UDP 直连测试 ViewModel</summary>
    public UdpTestViewModel(ConnectionConfig config)
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
    private Int32 _selectedAvType = 4;

    /// <summary>DataType 索引</summary>
    [ObservableProperty]
    private Int32 _selectedDataType;

    /// <summary>Payload 十六进制数据</summary>
    [ObservableProperty]
    private String _hexPayload = "";

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
    private void Connect()
    {
        if (IsConnected) return;

        try
        {
            Status = "⏳ 连接中...";
            _udpClient = new UdpClientService();
            _udpClient.OnMessageReceived += OnMessageReceived;
            _udpClient.OnRawDataReceived += OnRawDataReceived;
            _udpClient.OnConnectionChanged += OnConnectionChanged;
            _udpClient.OnError += OnError;

            _udpClient.Connect(_config.Host, _config.UdpPort);
        }
        catch (Exception ex)
        {
            Status = $"❌ 连接失败: {ex.Message}";
        }
    }

    /// <summary>断开</summary>
    [RelayCommand]
    private void Disconnect()
    {
        if (_udpClient != null)
        {
            _udpClient.Disconnect();
            _udpClient.Dispose();
            _udpClient = null;
        }
        IsConnected = false;
        Status = "已断开";
    }

    /// <summary>发送消息</summary>
    [RelayCommand]
    private async Task SendMessage()
    {
        if (_udpClient == null || !IsConnected)
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

            await _udpClient.SendAsync(msg);

            Messages.Insert(0, new MessageLogEntry
            {
                Direction = "→ 发送",
                Summary = $"{avType} {dataType} SIM={Sim} CH={Channel}",
                MessageType = "JTMessage",
            });
        }
        catch (Exception ex)
        {
            Status = $"❌ 发送失败: {ex.Message}";
        }
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

        if (decoded != null && decoded.IsParsed)
        {
            DecodedMessages.Insert(0, decoded);
            SelectedDecodedMessage = decoded;
        }
    }

    private void OnRawDataReceived(Byte[] data)
    {
        // 原始数据也在消息列表显示
    }

    private void OnConnectionChanged(Boolean connected)
    {
        IsConnected = connected;
        Status = connected ? "已连接" : "已断开";
    }

    private void OnError(Exception ex)
    {
        Status = $"❌ 错误: {ex.Message}";
        IsConnected = false;
    }
}
