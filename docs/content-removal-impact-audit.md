# Auditoria de impacto da remoção de conteúdo — Digitavox Adulto

Data: 5 de outubro de 2026. Revisão auditada: `94a76a717b09f4bf6a9453c82e6c56a5e8ce56b0`.

## Síntese e escopo

**Alterar somente o JSON do Bundle não remove conteúdo de instalações existentes.** O aplicativo importa os cursos para `FileSystem.AppDataDirectory/Courses` somente quando essa pasta não existe e utiliza esses arquivos locais no runtime. Além disso, a exclusão dos cursos Básico e Intermediário quebra a leitura atual, que pressupõe `NUMEROCURSO` consecutivos começando em 1. A remoção da última música exige normalização do progresso: `ULTIMACONCLUIDA = 5` continua produzindo uma lição disponível 5 mesmo quando o novo total é 4.

Recomenda-se uma **migração explícita de catálogo e dos registros afetados**, com versão, publicação recuperável, preservação de estatísticas históricas sem payload dos conteúdos retirados e bloqueio central de acesso. Não é necessário zerar nem reconstruir indiscriminadamente o progresso de todos os cursos.

Esta auditoria criou somente este documento no repositório. Foram feitas leituras do código e da tag local `v1.0.5`, inventário dos JSONs e simulações com fontes de produção em um projeto temporário, usando um sandbox fictício em `/private/tmp`. Nenhum sandbox real do aplicativo foi acessado. Código, JSONs, assets e dados persistidos do aplicativo permaneceram intactos.

Os nomes solicitados foram associados ao código disponível: **Básico e Intermediário são dois cursos independentes**, e o módulo de músicas é outro curso. Não há entidade `Module`, curso Avançado, percentual geral de curso ou pré-requisito entre cursos nesta revisão. O exemplo de Avançado bloqueado é tratado como hipótese de outra versão/produto, não como comportamento encontrado aqui.

Limite da evidência: a tag `v1.0.5` (`b50a6188a8402ff2c67415842a44da33cbe06130`) foi inspecionada, mas não foi verificado o binário efetivamente distribuído nas lojas nem dados reais de usuários. Antes da implementação, confirmar se há outra variante Adulto e quais versões participaram da distribuição. As conclusões de runtime abaixo referem-se ao checkout auditado, com diferenças históricas indicadas expressamente.

## 1. Arquitetura atual do catálogo

O aplicativo é .NET MAUI/C#, com modelos de sessão singleton e adaptadores de infraestrutura. `Course` usa `ICourseCatalogStore`; `UserProgress` usa `IUserProgressStore`. Os adaptadores delegam ao helper estático `DVPersistence`.

