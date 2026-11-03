# Matriz de smoke tests manuais

Esta matriz congela os fluxos observáveis que precisam permanecer equivalentes durante o refactoring arquitetural. Registre dispositivo, versão do sistema, versão do app, resultado e evidência em cada execução.

## Inicialização e ciclo de vida

- [ ] Primeira execução publica o catálogo versão 1 e abre o tutorial somente depois de concluir a preparação.
- [ ] Execuções seguintes verificam catálogo/perfis e abrem a identificação; estado já válido não sofre novas gravações.
- [ ] Suspender e retomar preserva tela, seleção, exercício e temporizador.
- [ ] Encerrar durante uma fala não deixa TTS ou áudio ativo.
- [ ] Retomar anuncia corretamente a página atual.
- [ ] Com inicialização deliberadamente pendente, pressionar/soltar teclas físicas não causa crash nem despacha comandos de página.
- [ ] Com falha de inicialização e a mensagem de erro visível, pressionar/soltar teclas físicas continua seguro.
- [ ] Depois da inicialização, teclas tratadas e não tratadas, modificadores e navegação mantêm o comportamento normal.
- [ ] Manter uma tecla pressionada na transição de StartupView para AppShell não causa crash; validar também Shell ainda sem CurrentPage.

## Login e sessão

- [ ] Enter sem nome solicita confirmação para usuário anônimo.
- [ ] Novo nome solicita cadastro e persiste o usuário após confirmação.
- [ ] Usuário existente entra sem solicitar novo cadastro.
- [ ] Backspace atualiza nome visual e falado.
- [ ] Logout limpa a sessão e retorna à identificação.

## Cursos, lições e ajuda

- [ ] Cursos podem ser percorridos por números, Tab, Shift+Tab e setas verticais.
- [ ] Enter seleciona o curso anunciado.
- [ ] Apenas lições desbloqueadas podem ser iniciadas.
- [ ] F1 abre a ajuda correspondente ao item atual.
- [ ] Escape retorna à rota anterior correta, inclusive ao sair de ajuda aninhada.
- [ ] Instruções habilitadas/desabilitadas alteram somente o conteúdo esperado.

## Exercícios e estatísticas

- [ ] Exercício inicia com texto, fala, repetição e tempo configurados.
- [ ] Tecla correta avança e recebe o feedback visual/sonoro atual.
- [ ] Tecla incorreta contabiliza erro na letra esperada e recebe o feedback atual.
- [ ] Comparação sem distinção de caixa aceita maiúscula/minúscula conforme configuração da lição.
- [ ] Espaços contabilizam palavras corretas ou incorretas.
- [ ] Repetições avançam e são anunciadas quando habilitadas.
- [ ] Expiração do tempo abre as estatísticas uma única vez.
- [ ] Conclusão salva percentuais, tempo, velocidade, palavras e erros por caractere.
- [ ] Lição concluída desbloqueia a próxima sem ultrapassar o total do curso.
- [ ] Consulta de repetições antigas não altera o progresso atual.

## Configurações

- [ ] Velocidade de fala é aplicada e persiste após reiniciar.
- [ ] Falar teclagem pode ser ativado e desativado.
- [ ] Tamanho do texto pode ser pré-visualizado, confirmado e cancelado.
- [ ] Divisor de tempo afeta o tempo total do exercício.
- [ ] Fala de repetições e instruções pode ser ativada/desativada.
- [ ] Restaurar padrões repõe todas as configurações e a velocidade do TTS.

## Teclado físico

Executar em Android, iOS/iPadOS com teclado, macOS e Windows:

- [ ] Letras, números, pontuação, espaço, Enter, Escape, Backspace e setas são normalizados igualmente.
- [ ] Shift, Caps Lock, Ctrl, AltGr/Fn e Num Lock mantêm o comportamento atual.
- [ ] Acentos agudo, grave, circunflexo e til produzem as combinações atuais.
- [ ] Pressionar e soltar modificadores em ordens diferentes não duplica comandos.
- [ ] Combinações reservadas ao leitor de tela são devolvidas quando esperado.
- [ ] Teclas não mapeadas produzem o anúncio atual sem interromper o fluxo.

