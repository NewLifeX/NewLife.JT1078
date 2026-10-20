using NewLife.JT1078.Models;
using NewLife.JT1078.Server;
using Microsoft.AspNetCore.Mvc;

namespace NewLife.JT1078.Web;

/// <summary>录像回放控制器。提供历史音视频回放端点</summary>
[ApiController]
public class RecordController : ControllerBase
{
    private readonly SessionManager _sessionManager;
    private readonly FileCache _fileCache;

    /// <summary>实例化录像回放控制器</summary>
    public RecordController(SessionManager sessionManager)
    {
        _sessionManager = sessionManager;
        _fileCache = new FileCache();
    }

    /// <summary>FLV 录像回放</summary>
    [Route("record.flv")]
    public async Task OnFlv([FromQuery] RecordModel model)
    {
        await StreamRecord(model, "flv", "video/x-flv");
    }

    /// <summary>fMP4 录像回放</summary>
    [Route("record.mp4")]
    public async Task OnMp4([FromQuery] RecordModel model)
    {
        await StreamRecord(model, "mp4", "video/mp4");
    }

    /// <summary>AAC 音频录像回放</summary>
    [Route("record.aac")]
    public async Task OnAac([FromQuery] RecordModel model)
    {
        await StreamRecord(model, "aac", "audio/aac");
    }

    /// <summary>PCM 音频录像回放</summary>
    [Route("record.pcm")]
    public async Task OnPcm([FromQuery] RecordModel model)
    {
        await StreamRecord(model, "pcm", "audio/L16;rate=8000;channels=1");
    }

    /// <summary>WAV 音频录像回放</summary>
    [Route("record.wav")]
    public async Task OnWav([FromQuery] RecordModel model)
    {
        await StreamRecord(model, "wav", "audio/wav");
    }

    private async Task StreamRecord(RecordModel model, String fileType, String contentType)
    {
        var response = HttpContext.Response;

        // 从文件缓存读取录制数据
        var date = model.Date?.Length == 8
            ? DateTime.ParseExact(model.Date, "yyyyMMdd", null)
            : (DateTime?)null;
        var data = _fileCache.ReadFile(model.SIM, model.Channel, fileType, date);

        if (data == null || data.Length == 0)
        {
            response.StatusCode = 404;
            await response.WriteAsync(L.T("Recording file not found."));
            return;
        }

        response.ContentType = contentType;
        response.Headers["Content-Length"] = data.Length.ToString();
        response.Headers["Cache-Control"] = "no-cache";
        await response.Body.WriteAsync(data);
        await response.Body.FlushAsync();
    }
}

/// <summary>录像回放查询参数</summary>
public class RecordModel
{
    /// <summary>SIM 卡号</summary>
    public String SIM { get; set; } = "";

    /// <summary>逻辑通道号</summary>
    public Int32 Channel { get; set; } = 1;

    /// <summary>录制日期，格式 yyyyMMdd。默认当天</summary>
    public String? Date { get; set; }

    /// <summary>设备认证 Token</summary>
    public String? Token { get; set; }
}

