# Digitavox USP

O Digitavox USP é um curso de digitação voltado principalmente para pessoas com deficiência visual que, por meio da fala, orienta e guia o usuário em exercícios de digitação em teclado padrão ABNT2. O aplicativo permite acompanhar o progresso ao longo do curso com estatísticas de acertos e tempos de resposta.

## Requisitos de uso

- Teclado padrão ABNT2 com cabo.
- Adaptador USB para micro USB, USB-C ou Lightning (conector OTG), conforme o modelo do celular.

## Reconhecimento de Origem

Este aplicativo é baseado no software original Digitavox, desenvolvido por Neno Henrique da Cunha Albernaz em seu mestrado de 2011 na Universidade Federal do Rio de Janeiro (UFRJ), e amplamente utilizado por instituições que apoiam pessoas com deficiência visual.

A presente versão foi desenvolvida pela Superintendência de Tecnologia da Informação da Universidade de São Paulo (STI-USP), com tecnologias atualizadas de vocalização e suporte multiplataforma. A especificação, validação e disseminação desta versão contam com apoio das seguintes instituições:

- Lar das Moças Cegas
- Bengala Verde
- ADEVA
- Associação Catarinense para Integração do Cego
- Fundação Dorina Nowill
- SENAI
- Comissão Permanente de Acessibilidade e Inclusão da UNESP
- Coordenadoria de Ações Educacionais (CAED), da Universidade Federal de Santa Maria (UFSM)
- Pró-Reitoria de Inclusão e Pertencimento da USP
- Departamento de Biomateriais e Biologia Oral da Faculdade de Odontologia da USP
- Secretaria de Estado dos Direitos da Pessoa com Deficiência do Governo de São Paulo

## Escopo de Publicação e Evolução

A versão disponibilizada neste repositório é equivalente à última versão publicada do aplicativo até a data de publicação deste repositório.

Qualquer trabalho derivado poderá e deverá ser realizado em repositório/projeto próprio.

A evolução do aplicativo principal permanece sob responsabilidade da STI no repositório original e não será automaticamente implementada neste repositório.

## Requisitos de desenvolvimento

- .NET SDK 8.0.x (Android/iOS)
- .NET SDK 9.0.x (Windows)
- Workloads MAUI conforme plataforma alvo
  - Android: `maui`, `android`
  - iOS (macOS): `maui`, `ios`, `maccatalyst`
  - Windows (Windows): `maui`

## Build

```bash
dotnet restore
dotnet build Digitavox/Digitavox.csproj -f net8.0-android
dotnet build Digitavox/Digitavox.csproj -f net8.0-ios
dotnet build Digitavox/Digitavox.csproj -f net9.0-windows10.0.19041.0
```

## Assinatura Android (release)

1. Copie `Digitavox/Signing.example.props` para `Digitavox/Signing.local.props`.
2. Preencha credenciais e caminho do keystore local.

`Signing.local.props` é local e não deve ser versionado.

## Licença / License

Este projeto é licenciado sob a Apache License 2.0.

### Referências legais / Legal References

- **Licença (texto oficial) / License (official text):** [LICENSE](LICENSE)
- **Avisos de atribuição / Attribution notices:** [NOTICE](NOTICE)
- **Aviso legal / Disclaimer:** [DISCLAIMER.md](DISCLAIMER.md)
