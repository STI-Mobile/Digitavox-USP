# Exibição de ponta a ponta no Android

`MainActivity.OnCreate` chama `WindowCompat.EnableEdgeToEdge(Window)` após a
inicialização do MAUI. O AndroidX configura a janela também em versões anteriores
ao Android 15. Os recuos do conteúdo ficam sob responsabilidade do MAUI:
o estilo global de `ContentPage`, em `Resources/Styles/Styles.xaml`, já define
`SafeAreaEdges="Container"` para respeitar barras do sistema e recortes da tela.
Não acrescente margens fixas para simular a altura dessas barras.

## APIs descontinuadas indicadas pelo Play Console

Na versão 23, os pontos `BottomSheetDialog.onCreate`,
`EdgeToEdgeUtils.applyEdgeToEdge` e `SheetDialog.onCreate` pertencem à dependência
Material Components, não ao código da atividade do aplicativo.

O projeto referencia `Xamarin.Google.Android.Material` 1.14.0.6 somente no alvo
Android. Essa versão usa verificações de versão antes de alterar as cores das
barras: `Window.setStatusBarColor` e `Window.setNavigationBarColor` ficam
restritas a `SDK_INT < 35`. As chamadas permanecem no bytecode para compatibilidade
com versões anteriores. Não é correto afirmar que foram removidas de todo o DEX.
Os pacotes AndroidX complementares também estão alinhados para que o restore
não resolva versões fora dos intervalos exigidos (`NU1608`).

## Validação de uma publicação

Depois de atualizar bibliotecas Android, faça uma compilação limpa antes de
validar o pacote para evitar reutilizar recursos e assemblies gerados antigos:

```bash
dotnet restore Digitavox.csproj -p:TargetFramework=net10.0-android
dotnet clean Digitavox.csproj -f net10.0-android -c Release
dotnet publish Digitavox.csproj -f net10.0-android -c Release --no-restore
```

Se houver intermediários de builds anteriores com `RuntimeIdentifier` explícito,
também descarte os diretórios gerados `obj/Release/net10.0-android` e
`bin/Release/net10.0-android` antes de recompilar. Na validação desta atualização,
`dotnet clean` sozinho deixou um designer de recursos antigo no diretório de
arquitetura. Isso causava uma falha em `NavigationRootManager.Connect` ao carregar
um layout pelo ID antigo. Preserve previamente qualquer pacote que queira guardar.

Teste o APK derivado do AAB Release: abertura, navegação por teclado, páginas
longas, rolagem até o fim, orientação retrato/paisagem, temas claro/escuro,
navegação por gestos/três botões e telas com recortes. Confira se texto e
controles continuam visíveis junto às barras. Inclua Android 15 e 16 e uma
versão anterior ao Android 15 para verificar a compatibilidade.

### Resultado local em 22/10/2026

O APK derivado do AAB Release passou pela abertura no Pixel 7 API 35 e no AVD
API 36. No API 35 foram conferidos o login, a entrada/cancelamento por teclado,
navegação por gestos e por três botões, retrato, paisagem e tema escuro. No API 36
foi conferida a tela de instruções com navegação por três botões. As capturas
dessas telas não mostraram sobreposição do conteúdo com barras do sistema, e
não houve novos registros de crash durante o teste final. As configurações de
tema, navegação e rotação do Pixel foram restauradas ao final.

O AAB final contém 8.786.160 bytes de DEX (8,79 MB) e o mapeamento do R8, mantendo
o tamanho abaixo dos 10 MB do alerta de otimização. O pacote de teste usa chave
de debug e mantém o `versionCode` 23; não foi enviado ao Google Play. Testes em
Android anterior ao 15, aparelhos físicos e dos demais fluxos do aplicativo
continuam necessários na homologação da publicação.

O Play Console analisa o pacote enviado. Os avisos da versão 23 só poderão ser
reavaliados após o envio de um novo pacote com `versionCode` maior que 23 e a
assinatura de upload. Uma análise estática ainda pode encontrar as chamadas
protegidas por versão nas dependências.

Referências:

- [Android: exibir conteúdo de ponta a ponta](https://developer.android.com/develop/ui/views/layout/edge-to-edge)
- [MAUI: layout de área segura](https://learn.microsoft.com/dotnet/maui/user-interface/safe-area?view=net-maui-10.0)
- [Material 1.14: tratamento das cores das barras](https://github.com/material-components/material-components-android/blob/1.14.0/lib/java/com/google/android/material/internal/EdgeToEdgeUtils.java)
