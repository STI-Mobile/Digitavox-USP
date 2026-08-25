# Arquitetura MVVM do Digitavox

## Objetivo

O aplicativo adota MVVM com dependências direcionadas para o núcleo. A interface do usuário renderiza estado e encaminha eventos; regras, coordenação e integrações ficam fora das Views.

```text
Views (XAML + code-behind passivo)
                  |
                  v
ViewModels --> Presentation --> Models / Domain
     |                 |              |
     +-----------------+--------------+
                       v
              Application contracts
                       ^
                       |
             Infrastructure adapters

MauiProgram / App / AppShell = composition root e limites do framework
```

As setas representam dependências de código. A infraestrutura implementa contratos do núcleo; o núcleo não conhece implementações MAUI ou APIs específicas de plataforma.

## Responsabilidades

### Views

- Definem layout, estilos, acessibilidade e bindings compilados com `x:DataType`.
- Recebem o ViewModel por injeção e o atribuem ao `BindingContext`.
- Encaminham ativação da página e entrada de teclado ao ViewModel.
- Não acessam persistência, fala, navegação, modelos de domínio ou APIs de plataforma.

O code-behind é mantido somente como adaptador do ciclo de vida e dos eventos do MAUI. Os overrides `async void OnAppearing()` são limites de evento do framework; operações internas retornam `Task`.

### ViewModels

- Expõem estado observável e recebem intenções da View.
- Coordenam navegação, fala, configurações e ambiente exclusivamente por contratos em `Application/Abstractions`.
- Usam mensagens tipadas para eventos que atravessam o limite de plataforma.
- Não acessam `Shell.Current`, `Application.Current`, singletons de plataforma nem persistência estática.
- Não bloqueiam a thread de UI; temporizações e navegação são assíncronas.

`DVViewModelFunctions` e `DVViewModelSpeak` são fachadas de compatibilidade para fluxos de apresentação compartilhados. Suas dependências externas já estão invertidas e as responsabilidades determinísticas foram extraídas para `Presentation`.

### Presentation

- Contém componentes determinísticos e testáveis de entrada, texto e estado da página.
- Define `IKeyboardInputHandler`, contrato usado entre os adaptadores nativos, a View e o ViewModel.
- Não conhece infraestrutura ou plataforma.

### Models e Domain

- Mantêm estado e regras de curso, lição, progresso, exercício e tempo.
- Dependem apenas de contratos quando precisam carregar ou salvar dados.
- Podem ser exercitados sem inicializar MAUI.

### Application

- Define portas para navegação, fala, configurações, persistência, ambiente, relógio e inicialização.
- Define mensagens como tipos, sem discriminadores textuais.
- Não referencia MAUI, infraestrutura nem projetos de plataforma.

### Infrastructure e Platforms

- Implementam as portas usando MAUI, armazenamento, áudio e APIs nativas.
- Concentram os acessos legados a `DVPersistence`, `DVSpeak`, `Shell.Current` e `Application.Current`.
- Traduzem eventos nativos para contratos e mensagens do núcleo.

### Composition root

- `MauiProgram` registra contratos, implementações e lifetimes.
- `App` coordena eventos globais de janela.
- `AppShell` registra rotas tipadas e executa a inicialização assíncrona antes de selecionar a primeira tela.

## Lifetimes

- Estado da sessão é singleton: `Course`, `CourseLesson`, `FingerMapping`, `UserProgress`, `DVViewModelSpeak` e `DVViewModelFunctions`.
- ViewModels de tela e Views são transient, evitando estado visual residual entre navegações.
- Adaptadores sem estado ou que representam um recurso compartilhado são registrados como singleton.
- `KeyboardInputProcessor` é transient porque conserva estado de combinações de teclas por tela.

## Fluxos principais

### Ativação de tela

```text
MAUI OnAppearing -> View -> ViewModel.OnPageAsync
                              |
                              +-> atualiza estado observável
                              +-> coordena fala por ISpeechService
                              +-> solicita foco por mensagem tipada
```

### Entrada de teclado

```text
evento nativo -> IKeyboardInputHandler da View
             -> IKeyboardInputHandler do ViewModel
             -> KeyboardInputProcessor
             -> altera estado / executa intenção
```

### Persistência

```text
Model ou ViewModel -> contrato Application
                  -> adaptador Infrastructure
                  -> armazenamento MAUI / legado encapsulado
```

## Garantias automatizadas

`ArchitectureBoundaryTests` protege as seguintes invariantes:

- o núcleo não referencia infraestrutura, plataforma ou globais legados;
- Views dependem somente de ViewModels e contratos de apresentação;
- todo XAML de tela possui binding compilado;
- ViewModels não executam espera bloqueante;
- adaptadores de plataforma não constroem ViewModels fora do contêiner;
- lifetimes de tela e sessão permanecem corretos;
- o contrato de teclado permanece na camada de apresentação.

Validação local:

```bash
dotnet test Tests/Digitavox.CharacterizationTests/Digitavox.CharacterizationTests.csproj
dotnet test Tests/Digitavox.PersistenceCharacterizationTests/Digitavox.PersistenceCharacterizationTests.csproj
dotnet build Digitavox.csproj -f net10.0-android -t:Compile --no-restore
```
