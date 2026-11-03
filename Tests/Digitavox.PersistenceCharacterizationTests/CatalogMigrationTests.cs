using System.Text.Json.Nodes;
using Digitavox.Helpers;
using Digitavox.Infrastructure.Persistence;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Digitavox.PersistenceCharacterizationTests;

[TestClass]
public class CatalogMigrationTests
{
    private string directory;

    [TestInitialize]
    public void SetUp()
    {
        directory = Path.Combine(Path.GetTempPath(), "DigitavoxCatalogTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        FileSystem.AppDataDirectory = directory;
        FileSystem.AppPackageDirectory = Path.Combine(AppContext.BaseDirectory, "Bundle");
    }

    [TestCleanup]
    public void TearDown() => Directory.Delete(directory, recursive: true);

    private CatalogMigration Migration(Action<string> checkpoint = null) => new(directory, FileSystem.OpenAppPackageFileAsync, checkpoint);

    [TestMethod]
    public void Missing_metadata_means_legacy_version_zero()
    {
        Directory.CreateDirectory(Path.Combine(directory, "Courses"));
        Assert.AreEqual(0, Migration().ReadVersion());
        Assert.IsFalse(File.Exists(Path.Combine(directory, "Catalog", "state.json")));
    }

    [TestMethod]
    public async Task Migration_publishes_version_one_outside_courses_and_keeps_original_numbers()
    {
        Assert.IsTrue(await Migration().EnsureCurrentAsync());
        Assert.AreEqual(1, Migration().ReadVersion());
        var (courses, files, _) = DVPersistence.ReadCourseFiles();
        CollectionAssert.AreEqual(new[] { "musicas.json", "musica_se_eu_quiser_falar_com_deus.json", "SENAI.json" }, files);
        CollectionAssert.AreEqual(new[] { 3, 4, 5 }, courses.Select(c => c.GetProperty("CURSO").GetProperty("NUMEROCURSO").GetInt32()).ToArray());
        Assert.AreEqual(4, courses[0].GetProperty("CURSO").GetProperty("QUANTIDADELICOES").GetInt32());
        Assert.IsFalse(courses[0].TryGetProperty("LICAO5", out _));
        Assert.IsFalse(Migration().IsPending);
    }

    [TestMethod]
    public async Task A_second_migration_of_version_one_does_not_modify_files()
    {
        await Migration().EnsureCurrentAsync();
        var before = Snapshot(directory);
        Assert.IsFalse(await Migration().EnsureCurrentAsync());
        CollectionAssert.AreEqual(before, Snapshot(directory));
    }

    [TestMethod]
    [DataRow("prepared")]
    [DataRow("catalog:musicas")]
    [DataRow("before-commit")]
    public async Task Interrupted_publication_is_not_marked_complete_and_can_be_retried(string stop)
    {
        await Assert.ThrowsExactlyAsync<IOException>(() => Migration(point =>
        {
            if (point == stop) throw new IOException("Interrupção simulada");
        }).EnsureCurrentAsync());

        Assert.AreEqual(0, Migration().ReadVersion());
        Assert.IsTrue(Migration().IsPending);
        Assert.IsTrue(await Migration().EnsureCurrentAsync());
        Assert.AreEqual(1, Migration().ReadVersion());
        Assert.IsFalse(Migration().IsPending);
    }

    [TestMethod]
    public async Task Unrelated_progress_is_preserved_byte_for_byte()
    {
        string users = Path.Combine(directory, "Users");
        Directory.CreateDirectory(users);
        string path = Path.Combine(users, "ANA.json");
        const string original = "{\"NOMEUSUARIO\":\"ANA\",\"type\":\"USUARIO\",\"SENAI\":{\"ULTIMACONCLUIDA\":6}}";
        File.WriteAllText(path, original);
        Assert.IsFalse(await Migration().EnsureCurrentAsync());
        Assert.AreEqual(original, File.ReadAllText(path));
    }

    [TestMethod]
    public async Task Unsupported_version_is_not_downgraded_or_overwritten()
    {
        string catalog = Path.Combine(directory, "Catalog");
        Directory.CreateDirectory(catalog);
        string path = Path.Combine(catalog, "state.json");
        File.WriteAllText(path, "{\"catalogVersion\":2}");
        await Assert.ThrowsExactlyAsync<InvalidDataException>(() => Migration().EnsureCurrentAsync());
        Assert.AreEqual("{\"catalogVersion\":2}", File.ReadAllText(path));
        Assert.IsFalse(Directory.Exists(Path.Combine(directory, "Courses")));
    }

    internal static string[] Snapshot(string root) => Directory.GetFiles(root, "*", SearchOption.AllDirectories)
        .OrderBy(p => p, StringComparer.Ordinal)
        .Select(p => Path.GetRelativePath(root, p) + ":" + Convert.ToHexString(File.ReadAllBytes(p)) + ":" + File.GetLastWriteTimeUtc(p).Ticks)
        .ToArray();
}