| Componente | Responsabilidade e referência |
| --- | --- |
| Empacotamento | `MauiAsset Include="Resources\Raw\**"`, nomes lógicos derivados do arquivo: [Digitavox.csproj](../Digitavox.csproj#L84), linha 84. |
| Inicialização | `App.OnWindowCreated` aguarda `InitializeAsync` antes de construir `AppShell`: [App.xaml.cs](../App.xaml.cs#L100), linhas 100–121. |
| Decisão de importação | `MauiAppStartupService.InitializeAsync`: [MauiAppStartupService.cs](../Infrastructure/Startup/MauiAppStartupService.cs#L24), linhas 24–34. |
| Importação e leitura local | `DVPersistence.CopyCourseFilesAsync` e `ReadCourseFiles`: [DVPersistence.cs](../Helpers/DVPersistence.cs#L125), linhas 125–203. |
| Adaptador de catálogo | `MauiCourseCatalogStore.Load`: [MauiCourseCatalogStore.cs](../Infrastructure/Persistence/MauiCourseCatalogStore.cs#L16), linhas 16–19. |
| Catálogo em memória | `Course.GetCoursesLists`, `SelectCourse`, `SelectLesson`, `GetExercises`: [Course.cs](../Models/Course.cs#L36), linhas 36–56 e 106–115. |
| Duração da sessão | `Course`, `CourseLesson` e `UserProgress` registrados como singleton: [MauiProgram.cs](../MauiProgram.cs#L43), linhas 43–55. |

### Arquivos de origem e identidade

Todos os JSONs de cursos ficam diretamente em `Resources/Raw`, não em uma subpasta `Courses` no checkout atual.

| Arquivo no Bundle | ID persistido do curso | `NUMEROCURSO` | Lições declaradas/encontradas | Exercícios encontrados |
| --- | --- | --- | --- | --- |
| `curso_abnt2_basico.json` | `curso_abnt2_basico` | 1 | 30/30 | 60 |
| `curso_abnt2_intermediario.json` | `curso_abnt2_intermediario` | 2 | 11/11 | 31 |
| `musicas.json` | `musicas` | 3 | 5/5 | 103 |
| `musica_se_eu_quiser_falar_com_deus.json` | `musica_se_eu_quiser_falar_com_deus` | 4 | 1/1 | 36 |
| `SENAI.json` | `SENAI` | 5 | 196/196 | 196 |

Os metadados estão nas linhas 10–15 dos cinco arquivos, por exemplo [Básico](../Resources/Raw/curso_abnt2_basico.json#L10), [Intermediário](../Resources/Raw/curso_abnt2_intermediario.json#L10), [músicas](../Resources/Raw/musicas.json#L10), [música individual](../Resources/Raw/musica_se_eu_quiser_falar_com_deus.json#L10) e [SENAI](../Resources/Raw/SENAI.json#L10). Contagens de exercícios foram obtidas por leitura dos objetos `LICAO*` e chaves `EXER*`, sem alteração dos arquivos.

`NUMEROCURSO` é ordem de carregamento, não ID persistido. `CourseNumber()` é a posição selecionada + 1; `CourseId()` é o nome do arquivo sem `.json` ([Course.cs](../Models/Course.cs#L46), linhas 46–51, 74–76 e 98–100). Mudar a posição visual não reassocia automaticamente o progresso; renomear arquivos reassocia ou perde a identificação desse progresso.

## 2. Fluxo Bundle → sandbox → runtime

```text
Resources/Raw/<curso>.json (MauiAsset no pacote)
    ↓ App.OnWindowCreated → MauiAppStartupService.InitializeAsync
    ↓ FingerMapping.InitializeAsync (recursos de teclado, fluxo independente)
    ↓ CourseDirectoryExists: Directory.Exists(AppDataDirectory/Courses)
    ├─ existe: Login; não consulta os JSONs de cursos no Bundle
    └─ não existe: CopyCourseFilesAsync → Tutorial
           ↓ lista fixa de cinco nomes
           ↓ OpenAppPackageFileAsync("<nome>.json")
           ↓ File.Create + CopyToAsync, apenas se arquivo de destino não existe
      AppDataDirectory/Courses/<curso>.json
           ↓ CoursesViewModel construtor → Course.GetCoursesLists
           ↓ MauiCourseCatalogStore.Load → DVPersistence.ReadCourseFiles
           ↓ Directory.GetFiles(Courses), parse de TODOS os arquivos
           ↓ ordenação por NUMEROCURSO → listas paralelas de JSON/nome/arquivo
           ↓ Course singleton → seleção de curso/lição → exercícios/texto/TTS
```

Referências: inicialização [linhas 24–34](../Infrastructure/Startup/MauiAppStartupService.cs#L24); importação [linhas 130–155](../Helpers/DVPersistence.cs#L130); leitura [linhas 157–203](../Helpers/DVPersistence.cs#L157); carregamento da tela [CoursesViewModel.cs:62](../ViewModels/CoursesViewModel.cs#L62).

| Situação | Comportamento atual |
| --- | --- |
| Primeiro lançamento sem `Courses` | Cria a pasta; copia os cinco arquivos; aguarda a cópia; abre Tutorial. |
| Atualização com `Courses` existente | Abre Login e ignora o catálogo do novo Bundle. Não compara versões, datas nem hashes. |
| Pasta existe, mas falta um curso | Startup não tenta reparar. O catálogo contém só os arquivos presentes, se a numeração permitir carregá-los. |
| Pasta vazia | É considerada instalação inicializada; não há recuperação automática. Listas vazias deixam a navegação vulnerável à seleção por índice. |
| Chamada direta ao copiador com arquivo existente | Ignora o arquivo, mesmo se o Bundle tiver conteúdo diferente. |
| Chamada direta ao copiador com arquivo ausente | Copia esse arquivo da lista fixa; não remove arquivos extras. Isso é preenchimento de arquivos ausentes, não mescla semântica de catálogo. |
| Exclusão de asset sem atualizar a lista fixa | Instalação limpa tenta abrir arquivo inexistente; inicialização falha. `App` mostra erro de startup. |
| Falha durante a primeira cópia | A pasta já foi criada. O próximo lançamento pode pular a importação e usar um catálogo parcial. |
| Alteração no sandbox depois de carregar `Course` | O singleton mantém os `JsonElement` anteriores, pois só carrega quando `jsonList.Count == 0`. |

**Não há substituição nem mescla de novos JSONs sobre arquivos existentes no fluxo normal de atualização.** Não há versionamento de catálogo, schema ou conteúdo nem migrations no código pesquisado. `type: CURSO`, `NUMEROCURSO` e `QUANTIDADELICOES` são discriminador/ordem/quantidade, respectivamente. `ApplicationDisplayVersion` e `ApplicationVersion` são versões do aplicativo ([Digitavox.csproj:21](../Digitavox.csproj#L21)); não comandam atualização dos dados.

### Fragilidade da leitura e da ordenação

`ReadCourseFiles` armazena cursos em `number2Course[number]` e percorre **1 até `number2Course.Count`**, buscando cada chave. Com os cursos 1 e 2 excluídos, sobram chaves 3, 4 e 5; a primeira consulta a `number2Course[1]` lança `KeyNotFoundException` ([DVPersistence.cs:182](../Helpers/DVPersistence.cs#L182), linhas 182–195). Números duplicados sobrescrevem um curso no dicionário; cursos sem número são anexados na ordem retornada pelo filesystem, sem ordenação explícita.

O leitor não filtra extensão e não isola erros por arquivo (linhas 166–171). Um manifesto, backup, arquivo temporário ou JSON inválido colocado dentro de `Courses` também pode quebrar o carregamento. Qualquer novo estado de migração deve ficar fora dessa pasta ou exigir adaptação prévia do leitor.

### Evidência histórica da tag `v1.0.5`

Verificado com `git show v1.0.5:<arquivo>` e `git diff v1.0.5 HEAD -- <arquivos>`:

- `AppShell.xaml.cs`, linhas 49–56 na tag: mesma condição de existência de `Courses`, chamando `DVPersistence.CopyCourseFiles()` somente quando ausente.
- `Helpers/DVPersistence.cs`, linhas 130–157 na tag: cópia `async void`, sem aguardar término. Compara caminhos completos retornados por `Directory.GetFiles` com nomes sem extensão; esse teste não detecta corretamente arquivos existentes e, se chamado diretamente outra vez, pode sobrescrevê-los. O startup ainda evita a chamada quando a pasta existe.
- O leitor da tag também exige numeração consecutiva (linhas 192–197 na tag). O formato de progresso e a lógica de avanço foram preservados pela refatoração; mudaram principalmente as dependências injetadas.
- Os três JSONs alvo não têm diferenças entre a tag e HEAD. A compatibilidade das chaves é sustentada pelo histórico local; a equivalência com todas as versões de loja ainda precisa ser confirmada.

Assim, não se deve chamar o copiador legado indiscriminadamente como mecanismo de atualização. A migration precisa reconhecer também instalações parcialmente importadas.

## 3. Modelo atual de persistência do progresso

### Representações e vínculos

| Conceito solicitado | Representação encontrada | Persistência/identidade |
| --- | --- | --- |
| Curso | JSON com `CURSO`, metadados e `LICAO*`; `Course` mantém curso selecionado | Arquivo em `Courses`; progresso pelo nome-base do arquivo. |
| Módulo | Não existe entidade separada | Básico/Intermediário correspondem a cursos completos. |
| Lição | Propriedade `LICAO<n>` do objeto raiz | Chave numérica embutida no nome; depende da numeração pedagógica. |
| Exercício | Texto em `EXER<n>` dentro da lição | Enumerado na ordem das propriedades JSON; nenhum registro individual de progresso por `EXER<n>`. |
| Música | Uma lição de `musicas`, ou um curso individual | Juca Chaves é `musicas/LICAO5`, não possui ID musical próprio. |
| Tentativa/histórico | `LICAO<n>.<tentativa>` dentro do curso no JSON de usuário | A tentativa é `Count(chaves com prefixo) + 1`; exclusões que criem lacunas podem causar colisões. |
| Conclusão de tentativa | `CONCLUIU: "S"` ou `"N"` | Estatística salva ao encerrar/expirar/sair do exercício. |
| Desbloqueio | `ULTIMACONCLUIDA`, inteiro por curso | Fronteira de avanço; não existe coleção explícita de IDs desbloqueados. |
| Conteúdo atual | `courseId`, `lessonId`, `courseKey`, `lessonKey`, índices de frase/caractere e rotas | Só memória da sessão; não há cursor persistido de retomada exata. |
| Pontuação/estatísticas | Acertos, erros, velocidade, tempos, palavras, repetições e erros por caractere | Valores agregados da tentativa de lição; nenhum placar geral encontrado. |

Fontes: [Course.cs:53](../Models/Course.cs#L53), linhas 53–56 e 106–115; [UserProgress.cs:22](../Models/UserProgress.cs#L22), linhas 22–26 e 56–99; [CourseLesson.cs:22](../Models/CourseLesson.cs#L22), linhas 22–35, 82–97 e 227–237.

**Identidade é parcialmente estável.** Curso permanece identificado pelo nome do arquivo enquanto este não mudar. `LICAO<n>` permanece identificada apenas enquanto o número não for reutilizado para outro conteúdo. Não há UUID nem vínculo persistido com um objeto serializado do catálogo. O registro guarda estatísticas, não apresentação/instrução/letra da lição. Ordenar os cursos de outra forma não muda suas chaves persistidas; renumerar lições muda o significado de seus registros.

### Armazenamento e reconstrução

O caminho é `FileSystem.AppDataDirectory/Users/<NOME_EM_MAIÚSCULAS>.json`, um arquivo por nome de usuário, incluindo o perfil `ANONIMO`. Há troca de usuário no app; uma migration não pode considerar apenas o perfil logado. `FilePath`, `LoadUserData` e `SaveUserJson`: [DVPersistence.cs:56](../Helpers/DVPersistence.cs#L56), linhas 56–122. Perfil anônimo: [LoginViewModel.cs:193](../ViewModels/LoginViewModel.cs#L193), linhas 193–225.

Exemplo **equivalente e reduzido**, baseado no formato escrito pelo aplicativo (não representa dados de uma pessoa real):

```json
{
  "NOMEUSUARIO": "AUDITORIA",
  "type": "USUARIO",
  "curso_abnt2_basico": { "ULTIMACONCLUIDA": 30 },
  "curso_abnt2_intermediario": { "ULTIMACONCLUIDA": 6 },
  "musicas": {
    "ULTIMACONCLUIDA": 5,
    "LICAO5.1": {
      "DATAINICIO": "2026-08-24 17:00:00",
      "DATAFIM": "2026-08-24 17:05:00",
      "CONCLUIU": "S",
      "PERCENTUALACERTO": 99,
      "TOTALLETRASLICAO": 100,
      "TEMPOPRATICALICAO": 300
    }
  }
}
```

As estatísticas completas incluem `DATAINICIO`, `DATAFIM`, `CONCLUIU`, `PERCENTUALACERTO`, `TOTALLETRASLICAO`, `NLETRASACERTOU`, `NLETRASERROU`, `PRATICALETRASLICAO`, `LETRASPORMINUTO`, `PERCENTUALPALAVRASCORRETAS`, `TOTALPALAVRASLICAO`, `NPALAVRASACERTOU`, `NPALAVRASERROU`, `DIVISORTEMPO`, `REPETICOESEXER`, `TEMPOTOTALLICAO`, `TEMPOPRATICALICAO`, `PERCENTUALTEMPO` e `ERROSEM_<caractere>` ([UserProgress.cs:119](../Models/UserProgress.cs#L119), linhas 119–150).

Após atualização e início de novo processo, o app vai ao Login; `UserRegistration` lê o arquivo pelo nome informado. A seleção posterior de curso registra o nome-base, e a tela de lições calcula a disponibilidade a partir desse curso. Não existe restauração automática de curso/música/exercício selecionado: [LoginViewModel.cs:152](../ViewModels/LoginViewModel.cs#L152), linhas 152–180 e 237–239; [CoursesViewModel.cs:160](../ViewModels/CoursesViewModel.cs#L160), linhas 160–165; [LessonsViewModel.cs:78](../ViewModels/LessonsViewModel.cs#L78), linhas 78–94.

Preferências MAUI guardam apenas configuração (`speakRate`, `speakInput`, `fontSize`, `timeDivider`, `countRepetitions`, `instructionsEnabled`, `VoiceOverStatus`), não o progresso ([DVPersistence.cs:28](../Helpers/DVPersistence.cs#L28), linhas 28–54). Não foi encontrado banco SQL, ORM, local storage web nem sincronização remota de progresso.

### Regras de avanço e limitações relevantes

1. `AddLesson` salva tentativa e aumenta `ULTIMACONCLUIDA` quando a tentativa conclui. Quando a chave ainda não existe, inicializa em **1**, inclusive se a lição concluída tiver outro número ([UserProgress.cs:64](../Models/UserProgress.cs#L64), linhas 64–92). Por isso, reconstrução apenas pelo maior `CONCLUIU = S` não é necessariamente equivalente à fronteira legada.
2. `LastAvailableLesson` retorna `last + 1` se `last < TotalLessons`; caso contrário retorna **`last` sem limitá-lo ao total** ([UserProgress.cs:102](../Models/UserProgress.cs#L102), linhas 102–117). O desbloqueio é sequencial dentro de cada curso. Não consulta outros cursos.
3. `LessonConcluded` usa `número da lição < LastAvailableLesson`, com exceção para curso de uma lição. A última lição de um curso com várias lições pode ser anunciada como não concluída mesmo com tentativa concluída ([UserProgress.cs:156](../Models/UserProgress.cs#L156), linhas 156–165). Preservar esse defeito após diminuir o total tornaria a lição 4 enganosa.
4. `PERCENTUALACERTO` é acerto da tentativa em relação aos caracteres da lição, não percentual de conclusão do curso ([CourseLesson.cs:201](../Models/CourseLesson.cs#L201), linhas 201–205). Não se deve reinterpretar um valor 60 como “Intermediário 60% concluído”. Para fixture de avanço parcial, usar fronteira 6/11 e tentativas apropriadas, sem chamá-la de 60% exatos.
5. A gravação atual substitui todo o arquivo com `File.WriteAllBytes`, sem transação entre catálogo e usuários, journal ou troca atômica explícita ([DVPersistence.cs:116](../Helpers/DVPersistence.cs#L116), linhas 116–122).
6. O leitor aceita strings na raiz; demais valores são tratados como objetos de curso. **Adicionar `catalogVersion: 1` numérico na raiz de um arquivo de usuário quebra esse parser**, que tenta `EnumerateObject` no número. Novos níveis aninhados, arrays e booleanos também não têm suporte genérico (linhas 74–109). Um arquivo de versão/arquivo histórico separado, fora de `Users` e `Courses`, evita mudar esse contrato na primeira migration.

## 4. Conteúdos e IDs afetados

| Alvo | Identidade exata | Registros de usuário afetados | Operação pretendida |
| --- | --- | --- | --- |
| Básico | `curso_abnt2_basico`; `LICAO1`…`LICAO30` | Objeto inteiro desse curso, incluindo `ULTIMACONCLUIDA` e `LICAO<n>.<tentativa>` | Retirar curso do catálogo ativo e payload local; preservar histórico numérico. |
| Intermediário | `curso_abnt2_intermediario`; `LICAO1`…`LICAO11` | Objeto inteiro desse curso | Mesma política. |
| A Cúmplice, Juca Chaves | `musicas/LICAO5`; `EXER1`…`EXER32` | `musicas/LICAO5.<tentativa>` e fronteira do curso de músicas | Retirar só a quinta lição, manter `LICAO1`…`LICAO4` com as mesmas identidades e tentativas. |

Referências: lições do Básico começam em [linha 25](../Resources/Raw/curso_abnt2_basico.json#L25) e a última em [linha 567](../Resources/Raw/curso_abnt2_basico.json#L567); Intermediário começa em [linha 25](../Resources/Raw/curso_abnt2_intermediario.json#L25) e termina com `LICAO11` na [linha 235](../Resources/Raw/curso_abnt2_intermediario.json#L235). A música alvo ocupa [musicas.json:156](../Resources/Raw/musicas.json#L156), linhas 156–202: apresentação, instrução, 32 frases e parâmetros de execução.

Escopo total: 42 lições e 123 entradas `EXER*` retiradas. Permanecem três cursos, com músicas reduzido de 5 para 4 lições; não há solicitação de remover SENAI nem a música individual de Gilberto Gil. `NUMEROCURSO` dos sobreviventes é 3/4/5; corrigir a leitura para aceitar essa ordem esparsa é preferível a tratar número visual como identidade.

## 5. Referências e dependências estruturais encontradas

| Área pesquisada | Evidência | Consequência para a remoção |
| --- | --- | --- |
| IDs hardcoded | Lista fixa no copiador: [DVPersistence.cs:133](../Helpers/DVPersistence.cs#L133), linhas 133–139 | Excluir os dois nomes e substituir o mecanismo por catálogo ativo/manifesto. Remover assets sozinho falha em instalação limpa. |
| Empacotamento | Glob real na linha 84; entradas `MauiAsset Update` com caminhos antigos `Resources\Raw\Courses\...`: [Digitavox.csproj:130](../Digitavox.csproj#L130), linhas 130–143 | Revisar caminhos e pacote final. As entradas antigas não são a localização dos JSONs atuais. |
| Ordenação | Dicionário e loop consecutivo: [DVPersistence.cs:172](../Helpers/DVPersistence.cs#L172), linhas 172–195 | A remoção integral de cursos quebra os sobreviventes. Duplicatas também precisam de validação. |
| Desbloqueio/pré-requisitos | [UserProgress.cs:102](../Models/UserProgress.cs#L102), linhas 102–117 | Nenhuma dependência Básico → Intermediário → outro curso. Fronteira de músicas precisa ser reconciliada. |
| Enum/rotas | [INavigationService.cs:11](../Application/Abstractions/INavigationService.cs#L11), linhas 11–29; [AppShell.xaml.cs:31](../AppShell.xaml.cs#L31), linhas 31–47 | Rotas são telas, não IDs de conteúdo. Nenhuma rota específica para Juca/Básico/Intermediário. |
| Seleção e listagens | Curso por índice e lição por número: [CoursesViewModel.cs:128](../ViewModels/CoursesViewModel.cs#L128), linhas 128–131 e 160–165; [LessonsViewModel.cs:162](../ViewModels/LessonsViewModel.cs#L162), linhas 162–166 e 201–207 | Índices visuais mudam; não persistir esses índices como identidade. A lista de lições supõe sequência inteira. |
| Próxima lição | [ExercisesStatisticsViewModel.cs:161](../ViewModels/ExercisesStatisticsViewModel.cs#L161), linhas 161–199 | Com fronteira 5 e total 4, Enter tenta `SelectLesson(5)` diretamente, sem clamp. |
| Execução | [ExercisesViewModel.cs:151](../ViewModels/ExercisesViewModel.cs#L151), linhas 151–179 | Assume pelo menos um exercício (`exercisesList[0]`). Retirar só as frases e manter a lição vazia também quebra. |
| Acessibilidade/ajuda | `LessonHelpOptions` expõe todos os exercícios por fala: [DVViewModelFunctions.cs:147](../ViewModels/DVViewModelFunctions.cs#L147), linhas 147–170; apresentação/instrução via `Course` | Ocultar a opção no menu não impede exposição pela ajuda e TTS de objetos carregados. Bloquear leitura/execução no domínio. |
| Textos embutidos | [musicas.json:5](../Resources/Raw/musicas.json#L5) diz que há cinco lições; apresentação retirada na linha 158 | Atualizar apresentação para quatro lições; não manter introdução/letra retirada em tombstone. Não foram encontrados recursos `.resx` ou títulos alvo hardcoded nas Views. |
| Histórico | [UserProgress.cs:183](../Models/UserProgress.cs#L183), linhas 183–195; [SecondHelpViewModel.cs:128](../ViewModels/SecondHelpViewModel.cs#L128), linhas 128–131 | Hoje histórico só é acessado pela lição selecionada. Remover a lição torna os registros remanescentes inacessíveis pela UI. |
| Caches | Cache de JSON no singleton [Course.cs:36](../Models/Course.cs#L36); listas de exercício [ExercisesViewModel.cs:156](../ViewModels/ExercisesViewModel.cs#L156) | Migrar antes do primeiro carregamento; não confiar em exclusão do arquivo para revogar objetos em memória. Nenhum cache persistente adicional de conteúdo encontrado. |
| Áudio | [MauiFeedbackSoundService.cs:16](../Infrastructure/Audio/MauiFeedbackSoundService.cs#L16), linhas 16–29; [MauiSpeechService.cs:18](../Infrastructure/Speech/MauiSpeechService.cs#L18) | Letra é texto ditado por TTS. Não há gravação específica da música entre os assets; `buzz.wav` é feedback compartilhado. |
| Campos de áudio legados | `SOMNOMECURSO`, `SOMAPRESENTACAO`, `SOMINSTRUCAO`, `SOMEXER*` nos JSONs | Não foi encontrado consumo desses campos pelos fontes C# atuais. Não excluir sons compartilhados a partir desses nomes. |
| Testes/fixtures | Cinco nomes no teste de cópia: [DVPersistenceCharacterizationTests.cs:138](../Tests/Digitavox.PersistenceCharacterizationTests/DVPersistenceCharacterizationTests.cs#L138), linhas 138–160 | Atualizar expectativas e adicionar fixtures de upgrade. Testes atuais não cobrem remoção. |
| Banco/local storage | `DVPersistence` + adaptadores `Maui*Store` | Persistência é arquivo JSON e configurações são Preferences; não há segunda base encontrada para migrar. |
| Playlists, favoritos, último conteúdo, analytics, deep links | Pesquisa de fontes, manifests e rotas não encontrou implementação dessas funções | Não há referências adicionais demonstradas nesta revisão. Isso não comprova ausência em variantes não fornecidas. |
| Backup do sistema | [AndroidManifest.xml:3](../Platforms/Android/AndroidManifest.xml#L3) define `allowBackup="true"` | Tratar dados legados restaurados como entrada de migration. Não assumir que reinstalação sempre corresponde a sandbox vazio. |

Não foram encontradas referências pelo nome de Juca Chaves fora de `musicas.json`. Os arquivos de reconhecimento institucional mencionam SENAI, mas não criam dependência executável dos cursos retirados. Não há integração de analytics de conteúdo identificada. Essas conclusões foram obtidas por inventário e buscas em `Models`, `ViewModels`, `Views`, `Helpers`, `Infrastructure`, `Application`, `Platforms`, `Resources`, testes e documentação, excluindo artefatos de build.

## 6. Matriz de impacto por cenário de usuário

Na coluna “remoção direta”, considera-se que os arquivos locais já foram alterados, sem as correções de leitor e progresso. Se **apenas o Bundle** for alterado, todos os usuários com `Courses` existente continuam com o conteúdo anterior.

| Cenário | Somente Bundle | Remoção direta no catálogo local atual | Resultado recomendado |
| --- | --- | --- | --- |
| Instalação limpa | Excluir assets sem mexer no copiador causa erro; editar músicas altera só novas cópias | Cursos 3/4/5 não carregam no leitor atual; música com total desatualizado permite selecionar lição ausente | Importar somente os três cursos ativos; músicas com quatro lições; IDs retirados ausentes do catálogo executável. |
| Upgrade sem progresso nos alvos | Alvos continuam disponíveis | Falha de ordenação independe de progresso; arquivos antigos extras permanecem se não houver limpeza | Mesmo catálogo ativo da instalação limpa; registros sobreviventes intactos. |
| Básico concluído | Curso/progresso continuam como antes | Falha geral se excluir cursos; progresso não é apagado automaticamente, mas perde caminho de consulta | Arquivar estatísticas do Básico por ID, sem payload; não alterar acesso aos outros cursos. |
| Básico concluído + Intermediário parcial | Conteúdo permanece | Progresso fica sem curso ativo; não existe cursor persistido nem cálculo global de 60% | Preservar todas as tentativas e fronteiras dos cursos retirados no histórico; manter cursos sobreviventes acessíveis. |
| Cursos posteriores/lições posteriores já acessíveis | Nenhuma mudança | Leitor quebrado impede acesso a todos; se corrigido, não há dependência dos cursos retirados | Preservar progresso de SENAI e música individual integralmente; não adicionar pré-requisitos. |
| Conteúdos retirados concluídos | Continuam disponíveis | Arquivos de usuário conservam dados, mas menus deixam de oferecer histórico; não há percentual geral a recalcular | Preservar métricas de tentativas e evidência histórica. Não fabricar um percentual geral novo. |
| Usuário estava no Básico/Intermediário ao atualizar | Novo processo vai ao Login; alvos ainda aparecem | Não existe referência persistida `currentModule` a resolver. Se migração ocorrer em sessão viva, objetos antigos continuam em memória | Migrar antes do runtime; começar em Login/menu. Invalidar seleção/TTS/timer se houver troca de catálogo em processo vivo. |
| Usuário em `musicas/LICAO5`, tentativa parcial | Letra permanece | Com `ULTIMACONCLUIDA = 4`, total 4 retorna lição 4; tentativa 5 permanece órfã de conteúdo | Arquivar tentativa 5 inclusive `CONCLUIU = N`; nenhuma retomada na letra retirada; oferecer menu/lições ativas. |
| Usuário concluiu Juca, `ULTIMACONCLUIDA = 5` | Letra permanece | Mesmo com total 4, retorna lição disponível 5; lista pode exibir 5 e Enter em estatísticas lança `KeyNotFoundException` | Preservar tentativa histórica 5; normalizar fronteira ativa para 4; jamais selecionar 5. |
| Usuário concluiu lição 4 e tinha 5 desbloqueada | Continua podendo iniciar 5 | Fronteira 4 é válida com total 4, mas `LessonConcluded(4)` anuncia “Não” | Manter lição 4 concluída; reconhecer conclusão das quatro lições ativas segundo política legada documentada. |
| Atualização executada duas vezes | Startup ignora Bundle em ambas | Operações sem marcador/transação podem duplicar arquivos de histórico ou perder dados | Segunda execução produz o mesmo estado sem duplicar tentativas/histórico. |

### Caso hipotético de Avançado bloqueado

O inventário dos cinco JSONs e o código não contêm Avançado nem regra que exija conclusão de Básico/Intermediário para selecionar outro curso. `CoursesViewModel` lista todos os cursos carregados e `UserProgress` consulta só o curso selecionado. Assim, **não há pré-requisito para Avançado a preservar neste checkout**. SENAI é um curso independente, não deve ser tratado como Avançado por inferência.

Se outra versão Adulto tiver essa trilha, acrescentar seus IDs/regras e fixtures antes de implementar. A política proposta para tal extensão é preservar direitos de acesso já adquiridos e eliminar pré-requisitos retirados, sem atribuir conclusão fictícia aos conteúdos sobreviventes.

### Música: avaliação específica de referências órfãs

- **Progresso:** `musicas/LICAO5.*` continua no arquivo de usuário; `ULTIMACONCLUIDA = 5` passa a exceder o total. Precisa de arquivamento e reconciliação.
- **Listagens:** lições são geradas de 1 até `LastAvailableLesson`, não por enumeração de lições existentes; lição 5 pode aparecer mesmo ausente ([LessonsViewModel.cs:162](../ViewModels/LessonsViewModel.cs#L162)). Não foi encontrada playlist separada.
- **Favoritos/último acesso:** não existem campos persistidos correspondentes encontrados. Tentativas guardam datas, mas estas não são usadas para reconstruir um último conteúdo selecionado.
- **Áudio:** sem asset musical específico. O risco é TTS recitar a letra ainda presente no JSON local/cache/ajuda; apagar WAVs de feedback não resolve.
- **Índices/ordem:** é a última lição. Remover `LICAO5` e conservar `LICAO1`…`LICAO4` evita renumerar músicas sobreviventes. Futuras remoções intermediárias exigirão lista explícita de IDs ativos, sem reutilizar números.
- **Navegação:** `SelectLesson` usa `GetProperty` sem fallback; próxima lição em estatísticas consulta a fronteira antiga. Um tombstone vazio continua inseguro se essas funções não forem adaptadas.

### Simulações executadas nesta auditoria

Foram executadas **16 verificações** em harness .NET 10 temporário, compilando os fontes atuais de `DVPersistence`, `Course`, `CourseLesson`, `UserProgress`, startup e adaptadores. Storage/Preferences/TTS foram substituídos pelos doubles do projeto de testes de persistência; `FingerMapping.InitializeAsync` foi substituído por no-op para isolar o catálogo. O pacote fictício começou com cópias dos cinco JSONs reais; as exclusões ocorreram exclusivamente nessas cópias temporárias.

Resultados observados: startup limpo copiou cinco cursos e escolheu Tutorial; atualização escolheu Login e manteve a música retirada do Bundle; chamadas repetidas ao copiador mantiveram os mesmos bytes antigos; nova definição de músicas com quatro lições preservou fronteira inválida 5; `SelectLesson(5)` lançou `KeyNotFoundException`; fronteira 4 retornou 4, mas conclusão da lição final foi anunciada como “Não”; tentativa 5 permaneceu no usuário; excluir cursos 1/2 lançou `KeyNotFoundException`; instância de `Course` já carregada conservou os cinco cursos; excluir o asset Básico antes da primeira importação lançou `FileNotFoundException`, deixou a pasta e fez o lançamento seguinte pular a importação.

São reproduções do comportamento legado, **não testes de uma migration implementada**. As demais células da matriz são avaliações estáticas fundamentadas nas funções referenciadas. As suítes completas existentes e a UI em dispositivo não foram executadas nesta etapa; o ambiente não possui workloads MAUI instalados. O harness não depende de MAUI nem de pacotes externos. A primeira tentativa usou o SDK padrão do diretório temporário e falhou por reference packs; fixar o SDK 10.0.301, como no `global.json` do repositório, permitiu executar as verificações.

## 7. Riscos

| Prioridade | Risco | Evidência/mitigação necessária |
| --- | --- | --- |
| Crítica | Conteúdo continua acessível para a base instalada | Importação condicionada à pasta; migration e revogação central, não apenas edição do Bundle. |
| Crítica | Excluir cursos torna o catálogo restante ilegível | Loop por chaves 1…Count; ordenação explícita que aceite lacunas. |
| Alta | Falha em instalação limpa por asset ausente | Lista fixa do copiador; importar apenas ativos e verificar pacote final. |
| Alta | Referência ativa a lição 5 inexistente | `LastAvailableLesson` sem clamp e seleção direta na tela de estatísticas; reconciliação do progresso e validação de seleção. |
| Alta | Perda de histórico/progresso ao resetar usuários ou recopiar catálogos | Arquivos separados por usuário e identidades por nome/número; migration direcionada, backup de progresso e escrita recuperável. |
| Alta | Exposição por ajuda, texto/TTS ou cache apesar de ocultação visual | Acesso bruto por `Course` e listas em memória; bloqueio em seleção e leitura, migration anterior à sessão. |
| Alta | Estado parcialmente migrado após interrupção | Escritas sem transação e condição por pasta; journal, commit de geração validada e repetição idempotente. |
| Alta | Nova metadata incompatível com parser de usuário | Raiz numérica/booleanos/arrays não aceitos; sidecars separados ou parser explicitamente versionado. |
| Média | “Lição final não concluída” após diminuir músicas | Regra de conclusão por comparação estrita; separar disponibilidade de evidência de conclusão. |
| Média | Histórico arquivado existe, mas usuário não consegue consultá-lo | UI atual depende do catálogo executável; acesso histórico separado a estatísticas sem letra/apresentação retirada. |
| Média | Reutilização de `LICAO5` atribui história antiga a música nova | Não reciclar ID; manter registro mínimo de retirada e ordem separada de identidade. |
| Média | Novos arquivos/backup dentro de `Courses` quebram parse | Leitor de todos os arquivos; manter staging/manifests fora e restringir leitura a manifestos validados. |
| Média | Usuários não logados ou dados restaurados escapam da migration | Vários arquivos em `Users`, backup Android permitido; cobertura por perfil e validação ao login. |

## 8. Alternativas avaliadas

| Alternativa | Instalações existentes | Instalações novas | Perda de progresso | Referências órfãs | Complexidade | Manutenção futura |
| --- | --- | --- | --- | --- | --- | --- |
| A. Remover diretamente | Bundle é ignorado; remover sandbox quebra ordenação e fronteira de músicas | Precisa mudar copiador, contagem e leitor | Dados permanecem em disco, mas histórico perde acesso; resets para contornar falhas perdem dados | Alta sem reconciliação | Baixa só na edição; alta para torná-la segura | Remoções seguintes repetem os mesmos problemas. |
| B. IDs existentes + inativo/indisponível | Adicionar flag só ao Bundle não alcança sandbox; todos os consumidores precisam respeitar estado | Pode impedir execução se estado for consultado centralmente | Baixa mantendo tentativas | Baixa apenas com seleção/avanço adaptados | Média | Contrato útil, mas payload retirado não deve continuar disponível via APIs/ajuda. |
| C. `deprecated`/`hidden` | Código atual ignora campos novos. `hidden` só no menu deixa ajuda/objetos antigos acessíveis; `deprecated` sozinho não significa proibido | Requer semântica explícita para omitir e bloquear | Baixa se histórico separado | Média/alta se esconder sem invalidar curso/lição atual | Média | Útil separar visibilidade e ciclo de vida; exige estados consistentes em todos os acessos. |
| D. Migration explícita de catálogo/progresso | Atualiza payload persistido, remove acesso, reconcilia fronteira, conserva histórico | Usa mesma definição ativa, sem importar alvos | Baixa com alteração dirigida e journal | Baixa com validação de integridade | Média/alta inicialmente | Custo inicial se paga em upgrades determinísticos e migrations seguintes. |
| E. Reconstruir progresso só pelo conteúdo restante | Pode perder tentativas e direitos de acesso se descartar fronteira legada | Estado vazio simples | Alta se substituição integral; reduzível com arquivo histórico | Baixa para catálogo ativo, mas história pode desaparecer | Média/alta; inferir conclusão a partir de registros incompletos é ambíguo | Exige política rigorosa para cada revisão de conteúdo. |

As alternativas não são exclusivas. **D combinada com metadata mínima de retirada de B/C** oferece o melhor resultado. O registro de retirada mantém identidade histórica e bloqueia reutilização; o payload executável é removido. E deve ser usada apenas como reconciliação localizada das quatro lições de músicas, preservando a semântica de acesso legado, nunca como limpeza indiscriminada dos usuários.

## 9. Estratégia recomendada

1. Tornar a definição ativa do novo catálogo autoritativa para instalação limpa e upgrade. Corrigir ordenação, validar IDs e usar manifesto/lista explícita em vez de considerar qualquer arquivo local um curso ativo.
2. Retirar os dois cursos completos e `musicas/LICAO5` do Bundle e do catálogo executável no sandbox. Conservar IDs dos três cursos e das quatro músicas sobreviventes. Corrigir `QUANTIDADELICOES` e a apresentação que menciona cinco lições.
3. Introduzir `catalogVersion` e uma migration explícita `legacy/0 → 1`, porque os dois mecanismos atuais de “já existe” impedem atualização e há progresso dependente da quantidade de lições. Versão do app não substitui a versão do catálogo.
4. Manter tombstones **somente de metadata**: ID de curso/lição, estado `removed`/indisponível, versão da retirada e, se necessário, identificação textual mínima para histórico. Sem letras, apresentações instrucionais ou exercícios retirados. Tombstone não é uma lição vazia elegível para execução.
5. Preservar todas as tentativas dos cursos retirados e da música em arquivo histórico separado, com seus IDs de origem. Retirá-las das relações operacionais que exigem curso/lição ativa. Não modificar estatísticas das tentativas sobreviventes nem renumerá-las.
6. Reconciliar a fronteira de músicas para o domínio válido 0…4. Preservar acesso já obtido às lições restantes. Separar “lição disponível” de “lição concluída”, usando tentativas bem-sucedidas e interpretação explícita da fronteira legada. Não recalcular acerto/tempo histórico usando o novo conteúdo.
7. Migrar antes de carregar o singleton e permitir abrir exercícios. Bloquear em domínio/serviço seleção, leitura de exercícios, apresentação e instrução de conteúdo retirado; a UI também omite as opções. Acesso histórico deve ler somente estatísticas, sem exigir `SelectLesson` do catálogo ativo.
8. Aplicar escrita recuperável e validação por perfil, incluindo usuários não logados e anônimo. Dados restaurados/versionamento ausente passam pela mesma verificação. Falha não pode resultar em reset de usuário nem fallback que reabra conteúdo retirado.

Não se recomenda introduzir banco de dados nem reescrever todo o modelo de progresso para esta retirada. Identidade explícita e ordem separada devem ser consolidadas no contrato de catálogo; é possível começar com IDs canônicos existentes (`curso` + `LICAO<n>`), sem inventar IDs que exijam remapear todo o histórico.

## 10. Plano de migração proposto

Todos os caminhos, tipos e estados abaixo são **proposta**, não existem no código atual.

### Versões e localização

- Bundle: manifesto do catálogo ativo com `catalogVersion: 1`, arquivos ativos, ordem, IDs e metadata de retirada. `schemaVersion` só deve ser introduzido separadamente se o formato passar a evoluir; não confundir ordem com versão.
- Sandbox: `AppDataDirectory/Catalog/state.json` com versão efetivamente publicada e referência à geração ativa. Staging/journal em `Catalog`, fora de `Courses`.
- Histórico: `AppDataDirectory/History/Users/<usuario>.json`, em schema próprio, contendo estatísticas arquivadas e IDs. Não colocar esse objeto aninhado no arquivo legado enquanto o leitor atual estiver em uso.
- Estado de migration por usuário: sidecar fora de `Users`, associando arquivo/perfil à versão reconciliada. Versão global sozinha não comprova que todos os perfis foram migrados.
- Sem marcador: considerar entrada legada `0`; não assumir integridade só porque `Courses` existe. Identificar os nomes conhecidos e validar forma do JSON; variantes não reconhecidas precisam de tratamento explícito sem apagamento automático.

### Operações `0 → 1`

| Operação | Resultado/invariante |
| --- | --- |
| `removeCourse(curso_abnt2_basico)` | Sem arquivo/payload ativo, sem seleção; histórico por ID preservado. |
| `removeCourse(curso_abnt2_intermediario)` | Mesmo resultado. |
| `removeLesson(musicas, LICAO5)` | Nenhum exercício/apresentação da música no catálogo executável; histórico sem payload. |
| Publicar músicas remanescente | Mesmas chaves `LICAO1`…`LICAO4`, quatro lições, texto de apresentação coerente. |
| Reconciliar progresso ativo | Arquivar `LICAO5.*`; fronteira de músicas válida e direitos de acesso sobreviventes conservados. |
| Validar seleção | IDs ausentes/retirados não alcançam `GetProperty`, índices de exercícios ou fala. |
| Registrar commit e versão por perfil | Só após validar/gravar catálogo, histórico e progresso correspondentes. |

### Sequência segura de execução

1. **Antes do runtime:** adquirir exclusão mútua de inicialização. Aplicar política de indisponibilidade desta release antes de qualquer eventual recuperação; nenhuma falha deve voltar a oferecer os alvos.
2. **Descobrir estado:** ler marcador/journal, validar catálogo local, inventariar todos os arquivos de perfil e verificar se há operação interrompida. Não usar existência de pasta como sucesso. Registrar diagnóstico sem letras/dados pessoais desnecessários.
3. **Preparar catálogo:** construir geração ativa a partir do Bundle novo, em staging externo a `Courses`. Validar IDs únicos, ordem, quantidade e lições/exercícios obrigatórios; manter cursos locais desconhecidos separados da decisão, sem apagá-los automaticamente nem carregá-los por acaso. A política de preservar conteúdo adicional precisa de regra explícita se tais dados forem encontrados.
4. **Planejar por perfil:** preservar progresso não afetado; preparar arquivo histórico e novo estado ativo dos alvos. Ler o JSON de origem sem descartar campos desconhecidos: o parser legado não é uma ferramenta genérica de migration. Uma chave histórica deve ser única por `(usuario, cursoId, lessonId, tentativa)`, sem append cego. Backups de progresso podem ser usados para recuperação; não conservar payload retirado em backups de catálogo reutilizáveis pelo runtime.
5. **Reconciliar músicas:** preservar tentativas 1–4, arquivar todas as tentativas 5, inclusive incompletas, e guardar a fronteira legada no histórico. Para a migração específica, usar fronteira operacional `min(max(legada, 0), 4)` para inteiros válidos. Corrigir conclusão da lição final separadamente. Não descartar a fronteira por faltar tentativa completa: ela é evidência de acesso concedido pelo código legado. Valores/tipos inválidos devem gerar recuperação/diagnóstico sem fabricação de desempenho.
6. **Gravar de forma recuperável:** persistir staging de histórico e perfil; usar journal com origem/destino e checkpoints para retomar cada arquivo. A troca atômica de um arquivo não garante transação entre todos eles: implementar protocolo de recuperação e bloquear uso de perfil parcialmente reconciliado. Confirmar arquivo histórico antes de remover os registros operacionais correspondentes.
7. **Publicar geração:** depois de validar as operações necessárias, selecionar geração ativa e versão; limpar payloads conhecidos retirados da pasta antiga. Não deixar o leitor atual varrer gerações, backups e staging. Caso a limpeza seja interrompida, retomar antes de expor o catálogo, mantendo a política de acesso aplicada.
8. **Carregar sessão:** criar/recarregar modelos após commit. Login lê perfil coerente com catálogo ativo. Se houver perfis com erro de leitura, conservar seus bytes e impedir seu uso inconsistente; não marcar migration daquele perfil como concluída. A recuperação deve permitir diagnóstico/repetição sem zerar os dados.
9. **Repetir/validar:** marcador já na versão alvo dispensa transformação, mas uma verificação de integridade detecta dados legados restaurados ou payload retirado reaparecido. Reaplicar operações pelo ID quando necessário sem duplicar histórico.

Instalação nova publica diretamente a versão 1, sem criar estatísticas históricas fictícias. Upgrade publica a mesma definição ativa, acrescentando apenas progresso/histórico legítimo. O Tutorial deve depender do estado de primeira utilização, não da recriação de `Courses` durante a migration.

### Posição atual e conclusão histórica

Não há cursor persistido a migrar na versão auditada. Em upgrade com reinício, o destino já é Login/menu. Se futuramente existir cursor explícito, resolver por ID: curso retirado → menu de cursos; lição retirada em curso ativo → menu/primeira lição ativa elegível; exercício retirado → reinício de lição elegível ou menu, nunca reutilização do mesmo índice em outro conteúdo. Em sessão viva, cancelar TTS/timer e limpar seleção/listas antes de trocar a geração.

Não há campo persistido de conclusão do curso inteiro. Arquivar `ULTIMACONCLUIDA`, tentativas `CONCLUIU = S/N` e versão de origem, sem inventar certificado ou indicador histórico novo como se já existisse. Se a UI passar a mostrar conclusão histórica, explicitar a regra: conclusão comprovada por tentativas versus inferida da fronteira legada. Músicas com fronteira 4/5 deve manter as quatro lições acessíveis e não declarar a última incompleta somente por ela ter se tornado final.

## 11. Testes necessários

Usar fixtures equivalentes ao schema legado e às identidades reais, com perfis criados pelo serializer de produção. Não é necessário conservar letras retiradas nas fixtures: exercícios sintéticos e os metadados/chaves legados reproduzem os vínculos. Para validar upgrades distribuídos, acrescentar snapshots anonimizados autorizados e variantes confirmadas da tag/lojas; não tratar o exemplo reduzido da seção 3 como fixture completa de estatísticas.

| Caso automatizado | Estado anterior | Asserções principais |
| --- | --- | --- |
| Instalação limpa | Sem `Courses`, perfis ou versão | Três cursos ativos; músicas 1–4; sem IDs/payload retirados; Tutorial correto; seleção/ajuda só de ativos. |
| Upgrade sem progresso | Cinco cursos legados, perfil registrado sem alvos | Mesma topologia da instalação limpa; identidade/configuração preservadas; startup aguarda commit. |
| Básico concluído | Fronteira 30 + tentativas coerentes | Histórico completo conservado; Básico indisponível; outros cursos/progresso intactos. |
| Intermediário parcial | Fronteira 6/11, tentativas concluídas e incompletas | Todas as tentativas arquivadas com valores originais; sem referência operacional ao curso retirado. |
| Posteriores desbloqueados | SENAI com fronteira e tentativas; música individual concluída | Progresso/estatísticas desses cursos inalterados; seleção funciona após remover cursos 1/2. |
| Usuário parado em curso retirado | Modelos de sessão selecionados no Básico/Intermediário; perfil persistido | Novo processo não restaura cursor inexistente; troca em memória limpa seleção/rota/listas/timer/TTS e retorna a destino válido. |
| Usuário parado na música retirada | `musicas` fronteira 4; tentativa 5 incompleta; sessão em lição 5 | Tentativa arquivada; nenhum menu, ajuda ou retomada expõe 5; fronteira ativa 4. |
| Música já executada/concluída | Fronteira 5; várias `LICAO5.*` S/N | Todas as tentativas históricas preservadas uma única vez; fronteira 4; Enter em estatísticas nunca consulta 5. |
| Música 4 já concluída, 5 disponível | Fronteira 4 e tentativa 4 S | Lição 4 continua concluída/acessível; não anunciar “Não” por comparação com total novo. |
| Músicas sem progresso/parcial anterior | Fronteira 0, 1, 2 ou 3 | Próxima lição e acessos equivalentes aos legados, limitados aos ativos; tentativas intactas. |
| Migration executada duas vezes | Mesmo input e estado após primeira execução | `M(M(estado)) = M(estado)`; igualdade de conteúdo persistido, versões, tentativas e histórico; nenhum incremento indevido. |
| Falha em cada checkpoint | Interromper após staging, histórico, perfil, publicação e limpeza | Reinício completa/recupera sem perda ou duplicação; conteúdo retirado nunca é servido como fallback. |
| Múltiplos perfis/anônimo | Vários arquivos, só um logado | Todos reconciliados ou explicitamente pendentes; perfil não usado recentemente também seguro no próximo login. |
| Restore de backup | Marker novo + perfil/catálogo antigo reaparecido, ou ausência de marker | Integridade prevalece sobre marcador; entrada legada é remigrada sem duplicar histórico. |
| Importação incompleta | Pasta vazia, curso ausente, arquivo truncado | Não considerar pasta como commit; reconstruir catálogo ativo conhecido; preservar perfil inválido sem reset. |
| Catálogo malformado | Números 3/4/5, duplicados, JSON extra, lição faltante, exercícios vazios | Ordem esparsa aceita; duplicata/arquivo inválido diagnosticado; nenhuma seleção fora do domínio; sidecars não carregados como cursos. |
| Identidade e compatibilidade | Mesmo nome-base, ordem visual nova, IDs retirados tombstonados | Progresso sobrevivente não muda de curso; números retirados não são reciclados; parser legado não recebe raiz numérica incompatível. |
| Histórico independente | Estatísticas retiradas com catálogo ativo sem o curso/lição | Consulta histórica funciona sem executar `SelectLesson` retirado nem carregar letras; métricas/datas não recalculadas. |

Base existente: [CourseCharacterizationTests.cs:17](../Tests/Digitavox.CharacterizationTests/CourseCharacterizationTests.cs#L17) cobre cache/índice e ordem de exercícios; [UserProgressCharacterizationTests.cs:38](../Tests/Digitavox.CharacterizationTests/UserProgressCharacterizationTests.cs#L38) cobre fronteira e [linha 73](../Tests/Digitavox.CharacterizationTests/UserProgressCharacterizationTests.cs#L73) cobre estatísticas/desbloqueio; [DVPersistenceCharacterizationTests.cs:70](../Tests/Digitavox.PersistenceCharacterizationTests/DVPersistenceCharacterizationTests.cs#L70) cobre round-trip, ordenação consecutiva e cópia. Ampliar essas suítes e criar testes específicos de migration/recuperação, sem apenas espelhar uma lista de arquivos da implementação.

Validação manual de release: instalar binário antigo com fixtures representativas, fazer upgrade preservando sandbox e testar Login, seleção por teclado, ajuda/F-keys, estatísticas/Enter, VoiceOver/TalkBack/TTS, encerramento/reabertura e restauração permitida pelo sistema. Inspecionar Bundle final para ausência dos dois JSONs e do payload da música; inspecionar catálogo ativo/histórico no sandbox de teste para ausência de letra retirada e preservação dos registros. Reaproveitar [MANUAL_SMOKE_TEST_MATRIX.md](testing/MANUAL_SMOKE_TEST_MATRIX.md).

## 12. Arquivos que precisariam ser alterados

Nenhuma das alterações desta lista foi implementada nesta auditoria.

| Arquivo/área | Alteração prevista |
| --- | --- |
| `Resources/Raw/curso_abnt2_basico.json`, `curso_abnt2_intermediario.json` | Remover do pacote na release de retirada; manter identificação histórica em metadata própria. |
| `Resources/Raw/musicas.json` | Retirar `LICAO5` por completo; total 4; atualizar apresentação; manter identidades e payload das lições 1–4. |
| `Digitavox.csproj`, linhas 84 e 130–143 | Revisar empacotamento e referências de caminhos antigos; incluir manifesto/metadata novos sem reintroduzir conteúdo retirado. |
| `Helpers/DVPersistence.cs`, linhas 56–203 | Corrigir enumeração/ordenação; substituir importação por existência; validar arquivos; dar suporte à escrita recuperável ou delegar a serviço novo. |
| `Infrastructure/Startup/MauiAppStartupService.cs` | Executar detecção, migration/recuperação e commit antes de escolher destino e carregar runtime. |
| `Application/Abstractions/IAppStartupService.cs` | Ajustar contrato somente se necessário para representar recuperação/erro sem criar catálogo parcialmente válido. |
| `Application/Abstractions/ICourseCatalogStore.cs`, `Infrastructure/Persistence/MauiCourseCatalogStore.cs` | Expor versão, IDs/ordem/estado ativo e origem da geração validada; impedir carregamento casual de qualquer arquivo. |
| `Models/Course.cs` | Validação de seleção/disponibilidade e leitura de payload; identidade separada da posição visual; invalidação controlada de cache. |
| `Models/UserProgress.cs` | Reconciliar fronteira dos ativos; corrigir conclusão final; separar consulta histórica de lição executável; não descartar acesso legado. |
| `Application/Abstractions/IUserProgressStore.cs`, `Infrastructure/Persistence/MauiUserProgressStore.cs` | Integrar normalização por perfil e gravação recuperável; manter leitura de formato legado. |
| Novos serviços em `Infrastructure/Persistence` e contratos em `Application/Abstractions` | Executor de migrations, manifesto/estado, journal e histórico, com operações puras testáveis por ID. Nomes finais a definir na implementação. |
| `MauiProgram.cs` | Registrar os novos serviços. |
| `ViewModels/CoursesViewModel.cs`, `LessonsViewModel.cs` | Listar ativos e mapear opção para ID válido; suportar ausência de seleção/itens sem acesso fora da lista. |
| `ViewModels/ExercisesViewModel.cs`, `ExercisesStatisticsViewModel.cs` | Guardar execução e avanço; não iniciar lição vazia/retirada; destino seguro para seleção inválida. |
| `ViewModels/DVViewModelFunctions.cs`, `SecondHelpViewModel.cs` | Ajuda/fala e histórico respeitam disponibilidade; consulta de retirados independente do catálogo ativo. |
| `App.xaml.cs` | Revisar tratamento de falha e ciclo de sessão somente se necessário ao protocolo de startup/recuperação. |
| Views de histórico/recuperação e `AppShell.xaml.cs`/`INavigationService.cs` | Ajustar/adicionar somente para oferecer consulta histórica segura e recuperação acessível; não criar rota por música. |
| `Tests/Digitavox.PersistenceCharacterizationTests/*`, `Tests/Digitavox.CharacterizationTests/*` | Corrigir expectativa fixa dos cinco assets; adicionar fixtures e testes da seção 11. |
| `docs/testing/MANUAL_SMOKE_TEST_MATRIX.md`, documentação de arquitetura | Documentar upgrade, restore, histórico e contrato de versões. |

Sons de feedback, `SENAI.json`, a música individual e preferências não precisam de alteração para esta retirada. Manifestos de plataforma requerem validação dos fluxos de backup/ativação, sem mudança automática de política nesta proposta.

## Plano incremental de implementação

| Etapa | Entrega concreta | Critério de saída |
| --- | --- | --- |
| 1. Confirmar baseline e política histórica | Identificar releases/variantes Adulto; congelar IDs/schema e decidir apresentação histórica; preparar fixtures equivalentes, sem payload retirado | Alvos e estados legados reconhecidos; expectativas de preservação/acesso aprovadas na revisão técnica. |
| 2. Preparar contratos e proteção | Ordenação esparsa, validação por ID, catálogo ativo/versionado, bloqueio central e escrita recuperável | Leitura de 3/4/5 válida; nenhum consumidor consegue executar ID indisponível; compatibilidade legada testada. |
| 3. Implementar transformação `0 → 1` | Operações puras sobre catálogo/progresso/histórico, política de fronteira e conclusão final | Todos os cenários de progresso e idempotência passam; estatísticas sobreviventes inalteradas. |
| 4. Integrar startup e recuperação | Journal/checkpoints, todos os perfis, invalidação de cache, restauração e falhas | Interrupção em qualquer ponto não perde dados nem oferece payload retirado; startup não libera estado parcial. |
| 5. Aplicar retirada na mesma release | Remover os dois assets, retirar `LICAO5`, ajustar textos/contagem e manifesto | Bundle e sandbox de teste sem payload retirado; instalação limpa e upgrade produzem o mesmo catálogo ativo. |
| 6. Homologar e distribuir | Upgrade real a partir das versões confirmadas; teste acessível e inspeção dos dados; documentação operacional de recuperação | UI/ajuda/TTS/estatísticas corretas; histórico preservado; segunda atualização sem efeitos adicionais. |

As etapas podem ser PRs incrementais, mas **a release que retira conteúdo deve conter conjuntamente proteção, migration e novo catálogo**. Não distribuir uma edição isolada de JSON esperando que o sandbox se atualize. Um eventual retorno de versão precisa preservar a política de indisponibilidade e o histórico; não usar rollback para restaurar payload retirado.
