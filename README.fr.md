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

### Protocole
- **Protocole RTP JT1078** : analyse et construction complètes des messages RTP avec 29 types de charge audio/vidéo
- **Délimitation de trames** : découpage des paquets collés et analyse « en-tête d'abord » basées sur `IMessageCodec` de NewLife.Core
- **Fusion de fragments RTP** : mise en mémoire tampon et fusion automatiques des fragments de trames I/P/B

### Codecs audio et vidéo
- **Décodage audio** : G.711A (A-law), G.711U (μ-law), IMA-ADPCM, avec suppression de l'en-tête HiSilicon
- **Encodage AAC** : encodage AAC via FaacEncoder (P/Invoke)
- **Analyse H.264** : découpage en unités NAL, analyse des SPS/PPS, gestion des codes de début et suppression des octets de prévention d'émulation

### Remuxage de conteneurs
- **Remuxage FLV** : conversion RTP vers HTTP-FLV / WebSocket-FLV en temps réel pour la lecture avec flv.js
- **Remuxage fMP4** : sortie de segments CMAF/fMP4 avec les boîtes ISO BMFF principales

### Serveur
- **Service TCP** : serveur JT1078 TCP prêt à l'emploi, avec configuration du port et journalisation de débogage
- **Service UDP** : transport UDP JT/T 1078 cohabitant avec TCP
- **Streaming HTTP/WS** : points d'accès ASP.NET Core intégrés pour FLV/fMP4/AAC/PCM/WAV
- **Relecture** : relecture historique audio/vidéo par SIM, canal et plage horaire
- **Commandes d'appareil** : bus de commandes en cluster basé sur IEventBus, autonome ou cluster Redis

### Gestion et intégration
- **Présence des appareils** : suivi en temps réel des événements de connexion/déconnexion des terminaux
- **Autorisation des flux** : authentification par jeton protégeant les points d'accès en direct
- **Builder DI** : enregistrement fluide `AddJT1078Gateway().AddTcp().AddHttp()`
- **IHostedService** : gestion automatique du cycle de vie ASP.NET Core
- **NativeAOT** : protocole et tables de localisation sans réflexion ; image native vérifiée par test de fumée
- **Localisation** : messages source en anglais, 10 traductions incluses et repli sur l'anglais

## Démarrage rapide

### Installer les paquets NuGet

```bash
dotnet add package NewLife.JT1078           # bibliothèque protocole principale (inclut le remuxage FLV/fMP4)
dotnet add package NewLife.JT1078.Server    # serveur TCP/UDP + HLS/MP3
```

### Analyser un message RTP

```csharp
using System.Buffers;
using NewLife.JT1078.Protocols;

// Délimitation de trames : un message est produit dès l'arrivée de l'en-tête ; le corps est lié par la couche de trames
var codec = new JTCodec();
var result = codec.TryParse(new ReadOnlySequence<Byte>(data));
if (result is { Invalid: false } rs && rs.Message is JTMessage msg)
{
    // Examiner le type de charge
    if (msg.Type == AVTypes.H264)
    {
        var decoder = new H264Decoder();
        var frame = decoder.ParseAVFrame(msg);
    }
}
```

### Décodage audio

```csharp
using NewLife.Audio;

var factory = new AudioCodecFactory();
// Charge de l'appareil -> PCM
var pcm = factory.ToPcm(NewLife.Audio.AVTypes.ADPCMA, packet);
// PCM -> G.711A
var g711a = factory.FromPcm(NewLife.Audio.AVTypes.G711A, pcm);
```

### Démarrer le serveur de streaming

```csharp
using NewLife.JT1078.Server;

// Option 1 : utilisation directe
var server = new JT1078Server();
server.Start();

// Option 2 : intégration ASP.NET Core
builder.Services.AddJT1078Gateway()
       .AddTcp()
       .AddHttp();
```

## Paquets NuGet

