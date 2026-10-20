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
🌐 [English](README.md) | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | [Español](README.es.md) | [Deutsch](README.de.md) | [日本語](README.ja.md) | [Português](README.pt.md) | [Русский](README.ru.md) | [한국어](README.ko.md) | [العربية](README.ar.md)
<!-- lang:end -->

Implémentation .NET du protocole de communication vidéo JT/T 1078 pour les systèmes de positionnement par satellite des véhicules de transport routier. La bibliothèque couvre l'analyse du protocole, les codecs audio/vidéo, le remuxage FLV/fMP4, les serveurs TCP/UDP et le streaming HTTP/WebSocket. NativeAOT et la localisation à l'exécution sont pris en charge.

## Fonctionnalités

- **Protocole RTP JT1078** : analyse et construction complètes avec 29 types de charge audio/vidéo
- **Délimitation de trames** : découpage des paquets collés et analyse header-first basés sur `IMessageCodec` de NewLife.Core
- **Fusion de fragments RTP** : mise en mémoire et fusion automatiques des fragments I/P/B
- **Décodage audio** : G.711A, G.711U, IMA-ADPCM avec suppression de l'en-tête HiSilicon
- **Analyse H.264** : découpage NAL, SPS/PPS, codes de début et octets de prévention
- **Remuxage** : FLV et fMP4 en temps réel, sortie HTTP-FLV/WS-FLV, fMP4 et HLS
- **Serveur** : TCP/UDP, points d'accès HTTP/WS, relecture et bus de commandes en cluster (IEventBus)
- **NativeAOT** : protocole et tables de langue sans réflexion ; image native vérifiée en CI
- **Localisation** : messages source en anglais, 10 traductions intégrées, repli sur l'anglais

## Démarrage rapide

```bash
dotnet add package NewLife.JT1078           # bibliothèque protocole (FLV/fMP4 inclus)
dotnet add package NewLife.JT1078.Server    # serveur TCP/UDP + HLS/MP3
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

// Ou intégration ASP.NET Core
builder.Services.AddJT1078Gateway().AddTcp().AddHttp();
```

## Paquets NuGet

| Paquet | Description |
|---|---|
| NewLife.JT1078 | Protocole + codecs + remuxage FLV/fMP4 |
| NewLife.JT1078.Server | Serveur TCP/UDP + builder DI + HLS/MP3 |

## Native AOT

La bibliothèque de protocole et le serveur en configuration par défaut prennent en charge NativeAOT et le trimming. Les analyseurs AOT/trim sont toujours actifs pour net8.0 et ultérieur, sans avertissement IL. Le projet `AotSmoke` publie une image native en CI avec `PublishAot=true` et exécute les codecs de trames et les tables de langue.

```bash
dotnet publish AotSmoke/AotSmoke.csproj -c Release -r linux-x64 -p:PublishAot=true -o out
./out/AotSmoke
```

## Localisation

Les exceptions et les journaux partagent une table générée à la compilation. Elle suit `CultureInfo.CurrentUICulture` et revient à l'anglais si une traduction manque. Sans resx ni assembly satellite : une seule DLL, compatible AOT.

```csharp
Localization.Culture = new CultureInfo("fr");
using var culture = RequestCultureScope.Create(Request.Headers.AcceptLanguage);
```

Langues incluses : `zh-Hans`, `zh-Hant`, `es`, `de`, `ja`, `fr`, `pt`, `ru`, `ko`, `ar`. Pour en ajouter une, renseignez `Locales/<culture>.yml`.

## Compilation et tests

```bash
dotnet test -c Release
```

Les tests couvrent le codec de trames, les séquences multi-segments, les trames invalides, la fusion de fragments, le BCD, les codecs audio, H.264, FLV/fMP4, le mode protocole du serveur et les tables de langue.

## Écosystème

La matrice complète des projets se trouve dans [README.md](README.md#ecosystem) (identique dans tous les dépôts NewLife).

## Licence

[MIT](LICENSE) © 2022-2026 NewLife
