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

- .NET SDK 10.0.301, conforme definido em `global.json`.
- Workload MAUI correspondente à plataforma alvo:
  - Android: `maui-android`
  - iOS (macOS): `maui-ios`
  - Mac Catalyst (macOS): `maui-maccatalyst`
  - Windows (Windows): `maui-windows`

## Build

Execute os comandos a partir da raiz do repositório. O restore é direcionado ao
framework desejado para não exigir os workloads das demais plataformas.

### Android

```bash
dotnet restore Digitavox.csproj -p:TargetFramework=net10.0-android
dotnet build Digitavox.csproj -f net10.0-android --no-restore
```

### Executar no emulador Android pelo terminal

Defina o caminho do Android SDK. O valor abaixo corresponde ao local padrão no
macOS; ajuste-o se o SDK estiver em outro diretório:

```bash
export ANDROID_SDK_ROOT="$HOME/Library/Android/sdk"
```

Liste os dispositivos virtuais disponíveis e copie o nome do AVD desejado:

```bash
"$ANDROID_SDK_ROOT/emulator/emulator" -list-avds
```

Inicialize o emulador. Substitua o valor abaixo pelo nome obtido no comando
anterior:

```bash
export ANDROID_AVD_NAME="NOME_DO_AVD"
"$ANDROID_SDK_ROOT/emulator/emulator" \
  -avd "$ANDROID_AVD_NAME" \
  >/tmp/digitavox-android-emulator.log 2>&1 &
```

Espere o dispositivo aparecer, consulte seu serial e defina-o explicitamente.
Normalmente o primeiro emulador utiliza `emulator-5554`:

```bash
"$ANDROID_SDK_ROOT/platform-tools/adb" wait-for-device
"$ANDROID_SDK_ROOT/platform-tools/adb" devices
export ANDROID_DEVICE_SERIAL="emulator-5554"
```

Espere o Android terminar a inicialização:

```bash
until [ "$("$ANDROID_SDK_ROOT/platform-tools/adb" \
  -s "$ANDROID_DEVICE_SERIAL" shell getprop sys.boot_completed \
  | tr -d '\r')" = "1" ]; do
  sleep 2
done
```

Restaure, compile, instale e execute o aplicativo no emulador selecionado:

```bash
dotnet restore Digitavox.csproj -p:TargetFramework=net10.0-android

dotnet build Digitavox.csproj \
  -t:Run \
  -f net10.0-android \
  -p:Device="$ANDROID_DEVICE_SERIAL" \
  --no-restore
```

Para acompanhar os logs do dispositivo em outro terminal:

```bash
"$ANDROID_SDK_ROOT/platform-tools/adb" \
  -s "$ANDROID_DEVICE_SERIAL" logcat
```

Para encerrar o emulador ao final:

```bash
"$ANDROID_SDK_ROOT/platform-tools/adb" \
  -s "$ANDROID_DEVICE_SERIAL" emu kill
```

### iOS (macOS)

```bash
dotnet restore Digitavox.csproj -p:TargetFramework=net10.0-ios
dotnet build Digitavox.csproj -f net10.0-ios --no-restore
```

### Executar no Simulator iOS pelo terminal (macOS)

Liste os simuladores disponíveis e copie o `UDID` do dispositivo desejado:

```bash
xcrun simctl list devices available
```

Defina o dispositivo, inicialize-o e abra a interface do Simulator. Substitua o
valor abaixo pelo `UDID` obtido no comando anterior:

```bash
export SIMULATOR_UDID="COLE_O_UDID_AQUI"
xcrun simctl boot "$SIMULATOR_UDID" 2>/dev/null || true
xcrun simctl bootstatus "$SIMULATOR_UDID" -b
open -a Simulator
```

Em Macs com Apple Silicon, restaure, compile, instale e execute o aplicativo com:

```bash
dotnet restore Digitavox.csproj \
  -p:TargetFramework=net10.0-ios \
  -p:RuntimeIdentifier=iossimulator-arm64

dotnet build Digitavox.csproj \
  -t:Run \
  -f net10.0-ios \
  -p:RuntimeIdentifier=iossimulator-arm64 \
  -p:_DeviceName=":v2:udid=$SIMULATOR_UDID" \
  --no-restore
```

O segundo comando permanece conectado ao processo e exibe os logs do aplicativo.
Use `Ctrl+C` para encerrar o acompanhamento. Em Macs Intel, substitua
`iossimulator-arm64` por `iossimulator-x64`.

Para desligar o Simulator selecionado ao final:

```bash
xcrun simctl shutdown "$SIMULATOR_UDID"
```

### Windows (Windows)

```powershell
dotnet restore Digitavox.csproj -p:TargetFramework=net10.0-windows10.0.19041.0
dotnet build Digitavox.csproj -f net10.0-windows10.0.19041.0 --no-restore
```

## Assinatura Android (release)

1. Copie `Signing.example.props` para `Signing.local.props`.
2. Preencha credenciais e caminho do keystore local.

`Signing.local.props` é local e não deve ser versionado.

## Licença / License

Este projeto é licenciado sob a Apache License 2.0.

### Referências legais / Legal References

- **Licença (texto oficial) / License (official text):** [LICENSE](LICENSE)
- **Avisos de atribuição / Attribution notices:** [NOTICE](NOTICE)
- **Aviso legal / Disclaimer:** [DISCLAIMER.md](DISCLAIMER.md)