## TTS, áudio e acessibilidade

- [ ] Espaço repete a apresentação da tela.
- [ ] Nova ação interrompe a fala anterior sem executar callbacks obsoletos.
- [ ] Linha corrente recebe o mesmo destaque visual durante a fala.
- [ ] Sons de erro, início, fim e conclusão não se sobrepõem incorretamente ao TTS.
- [ ] TalkBack ativo mostra o alerta modal; desativá-lo remove o alerta automaticamente.
- [ ] VoiceOver permite entrar e sair da política de privacidade com as combinações documentadas.
- [ ] Alto contraste e tema claro/escuro preservam legibilidade e destaques.

## Conteúdo legal

- [ ] Política de privacidade abre, carrega e permite retorno.
- [ ] Licenças de terceiros carregam introdução e seções completas.
- [ ] O tamanho de texto configurado é aplicado às licenças.

## Upgrade do catálogo versão 0 → 1

Estes itens estão **pendentes de execução em dispositivo/pacote nativo**. Os testes automatizados e os limites do ambiente estão registrados em [content-catalog-v1-migration.md](../content-catalog-v1-migration.md).

- [ ] Instalação limpa oferece somente músicas, música individual e SENAI, nessa ordem; campos `NUMEROCURSO` continuam 3/4/5.
- [ ] Bundle final não contém os dois JSONs retirados nem o payload de `musicas/LICAO5`.
- [ ] Upgrade de uma versão distribuída preserva sandbox/perfis, atualiza os JSONs antigos e abre identificação.
- [ ] Perfil sem progresso mantém configurações e dados cadastrais.
- [ ] Básico concluído e Intermediário parcial saem do catálogo executável; suas tentativas e fronteiras aparecem no histórico.
- [ ] SENAI e música individual mantêm disponibilidade, tentativas, datas e estatísticas.
- [ ] Músicas mantém `LICAO1`…`LICAO4` com as mesmas identidades, exercícios e estatísticas.
- [ ] Fronteiras legadas de músicas 0/1/2/3/4 continuam corretas; fronteira 5 vira 4 e preserva evidência anterior no histórico.
- [ ] Quinta música desbloqueada, concluída ou incompleta não pode ser iniciada por menu, ajuda, Enter em estatísticas ou retomada.
- [ ] Conclusão de música 4 continua sendo anunciada corretamente; terminar essa lição não tenta acessar lição 5.
- [ ] Usuário que estava em um conteúdo retirado antes do upgrade inicia em identificação; nenhum cursor antigo é restaurado.
- [ ] Todos os perfis, inclusive anônimo e usuários não logados recentemente, são tratados.
- [ ] Opção 3 do menu de usuário mostra histórico por texto/fala; Tab/setas consultam, Escape retorna e Enter não inicia exercício.
- [ ] Histórico informa tentativas incompletas, datas, acertos, tempos e erros por caractere; não fala letras/exercícios retirados.
- [ ] Pasta `Courses` vazia, importação parcial ou arquivo ativo ausente são reparados sem reset do usuário.
- [ ] Interrupções entre gravação de histórico, perfil, catálogo e commit são recuperadas no reinício sem duplicação/perda.
- [ ] Falha/JSON inválido mostra erro de inicialização, preserva os dados e não oferece conteúdo retirado como fallback.
- [ ] Restore de dados legados depois de marcador versão 1 provoca nova reconciliação, sem reutilizar os IDs retirados.
- [ ] Segunda atualização e startup repetido não alteram arquivos nem duplicam tentativas/histórico.
- [ ] VoiceOver/TalkBack/TTS e navegação por teclado funcionam no catálogo e na nova consulta histórica.
