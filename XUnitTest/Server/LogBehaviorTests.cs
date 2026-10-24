using System.Collections.Concurrent;
using System.Linq;
using NewLife.JT1078.Server;
using NewLife.Log;
using Xunit;

namespace XUnitTest.Server;

/// <summary>日志与生命周期行为测试。日志提供者为进程级全局状态，禁止与其他全局状态测试并行</summary>
[Collection("GlobalState")]
public class LogBehaviorTests
{
    private sealed class MemoryLogger : Logger
    {
        /// <summary>并发安全的日志集合。其他测试类可能与当前测试并行输出日志</summary>
        public ConcurrentQueue<String> Messages { get; } = new();

        protected override void OnWrite(LogLevel level, String format, params Object?[] args) => Messages.Enqueue(Format(format, args));
    }

    [Fact(DisplayName = "HttpSession.Detach 从消息中枢移除订阅者")]
    public void Detach_RemovesUser()
    {
        using var hub = new MsgHub { SIM = "DETACH-1", Channel = 1 };
        var session = new HttpSession(_ => ValueTask.CompletedTask) { SIM = "DETACH-1", Channel = 1 };

        Assert.True(hub.AddUser(session));
        Assert.True(hub.HasUsers);

        session.Detach(hub);
        Assert.False(hub.HasUsers);
        Assert.Null(session.Hub);
    }

    [Fact(DisplayName = "消息中枢重复关闭只记录一次日志")]
    public void MsgHub_CloseTwice_LogsOnce()
    {
        var prev = XTrace.Log;
        var logger = new MemoryLogger();
        XTrace.Log = logger;
        try
        {
            var hub = new MsgHub { SIM = "CLOSE-TWICE-1", Channel = 1 };
            hub.Close("first");
            hub.Dispose();

            var closes = logger.Messages.Count(e => e.Contains("CLOSE-TWICE-1"));
            Assert.Equal(1, closes);
        }
        finally
        {
            XTrace.Log = prev;
        }
    }

    [Fact(DisplayName = "HttpSession 重复关闭只记录一次日志")]
    public void HttpSession_CloseTwice_LogsOnce()
    {
        var prev = XTrace.Log;
        var logger = new MemoryLogger();
        XTrace.Log = logger;
        try
        {
            using var hub = new MsgHub { SIM = "SESSION-CLOSE-1", Channel = 1 };
            var session = new HttpSession(_ => ValueTask.CompletedTask) { SIM = "SESSION-CLOSE-1", Channel = 1 };
            hub.AddUser(session);

            session.Close("first");
            session.Close("second");
            session.Dispose();

            // 日志形如 "HttpSession 关闭 Id=1 Reason=first"，重复关闭不应再产生日志
            var first = logger.Messages.Where(e => e.Contains("Reason=first")).ToArray();
            Assert.Single(first);
            Assert.DoesNotContain(logger.Messages, e => e.Contains("Reason=second"));
            Assert.StartsWith("HttpSession ", first[0]);
        }
        finally
        {
            XTrace.Log = prev;
        }
    }

    [Fact(DisplayName = "消息中枢注册表并发只创建一个实例")]
    public void Registry_Concurrent_SingleInstance()
    {
        const String sim = "REG-CONCURRENT-1";
        var hubs = new MsgHub[8];
        Parallel.For(0, hubs.Length, i => hubs[i] = MsgHubRegistry.GetOrAdd(sim, 1));

        foreach (var hub in hubs)
        {
            Assert.Same(hubs[0], hub);
        }

        MsgHubRegistry.Remove(sim, 1);
        Assert.Null(MsgHubRegistry.Get(sim, 1));
    }
}
