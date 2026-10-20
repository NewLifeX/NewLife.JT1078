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

- **Protocolo RTP JT1078**: análise e construção completas com 29 tipos de carga de áudio/vídeo
- **Delimitação de quadros**: divisão de pacotes colados e análise header-first baseadas em `IMessageCodec` do NewLife.Core
- **Mesclagem de fragmentos RTP**: armazenamento e mesclagem automática de fragmentos I/P/B
- **Decodificação de áudio**: G.711A, G.711U, IMA-ADPCM com remoção do cabeçalho HiSilicon
- **Análise H.264**: divisão NAL, SPS/PPS, códigos de início e bytes de prevenção
- **Remuxagem**: FLV e fMP4 em tempo real, saída HTTP-FLV/WS-FLV, fMP4 e HLS
- **Servidor**: TCP/UDP, endpoints HTTP/WS, reprodução e barramento de comandos em cluster (IEventBus)
- **NativeAOT**: protocolo e tabelas de idioma sem reflexão; imagem nativa verificada em CI
- **Localização**: mensagens fonte em inglês, 10 traduções incluídas e fallback para inglês

## Início rápido

```bash
dotnet add package NewLife.JT1078           # biblioteca de protocolo (inclui FLV/fMP4)
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

// Ou integração com ASP.NET Core
builder.Services.AddJT1078Gateway().AddTcp().AddHttp();
```

## Pacotes NuGet

| Pacote | Descrição |
|---|---|
| NewLife.JT1078 | Protocolo + codecs + remuxagem FLV/fMP4 |
| NewLife.JT1078.Server | Servidor TCP/UDP + builder DI + HLS/MP3 |

## Native AOT

A biblioteca de protocolo e o servidor na configuração padrão suportam NativeAOT e trimming. Os analisadores AOT/trim estão sempre ativos para net8.0 e superior, sem avisos IL. O projeto `AotSmoke` publica uma imagem nativa em CI com `PublishAot=true` e executa os codecs de quadros e as tabelas de idioma.

```bash
dotnet publish AotSmoke/AotSmoke.csproj -c Release -r linux-x64 -p:PublishAot=true -o out
./out/AotSmoke
```

## Localização

Exceções e logs compartilham uma tabela gerada em tempo de compilação. Segue `CultureInfo.CurrentUICulture` e recorre ao inglês quando falta tradução. Sem resx ou assemblies satélite: uma única DLL, compatível com AOT.

```csharp
Localization.Culture = new CultureInfo("pt");
using var culture = RequestCultureScope.Create(Request.Headers.AcceptLanguage);
```

Idiomas incluídos: `zh-Hans`, `zh-Hant`, `es`, `de`, `ja`, `fr`, `pt`, `ru`, `ko`, `ar`. Para adicionar um idioma, preencha `Locales/<culture>.yml`.

## Compilação e testes

```bash
dotnet test -c Release
```

Os testes cobrem codec de quadros, sequências multi-segmento, quadros inválidos, mesclagem de fragmentos, BCD, codecs de áudio, H.264, FLV/fMP4, modo protocolo do servidor e tabelas de idioma.

## Ecossistema

A matriz completa de projetos está em [README.md](README.md#ecosystem) (idêntica em todos os repositórios NewLife).

## Licença

[MIT](LICENSE) © 2022-2026 NewLife
