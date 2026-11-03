# Implementação da migração do catálogo versão 1

Implementação de 5 de outubro de 2026, baseada na [auditoria de impacto](content-removal-impact-audit.md). A auditoria permanece como registro do baseline `94a76a7`; referências nela a arquivos posteriormente removidos devem ser consultadas nessa revisão do Git.

## Catálogo distribuído

O catálogo legado é versão **0**, identificada pela ausência de metadata. O catálogo atual é versão **1**, independente de `ApplicationVersion` e `ApplicationDisplayVersion`.

| ID de curso | `NUMEROCURSO` preservado | Lições ativas |
| --- | --- | --- |
| `musicas` | 3 | `LICAO1`…`LICAO4` |
| `musica_se_eu_quiser_falar_com_deus` | 4 | `LICAO1` |
| `SENAI` | 5 | `LICAO1`…`LICAO196` |

Foram excluídos do pacote `Resources/Raw/curso_abnt2_basico.json` e `Resources/Raw/curso_abnt2_intermediario.json`. `Resources/Raw/musicas.json` não contém mais `LICAO5`, sua apresentação, instruções nem exercícios. O total de lições e a apresentação foram ajustados para quatro. As quatro lições sobreviventes não foram renumeradas e conservam seus objetos JSON originais; testes com hashes do baseline protegem essa identidade e o conteúdo.

A ordem dos cursos é 3/4/5. Os números de opções do menu continuam sendo posições visuais 1/2/3, como no modelo anterior; isso não modifica `NUMEROCURSO`, o nome do arquivo nem a chave de progresso. SENAI, a música individual e sons compartilhados não foram alterados.

## Arquitetura implementada

- [CatalogMigration](../Infrastructure/Persistence/CatalogMigration.cs) contém a única evolução suportada, `0 → 1`, e a lista autoritativa dos três IDs ativos. O Bundle fornece os payloads. Não foi criado um framework de migrations nem um manifesto de arquivos adicional no Bundle.
- [CourseCatalogValidator](../Domain/Catalog/CourseCatalogValidator.cs) verifica estrutura, quantidade, lições, exercícios e parâmetros necessários. [DVPersistence.ReadCourseFiles](../Helpers/DVPersistence.cs) ordena por número sem exigir continuidade e rejeita duplicatas/JSONs inválidos. JSONs auxiliares sem discriminador/estrutura de curso não se tornam cursos.
- [UserProgressMigration](../Domain/Catalog/UserProgressMigration.cs) faz a transformação pura de perfis, conservando dados desconhecidos e estatísticas sem converter pelo parser legado de dicionários.
- [AtomicJsonFile](../Infrastructure/Persistence/AtomicJsonFile.cs) grava somente bytes diferentes, em temporário no mesmo diretório, com flush e substituição do arquivo. O journal permite repetir operações após interrupção entre arquivos.
- [MauiAppStartupService](../Infrastructure/Startup/MauiAppStartupService.cs) aguarda a migration, invalida seleção/cache de `Course`, carrega o catálogo validado e devolve Login ou Tutorial. `App` cria o Shell depois desse resultado.
- [MauiCourseCatalogStore](../Infrastructure/Persistence/MauiCourseCatalogStore.cs) recusa versão ausente/pending, carrega somente os IDs ativos e verifica topologia, números e ausência de `LICAO5`. Um arquivo local extra não se transforma em curso executável.
- [ContentAvailability](../Domain/Catalog/ContentAvailability.cs) guarda as identidades definitivamente retiradas. [Course](../Models/Course.cs) filtra cursos retirados e bloqueia seleção/leitura de lições indisponíveis, inclusive se receber uma definição legada por outro store. Esses IDs não devem ser reciclados.
- [IUserHistoryStore](../Application/Abstractions/IUserHistoryStore.cs), [MauiUserHistoryStore](../Infrastructure/Persistence/MauiUserHistoryStore.cs) e [ArchivedProgressReader](../Domain/Catalog/ArchivedProgressReader.cs) oferecem consulta de estatísticas independente do catálogo executável. A opção 3 do menu de usuário abre esse histórico; Enter não inicia exercícios nessa consulta.