| Paquet | Description | NuGet |
|---|---|---|
| [NewLife.JT1078](https://github.com/NewLifeX/NewLife.JT1078) | Protocole principal + codecs + remuxage FLV/fMP4 (Flv/FMp4 intégrés au cœur) | [![NuGet](https://img.shields.io/nuget/v/NewLife.JT1078.svg)](https://www.nuget.org/packages/NewLife.JT1078) |
| NewLife.JT1078.Server | Serveur TCP/UDP + builder DI + encodage HLS/MP3 | [![NuGet](https://img.shields.io/nuget/v/NewLife.JT1078.Server.svg)](https://www.nuget.org/packages/NewLife.JT1078.Server) |

## Native AOT

La bibliothèque de protocole principale et le serveur en configuration par défaut prennent en charge NativeAOT et le trimming. Les analyseurs AOT/trim sont toujours activés pour les cibles net8.0 et ultérieures, sans aucun avertissement IL à la compilation. Le projet `AotSmoke` de ce dépôt publie une image native en CI avec `PublishAot=true` et exerce les codecs de trames, la sérialisation par réflexion des modèles, l'assemblage DI, l'assemblage du serveur et les recherches dans les tables de localisation.

```bash
dotnet publish AotSmoke/AotSmoke.csproj -c Release -r linux-x64 -p:PublishAot=true -o out
./out/AotSmoke
```

> Les plugins, le scripting dynamique et l'enregistrement par réflexion à l'exécution sortent du périmètre de prise en charge AOT (conformément à NewLife.Core).

## Localisation

Les exceptions et les journaux partagent une table de localisation générée à la compilation. Elle suit `CultureInfo.CurrentUICulture` et revient à l'anglais si une traduction manque. Sans resx ni assembly satellite : une seule DLL, sûre nativement pour AOT.

| Paramètre | Rôle |
|---|---|
| `CultureInfo.CurrentUICulture` | Source de langue par défaut ; ASP.NET Core peut la changer par requête |
| `Localization.Culture` | Force la langue des exceptions et du texte courant |
| `Localization.LogCulture` | Langue du texte destiné aux développeurs (journaux, commentaires de configuration) |

```csharp
// Site entier en chinois
Localization.Culture = new CultureInfo("zh-CN");

// Service web respectant Accept-Language à chaque requête
using var culture = RequestCultureScope.Create(Request.Headers.AcceptLanguage);
```

Traductions incluses : `zh-Hans`, `zh-Hant`, `es`, `de`, `ja`, `fr`, `pt`, `ru`, `ko`, `ar` (10 traductions incluses). Pour en ajouter une, associez les chaînes source anglaises à leurs traductions dans `Locales/<culture>.yml` ; le générateur de source valide, compresse et enregistre la table à la compilation.

## Compilation et tests

```bash
dotnet test -c Release
```

Les tests couvrent la délimitation et la construction de trames, les séquences multi-segments, les trames invalides, la fusion de fragments, la sérialisation BCD, les codecs audio, l'analyse H.264, l'empaquetage FLV/fMP4, le mode protocole du serveur et les tables de localisation.

## Architecture

```
Appareil JT1078 (TCP/UDP:1078)
    │
    ▼
NewLife.JT1078.Server ─── JT1078Session / JT1078UdpSession
    │  fusion des fragments RTP
    ▼
JTMessage (message RTP)
    ├──► H264Decoder (vidéo)
    ├──► AudioCodecFactory (audio)
    │
    ├──► FlvEncoder ──────────► HTTP-FLV / WS-FLV
    ├──► FMp4Encoder ────────► fMP4
    ├──► HlsHandler / TSMuxer ──► HLS (/hls.m3u8)
    ├──► Mp3Handler ─────────► audio MP3
    ├──► gestionnaires audio ─► AAC / PCM / WAV
    │
    ├──► DeviceSessionManager ──► diffusion des commandes en cluster (IEventBus)
    │
    ▼
Lecteur (flv.js / hls.js)
```

## Déploiement en cluster

Le serveur prend en charge le déploiement multi-instances et transfère les commandes d'appareil entre instances via un bus d'événements :

```csharp
// Autonome (par défaut, EventBus en mémoire)
services.AddJT1078Gateway().AddTcp().AddHttp();

// Cluster (fournir ICacheProvider via Redis/Stardust)
// Un EventBus Redis est créé automatiquement et les commandes sont diffusées à toutes les instances
```

REST API :
- `POST /Command/Send` : envoie une commande à un appareil de façon synchrone, routée automatiquement vers l'instance qui détient l'appareil
- `POST /Command/SendAsync` : envoie une commande de façon asynchrone avec prise en charge de CancellationToken
- `GET /Command/Devices` : liste les appareils connectés localement (TCP/UDP)

## Points d'accès en direct

| Point d'accès | Format | Transport | Lecteur |
|----------|--------|-----------|--------|
| `/live.flv` | FLV | HTTP / WebSocket | flv.js |
| `/live.mp4` | fMP4 | HTTP / WebSocket | MSE natif |
| `/live.aac` | AAC | HTTP | audio natif |
| `/live.pcm` | PCM | HTTP | audio natif |
| `/live.wav` | WAV | HTTP | audio natif |
| `/live.mp3` | MP3 | HTTP | audio natif |
| `/hls.m3u8` + `/hls.ts` | HLS (MPEG-TS) | HTTP | hls.js |

## Références du protocole

- JT/T 1078-2016, Systèmes de positionnement par satellite des véhicules de transport routier - Protocole de communication vidéo
- ISO 14496-12 (ISO BMFF)
- Adobe Flash Video (FLV)

## Remerciements

La conception et l'implémentation de ce projet s'inspirent du projet open source [SmallChi/JT1078](https://github.com/SmallChi/JT1078). Merci beaucoup.

## Carte de l'écosystème

NewLife est un écosystème en couches de projets open source versionnés indépendamment. La carte ci-dessous est reprise à l'identique par tous les dépôts NewLife ; chaque projet renvoie à son dépôt et à son paquet NuGet principal.

| Projet | Rôle | Téléchargements |
| --- | --- | --- |
| **Fondation** | | |
| [NewLife.Core](https://github.com/NewLifeX/X) | Journalisation, configuration, cache, réseau, sérialisation, ordonnancement et traçage APM. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Core?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Core) |
| [NewLife.XCode](https://github.com/NewLifeX/NewLife.XCode) | Intergiciel de données : tables uniques à l'échelle de dizaines de milliards, partitionnement automatique, séparation lecture/écriture et plus de 10 bases de données. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.XCode?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.XCode) |
| [NewLife.Net](https://github.com/NewLifeX/NewLife.Net) | Bibliothèque réseau hautes performances : 1,77 M pps / 14,5 Gbps sur une machine (TCP 1KB, 2026-10), 4 M connexions TCP persistantes. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Net?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Net) |
| [NewLife.Remoting](https://github.com/NewLifeX/NewLife.Remoting) | Communication protocolaire et framework RPC pour applications client/serveur et appareils IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Remoting?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Remoting) |
| [NewLife.Agent](https://github.com/NewLifeX/NewLife.Agent) | Installe des applications .NET comme démons système : services Windows et systemd Linux. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Agent?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Agent) |
| [NewLife.Templates](https://github.com/NewLifeX/NewLife.Zero) | Modèles de projet `dotnet new` pour console, web, API, service, données et IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Templates?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Templates) |
| **Données et messagerie** | | |
| [NewLife.Redis](https://github.com/NewLifeX/NewLife.Redis) | Client Redis avec une latence en microsecondes, des millions d'opérations/s et des files de messages intégrées. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Redis?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Redis) |
| [NewLife.RocketMQ](https://github.com/NewLifeX/NewLife.RocketMQ) | Client RocketMQ entièrement managé pour Apache RocketMQ et Alibaba Cloud Message Queue. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.RocketMQ?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.RocketMQ) |
| [NewLife.MQTT](https://github.com/NewLifeX/NewLife.MQTT) | Client et serveur MQTT, y compris la connectivité Alibaba Cloud IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.MQTT?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.MQTT) |
| [NewLife.NovaDb](https://github.com/NewLifeX/NewLife.NovaDb) | Base de données hybride en C# : embarquée ou en réseau, relationnelle, séries temporelles, file de messages et KV réunis. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.NovaDb?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.NovaDb) |
| [NewLife.MySql](https://github.com/NewLifeX/NewLife.MySql) | Pilote et couche d'assistance MySql construits sur la pile de données NewLife. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.MySql?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.MySql) |
| **Web, IA et distribué** | | |
| [NewLife.Cube](https://github.com/NewLifeX/NewLife.Cube) | Framework rapide de plateforme d'administration : permissions, SSO, serveur OAuth et échafaudage CRUD. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Cube?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Cube) |
| [Stardust](https://github.com/NewLifeX/Stardust) | Plateforme de services distribués : gestion des nœuds, APM, centre de configuration, registre et centre de publication. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Stardust?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Stardust) |
| [AntJob](https://github.com/NewLifeX/AntJob) | Plateforme de tâches distribuées et d'ordonnancement de données en temps réel avec partitionnement, reprises et backfill. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.AntJob?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.AntJob) |
| [NewLife.AI](https://github.com/NewLifeX/NewLife.AI) | Client unifié pour 46 fournisseurs de LLM, plus ChatAI, une application web de chat prête à l'emploi. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.AI?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.AI) |
| **Industriel et IoT** | | |
| [NewLife.IoT](https://github.com/NewLifeX/NewLife.IoT) | Bibliothèque standard IoT définissant les contrats de protocole et les abstractions d'appareils. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.IoT?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.IoT) |
| [NewLife.Modbus](https://github.com/NewLifeX/NewLife.Modbus) | Implémentation Modbus TCP/RTU/ASCII au-dessus de NewLife.IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Modbus?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Modbus) |
| [NewLife.Siemens](https://github.com/NewLifeX/NewLife.Siemens) | Protocole pour automate Siemens au-dessus de NewLife.IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Siemens?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Siemens) |
| [NewLife.Omron](https://github.com/NewLifeX/NewLife.Omron) | Protocole pour automate Omron au-dessus de NewLife.IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Omron?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Omron) |
| [NewLife.Melsec](https://github.com/NewLifeX/NewLife.Melsec) | Protocole pour automate Mitsubishi Melsec au-dessus de NewLife.IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Melsec?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Melsec) |
| [NewLife.OPC](https://github.com/NewLifeX/NewLife.OPC) | Client OPC pour l'accès aux données industrielles. | — |
| [NewLife.BACnet](https://github.com/NewLifeX/NewLife.BACnet) | Pile de protocoles BACnet pour l'automatisation des bâtiments. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.BACnet?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.BACnet) |
| [ZeroIoT](https://github.com/NewLifeX/ZeroIoT) | Plateforme IoT open source légère avec gestion des passerelles et des appareils. | — |
| **Télématique embarquée** | | |
| [NewLife.JT808](https://github.com/NewLifeX/NewLife.JT808) | Protocole de positionnement de véhicules JT/T 808 : plus de 70 corps de message, 2011/2013/2019. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT808?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT808) |
| [NewLife.JT809](https://github.com/NewLifeX/NewLife.JT809) | Protocole plateforme à plateforme JT/T 809 pour les autorités de transport. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT809?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT809) |
| [NewLife.JT1078](https://github.com/NewLifeX/NewLife.JT1078) | Vidéo embarquée JT/T 1078 : analyse RTP, remuxage FLV/fMP4 et diffusion en direct. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT1078?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT1078) |
| **Composants métier** | | |
| [NewLife.Office](https://github.com/NewLifeX/NewLife.Office) | Un seul paquet pour Excel, Word, PPT, PDF et plus de 15 formats de documents. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Office?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Office) |
| [NewLife.Audio](https://github.com/NewLifeX/NewLife.Audio) | Audio multiplateforme : codecs, E/S d'appareils, DSP, conteneurs et streaming. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Audio?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Audio) |
| [NewLife.Video](https://github.com/NewLifeX/NewLife.Video) | Codecs vidéo, formats de conteneurs, traitement des images et streaming. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Video?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Video) |
| [NewLife.Map](https://github.com/NewLifeX/NewLife.Map) | Interface unifiée pour Baidu, AMap, Tencent et Tianditu avec géocodage et conversion de coordonnées. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Map?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Map) |
| [NewLife.IP](https://github.com/NewLifeX/NewLife.IP) | Géolocalisation IPv4 avec une base de données hors ligne mappée en mémoire et une API en ligne en secours. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.IP?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.IP) |
| **Outils et produits** | | |
| [CrazyCoder](https://github.com/NewLifeX/XCoder) | Boîte à outils du développeur : réseau, port série, cryptographie, regex, Modbus et MQTT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.XCoder?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.XCoder) |
| [GitCandy](https://github.com/NewLifeX/GitCandy) | Système de gestion de code source Git auto-hébergé. | — |
| [LuckyClover](https://github.com/NewLifeX/LuckyClover) | Installe automatiquement le runtime .NET correspondant lorsqu'il est lancé depuis un hôte plus ancien. | — |

> Les badges indiquent le paquet principal de chaque projet ; les paquets de thèmes, d'extensions et satellites sont répertoriés sur leurs pages NuGet. Les plus de 30 paquets non listés ici (FTP, LoRa, extensions Office, pilotes de protocoles, etc.) sont disponibles dans l'[organisation NewLifeX](https://github.com/NewLifeX) et sur [newlifex.com](https://newlifex.com).

## Équipe

<img src="https://newlifex.com/logo.png" width="48" align="right" alt="NewLife" />

NewLife a été fondée en 2002 et œuvre depuis plus de deux décennies dans l'écosystème .NET et IoT, maintient près de 100 projets open source (dont 34 principaux) et fournit des solutions matérielles et logicielles intégrées aux secteurs de l'énergie, de la logistique, de la santé, du contrôle industriel et d'Internet. Notre devise : « l'apprentissage ne connaît pas l'ancienneté — celui qui maîtrise devient maître ».

Les composants open source ont été téléchargés <!--nuget-total:start-->6.48M<!--nuget-total:end--> fois depuis NuGet (à l'échelle de l'écosystème, en date de 2026-10).

Site web : https://newlifex.com · Organisation open source : https://github.com/NewLifeX

<details>
<summary>Compte officiel WeChat : 智能大石头 (scannez pour suivre)</summary>

![智能大石头](https://newlifex.com/stone.jpg)

</details>

## Licence

[MIT](LICENSE) © 2022-2026 NewLife
