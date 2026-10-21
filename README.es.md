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

### Protocolo
- **Protocolo RTP JT1078**: análisis y construcción completos de mensajes RTP con 29 tipos de carga de audio/vídeo
- **Delimitación de tramas**: división de paquetes pegados y análisis con la cabecera primero, basados en `IMessageCodec` de NewLife.Core
- **Fusión de fragmentos RTP**: almacenamiento y fusión automática de fragmentos de tramas I/P/B

### Códecs de audio y vídeo
- **Decodificación de audio**: G.711A (A-law), G.711U (μ-law), IMA-ADPCM, con eliminación de la cabecera HiSilicon
- **Codificación AAC**: codificación AAC mediante FaacEncoder (P/Invoke)
- **Análisis H.264**: división de unidades NAL, análisis de SPS/PPS, manejo de códigos de inicio y eliminación de bytes de prevención de emulación

### Remultiplexado de contenedores
- **Remultiplexado FLV**: conversión en tiempo real de RTP a HTTP-FLV / WebSocket-FLV para reproducción con flv.js
- **Remultiplexado fMP4**: salida de segmentos CMAF/fMP4 con las cajas principales de ISO BMFF

### Servidor
- **Servicio TCP**: servidor JT1078 TCP listo para usar, con configuración de puerto y registro de depuración
- **Servicio UDP**: transporte UDP de JT/T 1078 que convive con TCP
- **Streaming HTTP/WS**: endpoints ASP.NET Core integrados para FLV/fMP4/AAC/PCM/WAV
- **Reproducción**: reproducción histórica de audio/vídeo por SIM, canal e intervalo de tiempo
- **Comandos de dispositivo**: bus de comandos en clúster basado en IEventBus, en modo autónomo o clúster Redis

### Gestión e integración
- **Presencia de dispositivos**: seguimiento en tiempo real de los eventos de conexión/desconexión de terminales
- **Autorización de streams**: autenticación por token que protege los endpoints en directo
- **Constructor DI**: registro fluido con `AddJT1078Gateway().AddTcp().AddHttp()`
- **IHostedService**: gestión automática del ciclo de vida de ASP.NET Core
- **NativeAOT**: protocolo y tablas de idioma sin reflexión; imagen nativa verificada por prueba de humo
- **Localización**: mensajes fuente en inglés con 10 traducciones incluidas y retroceso al inglés

## Inicio rápido

### Instalar paquetes NuGet

```bash
dotnet add package NewLife.JT1078           # biblioteca de protocolo (incluye FLV/fMP4)
dotnet add package NewLife.JT1078.Server    # servidor TCP/UDP + HLS/MP3
```

### Analizar un mensaje RTP

```csharp
using System.Buffers;
using NewLife.JT1078.Protocols;

// Delimitación de tramas: el mensaje se produce en cuanto llega la cabecera; el cuerpo lo enlaza la capa de trama
var codec = new JTCodec();
var result = codec.TryParse(new ReadOnlySequence<Byte>(data));
if (result is { Invalid: false } rs && rs.Message is JTMessage msg)
{
    // Inspeccionar el tipo de carga
    if (msg.Type == AVTypes.H264)
    {
        var decoder = new H264Decoder();
        var frame = decoder.ParseAVFrame(msg);
    }
}
```

### Decodificación de audio

```csharp
using NewLife.Audio;

var factory = new AudioCodecFactory();
// Carga del dispositivo -> PCM
var pcm = factory.ToPcm(NewLife.Audio.AVTypes.ADPCMA, packet);
// PCM -> G.711A
var g711a = factory.FromPcm(NewLife.Audio.AVTypes.G711A, pcm);
```

### Iniciar el servidor de streaming

```csharp
using NewLife.JT1078.Server;

// Opción 1: uso directo
var server = new JT1078Server();
server.Start();

// Opción 2: integración con ASP.NET Core
builder.Services.AddJT1078Gateway()
       .AddTcp()
       .AddHttp();
```

## Paquetes NuGet

