using System.Net.Http.Json;
using NewLife.JT1078.Client.Models;
using NewLife.JT1078.Protocols;

namespace NewLife.JT1078.Client.Services;

/// <summary>HTTP API 服务。封装对 JT1078 Web 服务 REST API 的调用</summary>
public class HttpApiService : IDisposable
{
    private readonly HttpClient _httpClient;
    private ConnectionConfig _config;

    /// <summary>实例化 HTTP API 服务</summary>
    public HttpApiService(ConnectionConfig config)
    {
        _config = config;
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30),
        };
    }

    /// <summary>更新连接配置</summary>
    public void UpdateConfig(ConnectionConfig config) => _config = config;

    /// <summary>查询本地设备列表</summary>
    public async Task<DeviceInfo[]> GetDevicesAsync()
    {
        try
        {
            var url = $"{_config.HttpBaseUrl}/Command/Devices";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var list = await response.Content.ReadFromJsonAsync<DeviceInfo[]>();
            return list ?? [];
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(Localization.Translate("Failed to query device list: {0}", ex.Message), ex);
        }
    }

    /// <summary>发送下行信令</summary>
    public async Task<(Boolean success, String message)> SendCommandAsync(
        String sim, Int32 channel, AVTypes avType, DataTypes dataType, String? hexData)
    {
        try
        {
            var url = $"{_config.HttpBaseUrl}/Command/SendAsync";
            var request = new
            {
                SIM = sim,
                Channel = channel,
                AVType = avType.ToString(),
                DataType = dataType.ToString(),
                Data = hexData ?? ""
            };
            var response = await _httpClient.PostAsJsonAsync(url, request);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<CommandResponse>();
            return (result?.success ?? false, result?.message ?? "");
        }
        catch (Exception ex)
        {
            return (false, $"命令发送失败: {ex.Message}");
        }
    }

    /// <summary>释放资源</summary>
    public void Dispose() => _httpClient.Dispose();

    private class CommandResponse
    {
        public Boolean success { get; set; }
        public String message { get; set; } = "";
    }
}