O método legado `DVPersistence.CopyCourseFilesAsync` agora delega à migration. Não há um segundo importador que possa copiar payload antigo ignorando as proteções.

## Dados no sandbox

Todos os caminhos são relativos a `FileSystem.AppDataDirectory`; a localização física varia por plataforma.

| Caminho | Conteúdo |
| --- | --- |
| `Courses/<id>.json` | Os três catálogos executáveis atuais. |
| `Users/<NOME>.json` | Progresso operacional no mesmo schema legado, sem novas metadata de versão na raiz. |
| `History/Users/<arquivo-do-perfil>.json` | Objetos de progresso/tentativas arquivados, identificados pelos IDs originais; não são cópias de catálogos. |
| `Catalog/state.json` | Certificação do estado publicado: `{"catalogVersion":1}`. |
| `Catalog/pending.json` | Journal: versão alvo e indicação de primeira instalação, mantida durante a recuperação. Ausente após sucesso. |

Metadata e histórico ficam fora de `Courses` e `Users`. Os históricos usam schema próprio com `catalogVersion` e `courses`. Não foi inserida uma raiz numérica incompatível no JSON de usuário.

Não há marcador separado por perfil: **todos os perfis são inspecionados a cada inicialização**, incluindo anônimo e perfis não utilizados recentemente. O estado global só é publicado depois das gravações planejadas serem verificadas. Isso também detecta perfis legados restaurados depois de uma migration anterior.

## Fluxo de atualização e recuperação

```text
App aguarda InitializeAsync
  → inicializa mapeamento de teclado
  → detecta versão e journal pendente
  → prepara e valida os três cursos do Bundle
  → prepara transformações de TODOS os perfis e valida histórico existente
  → se versão 1 e estado já íntegro: nenhuma gravação
  → senão: grava journal e retira certificação anterior
  → grava histórico antes de progresso de cada perfil
  → grava cursos ativos
  → exclui arquivos locais dos dois cursos retirados
  → verifica bytes publicados e ausência dos arquivos retirados
  → publica catalogVersion = 1 e remove journal
  → invalida cache/seleção e carrega Course
  → Login ou Tutorial
```

As definições são preparadas em memória antes de qualquer gravação. O journal não guarda letras nem catálogos antigos: a recuperação recompõe os resultados a partir do Bundle atual, perfis e históricos já gravados. Cada escrita é substituída individualmente; não existe uma transação única entre todos os arquivos. Enquanto há journal, o adapter bloqueia carregamento do catálogo, e uma interrupção anterior ao commit não é certificada como versão 1.

Uma repetição após histórico gravado, mas perfil ainda legado, faz merge por chave original. Registros iguais não são duplicados; conflitos entre tentativas com a mesma identidade interrompem o processo sem sobrescrever o histórico. Se o perfil já foi atualizado, sua transformação é um no-op. A certificação é gravada somente depois da conferência final.

Arquivos locais desconhecidos não são apagados automaticamente e não são carregados pelo catálogo ativo. A limpeza física elimina os dois nomes conhecidos retirados e substitui músicas por uma definição sem a quinta lição. Não se usa backup de catálogo com payload retirado como fallback.

## Progresso e política histórica

| Dados legados | Tratamento |
| --- | --- |
| `curso_abnt2_basico`, `curso_abnt2_intermediario` | Objetos de progresso arquivados integralmente por ID e removidos do perfil operacional. Nenhum curso executável correspondente permanece. |
| `musicas/LICAO5.<tentativa>` | Todas as tentativas arquivadas, incluindo `CONCLUIU = N` e lacunas nos números de tentativa. Chaves não são renumeradas. |
| Fronteira de músicas válida 0…5 | Valor operacional limitado a 4. A fronteira anterior é guardada no histórico quando há quinta lição arquivada ou fronteira 5. |
| `musicas/LICAO1`…`LICAO4` | Tentativas e valores estatísticos preservados. |
| SENAI e música individual | Progresso preservado; perfis sem transformação necessária permanecem byte a byte iguais. |
| Fronteira negativa, maior que 5, não inteira ou de tipo inválido em músicas | Erro explícito antes de gravações; sem reset silencioso. |
| JSON inválido, histórico incompatível ou tentativa em conflito | Erro explícito; manter arquivos para diagnóstico e recuperação. |

