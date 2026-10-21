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

### 프로토콜
- **JT1078 RTP 프로토콜**: 29가지 오디오/비디오 페이로드 유형을 지원하는 RTP 메시지 해석 및 생성
- **프레임 구분**: NewLife.Core `IMessageCodec` 기반의 스티키 패킷 분할과 헤더 우선 해석
- **RTP 프래그먼트 병합**: I/P/B 프레임 프래그먼트 자동 버퍼링 및 병합

### 오디오 및 비디오 코덱
- **오디오 디코딩**: G.711A (A-law), G.711U (μ-law), IMA-ADPCM, HiSilicon 헤더 제거 지원
- **AAC 인코딩**: FaacEncoder (P/Invoke) 기반 AAC 인코딩
- **H.264 해석**: NAL 유닛 분할, SPS/PPS 해석, 시작 코드 처리, 에뮬레이션 방지 바이트 제거

### 컨테이너 리먹싱
- **FLV 리먹싱**: RTP를 실시간으로 HTTP-FLV / WebSocket-FLV로 변환하여 flv.js 재생 지원
- **fMP4 리먹싱**: 핵심 ISO BMFF 박스를 지원하는 CMAF/fMP4 세그먼트 출력

### 서버
- **TCP 서비스**: 포트 설정과 디버그 로그를 지원하는 바로 사용 가능한 JT1078 TCP 서버
- **UDP 서비스**: TCP와 함께 동작하는 JT/T 1078 UDP 전송
- **HTTP/WS 라이브 스트리밍**: FLV/fMP4/AAC/PCM/WAV용 ASP.NET Core 엔드포인트 내장
- **녹화 재생**: SIM, 채널, 시간 범위로 과거 오디오/비디오 재생
- **장치 명령**: IEventBus 기반 클러스터 명령 버스, 단독 실행 또는 Redis 클러스터

### 관리 및 통합
- **장치 온라인 상태**: 단말 연결/해제 이벤트 실시간 추적
- **스트림 권한 부여**: 토큰 인증으로 라이브 엔드포인트 보호
- **DI 빌더**: `AddJT1078Gateway().AddTcp().AddHttp()` 체이닝 등록
- **IHostedService**: ASP.NET Core 수명 주기 자동 관리
- **NativeAOT**: 리플렉션 없는 프로토콜 및 언어 테이블, 스모크 테스트로 네이티브 이미지 검증
- **다국어**: 영어 원문에 10개 언어 번역 내장, 번역이 없으면 영어로 폴백

## 빠른 시작

### NuGet 패키지 설치

```bash
dotnet add package NewLife.JT1078           # 코어 프로토콜 라이브러리(FLV/fMP4 리먹싱 포함)
dotnet add package NewLife.JT1078.Server    # TCP/UDP 서버 + HLS/MP3
```

### RTP 메시지 해석

```csharp
using System.Buffers;
using NewLife.JT1078.Protocols;

// 프레임 구분: 헤더가 도착하면 메시지가 생성되고, 본문은 프레임 계층에서 바인딩됩니다
var codec = new JTCodec();
var result = codec.TryParse(new ReadOnlySequence<Byte>(data));
if (result is { Invalid: false } rs && rs.Message is JTMessage msg)
{
    // 페이로드 유형 확인
    if (msg.Type == AVTypes.H264)
    {
        var decoder = new H264Decoder();
        var frame = decoder.ParseAVFrame(msg);
    }
}
```

### 오디오 디코딩

```csharp
using NewLife.Audio;

var factory = new AudioCodecFactory();
// 장치 페이로드 -> PCM
var pcm = factory.ToPcm(NewLife.Audio.AVTypes.ADPCMA, packet);
// PCM -> G.711A
var g711a = factory.FromPcm(NewLife.Audio.AVTypes.G711A, pcm);
```

### 스트리밍 서버 시작

```csharp
using NewLife.JT1078.Server;

// 방법 1: 직접 사용
var server = new JT1078Server();
server.Start();

// 방법 2: ASP.NET Core 통합
builder.Services.AddJT1078Gateway()
       .AddTcp()
       .AddHttp();
```

