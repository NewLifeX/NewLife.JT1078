using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Platform;
using LibVLCSharp.Shared;

namespace NewLife.JT1078.Client.Controls;

/// <summary>LibVLC 视频渲染表面。在 Windows 上创建原生子窗口，并把窗口句柄交给播放器渲染</summary>
public class VideoSurface : NativeControlHost
{
    private const Int32 WS_CHILD = 0x40000000;
    private const Int32 WS_VISIBLE = 0x10000000;
    private const Int32 WS_CLIPCHILDREN = 0x02000000;
    private const Int32 WS_CLIPSIBLINGS = 0x04000000;

    private IPlatformHandle? _nativeHandle;
    private MediaPlayer? _player;

    /// <summary>要渲染的播放器，null 表示不渲染</summary>
    public MediaPlayer? Player
    {
        get => _player;
        set
        {
            _player = value;
            AttachPlayer();
        }
    }

    /// <summary>原生窗口是否已创建并可承载渲染</summary>
    public Boolean IsHandleReady => _nativeHandle != null && _nativeHandle.Handle != IntPtr.Zero;

    /// <inheritdoc/>
    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        if (!OperatingSystem.IsWindows() || parent.Handle == IntPtr.Zero) return base.CreateNativeControlCore(parent);

        var hwnd = CreateWindowEx(0, "static", null, WS_CHILD | WS_VISIBLE | WS_CLIPCHILDREN | WS_CLIPSIBLINGS,
                                  0, 0, 1, 1, parent.Handle, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
        _nativeHandle = new PlatformHandle(hwnd, "HWND");
        AttachPlayer();

        return _nativeHandle;
    }

    /// <inheritdoc/>
    protected override void DestroyNativeControlCore(IPlatformHandle control)
    {
        DetachPlayer();
        if (_nativeHandle != null)
        {
            DestroyWindow(_nativeHandle.Handle);
            _nativeHandle = null;
        }

        base.DestroyNativeControlCore(control);
    }

    private void AttachPlayer()
    {
        if (_player != null && IsHandleReady) _player.Hwnd = _nativeHandle!.Handle;
    }

    private void DetachPlayer()
    {
        if (_player == null || !IsHandleReady) return;

        try
        {
            _player.Hwnd = IntPtr.Zero;
        }
        catch (Exception)
        {
            // 播放器停止过程中可能已释放原生引用，忽略解绑失败
        }
    }

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr CreateWindowEx(Int32 dwExStyle, String lpClassName, String? lpWindowName, Int32 dwStyle,
                                                Int32 x, Int32 y, Int32 nWidth, Int32 nHeight,
                                                IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern Boolean DestroyWindow(IntPtr hWnd);
}
