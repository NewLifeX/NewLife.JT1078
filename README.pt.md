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
🌐 [English](README.md) | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | [Español](README.es.md) | [Deutsch](README.de.md) | [日本語](README.ja.md) | [Français](README.fr.md) | [Русский](README.ru.md) | [한국어](README.ko.md) | [العربية](README.ar.md)
<!-- lang:end -->

Implementação .NET do protocolo de comunicação de vídeo JT/T 1078 para sistemas de posicionamento por satélite de veículos de transporte rodoviário. Cobre o pipeline completo de análise de protocolo, codecs de áudio/vídeo, remuxagem FLV/fMP4, servidores TCP/UDP e streaming HTTP/WebSocket. Compatível com NativeAOT e localização em tempo de execução.

## Recursos

### Protocolo
- **Protocolo RTP JT1078**: análise e construção completas de mensagens RTP com 29 tipos de carga de áudio/vídeo
- **Delimitação de quadros**: divisão de pacotes colados e análise com cabeçalho primeiro, baseadas em `IMessageCodec` do NewLife.Core
- **Mesclagem de fragmentos RTP**: armazenamento e mesclagem automática de fragmentos de quadros I/P/B

### Codecs de áudio e vídeo
- **Decodificação de áudio**: G.711A (A-law), G.711U (μ-law), IMA-ADPCM, com remoção do cabeçalho HiSilicon
- **Codificação AAC**: codificação AAC via FaacEncoder (P/Invoke)
- **Análise H.264**: divisão de unidades NAL, análise de SPS/PPS, tratamento de códigos de início e remoção de bytes de prevenção de emulação

### Remuxagem de contêineres
- **Remuxagem FLV**: conversão em tempo real de RTP para HTTP-FLV / WebSocket-FLV para reprodução com flv.js
- **Remuxagem fMP4**: saída de segmentos CMAF/fMP4 com as caixas principais do ISO BMFF

### Servidor
- **Serviço TCP**: servidor JT1078 TCP pronto para uso, com configuração de porta e log de depuração
- **Serviço UDP**: transporte UDP do JT/T 1078 coexistindo com TCP
- **Streaming HTTP/WS**: endpoints ASP.NET Core integrados para FLV/fMP4/AAC/PCM/WAV
- **Reprodução**: reprodução histórica de áudio/vídeo por SIM, canal e intervalo de tempo
- **Comandos de dispositivo**: barramento de comandos em cluster baseado em IEventBus, autônomo ou cluster Redis

### Gestão e integração
- **Presença de dispositivos**: rastreamento em tempo real dos eventos de conexão/desconexão dos terminais
- **Autorização de streams**: autenticação por token que protege os endpoints ao vivo
- **Builder DI**: registro fluente com `AddJT1078Gateway().AddTcp().AddHttp()`
- **IHostedService**: gestão automática do ciclo de vida do ASP.NET Core
- **NativeAOT**: protocolo e tabelas de idioma sem reflexão; imagem nativa verificada por teste de fumaça
- **Localização**: mensagens fonte em inglês com 10 traduções incluídas e fallback para inglês

## Início rápido

### Instalar pacotes NuGet

```bash
dotnet add package NewLife.JT1078           # biblioteca de protocolo principal (inclui remuxagem FLV/fMP4)
dotnet add package NewLife.JT1078.Server    # servidor TCP/UDP + HLS/MP3
```

### Analisar uma mensagem RTP

```csharp
using System.Buffers;
using NewLife.JT1078.Protocols;

// Delimitação de quadros: a mensagem é produzida assim que o cabeçalho chega; o corpo é vinculado pela camada de quadro
var codec = new JTCodec();
var result = codec.TryParse(new ReadOnlySequence<Byte>(data));
if (result is { Invalid: false } rs && rs.Message is JTMessage msg)
{
    // Inspecionar o tipo de carga
    if (msg.Type == AVTypes.H264)
    {
        var decoder = new H264Decoder();
        var frame = decoder.ParseAVFrame(msg);
    }
}
```