## NuGet 패키지

| 패키지 | 설명 | NuGet |
|---------|-------------|-------|
| [NewLife.JT1078](https://github.com/NewLifeX/NewLife.JT1078) | 코어 프로토콜 + 코덱 + FLV/fMP4 리먹싱(Flv/FMp4가 코어에 통합됨) | [![NuGet](https://img.shields.io/nuget/v/NewLife.JT1078.svg)](https://www.nuget.org/packages/NewLife.JT1078) |
| NewLife.JT1078.Server | TCP/UDP 서버 + DI 빌더 + HLS/MP3 인코딩 | [![NuGet](https://img.shields.io/nuget/v/NewLife.JT1078.Server.svg)](https://www.nuget.org/packages/NewLife.JT1078.Server) |

## Native AOT

코어 프로토콜 라이브러리와 기본 구성의 서버는 NativeAOT와 트리밍을 지원합니다. net8.0 이상 대상에서는 AOT/트림 분석기가 항상 활성화되어 빌드 시 IL 경고가 없습니다. 이 저장소의 `AotSmoke` 프로젝트는 CI에서 `PublishAot=true`로 네이티브 이미지를 게시하고 프레임 코덱, 모델 리플렉션 직렬화, DI 조립, 서버 조립, 언어 테이블 조회를 실행합니다.

```bash
dotnet publish AotSmoke/AotSmoke.csproj -c Release -r linux-x64 -p:PublishAot=true -o out
./out/AotSmoke
```

> 플러그인, 동적 스크립팅, 런타임 리플렉션 등록은 AOT 지원 범위에 포함되지 않습니다(NewLife.Core와 동일한 경계).

## 현지화

예외와 로그는 컴파일 타임에 생성되는 공용 언어 테이블을 사용합니다. 기본적으로 `CultureInfo.CurrentUICulture`를 따르며 번역이 없으면 영어로 폴백합니다. resx나 위성 어셈블리가 필요 없으며 단일 DLL, AOT 안전합니다.

| 설정 | 용도 |
|---|---|
| `CultureInfo.CurrentUICulture` | 기본 언어 소스, ASP.NET Core는 요청별로 변경 가능 |
| `Localization.Culture` | 예외와 일반 텍스트의 언어 강제 지정 |
| `Localization.LogCulture` | 개발자용 텍스트(로그, 구성 주석)의 언어 |

```csharp
// 사이트 전체 한국어
Localization.Culture = new CultureInfo("ko");

// 요청별 Accept-Language를 따르는 웹 서비스
using var culture = RequestCultureScope.Create(Request.Headers.AcceptLanguage);
```

10개 언어 번역 내장: `zh-Hans`, `zh-Hant`, `es`, `de`, `ja`, `fr`, `pt`, `ru`, `ko`, `ar`. 새 언어는 `Locales/<culture>.yml`에 영어 원문 문자열과 번역을 매핑하여 추가하며, 소스 생성기가 빌드 시 테이블을 검증, 압축, 등록합니다.

## 빌드 및 테스트

```bash
dotnet test -c Release
```

프레임 코덱, 다중 세그먼트, 잘못된 프레임, 프래그먼트 병합, BCD, 오디오 코덱, H.264, FLV/fMP4, 서버 프로토콜 모드, 언어 테이블을 검증합니다.

## 아키텍처

```
JT1078 장치 (TCP/UDP:1078)
    │
    ▼
NewLife.JT1078.Server ─── JT1078Session / JT1078UdpSession
    │  RTP 프래그먼트 병합
    ▼
JTMessage (RTP 메시지)
    ├──► H264Decoder (비디오)
    ├──► AudioCodecFactory (오디오)
    │
    ├──► FlvEncoder ──────────► HTTP-FLV / WS-FLV
    ├──► FMp4Encoder ────────► fMP4
    ├──► HlsHandler / TSMuxer ──► HLS (/hls.m3u8)
    ├──► Mp3Handler ─────────► MP3 오디오
    ├──► 오디오 핸들러 ───────► AAC / PCM / WAV
    │
    ├──► DeviceSessionManager ──► 클러스터 명령 브로드캐스트 (IEventBus)
    │
    ▼
플레이어 (flv.js / hls.js)
```

## 클러스터 배포

서버는 다중 인스턴스 배포를 지원하며, 이벤트 버스를 통해 장치 명령을 인스턴스 간에 전달합니다:

```csharp
// 단독 실행(기본, 인메모리 EventBus)
services.AddJT1078Gateway().AddTcp().AddHttp();

// 클러스터(Redis/Stardust로 ICacheProvider 제공)
// Redis EventBus가 자동 생성되며 명령이 모든 인스턴스로 브로드캐스트됩니다
```

REST API:
- `POST /Command/Send`: 장치로 명령을 동기 전송하며, 장치를 보유한 인스턴스로 자동 라우팅
- `POST /Command/SendAsync`: CancellationToken을 지원하는 비동기 명령 전송
- `GET /Command/Devices`: 로컬에 연결된 장치 목록(TCP/UDP)

## 라이브 엔드포인트

| 엔드포인트 | 형식 | 전송 | 플레이어 |
|----------|--------|-----------|--------|
| `/live.flv` | FLV | HTTP / WebSocket | flv.js |
| `/live.mp4` | fMP4 | HTTP / WebSocket | 네이티브 MSE |
| `/live.aac` | AAC | HTTP | 네이티브 오디오 |
| `/live.pcm` | PCM | HTTP | 네이티브 오디오 |
| `/live.wav` | WAV | HTTP | 네이티브 오디오 |
| `/live.mp3` | MP3 | HTTP | 네이티브 오디오 |
| `/hls.m3u8` + `/hls.ts` | HLS (MPEG-TS) | HTTP | hls.js |

## 프로토콜 참고 자료

- JT/T 1078-2016, 도로 운송 차량 위성 측위 시스템 - 영상 통신 프로토콜
- ISO 14496-12 (ISO BMFF)
- Adobe Flash Video (FLV)

## 감사의 말

이 프로젝트의 설계와 구현은 오픈소스 프로젝트 [SmallChi/JT1078](https://github.com/SmallChi/JT1078)을 참고했습니다. 깊이 감사드립니다.

## 생태계 지도

NewLife는 독립적으로 버전이 관리되는 오픈소스 프로젝트의 계층형 생태계입니다. 아래 지도는 모든 NewLife 리포지토리에서 그대로 동일하게 공유되며, 각 프로젝트는 코드 리포지토리와 기본 NuGet 패키지로 연결됩니다.

| 프로젝트 | 한 줄 소개 | 다운로드 |
| --- | --- | --- |
| **기반** | | |
| [NewLife.Core](https://github.com/NewLifeX/X) | 로깅, 구성, 캐시, 네트워크, 직렬화, 스케줄링, APM 추적. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Core?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Core) |
| [NewLife.XCode](https://github.com/NewLifeX/NewLife.XCode) | 데이터 미들웨어: 단일 테이블 수백억 건, 자동 분할 테이블, 읽기/쓰기 분리, 10여 종 데이터베이스 지원. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.XCode?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.XCode) |
| [NewLife.Net](https://github.com/NewLifeX/NewLife.Net) | 고성능 네트워크 라이브러리: 단일 노드 177만 pps, 14.5 Gbps(TCP 1KB, 2026-10 실측), 400만 Tcp 장기 연결. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Net?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Net) |
| [NewLife.Remoting](https://github.com/NewLifeX/NewLife.Remoting) | CS 애플리케이션과 IoT 장치를 위한 프로토콜 통신 및 RPC 프레임워크. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Remoting?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Remoting) |
| [NewLife.Agent](https://github.com/NewLifeX/NewLife.Agent) | .NET 애플리케이션을 운영체제 데몬으로 설치: Windows 서비스와 Linux systemd. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Agent?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Agent) |
| [NewLife.Templates](https://github.com/NewLifeX/NewLife.Zero) | 콘솔, Web, API, 서비스, 데이터, IoT용 `dotnet new` 프로젝트 템플릿. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Templates?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Templates) |
| **데이터 및 메시징** | | |
| [NewLife.Redis](https://github.com/NewLifeX/NewLife.Redis) | Redis 클라이언트: 마이크로초 수준 지연, 백만 수준 처리량, 메시지 큐 내장. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Redis?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Redis) |
| [NewLife.RocketMQ](https://github.com/NewLifeX/NewLife.RocketMQ) | Apache RocketMQ와 알리바바 클라우드 메시지 큐를 지원하는 순수 관리형 RocketMQ 클라이언트. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.RocketMQ?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.RocketMQ) |
| [NewLife.MQTT](https://github.com/NewLifeX/NewLife.MQTT) | MQTT 클라이언트와 서버, 알리바바 클라우드 IoT 연결 포함. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.MQTT?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.MQTT) |
| [NewLife.NovaDb](https://github.com/NewLifeX/NewLife.NovaDb) | C# 하이브리드 데이터베이스: 임베디드/네트워크 두 모드, 관계형, 시계열, 메시지 큐, KV를 하나로. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.NovaDb?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.NovaDb) |
| [NewLife.MySql](https://github.com/NewLifeX/NewLife.MySql) | NewLife 데이터 스택 위에 구축한 MySql 드라이버와 도우미 계층. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.MySql?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.MySql) |
| **Web, AI 및 분산** | | |
| [NewLife.Cube](https://github.com/NewLifeX/NewLife.Cube) | 빠른 관리 플랫폼 프레임워크: 권한, SSO, OAuth 서버, CRUD 스캐폴딩. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Cube?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Cube) |
| [Stardust](https://github.com/NewLifeX/Stardust) | 분산 서비스 플랫폼: 노드 관리, APM, 구성 센터, 등록 센터, 배포 센터. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Stardust?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Stardust) |
| [AntJob](https://github.com/NewLifeX/AntJob) | 샤딩, 재시도, 백필을 지원하는 분산 작업 및 실시간 데이터 스케줄링 플랫폼. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.AntJob?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.AntJob) |
| [NewLife.AI](https://github.com/NewLifeX/NewLife.AI) | 46개 LLM 공급자를 통합 연결하는 클라이언트와 바로 사용할 수 있는 ChatAI 웹 채팅 애플리케이션. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.AI?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.AI) |
| **산업 및 IoT** | | |
| [NewLife.IoT](https://github.com/NewLifeX/NewLife.IoT) | 프로토콜 계약과 장치 추상을 정의하는 IoT 표준 라이브러리. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.IoT?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.IoT) |
| [NewLife.Modbus](https://github.com/NewLifeX/NewLife.Modbus) | NewLife.IoT 기반으로 구현한 Modbus TCP/RTU/ASCII. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Modbus?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Modbus) |
| [NewLife.Siemens](https://github.com/NewLifeX/NewLife.Siemens) | NewLife.IoT 기반으로 구현한 지멘스 PLC 프로토콜. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Siemens?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Siemens) |
| [NewLife.Omron](https://github.com/NewLifeX/NewLife.Omron) | NewLife.IoT 기반으로 구현한 오므론 PLC 프로토콜. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Omron?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Omron) |
| [NewLife.Melsec](https://github.com/NewLifeX/NewLife.Melsec) | NewLife.IoT 기반으로 구현한 미쓰비시 Melsec PLC 프로토콜. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Melsec?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Melsec) |
| [NewLife.OPC](https://github.com/NewLifeX/NewLife.OPC) | 산업 데이터 수집용 OPC 클라이언트. | — |
| [NewLife.BACnet](https://github.com/NewLifeX/NewLife.BACnet) | BACnet 빌딩 자동화 프로토콜 스택. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.BACnet?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.BACnet) |
| [ZeroIoT](https://github.com/NewLifeX/ZeroIoT) | 게이트웨이와 장치 관리를 갖춘 경량 오픈소스 IoT 플랫폼. | — |
| **차량 텔레매틱스** | | |
| [NewLife.JT808](https://github.com/NewLifeX/NewLife.JT808) | JT/T 808 차량 위치 프로토콜: 70개 이상 메시지 본문, 2011/2013/2019 지원. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT808?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT808) |
| [NewLife.JT809](https://github.com/NewLifeX/NewLife.JT809) | 운송 관리 부서를 위한 JT/T 809 플랫폼 간 프로토콜. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT809?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT809) |
| [NewLife.JT1078](https://github.com/NewLifeX/NewLife.JT1078) | JT/T 1078 차량용 비디오: RTP 파싱, FLV/fMP4 리먹싱, 라이브 스트리밍. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT1078?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT1078) |
| **도메인 컴포넌트** | | |
| [NewLife.Office](https://github.com/NewLifeX/NewLife.Office) | Excel, Word, PPT, PDF 등 15개 이상 문서 형식을 하나의 패키지로. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Office?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Office) |
| [NewLife.Audio](https://github.com/NewLifeX/NewLife.Audio) | 크로스 플랫폼 오디오: 코덱, 장치 IO, DSP, 컨테이너, 스트리밍. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Audio?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Audio) |
| [NewLife.Video](https://github.com/NewLifeX/NewLife.Video) | 비디오 코덱, 컨테이너 형식, 프레임 처리, 스트리밍. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Video?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Video) |
| [NewLife.Map](https://github.com/NewLifeX/NewLife.Map) | 바이두/고덕/텐센트/톈디투 지도 통합 인터페이스, 지오코딩과 좌표 변환 지원. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Map?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Map) |
| [NewLife.IP](https://github.com/NewLifeX/NewLife.IP) | IPv4 위치 정보: 오프라인 메모리 매핑 데이터베이스와 온라인 API 이중 소스. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.IP?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.IP) |
| **도구 및 제품** | | |
| [CrazyCoder](https://github.com/NewLifeX/XCoder) | 개발자 도구 모음: 네트워크, 시리얼 포트, 암호화, 정규식, Modbus, MQTT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.XCoder?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.XCoder) |
| [GitCandy](https://github.com/NewLifeX/GitCandy) | 셀프 호스팅 Git 소스 코드 관리 시스템. | — |
| [LuckyClover](https://github.com/NewLifeX/LuckyClover) | 구버전 호스트에서 시작할 때 일치하는 .NET 런타임을 자동으로 설치합니다. | — |

> 배지는 각 프로젝트의 기본 패키지를 나타냅니다. 테마, 확장, 위성 패키지는 해당 NuGet 페이지에서 확인하세요. 여기에 나열하지 않은 30여 개 패키지(FTP, LoRa, Office 확장, 각종 프로토콜 드라이버 등)는 [NewLifeX 조직](https://github.com/NewLifeX)과 [newlifex.com](https://newlifex.com)에서 확인할 수 있습니다.

## 뉴라이프 개발팀

<img src="https://newlifex.com/logo.png" width="48" align="right" alt="NewLife" />

뉴라이프 팀(NewLife)은 2002년에 설립되어 20여 년간 .NET과 IoT 기술 생태계에 집중하며 100여 개의 오픈소스 프로젝트를 주도하고(이 중 34개가 핵심 프로젝트), 전력, 물류, 의료, 산업 제어, 인터넷 등 산업에 소프트웨어와 하드웨어를 아우르는 통합 솔루션을 제공해 왔습니다. "학문에는 선후가 없으며, 이룬 사람이 스승이다"라는 신조를 따릅니다.

오픈소스 컴포넌트는 NuGet에서 누적 <!--nuget-total:start-->6.48M<!--nuget-total:end-->회 다운로드되었습니다(생태계 전체 기준, 2026-10).

웹사이트: https://newlifex.com · 오픈소스 조직: https://github.com/NewLifeX

<details>
<summary>위챗 공식 계정: 智能大石头(스캔하여 팔로우)</summary>

![智能大石头](https://newlifex.com/stone.jpg)

</details>

## 라이선스

[MIT](LICENSE) © 2022-2026 NewLife
