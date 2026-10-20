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
🌐 [English](README.md) | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | [Español](README.es.md) | [Deutsch](README.de.md) | [日本語](README.ja.md) | [Français](README.fr.md) | [Português](README.pt.md) | [Русский](README.ru.md) | [العربية](README.ar.md)
<!-- lang:end -->

도로 운송 차량 위성 측위 시스템용 JT/T 1078 영상 통신 프로토콜의 .NET 구현입니다. 프로토콜 해석, 오디오/비디오 코덱, FLV/fMP4 리먹싱, TCP/UDP 서버, HTTP/WebSocket 라이브 스트리밍까지 전 구간을 제공합니다. NativeAOT와 런타임 다국어를 지원합니다.

## 기능

- **JT1078 RTP 프로토콜**: 29가지 오디오/비디오 유형을 지원하는 RTP 메시지 해석 및 생성
- **프레임 구분**: NewLife.Core `IMessageCodec` 기반의 스티키 패킷 분할과 헤더 우선 해석
- **RTP 프래그먼트 병합**: I/P/B 프레임 프래그먼트 자동 버퍼링 및 병합
- **오디오 디코딩**: G.711A, G.711U, IMA-ADPCM, HiSilicon 헤더 제거 지원
- **H.264 해석**: NAL 분할, SPS/PPS, 시작 코드, 에뮬레이션 방지 바이트 제거
- **리먹싱**: 실시간 FLV / fMP4, HTTP-FLV / WS-FLV / fMP4 / HLS 출력
- **서버**: TCP/UDP, HTTP/WS 엔드포인트, 녹화 재생, IEventBus 기반 클러스터 명령 버스
- **NativeAOT**: 프로토콜과 언어 테이블에 리플렉션 없음, CI 스모크 테스트로 네이티브 이미지 검증
- **다국어**: 영어 원문과 10개 언어 번역 내장, 번역이 없으면 영어로 폴백

## 빠른 시작

```bash
dotnet add package NewLife.JT1078           # 코어 프로토콜 라이브러리(FLV/fMP4 포함)
dotnet add package NewLife.JT1078.Server    # TCP/UDP 서버 + HLS/MP3
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

// ASP.NET Core 통합
builder.Services.AddJT1078Gateway().AddTcp().AddHttp();
```

## NuGet 패키지

| 패키지 | 설명 |
|---|---|
| NewLife.JT1078 | 프로토콜 + 코덱 + FLV/fMP4 리먹싱 |
| NewLife.JT1078.Server | TCP/UDP 서버 + DI 빌더 + HLS/MP3 |

## Native AOT

코어 프로토콜 라이브러리와 기본 구성의 서버는 NativeAOT와 트리밍을 지원합니다. net8.0 이상에서는 AOT/트림 분석기가 항상 활성화되어 빌드 시 IL 경고가 없습니다. `AotSmoke` 프로젝트는 CI에서 `PublishAot=true`로 네이티브 이미지를 게시하고 프레임 코덱과 언어 테이블을 검증합니다.

```bash
dotnet publish AotSmoke/AotSmoke.csproj -c Release -r linux-x64 -p:PublishAot=true -o out
./out/AotSmoke
```

## 현지화

예외와 로그는 컴파일 타임에 생성되는 공용 언어 테이블을 사용합니다. 기본적으로 `CultureInfo.CurrentUICulture`를 따르며 번역이 없으면 영어로 폴백합니다. resx나 위성 어셈블리가 필요 없으며 단일 DLL, AOT 안전합니다.

```csharp
Localization.Culture = new CultureInfo("ko");
using var culture = RequestCultureScope.Create(Request.Headers.AcceptLanguage);
```

내장 언어: `zh-Hans`, `zh-Hant`, `es`, `de`, `ja`, `fr`, `pt`, `ru`, `ko`, `ar`. 새 언어는 `Locales/<culture>.yml`에 추가합니다.

## 빌드 및 테스트

```bash
dotnet test -c Release
```

프레임 코덱, 다중 세그먼트, 잘못된 프레임, 프래그먼트 병합, BCD, 오디오 코덱, H.264, FLV/fMP4, 서버 프로토콜 모드, 언어 테이블을 검증합니다.

## 생태계

전체 프로젝트 매트릭스는 [README.md](README.md#ecosystem)를 참조하세요(모든 NewLife 저장소에서 동일).

## 라이선스

[MIT](LICENSE) © 2022-2026 NewLife
