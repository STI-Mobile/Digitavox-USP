# Otimização DEX na publicação Android

O projeto habilita `AndroidLinkTool=r8` somente em `Release` Android. O R8 remove
código Java/Kotlin não utilizado. `AndroidCreateProguardMappingFile=True` gera
`mapping.txt`, que o SDK também incorpora ao AAB. A redução dos assemblies C#
por trimming é uma etapa diferente e não mede a ofuscação DEX.

## Gerar e medir o pacote

Use o SDK indicado em `global.json`, com o workload `maui-android` instalado:

```bash
dotnet restore Digitavox.csproj -p:TargetFramework=net10.0-android
dotnet publish Digitavox.csproj -f net10.0-android -c Release --no-restore
```

Meça todos os arquivos DEX descompactados do AAB que será enviado, substituindo
o caminho abaixo pelo pacote final:

```bash
python3 - caminho/para/aplicativo.aab <<'PY'
import sys
from zipfile import ZipFile

with ZipFile(sys.argv[1]) as bundle:
    dex = [item for item in bundle.infolist() if item.filename.endswith('.dex')]
    for item in dex:
        print(f'{item.filename}: {item.file_size:,} bytes')
    total = sum(item.file_size for item in dex)
    print(f'DEX total: {total:,} bytes ({total / 1_000_000:.2f} MB)')
    for name in bundle.namelist():
        if name.startswith('BUNDLE-METADATA/') and (
            name.endswith('proguard.map') or name.endswith('r8.json')
        ):
            print(f'Metadados: {name}')
PY
```

Segundo a [documentação do Google](https://developer.android.com/google/play/vitals/code-optimization),
o Play Console exibe métricas para todos os apps, mas aplica o alerta de otimização
a apps com pelo menos 10 MB de DEX descompactado. O tamanho do AAB completo,
dos áudios e das bibliotecas nativas não corresponde a essa medida. A porcentagem
final deve ser confirmada no Play Console após o processamento do pacote.

Na validação local de 22/10/2026, o AAB anterior continha 15.311.876 bytes de DEX.
O novo build Release com R8 contém 8.470.616 bytes (8,47 MB), abaixo do limite
de 10 MB, e inclui `BUNDLE-METADATA/com.android.tools.build.obfuscation/proguard.map`.
O APK derivado desse AAB foi instalado no Pixel 7 API 35 e passou pela abertura
e pela entrada/cancelamento da confirmação de login via teclado, sem registros
de crash. Essa verificação não substitui testes completos de cursos e voz nem
confirma o processamento no Google Play. O pacote de validação usa chave de debug.

Após a atualização de Material Components e AndroidX para a exibição de ponta
a ponta, o AAB final passou a conter 8.786.160 bytes (8,79 MB) de DEX. Veja a
[validação da atualização](android-edge-to-edge.md#resultado-local-em-22102026).

## Limitação de ofuscação no SDK instalado

O workload `Microsoft.Android.Sdk.Darwin` 36.1.69 desativa a renomeação de símbolos
com `-dontobfuscate`. Habilitar R8 nesse workload reduz DEX, mas não garante 25%
de ofuscação. Não remova as regras de preservação do SDK: elas protegem o acesso
do código .NET às classes Java via JNI.

A [Microsoft documenta](https://learn.microsoft.com/dotnet/android/building-apps/build-properties#androidr8obfuscationmode)
`AndroidR8ObfuscationMode=private-members` em uma atualização de manutenção do
.NET 10. Essa propriedade não está implementada no workload 36.1.69; apenas
declará-la no projeto não habilita a funcionalidade. Consulte também o
[acompanhamento no .NET Android](https://github.com/dotnet/android/issues/12535).
Se o DEX continuar acima do limite, atualize para um workload que implemente essa
opção, valide o mapeamento e teste a versão Release, incluindo voz, teclado e
acessibilidade, antes da publicação.

Para substituir a versão 23 no Google Play, use um `ApplicationVersion`
(`versionCode`) maior que 23. Pacotes assinados com a chave de debug servem
somente para validação local; a publicação exige a chave de upload configurada.
