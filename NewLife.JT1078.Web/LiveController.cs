using System.Net.WebSockets;
using NewLife.JT1078.Models;
using NewLife.JT1078.Server;
using Microsoft.AspNetCore.Mvc;

namespace NewLife.JT1078.Web;

/// <summary>直播控制器。提供 HTTP-FLV、fMP4、音频等实时流端点</summary>
[ApiController]
public class LiveController : ControllerBase
{
    private readonly SessionManager _sessionManager;

    /// <summary>实例化直播控制器</summary>
    public LiveController(SessionManager sessionManager)
    {
        _sessionManager = sessionManager;
    }

    /// <summary>HTTP-FLV 直播</summary>
    [Route("live.flv")]
    public async Task OnFlv([FromQuery] LiveModel model)
    {
        var context = HttpContext;
        if (context.WebSockets.IsWebSocketRequest)
            await ProcessWebSocket(context, model, RTPType.Flv);
        else
            await ProcessHttp(context, model, RTPType.Flv);
    }

    /// <summary>fMP4 直播</summary>
    [Route("live.mp4")]
    public async Task OnMp4([FromQuery] LiveModel model)
    {
        var context = HttpContext;
        if (context.WebSockets.IsWebSocketRequest)
            await ProcessWebSocket(context, model, RTPType.Mp4);
        else
            await ProcessHttp(context, model, RTPType.Mp4);
    }

    /// <summary>PCM 原始音频直播</summary>
    [Route("live.pcm")]
    public async Task OnPcm([FromQuery] LiveModel model)
    {
        var context = HttpContext;
        if (context.WebSockets.IsWebSocketRequest)
            await ProcessWebSocket(context, model, RTPType.Pcm);
        else
            await ProcessHttp(context, model, RTPType.Pcm);
    }

    /// <summary>AAC 音频直播</summary>
    [Route("live.aac")]
    public async Task OnAac([FromQuery] LiveModel model)
    {
        var context = HttpContext;
        if (context.WebSockets.IsWebSocketRequest)
            await ProcessWebSocket(context, model, RTPType.Aac);
        else
            await ProcessHttp(context, model, RTPType.Aac);
    }

    /// <summary>WAV 音频直播</summary>
    [Route("live.wav")]
    public async Task OnWav([FromQuery] LiveModel model)
    {
        var context = HttpContext;
        if (context.WebSockets.IsWebSocketRequest)
            await ProcessWebSocket(context, model, RTPType.Wav);
        else
            await ProcessHttp(context, model, RTPType.Wav);
    }

    private async Task ProcessWebSocket(HttpContext context, LiveModel model, RTPType type)
    {
        if (!context.WebSockets.IsWebSocketRequest) return;

        var socket = await context.WebSockets.AcceptWebSocketAsync();
        var session = new HttpSession(async data =>
        {
            if (socket.State == WebSocketState.Open)
                await socket.SendAsync(new ArraySegment<Byte>(data), WebSocketMessageType.Binary, true, CancellationToken.None);
            return;
        })
        {
            SIM = model.SIM,
            Channel = model.Channel,
            Type = type,
            IP = context.Connection.RemoteIpAddress?.ToString(),
        };

        _sessionManager.Add(session);

        try
        {
            var buffer = new Byte[1024];
            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(new ArraySegment<Byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                    break;
            }
        }
        finally
        {
            _sessionManager.Remove(session);
            session.Dispose();
        }
    }

    private async Task ProcessHttp(HttpContext context, LiveModel model, RTPType type)
    {
        var response = context.Response;
        response.ContentType = type switch
        {
            RTPType.Flv => "video/x-flv",
            RTPType.Mp4 => "video/mp4",
            _ => "application/octet-stream",
        };
        response.Headers["Cache-Control"] = "no-cache";
        response.Headers["Connection"] = "keep-alive";

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
            IP = context.Connection.RemoteIpAddress?.ToString(),
        };

        _sessionManager.Add(session);

        try
        {
            await Task.Delay(Timeout.Infinite, context.RequestAborted);
        }
        catch (OperationCanceledException) { }
        finally
        {
            _sessionManager.Remove(session);
            session.Dispose();
        }
    }
}
