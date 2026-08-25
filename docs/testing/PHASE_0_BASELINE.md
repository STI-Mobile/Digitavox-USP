# Baseline da Fase 0

Data: 2026-08-24

Branch: `feature/mvvm-architecture-refactor`

## Builds de referência

| Alvo | Resultado | Observações |
| --- | --- | --- |
| `net10.0-android` | Sucesso | 13 avisos, 0 erros. O feed de vulnerabilidades do NuGet estava indisponível. |
| `net10.0-ios` / `iossimulator-arm64` | Sucesso | 13 avisos, 0 erros. O build precisou acessar o CoreSimulatorService fora do sandbox. |
| `net10.0-maccatalyst` | Não executado | O workload `maccatalyst` não está instalado no ambiente. |
| `net10.0-windows` | Não executado | Requer ambiente Windows. |

## Avisos preexistentes relevantes

- `AppShell` e `AlertView` ocultam membros de notificação herdados.
- APIs antigas de callback do TTS são usadas no Android.
- Recursos de teclado/som aparecem duplicados no bundle iOS.
- Há usos de APIs Apple obsoletas e campos não utilizados.
- O projeto usa anotação nullable sem habilitar o contexto nullable.
- A apresentação de tempo preserva plurais em algumas combinações de uma unidade, como `1 minutos` e `1 segundos`.
- Teclas mortas de acento não produzem saída isolada antes de compor a vogal seguinte.

Esses avisos não são corrigidos na Fase 0 para não misturar caracterização com mudança de comportamento.

## Escopo automatizado

As suítes host-side compilam diretamente os fontes atuais de `Course`, `CourseLesson`, `FingerMapping`, `UserProgress` e `DVPersistence`. Dependências de MAUI e mensageria são substituídas somente nos assemblies de testes.

Cobertura inicial:

- Carregamento, seleção e propriedades de curso/lição.
- Ordem dos exercícios.
- Contagem de caracteres, palavras, acertos e erros.
- Percentuais, conclusão, repetições e apresentação de tempo.
- Normalização de teclas Android, Apple e Windows.
- Shift, Caps Lock e composição com acento agudo.
- Cadastro inicial, retomada e avanço do usuário.
- Persistência das principais estatísticas da lição.
- Valores padrão e restauração das configurações.
- Round-trip do arquivo JSON do usuário.
- Ordenação dos arquivos de curso e detecção do diretório inicial.

Os comportamentos dependentes de dispositivo permanecem cobertos pela matriz manual em `MANUAL_SMOKE_TEST_MATRIX.md`.

### Resultado

- `Digitavox.CharacterizationTests`: 21 aprovados, 0 falhas.
- `Digitavox.PersistenceCharacterizationTests`: 5 aprovados, 0 falhas.
- Total: 26 aprovados, 0 falhas.

### Como executar

```bash
dotnet test Tests/Digitavox.CharacterizationTests/Digitavox.CharacterizationTests.csproj
dotnet test Tests/Digitavox.PersistenceCharacterizationTests/Digitavox.PersistenceCharacterizationTests.csproj
```

Não use a solução completa para executar somente os testes em um ambiente sem todos os workloads MAUI: ela também tentará avaliar os alvos do aplicativo.

## Métricas arquiteturais iniciais

| Métrica | Baseline |
| --- | ---: |
| Views XAML | 17 |
| Views com ViewModel injetado | 16 |
| Views com cast concreto de `BindingContext` | 16 |
| Views com `OnAppearing` | 15 |
| Views com `x:DataType` | 0 |
| Arquivos em `ViewModels` | 18 |
| ViewModels/serviços de apresentação que usam `DVPersistence` | 17 |
| Arquivos de ViewModel que usam `Shell.Current` | 6 |
| Arquivos de ViewModel ligados a tipos/APIs de apresentação MAUI | 17 |

## Limites conhecidos da automação inicial

Login, navegação, TTS, áudio, alertas de acessibilidade e o fluxo visual de configurações continuam dependentes de objetos concretos/globais do MAUI. Na Fase 0 eles são congelados pela matriz manual; a introdução das interfaces na Fase 1 permitirá testes host-side desses fluxos sem reproduzir o framework dentro da suíte.
