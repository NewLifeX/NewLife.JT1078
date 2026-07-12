using NewLife.JT1078.Models;
using NewLife.JT1078.Server;
using Microsoft.AspNetCore.Mvc;

namespace NewLife.JT1078.Web;

/// <summary>录像回放控制器。提供历史音视频回放端点</summary>
[ApiController]
public class RecordController : ControllerBase
{
    private readonly SessionManager _sessionManager;

    /// <summary>实例化录像回放控制器</summary>
    public RecordController(SessionManager sessionManager)
    {
        _sessionManager = sessionManager;
    }

    /// <summary>FLV 录像回放</summary>
    [Route("record.flv")]
    public async Task OnFlv([FromQuery] LiveModel model)
    {
        await StreamRecord(model, RTPType.Flv, "video/x-flv");
    }

    /// <summary>fMP4 录像回放</summary>
    [Route("record.mp4")]
    public async Task OnMp4([FromQuery] LiveModel model)
    {
        await StreamRecord(model, RTPType.Mp4, "video/mp4");
    }

    /// <summary>AAC 音频录像回放</summary>
    [Route("record.aac")]
    public async Task OnAac([FromQuery] LiveModel model)
    {
        await StreamRecord(model, RTPType.Aac, "audio/aac");
    }

    /// <summary>PCM 音频录像回放</summary>
    [Route("record.pcm")]
    public async Task OnPcm([FromQuery] LiveModel model)
    {
        await StreamRecord(model, RTPType.Pcm, "audio/L16;rate=8000;channels=1");
    }

    /// <summary>WAV 音频录像回放</summary>
    [Route("record.wav")]
    public async Task OnWav([FromQuery] LiveModel model)
    {
        await StreamRecord(model, RTPType.Wav, "audio/wav");
    }

    private async Task StreamRecord(LiveModel model, RTPType type, String contentType)
    {
        var response = HttpContext.Response;
        response.ContentType = contentType;
        response.Headers["Cache-Control"] = "no-cache";

        var session = new HttpSession(async data =>
        {
            await response.Body.WriteAsync(data);
            await response.Body.FlushAsync();
            return;
        })
        {
            SIM = model.SIM,
            Channel = model.Channel,
            Type = type,
            IP = HttpContext.Connection.RemoteIpAddress?.ToString(),
        };

        _sessionManager.Add(session);

        try
        {
            await Task.Delay(Timeout.Infinite, HttpContext.RequestAborted);
        }
        catch (OperationCanceledException) { }
        finally
        {
            _sessionManager.Remove(session);
            session.Dispose();
        }
    }
}
