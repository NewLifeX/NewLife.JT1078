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
🌐 [English](README.md) | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | [Español](README.es.md) | [Deutsch](README.de.md) | [Français](README.fr.md) | [Português](README.pt.md) | [Русский](README.ru.md) | [한국어](README.ko.md) | [العربية](README.ar.md)
<!-- lang:end -->

JT/T 1078（道路輸送車両衛星測位システムの映像通信プロトコル）の .NET 実装です。プロトコル解析、音声・映像コーデック、FLV/fMP4 リマックス、TCP/UDP サーバー、HTTP/WebSocket ライブ配信までを一貫して提供します。NativeAOT と実行時多言語に対応します。

## 機能

- **JT1078 RTP プロトコル**: 29 種類の音声・映像タイプに対応した RTP メッセージの解析と構築
- **フレーム区切り**: NewLife.Core `IMessageCodec` によるスティッキーパケット分割とヘッダー先行解析
- **RTP フラグメント結合**: I/P/B フレームのフラグメントを自動的にバッファリングして結合
- **音声デコード**: G.711A、G.711U、IMA-ADPCM（HiSilicon ヘッダー除去対応）
- **H.264 解析**: NAL 分割、SPS/PPS、スタートコード、エミュレーション防止バイトの除去
- **リマックス**: リアルタイム FLV / fMP4、HTTP-FLV / WS-FLV / fMP4 / HLS 出力
- **サーバー**: TCP/UDP、HTTP/WS エンドポイント、録画再生、IEventBus によるクラスタコマンド配信
- **NativeAOT**: プロトコルと言語テーブルはリフレクション不要、CI のスモークテストでネイティブイメージを検証
- **多言語**: 英語を原文とし 10 言語の翻訳を内蔵、未翻訳時は英語にフォールバック

## クイックスタート

```bash
dotnet add package NewLife.JT1078           # コアプロトコル（FLV/fMP4 を含む）
dotnet add package NewLife.JT1078.Server    # TCP/UDP サーバー + HLS/MP3
```

```csharp
using System.Buffers;
using NewLife.JT1078.Protocols;

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

// ASP.NET Core 統合
builder.Services.AddJT1078Gateway().AddTcp().AddHttp();
```

## NuGet パッケージ

| パッケージ | 説明 |
|---|---|
| NewLife.JT1078 | プロトコル + コーデック + FLV/fMP4 リマックス |
| NewLife.JT1078.Server | TCP/UDP サーバー + DI ビルダー + HLS/MP3 |

## Native AOT

コアプロトコルライブラリと既定構成のサーバーは NativeAOT とトリミングに対応します。net8.0 以降では AOT/トリムアナライザーが常時有効で、ビルド時の IL 警告はゼロです。`AotSmoke` プロジェクトは CI で `PublishAot=true` によりネイティブイメージを公開し、フレームコーデックと言語テーブルを検証します。

```bash
dotnet publish AotSmoke/AotSmoke.csproj -c Release -r linux-x64 -p:PublishAot=true -o out
./out/AotSmoke
```

## ローカライズ

例外とログはコンパイル時に生成される共通の言語テーブルを使用します。既定では `CultureInfo.CurrentUICulture` に従い、翻訳がない場合は英語にフォールバックします。resx やサテライトアセンブリは不要で、単一 DLL・AOT 対応です。

```csharp
Localization.Culture = new CultureInfo("ja");
using var culture = RequestCultureScope.Create(Request.Headers.AcceptLanguage);
```

同梱言語: `zh-Hans`、`zh-Hant`、`es`、`de`、`ja`、`fr`、`pt`、`ru`、`ko`、`ar`。追加する場合は `Locales/<culture>.yml` に英語原文と訳文を記述します。

## ビルドとテスト

```bash
dotnet test -c Release
```

フレームコーデック、複数セグメント、不正フレーム、フラグメント結合、BCD、音声コーデック、H.264、FLV/fMP4、サーバーのプロトコルモード、言語テーブルを検証します。

## エコシステム

完全なプロジェクト一覧は [README.md](README.md#ecosystem) を参照してください（全 NewLife リポジトリで共通）。

## ライセンス

[MIT](LICENSE) © 2022-2026 NewLife
