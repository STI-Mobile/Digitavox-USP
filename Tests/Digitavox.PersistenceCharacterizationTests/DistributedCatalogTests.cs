using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Digitavox.PersistenceCharacterizationTests;

[TestClass]
public class DistributedCatalogTests
{
    [TestMethod]
    public void Source_bundle_contains_exactly_the_three_active_course_definitions()
    {
        string raw = Path.Combine(RepositoryRoot(), "Resources", "Raw");
        var courses = Directory.GetFiles(raw, "*.json")
            .Where(p =>
            {
                using var document = JsonDocument.Parse(File.ReadAllText(p));
                return document.RootElement.TryGetProperty("CURSO", out _);
            })
            .Select(Path.GetFileName).OrderBy(n => n, StringComparer.Ordinal).ToArray();
        CollectionAssert.AreEqual(new[] { "SENAI.json", "musica_se_eu_quiser_falar_com_deus.json", "musicas.json" }, courses);
        Assert.IsFalse(File.Exists(Path.Combine(raw, "curso_abnt2_basico.json")));
        Assert.IsFalse(File.Exists(Path.Combine(raw, "curso_abnt2_intermediario.json")));
    }

    [TestMethod]
    public void Music_has_four_original_lesson_identities_without_retired_payload()
    {
        string text = File.ReadAllText(Path.Combine(RepositoryRoot(), "Resources", "Raw", "musicas.json"));
        using var document = JsonDocument.Parse(text);
        var root = document.RootElement;
        Assert.AreEqual(3, root.GetProperty("CURSO").GetProperty("NUMEROCURSO").GetInt32());
        Assert.AreEqual(4, root.GetProperty("CURSO").GetProperty("QUANTIDADELICOES").GetInt32());
        CollectionAssert.AreEqual(new[] { "LICAO1", "LICAO2", "LICAO3", "LICAO4" }, root.EnumerateObject().Where(p => p.Name.StartsWith("LICAO")).Select(p => p.Name).ToArray());
        Assert.IsFalse(text.Contains("Juca", StringComparison.OrdinalIgnoreCase));
        Assert.IsFalse(text.Contains("Chaves", StringComparison.OrdinalIgnoreCase));
        Assert.IsFalse(text.Contains("CÚMPLICE", StringComparison.OrdinalIgnoreCase));
        Assert.IsFalse(text.Contains("cinco lições", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    [DataRow(1, "9638DA03D8209434A4587F100E1B8B7F14BD22857DEAD4A4FB55B572A98C154A")]
    [DataRow(2, "6BD4507DA60A7F1F16CDA664BE5B4A611EBF8CBB0C9B0E051BEABCA58E461E05")]
    [DataRow(3, "C0BD6502F6A74FF10D5AB65B8F3096919EF24EA4EE8165F9CEAF33619BA02A4F")]
    [DataRow(4, "C720C881A7F3F3D926C1DBCF8650F6DD1C968A096C974B707DD980E7DA38FA84")]
    public void Surviving_music_lessons_match_the_audited_baseline_byte_for_byte(int lesson, string hash)
    {
        using var document = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(RepositoryRoot(), "Resources", "Raw", "musicas.json")));
        string rawLesson = document.RootElement.GetProperty("LICAO" + lesson).GetRawText();
        Assert.AreEqual(hash, Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawLesson))));
    }

    [TestMethod]
    public async Task Evaluated_maui_asset_items_exclude_retired_files()
    {
        var start = new ProcessStartInfo("dotnet") { WorkingDirectory = RepositoryRoot(), RedirectStandardOutput = true, RedirectStandardError = true };
        foreach (string argument in new[] { "msbuild", "Digitavox.csproj", "-getItem:MauiAsset", "-p:TargetFramework=net10.0", "-p:TargetFrameworks=net10.0", "-p:MSBuildEnableWorkloadResolver=false" }) start.ArgumentList.Add(argument);
        using var process = Process.Start(start);
        Task<string> output = process.StandardOutput.ReadToEndAsync();
        Task<string> error = process.StandardError.ReadToEndAsync();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        await process.WaitForExitAsync(timeout.Token);
        Assert.AreEqual(0, process.ExitCode, await error);
        using var document = JsonDocument.Parse(await output);
        var names = document.RootElement.GetProperty("Items").GetProperty("MauiAsset").EnumerateArray().Select(a => a.GetProperty("LogicalName").GetString()).ToArray();
        foreach (string active in new[] { "musicas.json", "musica_se_eu_quiser_falar_com_deus.json", "SENAI.json" }) CollectionAssert.Contains(names, active);
        CollectionAssert.DoesNotContain(names, "curso_abnt2_basico.json");
        CollectionAssert.DoesNotContain(names, "curso_abnt2_intermediario.json");
    }

    internal static string RepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
            if (File.Exists(Path.Combine(directory.FullName, "Digitavox.csproj"))) return directory.FullName;
        throw new DirectoryNotFoundException("Repositório não encontrado.");
    }
}