### Decodificação de áudio

```csharp
using NewLife.Audio;

var factory = new AudioCodecFactory();
// Carga do dispositivo -> PCM
var pcm = factory.ToPcm(NewLife.Audio.AVTypes.ADPCMA, packet);
// PCM -> G.711A
var g711a = factory.FromPcm(NewLife.Audio.AVTypes.G711A, pcm);
```

### Iniciar o servidor de streaming

```csharp
using NewLife.JT1078.Server;

// Opção 1: usar diretamente
var server = new JT1078Server();
server.Start();

// Opção 2: integração com ASP.NET Core
builder.Services.AddJT1078Gateway()
       .AddTcp()
       .AddHttp();
```

## Pacotes NuGet

| Pacote | Descrição | NuGet |
|---|---|---|
| [NewLife.JT1078](https://github.com/NewLifeX/NewLife.JT1078) | Protocolo principal + codecs + remuxagem FLV/fMP4 (Flv/FMp4 integrados ao núcleo) | [![NuGet](https://img.shields.io/nuget/v/NewLife.JT1078.svg)](https://www.nuget.org/packages/NewLife.JT1078) |
| NewLife.JT1078.Server | Servidor TCP/UDP + builder DI + codificação HLS/MP3 | [![NuGet](https://img.shields.io/nuget/v/NewLife.JT1078.Server.svg)](https://www.nuget.org/packages/NewLife.JT1078.Server) |

## Native AOT

A biblioteca de protocolo principal e o servidor na configuração padrão suportam NativeAOT e trimming. Os analisadores AOT/trim estão sempre ativos para os alvos net8.0 e superiores, com zero avisos IL em tempo de compilação. O projeto `AotSmoke` deste repositório publica uma imagem nativa em CI com `PublishAot=true` e exercita os codecs de quadros, a serialização de modelos por reflexão, a montagem de DI, a montagem do servidor e as consultas às tabelas de idioma.

```bash
dotnet publish AotSmoke/AotSmoke.csproj -c Release -r linux-x64 -p:PublishAot=true -o out
./out/AotSmoke
```

> Plugins, script dinâmico e registro por reflexão em tempo de execução ficam fora do escopo de suporte de AOT (coerente com o NewLife.Core).

## Localização

Exceções e logs compartilham uma tabela de localização gerada em tempo de compilação. Ela segue `CultureInfo.CurrentUICulture` por padrão e recorre ao inglês quando falta uma tradução. Sem resx ou assemblies satélite: uma única DLL, segura para AOT nativamente.

| Ajuste | Finalidade |
|---|---|
| `CultureInfo.CurrentUICulture` | Origem de idioma padrão; o ASP.NET Core pode variá-la por requisição |
| `Localization.Culture` | Força o idioma das exceções e do texto normal |
| `Localization.LogCulture` | Idioma do texto voltado ao desenvolvedor (logs, comentários de configuração) |

```csharp
// Todo o site em português
Localization.Culture = new CultureInfo("pt");

// Serviço web que respeita Accept-Language por requisição
using var culture = RequestCultureScope.Create(Request.Headers.AcceptLanguage);
```

Traduções incluídas: `zh-Hans`, `zh-Hant`, `es`, `de`, `ja`, `fr`, `pt`, `ru`, `ko`, `ar` (10 traduções incluídas). Para adicionar um idioma, mapeie o texto fonte em inglês para a tradução em `Locales/<culture>.yml`; o gerador de código valida, comprime e registra a tabela em tempo de compilação.

## Compilação e testes

```bash
dotnet test -c Release
```

Os testes cobrem codec de quadros, sequências multi-segmento, quadros inválidos, mesclagem de fragmentos, BCD, codecs de áudio, H.264, FLV/fMP4, modo protocolo do servidor e tabelas de idioma.

## Arquitetura

```
Dispositivo JT1078 (TCP/UDP:1078)
    │
    ▼
NewLife.JT1078.Server ─── JT1078Session / JT1078UdpSession
    │  mesclagem de fragmentos RTP
    ▼
JTMessage (mensagem RTP)
    ├──► H264Decoder (vídeo)
    ├──► AudioCodecFactory (áudio)
    │
    ├──► FlvEncoder ──────────► HTTP-FLV / WS-FLV
    ├──► FMp4Encoder ────────► fMP4
    ├──► HlsHandler / TSMuxer ──► HLS (/hls.m3u8)
    ├──► Mp3Handler ─────────► áudio MP3
    ├──► handlers de áudio ───► AAC / PCM / WAV
    │
    ├──► DeviceSessionManager ──► difusão de comandos em cluster (IEventBus)
    │
    ▼
Reprodutor (flv.js / hls.js)
```

## Implantação em cluster

O servidor suporta implantação multi-instância e encaminha comandos de dispositivo entre instâncias por meio de um barramento de eventos:

```csharp
// Autônomo (padrão, EventBus em memória)
services.AddJT1078Gateway().AddTcp().AddHttp();

// Cluster (forneça ICacheProvider via Redis/Stardust)
// Um EventBus Redis é criado automaticamente e os comandos são difundidos para todas as instâncias
```

REST API:
- `POST /Command/Send`: envia um comando a um dispositivo de forma síncrona, roteado automaticamente para a instância que possui o dispositivo
- `POST /Command/SendAsync`: envia um comando de forma assíncrona com suporte a CancellationToken
- `GET /Command/Devices`: lista os dispositivos conectados localmente (TCP/UDP)

## Endpoints ao vivo

| Endpoint | Formato | Transporte | Reprodutor |
|----------|--------|-----------|--------|
| `/live.flv` | FLV | HTTP / WebSocket | flv.js |
| `/live.mp4` | fMP4 | HTTP / WebSocket | MSE nativo |
| `/live.aac` | AAC | HTTP | áudio nativo |
| `/live.pcm` | PCM | HTTP | áudio nativo |
| `/live.wav` | WAV | HTTP | áudio nativo |
| `/live.mp3` | MP3 | HTTP | áudio nativo |
| `/hls.m3u8` + `/hls.ts` | HLS (MPEG-TS) | HTTP | hls.js |

## Referências de protocolo

- JT/T 1078-2016, Sistemas de posicionamento por satélite de veículos de transporte rodoviário: protocolo de comunicação de vídeo
- ISO 14496-12 (ISO BMFF)
- Adobe Flash Video (FLV)

## Agradecimentos

O design e a implementação deste projeto se baseiam no projeto de código aberto [SmallChi/JT1078](https://github.com/SmallChi/JT1078). Muito obrigado.

## Mapa do ecossistema

NewLife é um ecossistema em camadas de projetos open source versionados de forma independente. O mapa abaixo é compartilhado literalmente por todos os repositórios NewLife; cada projeto tem links para o seu repositório e para o seu pacote principal no NuGet.

| Projeto | O que faz | Downloads |
| --- | --- | --- |
| **Base** | | |
| [NewLife.Core](https://github.com/NewLifeX/X) | Log, configuração, cache, rede, serialização, agendamento e rastreamento APM. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Core?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Core) |
| [NewLife.XCode](https://github.com/NewLifeX/NewLife.XCode) | Middleware de dados: tabelas únicas na escala de dezenas de bilhões de linhas, particionamento automático, separação de leitura e escrita, mais de 10 bancos de dados. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.XCode?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.XCode) |
| [NewLife.Net](https://github.com/NewLifeX/NewLife.Net) | Biblioteca de rede de alto desempenho: 1,77M pps / 14,5 Gbps em uma máquina (TCP 1KB, 2026-10), 4M de conexões TCP persistentes. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Net?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Net) |
| [NewLife.Remoting](https://github.com/NewLifeX/NewLife.Remoting) | Framework de comunicação por protocolo e RPC para aplicações cliente/servidor e dispositivos IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Remoting?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Remoting) |
| [NewLife.Agent](https://github.com/NewLifeX/NewLife.Agent) | Instala aplicações .NET como daemons do sistema operacional: serviços do Windows e systemd do Linux. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Agent?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Agent) |
| [NewLife.Templates](https://github.com/NewLifeX/NewLife.Zero) | Modelos de projeto `dotnet new` para console, web, API, serviço, dados e IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Templates?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Templates) |
| **Dados e mensagens** | | |
| [NewLife.Redis](https://github.com/NewLifeX/NewLife.Redis) | Cliente Redis com latência em microssegundos, milhões de operações por segundo e filas de mensagens integradas. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Redis?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Redis) |
| [NewLife.RocketMQ](https://github.com/NewLifeX/NewLife.RocketMQ) | Cliente RocketMQ totalmente gerenciado para Apache RocketMQ e Alibaba Cloud Message Queue. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.RocketMQ?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.RocketMQ) |
| [NewLife.MQTT](https://github.com/NewLifeX/NewLife.MQTT) | Cliente e servidor MQTT, incluindo conectividade com o Alibaba Cloud IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.MQTT?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.MQTT) |
| [NewLife.NovaDb](https://github.com/NewLifeX/NewLife.NovaDb) | Banco de dados híbrido em C#: embarcado ou em rede, relacional, séries temporais, fila de mensagens e KV em um só. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.NovaDb?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.NovaDb) |
| [NewLife.MySql](https://github.com/NewLifeX/NewLife.MySql) | Driver MySql e camada auxiliar construídos sobre a pilha de dados NewLife. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.MySql?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.MySql) |
| **Web, IA e distribuído** | | |
| [NewLife.Cube](https://github.com/NewLifeX/NewLife.Cube) | Framework de plataforma administrativa rápida: permissões, SSO, servidor OAuth e scaffolding CRUD. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Cube?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Cube) |
| [Stardust](https://github.com/NewLifeX/Stardust) | Plataforma de serviços distribuídos: gestão de nós, APM, centro de configuração, registro e centro de publicação. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Stardust?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Stardust) |
| [AntJob](https://github.com/NewLifeX/AntJob) | Plataforma distribuída de tarefas e agendamento de dados em tempo real, com particionamento, repetição e reprocessamento. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.AntJob?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.AntJob) |
| [NewLife.AI](https://github.com/NewLifeX/NewLife.AI) | Cliente unificado para 46 provedores de LLM, além do ChatAI, uma aplicação web de chat pronta para uso. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.AI?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.AI) |
| **Industrial e IoT** | | |
| [NewLife.IoT](https://github.com/NewLifeX/NewLife.IoT) | Biblioteca padrão de IoT que define contratos de protocolo e abstrações de dispositivos. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.IoT?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.IoT) |
| [NewLife.Modbus](https://github.com/NewLifeX/NewLife.Modbus) | Implementação Modbus TCP/RTU/ASCII sobre NewLife.IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Modbus?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Modbus) |
| [NewLife.Siemens](https://github.com/NewLifeX/NewLife.Siemens) | Protocolo Siemens PLC sobre NewLife.IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Siemens?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Siemens) |
| [NewLife.Omron](https://github.com/NewLifeX/NewLife.Omron) | Protocolo Omron PLC sobre NewLife.IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Omron?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Omron) |
| [NewLife.Melsec](https://github.com/NewLifeX/NewLife.Melsec) | Protocolo Mitsubishi Melsec PLC sobre NewLife.IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Melsec?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Melsec) |
| [NewLife.OPC](https://github.com/NewLifeX/NewLife.OPC) | Cliente OPC para acesso a dados industriais. | — |
| [NewLife.BACnet](https://github.com/NewLifeX/NewLife.BACnet) | Pilha do protocolo de automação predial BACnet. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.BACnet?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.BACnet) |
| [ZeroIoT](https://github.com/NewLifeX/ZeroIoT) | Plataforma IoT de código aberto leve, com gestão de gateways e dispositivos. | — |
| **Telemática veicular** | | |
| [NewLife.JT808](https://github.com/NewLifeX/NewLife.JT808) | Protocolo de posicionamento veicular JT/T 808: mais de 70 corpos de mensagem, 2011/2013/2019. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT808?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT808) |
| [NewLife.JT809](https://github.com/NewLifeX/NewLife.JT809) | Protocolo plataforma-a-plataforma JT/T 809 para autoridades de transporte. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT809?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT809) |
| [NewLife.JT1078](https://github.com/NewLifeX/NewLife.JT1078) | Vídeo veicular JT/T 1078: análise RTP, remux para FLV/fMP4 e transmissão ao vivo. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT1078?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT1078) |
| **Componentes de domínio** | | |
| [NewLife.Office](https://github.com/NewLifeX/NewLife.Office) | Um pacote para Excel, Word, PPT, PDF e mais de 15 formatos de documento. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Office?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Office) |
| [NewLife.Audio](https://github.com/NewLifeX/NewLife.Audio) | Áudio multiplataforma: codecs, E/S de dispositivos, DSP, contêineres e streaming. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Audio?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Audio) |
| [NewLife.Video](https://github.com/NewLifeX/NewLife.Video) | Codecs de vídeo, formatos de contêiner, processamento de frames e streaming. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Video?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Video) |
| [NewLife.Map](https://github.com/NewLifeX/NewLife.Map) | Interface unificada sobre Baidu, AMap, Tencent e Tianditu, com geocodificação e conversão de coordenadas. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Map?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Map) |
| [NewLife.IP](https://github.com/NewLifeX/NewLife.IP) | Geolocalização IPv4 com banco de dados offline mapeado em memória e API online como alternativa. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.IP?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.IP) |
| **Ferramentas e produtos** | | |
| [CrazyCoder](https://github.com/NewLifeX/XCoder) | Caixa de ferramentas do desenvolvedor: rede, porta serial, criptografia, regex, Modbus e MQTT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.XCoder?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.XCoder) |
| [GitCandy](https://github.com/NewLifeX/GitCandy) | Sistema auto-hospedado de gerenciamento de código-fonte Git. | — |
| [LuckyClover](https://github.com/NewLifeX/LuckyClover) | Instala automaticamente o runtime .NET correspondente quando iniciado em um host mais antigo. | — |

> Os badges mostram o pacote principal de cada projeto; pacotes de tema, extensão e satélites estão listados nas respectivas páginas do NuGet. Os mais de 30 pacotes não listados aqui (FTP, LoRa, extensões do Office, drivers de protocolo e outros) estão disponíveis na [organização NewLifeX](https://github.com/NewLifeX) e em [newlifex.com](https://newlifex.com).

## Equipe

<img src="https://newlifex.com/logo.png" width="48" align="right" alt="NewLife" />

Fundada em 2002, a equipe NewLife dedica-se há mais de duas décadas ao ecossistema .NET e de IoT, mantém cerca de 100 projetos de código aberto (34 deles principais) e oferece soluções integradas de software e hardware para os setores de energia, logística, saúde, controle industrial e internet. Seguimos o lema "não há hierarquia no aprendizado — o domínio conquista o respeito".

Os componentes de código aberto já foram baixados <!--nuget-total:start-->6.48M<!--nuget-total:end--> vezes no NuGet (todo o ecossistema, até 2026-10).

Website: https://newlifex.com · Organização de código aberto: https://github.com/NewLifeX

<details>
<summary>Conta oficial no WeChat: 智能大石头 (escaneie para seguir)</summary>

![智能大石头](https://newlifex.com/stone.jpg)

</details>

## Licença

[MIT](LICENSE) © 2022-2026 NewLife