O avanço existente é conservado: uma fronteira 4 ou 5 legada mantém as quatro lições sobreviventes acessíveis. `LastAvailableLesson` limita a disponibilidade ao total atual. `LessonConcluded` reconhece a fronteira legada e tentativas bem-sucedidas sem depender de existir uma próxima lição. Assim, a lição 4 não passa a ser apresentada como incompleta apenas por se tornar a última.

O histórico mostra fronteira anterior e estatísticas de tentativas, com datas, acertos, erros, velocidade, palavras, tempos e repetições presentes nos registros. Não fabrica certificado, percentual geral ou conclusão histórica que não existiam. A fronteira é evidência do avanço registrado pelo modelo legado, não uma reconstrução de tentativas ausentes. Acerto e tempo histórico não são recalculados com o catálogo novo.

Perfis/metadata/histórico aceitam UTF-8 com BOM, preservando compatibilidade de leitura. Campos desconhecidos no perfil não são descartados pela migration. O parser operacional legado continua com suas limitações de tipos, como já documentado na auditoria; esta mudança não é uma reescrita geral da persistência.

## Instalação limpa, upgrade e repetição

**Instalação limpa:** prepara diretamente a versão 1 com três cursos e quatro músicas. Não importa nem cria histórico fictício dos alvos retirados. Abre Tutorial após concluir a preparação; o startup seguinte abre Login.

**Upgrade:** verifica o estado mesmo quando `Courses` existe. Pasta vazia, importação parcial e arquivo ativo ausente são reparados a partir do Bundle atual. Perfis existentes e perfil anônimo são reconciliados antes do catálogo em memória. A criação de `Courses` durante reparo não reinicia o Tutorial de uma instalação reconhecida como existente; o journal conserva o destino de primeira instalação durante uma tentativa interrompida.

**Posição atual:** esta versão não persiste um cursor de curso/lição/exercício. Ao iniciar uma nova sessão, seleção e cache são invalidados, e o usuário volta à identificação/tutorial. Não há remapeamento de um `currentExercise` persistido inexistente nem alteração do ciclo de suspensão/retomada dentro de uma sessão já ativa.

**Idempotência:** versão 1 não é um atalho que dispense conferir dados restaurados. A definição esperada e todos os perfis são verificados novamente. Se já estão coerentes, nenhum arquivo é regravado. Testes com conteúdo e timestamps dos arquivos comprovam:

```text
migration(migration(estado)) == migration(estado)
```

Também há verificação de que um novo startup depois do sucesso não altera os arquivos e de que novas estatísticas de uma lição ativa continuam sendo gravadas normalmente sem desbloquear `LICAO5`.

## Diagnóstico de falha

1. O usuário verá a tela de erro de inicialização; o aplicativo não deve abrir um catálogo legado como fallback. `App.OnWindowCreated` registra a exceção e sua causa no logger existente.
2. Em um ambiente autorizado de suporte/teste, identificar `FileSystem.AppDataDirectory` da instalação afetada. Preservar cópias dos perfis, histórico e metadata antes de qualquer intervenção. Não coletar conteúdo pessoal sem necessidade.
3. Verificar `Catalog/pending.json` e `Catalog/state.json`. Journal indica publicação pendente; presença da pasta `Courses` não comprova sucesso. A versão publicada esperada é 1.
4. Conferir a exceção: perfil JSON inválido, fronteira inválida, conflito de histórico, Bundle incompleto, versão incompatível ou falha de filesystem. Não apagar `Users`/`History`, zerar fronteiras nem sobrescrever tentativas para eliminar o erro.
5. Resolver a causa com dados válidos, preservando os registros originais. Reiniciar com a mesma release: o processo recupera operações pendentes e confere todos os perfis novamente. Não restaurar os cursos retirados como mecanismo de recuperação.
6. Confirmar três IDs ativos, música sem `LICAO5`, histórico esperado, ausência do journal e `catalogVersion = 1`. Executar novamente e comparar estado. Para versões de catálogo futuras não suportadas, utilizar uma versão do app compatível; esta release não faz downgrade automático.

Erros de consulta de histórico na UI mostram mensagem de indisponibilidade sem reset de progresso. A migration não elimina dados inválidos silenciosamente; um perfil inválido pode bloquear a inicialização até ser recuperado. Essa escolha evita publicação parcialmente válida e perda de dados.

