# NewLife.JT1078

![GitHub License](https://img.shields.io/github/license/NewLifeX/NewLife.JT1078?logo=github)
![Nuget Downloads](https://img.shields.io/nuget/dt/NewLife.JT1078?logo=nuget)
![Nuget](https://img.shields.io/nuget/v/NewLife.JT1078?logo=nuget)
![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/NewLife.JT1078?label=dev%20nuget&logo=nuget)
[![NativeAOT](https://github.com/NewLifeX/NewLife.JT1078/actions/workflows/aot.yml/badge.svg?branch=master)](https://github.com/NewLifeX/NewLife.JT1078/actions/workflows/aot.yml)
[![test](https://github.com/NewLifeX/NewLife.JT1078/actions/workflows/test.yml/badge.svg?branch=master)](https://github.com/NewLifeX/NewLife.JT1078/actions/workflows/test.yml)
![TFM](https://img.shields.io/badge/TFM-5%20%28net461%20%E2%86%92%20net10.0%29-512BD4?logo=dotnet)
![locales](https://img.shields.io/badge/locales-10-2ea44f?logo=dotnet)
![platform](https://img.shields.io/badge/platform-Windows%20%7C%20Linux%20%7C%20macOS-blue)

<!-- lang:start -->
🌐 [English](README.md) | [简体中文](README.zh-CN.md) | [Español](README.es.md) | [Deutsch](README.de.md) | [日本語](README.ja.md) | [Français](README.fr.md) | [Português](README.pt.md) | [Русский](README.ru.md) | [한국어](README.ko.md) | [العربية](README.ar.md)
<!-- lang:end -->

JT/T 1078 道路運輸車輛衛星定位系統視訊通訊協定的 .NET 實作，涵蓋協定解析、音視訊編解碼、FLV/fMP4 轉封裝、TCP/UDP 伺服器、HTTP/WebSocket 直播推送全鏈路。支援 NativeAOT 與執行階段多語言。

## 功能特性

- **JT1078 RTP 協定**：完整的 RTP 封包解析與建構，支援 29 種音視訊編碼類型
- **訊框定界**：基於 NewLife.Core `IMessageCodec` 的黏包/拆包與標頭先行解析
- **RTP 分包合併**：自動快取並合併 I/P/B 訊框的分包
- **音訊解碼**：G.711A、G.711U、IMA-ADPCM，支援海思晶片標頭剝離
- **H.264 解析**：NAL 單元分割、SPS/PPS 解析、起始碼處理、競爭位元組剝離
- **FLV/fMP4 轉封裝**：即時轉換為 HTTP-FLV / WebSocket-FLV / fMP4 / HLS
- **服務端**：TCP/UDP 雙棧、HTTP/WS 直播端點、錄影回放、IEventBus 叢集命令下發
- **NativeAOT**：核心協定與多語言詞表零反射，原生映像建置通過冒煙驗證
- **多語言**：例外與日誌以英文為源文，內建 10 種語言譯文，缺翻譯自動回退英文

## 快速開始

```bash
dotnet add package NewLife.JT1078           # 核心協定庫（含 FLV/fMP4 轉封裝）
dotnet add package NewLife.JT1078.Server    # TCP/UDP 伺服器 + HLS/MP3
```

```csharp
using System.Buffers;
using NewLife.JT1078.Protocols;

// 訊框定界：標頭到齊即可產出訊息，負載由訊框層綁定
var codec = new JTCodec();
var result = codec.TryParse(new ReadOnlySequence<Byte>(data));
if (result is { Invalid: false } rs && rs.Message is JTMessage msg)
{
    if (msg.Type == AVTypes.H264)
    {
        var frame = new H264Decoder().ParseAVFrame(msg);
    }
}
```

```csharp
using NewLife.JT1078.Server;

var server = new JT1078Server();
server.Start();

// 或 ASP.NET Core 整合
builder.Services.AddJT1078Gateway().AddTcp().AddHttp();
```

## NuGet 套件

| 套件 | 說明 |
|---|---|
| [NewLife.JT1078](https://github.com/NewLifeX/NewLife.JT1078) | 核心協定 + 編解碼 + FLV/fMP4 轉封裝 |
| NewLife.JT1078.Server | TCP/UDP 伺服器 + DI 建構器 + HLS/MP3 編碼 |

## Native AOT

核心協定庫與預設設定的服務端支援 NativeAOT 與裁剪。net8.0 以上目標常駐啟用 AOT/裁剪分析器，建置期零 IL 警告；`AotSmoke` 冒煙工程於 CI 以 `PublishAot=true` 發布原生映像並執行協定編解碼與多語言查表。

```bash
dotnet publish AotSmoke/AotSmoke.csproj -c Release -r linux-x64 -p:PublishAot=true -o out
./out/AotSmoke
```

## 多語言本地化

例外與日誌共用一套編譯期產生的多語言詞表，預設跟隨 `CultureInfo.CurrentUICulture`，缺翻譯自動回退英文；不依賴 resx 與衛星組件，單一 DLL、原生支援 AOT。

```csharp
Localization.Culture = new CultureInfo("zh-TW");
using var culture = RequestCultureScope.Create(Request.Headers.AcceptLanguage);
```

內建 `zh-Hans`、`zh-Hant`、`es`、`de`、`ja`、`fr`、`pt`、`ru`、`ko`、`ar` 十種譯文；新增語言只需在 `Locales/<culture>.yml` 補充英文源文到譯文的對應。

## 建置與測試

```bash
dotnet test -c Release
```

測試涵蓋訊框編解碼、多段序列、損壞訊框、訊息分包合併、BCD 序列化、音訊編解碼、H.264 解析、FLV/fMP4 封裝、服務端協定模式與多語言詞表。

## 生態版圖

完整專案矩陣見 [README.md](README.md#ecosystem)（逐字一致，於所有 NewLife 倉庫共用）。

## 授權

[MIT](LICENSE) © 2022-2026 新生命開發團隊

## 新生命開發團隊

<img src="https://newlifex.com/logo.png" width="48" align="right" alt="NewLife" />

新生命團隊成立於 2002 年，二十餘年專注 .NET 與物聯網技術生態，主導開源專案近百個（生態版圖收錄 34 個主幹專案），為電力、物流、醫療、工控、網際網路等行業提供軟硬體一體化解決方案；以「學無先後達者為師」為信條。

開源元件在 NuGet 累計下載 <!--nuget-total:start-->6.48M<!--nuget-total:end--> 次（全生態口徑，截至 2026-10）。

網站：https://newlifex.com · 開源組織：https://github.com/NewLifeX

<details>
<summary>微信公眾號：智能大石头（掃碼關注）</summary>

![智能大石头](https://newlifex.com/stone.jpg)

</details>
