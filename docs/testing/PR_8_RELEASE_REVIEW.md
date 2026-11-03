# Validação do PR #8 — release/v1.1.0

Data: 03/11/2026. Base analisada: `origin/main` (`f27e41b`),
head original: `a5c6cb4`. Review consultado:
https://github.com/STI-Mobile/Digitavox-USP/pull/8#discussion_r4455906153.

## P2 corrigido

`App.CreateWindow` exibe `StartupView` até `InitializeAsync` concluir. Nesse
intervalo, e depois de uma falha, `Shell.Current` pode ser nulo. O acesso direto
a `CurrentPage` nos adaptadores nativos podia lançar `NullReferenceException`
antes de verificar o contrato de teclado. A falha de inicialização prolongava
essa condição indefinidamente.

Todos os acessos de despacho de teclado passaram a usar `Shell.Current?.CurrentPage`:

| Plataforma | Entradas |
| --- | --- |
| Android | `MainActivity.OnKeyDown`, `MainActivity.OnKeyUp` |
| Windows | `App.OnKeyDown`, `App.OnKeyUp`, chamados por `HookProcedure` |
| iOS | `DVLayoutView.PressesBegan`, `DVLayoutView.PressesEnded` |
| Mac Catalyst | `DVLayoutView.PressesCancelled` |

Sem Shell/página/contrato, Android e Apple mantêm a chamada à implementação base;
Windows retorna `false`, como já fazia para páginas sem contrato. Não há fila
de eventos nem acesso ao mapeamento de teclas antes de existir uma página apta.
Depois da instalação do Shell, o despacho, os modificadores e os valores de
retorno continuam os mesmos. Não foi necessário alterar a inicialização,
a navegação ou a persistência.

## Evidências automatizadas

Execução final com SDK 10.0.301, em macOS ARM64:

| Suíte | Aprovados | Falhas | Ignorados |
| --- | ---: | ---: | ---: |
| Digitavox.CharacterizationTests | 51 | 0 | 0 |
| Digitavox.PersistenceCharacterizationTests | 84 | 0 | 0 |
| Digitavox.PresentationTests | 9 | 0 | 0 |

Comando por projeto: `dotnet test Tests/<projeto>/<projeto>.csproj --no-restore --nologo`.
Total: **144 aprovados**. `git diff --check` sem erros.

Foram acrescentados quatro casos de fronteira no código-fonte, um por plataforma,
contra a reintrodução de desreferência direta do Shell nos adaptadores de teclado.
São verificações estáticas, não testes de execução dos callbacks nativos.
Os testes existentes também exercitam mapeamento/modificadores, isolamento do
estado de teclado, startup normal/falha/repetição, migração interrompida,
idempotência, preservação de histórico e apresentação de login/histórico.

A primeira execução no sandbox foi abortada por bloqueio do socket local do
VSTest; a reexecução fora do sandbox concluiu as três suítes. Houve aviso NU1900:
a consulta de vulnerabilidades do NuGet não estava acessível. Isso não constitui
uma validação de segurança das dependências.

## Builds e limites

Foram tentados `dotnet build Digitavox.csproj -f <alvo> --no-restore --nologo`
para Android, iOS, Mac Catalyst e Windows. **Nenhum build nativo foi validado**:
os três primeiros pararam em NETSDK1147, por ausência de `maui-android`,
`maui-ios` e `maui-maccatalyst`. Windows não integra os TargetFrameworks deste
host macOS; a tentativa explícita também parou na resolução de workloads
(o SDK reportou `maui-tizen`). A validação Windows precisa ocorrer em Windows.

Não foram executados testes em dispositivo/simulador, instalação/upgrade de
pacote distribuído, inspeção de bundle, assinatura, TTS/leitor de tela ou eventos
reais de teclado. Os casos pendentes, de erro e de transição para o Shell foram
acrescentados à matriz manual. Não se atribui aprovação a esses cenários nativos.

## Revisão complementar e pendências separadas

Revisão dirigida de composição/rotas, callbacks de teclado/ciclo de vida,
estado de entrada e persistência/migração: nenhum bloqueio adicional confirmado
nesses caminhos pela inspeção e pelas suítes executadas. A migração publica o
histórico antes do perfil, usa journal e gravação atômica, e bloqueia o catálogo
incompleto; os testes de interrupção e repetição passaram.

- **Não bloqueante, acessibilidade:** `StartupView.xaml` mantém a descrição
  semântica fixa “Inicializando aplicativo” depois que `ShowInitializationError`
  muda o texto. O erro é anunciado explicitamente, mas a descrição ao voltar
  o foco merece ajuste e validação com leitor de tela em trabalho separado.
- **Risco a validar, sem reprodução nativa:** vários `OnAppearing` são `async void`
  e aguardam métodos com `Task.Delay`; tarefas de navegação também são descartadas.
  Falhas nessas operações e navegação rápida podem escapar ao tratamento local
  ou deixar continuações de telas anteriores. A proteção do Shell não cobre
  esse ciclo de vida; não se introduziu uma refatoração assíncrona nesta correção.

O P2 está corrigido e o PR está pronto para **nova revisão**. A prontidão para
merge continua condicionada aos builds nativos e ao smoke test da matriz,
especialmente teclado durante startup/falha e upgrade de uma instalação existente.