## Validação automatizada e limites

| Suíte | Executados | Aprovados | Falhos | Ignorados |
| --- | --- | --- | --- | --- |
| `Digitavox.CharacterizationTests` | 47 | 47 | 0 | 0 |
| `Digitavox.PersistenceCharacterizationTests` | 84 | 84 | 0 | 0 |
| `Digitavox.PresentationTests` | 4 | 4 | 0 | 0 |
| **Total** | **135** | **135** | **0** | **0** |

Os testes compilam fontes reais de modelos, migration, startup e adapters. Storage/Preferences são doubles direcionados a diretórios temporários. A suíte de apresentação compila o ViewModel real e tipos MAUI neutros, usando doubles de fala, funções comuns, teclado e navegação; não representa validação nativa da UI/TTS.

Cobertura integrada: 16 cenários (os 15 solicitados, mais perfil UTF-8 com BOM), nove checkpoints de interrupção, corrupção antes do commit, restauração após marcador 1, avanço/conclusão real de exercício ativo, histórico e repetição de startup. Fixtures retiradas contêm payload sintético com identidades/topologia legadas, sem perpetuar letras retiradas.

Falhas encontradas durante as fases foram corrigidas antes dos respectivos commits: compilação de uma fixture, leitor de inventário que não aceitava BOM, compatibilidade da migration com perfis BOM e rótulo de caractere no histórico de erros. Nenhum teste foi relaxado para aceitar perda de dados.

Os itens `MauiAsset` do projeto foram avaliados pelo MSBuild e verificados automaticamente, incluindo ausência dos dois arquivos retirados. Essa avaliação **não é construção nem inspeção de pacote nativo final**.

Os builds `net10.0-android`, `net10.0-ios` e `net10.0-maccatalyst` foram tentados e não puderam ser executados: `NETSDK1147`, workloads `maui-android`, `maui-ios` e `maui-maccatalyst` ausentes. Builds Windows, dispositivos, assinatura, pacote final e upgrade de binário de loja não foram executados. A [matriz manual](testing/MANUAL_SMOKE_TEST_MATRIX.md) permanece pendente de homologação nesses ambientes. Não houve instalação de workloads, publicação, push ou merge.

Houve avisos `NU1900` porque a consulta de vulnerabilidades do feed NuGet estava indisponível; isso não é teste de segurança aprovado. As dependências de testes estavam disponíveis e as três suítes foram executadas.

Comandos de regressão, com SDK definido no `global.json`:

```bash
dotnet test Tests/Digitavox.CharacterizationTests/Digitavox.CharacterizationTests.csproj
dotnet test Tests/Digitavox.PersistenceCharacterizationTests/Digitavox.PersistenceCharacterizationTests.csproj
dotnet test Tests/Digitavox.PresentationTests/Digitavox.PresentationTests.csproj
```

O runner VSTest usa socket local. Em ambiente restrito, executar com a permissão necessária para esse socket; um abort por `SocketException` não conta como teste aprovado. Os testes usam dados temporários e não acessam o sandbox instalado do aplicativo.

## Histórico das fases

| Fase | Commit | Gate automatizado |
| --- | --- | --- |
| 1. Proteção de catálogo | `b5e2e82` — permitir numeração não consecutiva | 54 aprovados |
| 2. Versão e publicação recuperável | `3f0363b` — versionamento e migração | 62 aprovados |
| 3. Progresso e consulta histórica | `bd7ce0f` — migrar progresso dos conteúdos removidos | 89 aprovados |
| 4. Startup e bloqueio do legado | `2b50e32` — integrar migração à inicialização | 100 aprovados |
| 5. Retirada do Bundle | `0effd46` — remover conteúdos descontinuados | 107 aprovados |
| 6. Upgrade e compatibilidade | `726b1d9` — validar upgrades e corrigir histórico | 135 aprovados |
| 7. Documentação | `docs: documentar migração do catálogo versão 1` | Regressão final das três suítes |

A implementação mantém commits incrementais na branch de trabalho. A homologação nativa e a inspeção do pacote final devem ocorrer antes de distribuir esta release.
