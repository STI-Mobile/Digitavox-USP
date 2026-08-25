# Matriz de smoke tests manuais

Esta matriz congela os fluxos observáveis que precisam permanecer equivalentes durante o refactoring arquitetural. Registre dispositivo, versão do sistema, versão do app, resultado e evidência em cada execução.

## Inicialização e ciclo de vida

- [ ] Primeira execução copia os cursos e abre o tutorial somente depois de concluir a preparação.
- [ ] Execuções seguintes abrem a identificação sem repetir a cópia inicial.
- [ ] Suspender e retomar preserva tela, seleção, exercício e temporizador.
- [ ] Encerrar durante uma fala não deixa TTS ou áudio ativo.
- [ ] Retomar anuncia corretamente a página atual.

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