| Paquete | Descripción | NuGet |
|---|---|---|
| [NewLife.JT1078](https://github.com/NewLifeX/NewLife.JT1078) | Protocolo principal + códecs + remultiplexado FLV/fMP4 (Flv/FMp4 integrados en el núcleo) | [![NuGet](https://img.shields.io/nuget/v/NewLife.JT1078.svg)](https://www.nuget.org/packages/NewLife.JT1078) |
| NewLife.JT1078.Server | Servidor TCP/UDP + constructor DI + codificación HLS/MP3 | [![NuGet](https://img.shields.io/nuget/v/NewLife.JT1078.Server.svg)](https://www.nuget.org/packages/NewLife.JT1078.Server) |

## Native AOT

La biblioteca de protocolo principal y el servidor con la configuración predeterminada admiten NativeAOT y recorte. Los analizadores AOT/trim están siempre activos para los destinos net8.0 y posteriores, con cero advertencias IL en tiempo de compilación. El proyecto `AotSmoke` de este repositorio publica una imagen nativa en CI con `PublishAot=true` y ejercita los códecs de tramas, la serialización por reflexión de modelos, el ensamblado de DI, el ensamblado del servidor y las consultas a las tablas de idioma.

```bash
dotnet publish AotSmoke/AotSmoke.csproj -c Release -r linux-x64 -p:PublishAot=true -o out
./out/AotSmoke
```

> Los plugins, el scripting dinámico y el registro por reflexión en tiempo de ejecución quedan fuera del alcance de soporte de AOT (coherente con NewLife.Core).

## Localización

Las excepciones y los registros comparten una tabla de localización generada en tiempo de compilación. Sigue `CultureInfo.CurrentUICulture` y recurre al inglés si falta una traducción. Sin resx ni ensamblados satélite: un solo DLL y compatible con AOT de forma nativa.

| Ajuste | Propósito |
|---|---|
| `CultureInfo.CurrentUICulture` | Origen de idioma predeterminado; ASP.NET Core puede variarlo por solicitud |
| `Localization.Culture` | Fuerza el idioma de las excepciones y el texto normal |
| `Localization.LogCulture` | Idioma del texto para desarrolladores (registros, comentarios de configuración) |

```csharp
// Todo el sitio en español
Localization.Culture = new CultureInfo("es");

// Servicio web que respeta Accept-Language por solicitud
using var culture = RequestCultureScope.Create(Request.Headers.AcceptLanguage);
```

Traducciones incluidas: `zh-Hans`, `zh-Hant`, `es`, `de`, `ja`, `fr`, `pt`, `ru`, `ko`, `ar` (10 traducciones incluidas). Para añadir un idioma, asigne el texto fuente en inglés a su traducción en `Locales/<culture>.yml`; el generador de código valida, comprime y registra la tabla en tiempo de compilación.

## Compilación y pruebas

```bash
dotnet test -c Release
```

Las pruebas cubren delimitación y construcción de tramas, secuencias multisegmento, tramas inválidas, fusión de fragmentos, BCD, códecs de audio, H.264, FLV/fMP4, modo protocolo del servidor y tablas de idioma.

## Arquitectura

```
Dispositivo JT1078 (TCP/UDP:1078)
    │
    ▼
NewLife.JT1078.Server ─── JT1078Session / JT1078UdpSession
    │  fusión de fragmentos RTP
    ▼
JTMessage (mensaje RTP)
    ├──► H264Decoder (vídeo)
    ├──► AudioCodecFactory (audio)
    │
    ├──► FlvEncoder ──────────► HTTP-FLV / WS-FLV
    ├──► FMp4Encoder ────────► fMP4
    ├──► HlsHandler / TSMuxer ──► HLS (/hls.m3u8)
    ├──► Mp3Handler ─────────► audio MP3
    ├──► manejadores de audio ─► AAC / PCM / WAV
    │
    ├──► DeviceSessionManager ──► difusión de comandos en clúster (IEventBus)
    │
    ▼
Reproductor (flv.js / hls.js)
```

## Despliegue en clúster

El servidor admite despliegue multiinstancia y reenvía los comandos de dispositivo entre instancias mediante un bus de eventos:

```csharp
// Autónomo (predeterminado, EventBus en memoria)
services.AddJT1078Gateway().AddTcp().AddHttp();

// Clúster (proporcione ICacheProvider mediante Redis/Stardust)
// Se crea automáticamente un EventBus de Redis y los comandos se difunden a todas las instancias
```

REST API:
- `POST /Command/Send`: envía un comando a un dispositivo de forma síncrona; se enruta automáticamente a la instancia que tiene el dispositivo
- `POST /Command/SendAsync`: envía un comando de forma asíncrona con soporte de CancellationToken
- `GET /Command/Devices`: lista los dispositivos conectados localmente (TCP/UDP)

## Endpoints en directo

| Endpoint | Formato | Transporte | Reproductor |
|----------|--------|-----------|--------|
| `/live.flv` | FLV | HTTP / WebSocket | flv.js |
| `/live.mp4` | fMP4 | HTTP / WebSocket | MSE nativo |
| `/live.aac` | AAC | HTTP | audio nativo |
| `/live.pcm` | PCM | HTTP | audio nativo |
| `/live.wav` | WAV | HTTP | audio nativo |
| `/live.mp3` | MP3 | HTTP | audio nativo |
| `/hls.m3u8` + `/hls.ts` | HLS (MPEG-TS) | HTTP | hls.js |

## Referencias de protocolo

- JT/T 1078-2016, Sistemas de posicionamiento por satélite de vehículos de transporte por carretera: protocolo de comunicación de vídeo
- ISO 14496-12 (ISO BMFF)
- Adobe Flash Video (FLV)

## Agradecimientos

El diseño y la implementación de este proyecto se basan en el proyecto de código abierto [SmallChi/JT1078](https://github.com/SmallChi/JT1078). Muchas gracias.

## Mapa del ecosistema

NewLife es un ecosistema por capas de proyectos de código abierto con versionado independiente. El mapa siguiente se comparte literalmente en todos los repositorios de NewLife; cada proyecto enlaza a su repositorio y a su paquete principal de NuGet.

| Proyecto | Qué hace | Descargas |
| --- | --- | --- |
| **Fundación** | | |
| [NewLife.Core](https://github.com/NewLifeX/X) | Registro de logs, configuración, caché, red, serialización, programación de tareas y seguimiento APM. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Core?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Core) |
| [NewLife.XCode](https://github.com/NewLifeX/NewLife.XCode) | Middleware de datos: tablas únicas a escala de decenas de miles de millones, particionado automático, separación lectura/escritura y más de 10 bases de datos. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.XCode?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.XCode) |
| [NewLife.Net](https://github.com/NewLifeX/NewLife.Net) | Biblioteca de red de alto rendimiento: 1,77 M pps / 14,5 Gbps en una máquina (TCP 1KB, 2026-10), 4 M conexiones TCP persistentes. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Net?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Net) |
| [NewLife.Remoting](https://github.com/NewLifeX/NewLife.Remoting) | Framework de comunicación por protocolo y RPC para aplicaciones cliente/servidor y dispositivos IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Remoting?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Remoting) |
| [NewLife.Agent](https://github.com/NewLifeX/NewLife.Agent) | Instala aplicaciones .NET como demonios del sistema operativo: servicios de Windows y systemd de Linux. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Agent?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Agent) |
| [NewLife.Templates](https://github.com/NewLifeX/NewLife.Zero) | Plantillas de proyecto `dotnet new` para consola, web, API, servicio, datos e IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Templates?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Templates) |
| **Datos y mensajería** | | |
| [NewLife.Redis](https://github.com/NewLifeX/NewLife.Redis) | Cliente Redis con latencia de microsegundos, millones de operaciones por segundo y colas de mensajes integradas. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Redis?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Redis) |
| [NewLife.RocketMQ](https://github.com/NewLifeX/NewLife.RocketMQ) | Cliente RocketMQ totalmente administrado para Apache RocketMQ y Alibaba Cloud Message Queue. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.RocketMQ?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.RocketMQ) |
| [NewLife.MQTT](https://github.com/NewLifeX/NewLife.MQTT) | Cliente y servidor MQTT, incluida la conectividad con Alibaba Cloud IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.MQTT?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.MQTT) |
| [NewLife.NovaDb](https://github.com/NewLifeX/NewLife.NovaDb) | Base de datos híbrida en C#: embebida o en red, relacional, series temporales, cola de mensajes y KV en una sola. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.NovaDb?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.NovaDb) |
| [NewLife.MySql](https://github.com/NewLifeX/NewLife.MySql) | Controlador y capa de ayuda de MySql construidos sobre la pila de datos de NewLife. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.MySql?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.MySql) |
| **Web, IA y distribuido** | | |
| [NewLife.Cube](https://github.com/NewLifeX/NewLife.Cube) | Framework rápido para plataformas de administración: permisos, SSO, servidor OAuth y andamiaje CRUD. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Cube?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Cube) |
| [Stardust](https://github.com/NewLifeX/Stardust) | Plataforma distribuida de servicios: gestión de nodos, APM, centro de configuración, registro y centro de despliegues. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Stardust?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Stardust) |
| [AntJob](https://github.com/NewLifeX/AntJob) | Plataforma de tareas distribuidas y programación de datos en tiempo real con particionado, reintentos y relleno histórico. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.AntJob?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.AntJob) |
| [NewLife.AI](https://github.com/NewLifeX/NewLife.AI) | Cliente unificado para 46 proveedores de LLM más ChatAI, una aplicación web de chat lista para usar. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.AI?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.AI) |
| **Industrial e IoT** | | |
| [NewLife.IoT](https://github.com/NewLifeX/NewLife.IoT) | Biblioteca estándar de IoT que define contratos de protocolo y abstracciones de dispositivos. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.IoT?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.IoT) |
| [NewLife.Modbus](https://github.com/NewLifeX/NewLife.Modbus) | Implementación de Modbus TCP/RTU/ASCII sobre NewLife.IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Modbus?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Modbus) |
| [NewLife.Siemens](https://github.com/NewLifeX/NewLife.Siemens) | Protocolo para PLC Siemens sobre NewLife.IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Siemens?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Siemens) |
| [NewLife.Omron](https://github.com/NewLifeX/NewLife.Omron) | Protocolo para PLC Omron sobre NewLife.IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Omron?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Omron) |
| [NewLife.Melsec](https://github.com/NewLifeX/NewLife.Melsec) | Protocolo para PLC Mitsubishi Melsec sobre NewLife.IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Melsec?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Melsec) |
| [NewLife.OPC](https://github.com/NewLifeX/NewLife.OPC) | Cliente OPC para el acceso a datos industriales. | — |
| [NewLife.BACnet](https://github.com/NewLifeX/NewLife.BACnet) | Pila de protocolo BACnet para la automatización de edificios. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.BACnet?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.BACnet) |
| [ZeroIoT](https://github.com/NewLifeX/ZeroIoT) | Plataforma IoT de código abierto y ligera con gestión de pasarelas y dispositivos. | — |
| **Telemática vehicular** | | |
| [NewLife.JT808](https://github.com/NewLifeX/NewLife.JT808) | Protocolo de posicionamiento vehicular JT/T 808: más de 70 cuerpos de mensaje, 2011/2013/2019. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT808?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT808) |
| [NewLife.JT809](https://github.com/NewLifeX/NewLife.JT809) | Protocolo plataforma a plataforma JT/T 809 para autoridades de transporte. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT809?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT809) |
| [NewLife.JT1078](https://github.com/NewLifeX/NewLife.JT1078) | Vídeo a bordo JT/T 1078: análisis RTP, remultiplexado FLV/fMP4 y transmisión en directo. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT1078?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT1078) |
| **Componentes de dominio** | | |
| [NewLife.Office](https://github.com/NewLifeX/NewLife.Office) | Un solo paquete para Excel, Word, PPT, PDF y más de 15 formatos de documento. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Office?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Office) |
| [NewLife.Audio](https://github.com/NewLifeX/NewLife.Audio) | Audio multiplataforma: códecs, E/S de dispositivos, DSP, contenedores y streaming. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Audio?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Audio) |
| [NewLife.Video](https://github.com/NewLifeX/NewLife.Video) | Códecs de vídeo, formatos de contenedor, procesamiento de fotogramas y streaming. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Video?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Video) |
| [NewLife.Map](https://github.com/NewLifeX/NewLife.Map) | Interfaz unificada sobre Baidu, AMap, Tencent y Tianditu con geocodificación y conversión de coordenadas. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Map?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Map) |
| [NewLife.IP](https://github.com/NewLifeX/NewLife.IP) | Geolocalización IPv4 con una base de datos offline mapeada en memoria y una API en línea de respaldo. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.IP?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.IP) |
| **Herramientas y productos** | | |
| [CrazyCoder](https://github.com/NewLifeX/XCoder) | Caja de herramientas del desarrollador: red, puerto serie, criptografía, expresiones regulares, Modbus y MQTT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.XCoder?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.XCoder) |
| [GitCandy](https://github.com/NewLifeX/GitCandy) | Sistema de gestión de código fuente Git autoalojado. | — |
| [LuckyClover](https://github.com/NewLifeX/LuckyClover) | Instala automáticamente el runtime de .NET correspondiente cuando se ejecuta desde un host más antiguo. | — |

> Las insignias muestran el paquete principal de cada proyecto; los paquetes de temas, extensiones y satélites figuran en sus páginas de NuGet. Los más de 30 paquetes no listados aquí (FTP, LoRa, extensiones de Office, controladores de protocolo, etc.) están disponibles en la [organización NewLifeX](https://github.com/NewLifeX) y en [newlifex.com](https://newlifex.com).

## Equipo

<img src="https://newlifex.com/logo.png" width="48" align="right" alt="NewLife" />

NewLife se fundó en 2002 y lleva más de dos décadas dedicado al ecosistema .NET e IoT, mantiene cerca de 100 proyectos de código abierto (34 de ellos principales) y ofrece soluciones integradas de software y hardware para los sectores de energía, logística, salud, control industrial e Internet. Nos guía el lema «en el aprendizaje no hay antigüedad: quien domina es maestro».

Los componentes de código abierto acumulan <!--nuget-total:start-->6.48M<!--nuget-total:end--> descargas en NuGet (todo el ecosistema, a fecha de 2026-10).

Sitio web: https://newlifex.com · Organización de código abierto: https://github.com/NewLifeX

<details>
<summary>Cuenta oficial de WeChat: 智能大石头 (escanea para seguir)</summary>

![智能大石头](https://newlifex.com/stone.jpg)

</details>

## Licencia

[MIT](LICENSE) © 2022-2026 NewLife
