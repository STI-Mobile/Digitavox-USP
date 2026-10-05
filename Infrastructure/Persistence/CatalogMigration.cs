using System.Text.Json;
using System.Text.Json.Nodes;
using Digitavox.Domain.Catalog;

namespace Digitavox.Infrastructure.Persistence;

/// <summary>The single supported migration: the distributed legacy catalog (0) to catalog 1.</summary>
public sealed class CatalogMigration
{
    public const int CurrentVersion = 1;
    public static IReadOnlyList<string> ActiveCourseIds { get; } = Array.AsReadOnly(new[]
    {
        "musicas", "musica_se_eu_quiser_falar_com_deus", "SENAI"
    });
    private static readonly SemaphoreSlim InitializationLock = new(1, 1);
    private readonly string appDataDirectory;
    private readonly Func<string, Task<Stream>> openBundle;
    private readonly Action<string> checkpoint;
    private string StatePath => Path.Combine(appDataDirectory, "Catalog", "state.json");
    private string PendingPath => Path.Combine(appDataDirectory, "Catalog", "pending.json");

    public CatalogMigration(string appDataDirectory, Func<string, Task<Stream>> openBundle, Action<string> checkpoint = null)
    {
        this.appDataDirectory = appDataDirectory;
        this.openBundle = openBundle;
        this.checkpoint = checkpoint;
    }

    public int ReadVersion()
    {
        if (!File.Exists(StatePath)) return 0;
        try
        {
            var state = JsonNode.Parse(File.ReadAllBytes(StatePath)) as JsonObject;
            if (state?["catalogVersion"] is not JsonValue value || !value.TryGetValue<int>(out int version) || version != CurrentVersion)
                throw new InvalidDataException("Versão de catálogo não suportada.");
            return version;
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException("Metadata do catálogo inválida; nenhum dado foi resetado.", exception);
        }
    }

    public bool IsPending => File.Exists(PendingPath);

    /// <returns>True only when completing the first installation, including a retry of that installation.</returns>
    public async Task<bool> EnsureCurrentAsync()
    {
        await InitializationLock.WaitAsync();
        try
        {
            int version = ReadVersion();
            string coursesDirectory = Path.Combine(appDataDirectory, "Courses");
            bool firstInstallation = version == 0 && !Directory.Exists(coursesDirectory) &&
                (!Directory.Exists(Path.Combine(appDataDirectory, "Users")) || !Directory.EnumerateFiles(Path.Combine(appDataDirectory, "Users"), "*.json").Any());
            if (IsPending)
            {
                var pending = JsonNode.Parse(File.ReadAllBytes(PendingPath)) as JsonObject;
                if (pending?["targetVersion"]?.GetValue<int>() != CurrentVersion || pending["firstInstallation"] is not JsonValue first)
                    throw new InvalidDataException("Journal de catálogo inválido; recuperação necessária.");
                firstInstallation = first.GetValue<bool>();
            }

            // Prepare and validate the entire target before mutating any installed file.
            var files = new List<(string Path, byte[] Bytes)>();
            foreach (string id in ActiveCourseIds)
            {
                using Stream stream = await openBundle(id + ".json");
                var root = await JsonNode.ParseAsync(stream) as JsonObject ?? throw new InvalidDataException($"Curso inválido no Bundle: {id}.");
                if (id == "musicas")
                {
                    root.Remove("LICAO5");
                    var course = root["CURSO"] as JsonObject ?? throw new InvalidDataException("CURSO ausente em musicas.");
                    course["QUANTIDADELICOES"] = 4;
                    var present = course["Present"] as JsonObject ?? throw new InvalidDataException("Apresentação ausente em musicas.");
                    string text = present["APT"]?.GetValue<string>() ?? throw new InvalidDataException("APT ausente em musicas.");
                    present["APT"] = text.Replace("cinco lições", "quatro lições", StringComparison.Ordinal);
                }
                byte[] bytes = AtomicJsonFile.Encode(root);
                using var document = JsonDocument.Parse(bytes);
                CourseCatalogValidator.Validate(document.RootElement, id + ".json");
                int expectedNumber = id == "musicas" ? 3 : id == "SENAI" ? 5 : 4;
                if (!CourseCatalogValidator.TryInteger(document.RootElement.GetProperty("CURSO"), "NUMEROCURSO", out int number) || number != expectedNumber)
                    throw new InvalidDataException($"NUMEROCURSO inesperado em {id}.");
                files.Add((Path.Combine(coursesDirectory, id + ".json"), bytes));
            }

            string[] removedPaths = new[] { "curso_abnt2_basico", "curso_abnt2_intermediario" }
                .Select(id => Path.Combine(coursesDirectory, id + ".json")).ToArray();
            if (version == CurrentVersion && !IsPending && files.All(f => AtomicJsonFile.Matches(f.Path, f.Bytes)) && !removedPaths.Any(File.Exists))
                return false;

            var journal = new JsonObject { ["targetVersion"] = CurrentVersion, ["firstInstallation"] = firstInstallation };
            AtomicJsonFile.Write(PendingPath, AtomicJsonFile.Encode(journal));
            // A version marker certifies a fully published state, not a partially repaired directory.
            if (File.Exists(StatePath)) File.Delete(StatePath);
            checkpoint?.Invoke("prepared");
            foreach (var file in files)
            {
                AtomicJsonFile.Write(file.Path, file.Bytes);
                checkpoint?.Invoke("catalog:" + Path.GetFileNameWithoutExtension(file.Path));
            }
            foreach (string path in removedPaths)
            {
                if (File.Exists(path)) File.Delete(path);
                checkpoint?.Invoke("removed:" + Path.GetFileNameWithoutExtension(path));
            }
            checkpoint?.Invoke("before-commit");
            AtomicJsonFile.Write(StatePath, AtomicJsonFile.Encode(new JsonObject { ["catalogVersion"] = CurrentVersion }));
            File.Delete(PendingPath);
            return firstInstallation;
        }
        finally
        {
            InitializationLock.Release();
        }
    }
}
