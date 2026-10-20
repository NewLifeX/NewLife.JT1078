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
🌐 [English](README.md) | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | [Deutsch](README.de.md) | [日本語](README.ja.md) | [Français](README.fr.md) | [Português](README.pt.md) | [Русский](README.ru.md) | [한국어](README.ko.md) | [العربية](README.ar.md)
<!-- lang:end -->

Implementación .NET del protocolo de comunicación de vídeo JT/T 1078 para sistemas de posicionamiento por satélite de vehículos de transporte por carretera. Cubre el pipeline completo de análisis de protocolo, códecs de audio/vídeo, remultiplexado FLV/fMP4, servidores TCP/UDP y streaming HTTP/WebSocket. Compatible con NativeAOT y localización en tiempo de ejecución.

## Características

- **Protocolo RTP JT1078**: análisis y construcción completos con 29 tipos de carga de audio/vídeo
- **Delimitación de tramas**: división de paquetes pegados y análisis header-first basados en `IMessageCodec` de NewLife.Core
- **Fusión de fragmentos RTP**: almacenamiento y fusión automática de fragmentos de tramas I/P/B
- **Códecs de audio**: G.711A, G.711U, IMA-ADPCM con eliminación de cabecera HiSilicon
- **Análisis H.264**: división de NAL, SPS/PPS, códigos de inicio y bytes de prevención
- **Remultiplexado**: FLV y fMP4 en tiempo real, salida HTTP-FLV/WS-FLV, fMP4 y HLS
- **Servidor**: TCP/UDP, endpoints HTTP/WS, reproducción histórica y bus de comandos en clúster (IEventBus)
- **NativeAOT**: protocolo y tablas de idioma sin reflexión; imagen nativa verificada por smoke test
- **Localización**: mensajes fuente en inglés, 10 traducciones incluidas y retroceso al inglés

## Inicio rápido

```bash
dotnet add package NewLife.JT1078           # biblioteca de protocolo (incluye FLV/fMP4)
dotnet add package NewLife.JT1078.Server    # servidor TCP/UDP + HLS/MP3
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

// O integración con ASP.NET Core
builder.Services.AddJT1078Gateway().AddTcp().AddHttp();
```

## Paquetes NuGet

| Paquete | Descripción |
|---|---|
| NewLife.JT1078 | Protocolo + códecs + remultiplexado FLV/fMP4 |
| NewLife.JT1078.Server | Servidor TCP/UDP + constructor DI + HLS/MP3 |

## Native AOT

La biblioteca de protocolo y el servidor con la configuración predeterminada admiten NativeAOT y recorte. Los analizadores AOT/trim están siempre activos para net8.0 y posteriores, con cero advertencias IL. El proyecto `AotSmoke` publica una imagen nativa en CI con `PublishAot=true` y ejecuta los códecs de tramas y las tablas de idioma.

```bash
dotnet publish AotSmoke/AotSmoke.csproj -c Release -r linux-x64 -p:PublishAot=true -o out
./out/AotSmoke
```

## Localización

Las excepciones y los registros comparten una tabla generada en tiempo de compilación. Sigue `CultureInfo.CurrentUICulture` y recurre al inglés si falta una traducción. Sin resx ni ensamblados satélite: un solo DLL y compatible con AOT.

```csharp
Localization.Culture = new CultureInfo("es");
using var culture = RequestCultureScope.Create(Request.Headers.AcceptLanguage);
```

Traducciones incluidas: `zh-Hans`, `zh-Hant`, `es`, `de`, `ja`, `fr`, `pt`, `ru`, `ko`, `ar`. Para añadir un idioma, asigne el texto inglés a su traducción en `Locales/<culture>.yml`.

## Compilación y pruebas

```bash
dotnet test -c Release
```

Las pruebas cubren delimitación y construcción de tramas, secuencias multisegmento, tramas inválidas, fusión de fragmentos, BCD, códecs de audio, H.264, FLV/fMP4, modo protocolo del servidor y tablas de idioma.

## Ecosistema

Consulte la matriz completa en [README.md](README.md#ecosystem) (idéntica en todos los repositorios NewLife).

## Licencia

[MIT](LICENSE) © 2022-2026 NewLife
