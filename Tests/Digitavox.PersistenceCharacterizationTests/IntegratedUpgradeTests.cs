using System.Text.Json.Nodes;
using Digitavox.Core.Abstractions;
using Digitavox.Infrastructure.Persistence;
using Digitavox.Infrastructure.Settings;
using Digitavox.Infrastructure.Startup;
using Digitavox.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Digitavox.PersistenceCharacterizationTests;

[TestClass]
public class IntegratedUpgradeTests
{
    [TestMethod]
    [DataRow("clean")]
    [DataRow("no-progress")]
    [DataRow("basic-completed")]
    [DataRow("intermediate-partial")]
    [DataRow("senai")]
    [DataRow("individual-song")]
    [DataRow("music-through-four")]
    [DataRow("retired-song-unlocked")]
    [DataRow("retired-song-completed")]
    [DataRow("retired-song-incomplete")]
    [DataRow("multiple-profiles")]
    [DataRow("anonymous")]
    [DataRow("migration-twice")]
    [DataRow("startup-again")]
    [DataRow("partial-import")]
    [DataRow("utf8-bom")]
    public async Task Full_upgrade_preserves_progress_history_navigation_and_idempotence(string scenario)
    {
        string root = Path.Combine(Path.GetTempPath(), "DigitavoxIntegratedUpgrade", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            FileSystem.AppDataDirectory = root;
            FileSystem.AppPackageDirectory = Path.Combine(AppContext.BaseDirectory, "Bundle");
            Preferences.Default.Clear();
            if (scenario != "clean")
                LegacyCatalogFixture.Write(root, scenario == "partial-import"
                    ? new[] { "curso_abnt2_intermediario" }
                    : new[] { "curso_abnt2_basico", "curso_abnt2_intermediario", "musicas", "musica_se_eu_quiser_falar_com_deus", "SENAI" });

            var originals = new Dictionary<string, JsonObject>();
            foreach (string name in scenario == "multiple-profiles" ? new[] { "ANA", "BIA", "ANONIMO" } : new[] { scenario == "anonymous" ? "ANONIMO" : "ANA" })
            {
                var user = BuildUser(name, scenario);
                originals[name] = user;
                if (scenario == "clean") continue;
                Directory.CreateDirectory(Path.Combine(root, "Users"));
                File.WriteAllText(Path.Combine(root, "Users", name + ".json"), user.ToJsonString(), new System.Text.UTF8Encoding(scenario == "utf8-bom"));
            }

            var migration = new CatalogMigration(root, FileSystem.OpenAppPackageFileAsync);
            var course = new Course(new MauiCourseCatalogStore(migration));
            var startup = new MauiAppStartupService(new FingerMapping(new Environment()), migration, course);
            Assert.AreEqual(scenario == "clean" ? AppStartupDestination.Tutorial : AppStartupDestination.Login, await startup.InitializeAsync());
            Assert.AreEqual(1, migration.ReadVersion());
            var catalog = new MauiCourseCatalogStore(migration).Load();
            CollectionAssert.AreEqual(new[] { "musicas.json", "musica_se_eu_quiser_falar_com_deus.json", "SENAI.json" }, catalog.FileNames);
            Assert.IsFalse(File.Exists(Path.Combine(root, "Courses", "curso_abnt2_basico.json")));
            Assert.IsFalse(File.Exists(Path.Combine(root, "Courses", "curso_abnt2_intermediario.json")));
            string music = File.ReadAllText(Path.Combine(root, "Courses", "musicas.json"));
            Assert.IsFalse(music.Contains("LICAO5"));
            Assert.IsFalse(music.Contains("Juca", StringComparison.OrdinalIgnoreCase));
            Assert.IsFalse(music.Contains("Lição legada retirada"));

            foreach (var original in originals)
            {
                string name = original.Key;
                if (scenario != "clean") AssertPreservation(root, name, original.Value);
                var progress = new UserProgress(new CourseLesson(), course, new MauiUserProgressStore(), new MauiSettingsService());
                progress.UserRegistration(name);
                for (int i = 0; i < 3; i++)
                {
                    course.SelectCourse(i);
                    progress.CourseRegistration(course.CourseId());
                    int available = progress.LastAvailableLesson();
                    Assert.IsTrue(available >= 1 && available <= course.TotalLessons());
                    for (int lesson = 1; lesson <= available; lesson++)
                    {
                        course.SelectLesson(lesson);
                        progress.LessonRegistration(course.LessonId());
                        Assert.IsTrue(course.GetExercises().Count > 0);
                        Assert.IsNotNull(course.LessonApresentation());
                        Assert.IsNotNull(course.LessonInstruction());
                    }
                    if (course.CourseId() == "musicas")
                    {
                        Assert.AreEqual(4, course.TotalLessons());
                        Assert.ThrowsExactly<KeyNotFoundException>(() => course.SelectLesson(5));
                        if (original.Value["musicas"]?["ULTIMACONCLUIDA"]?.GetValue<int>() >= 4)
                        {
                            Assert.AreEqual(4, available);
                            Assert.AreEqual("Sim", progress.LessonConcluded());
                        }
                    }
                }
                Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => course.SelectCourse(3));
                var historyLines = new MauiUserHistoryStore().LoadLines(name);
                bool hasRetired = original.Value.ContainsKey("curso_abnt2_basico") || original.Value.ContainsKey("curso_abnt2_intermediario") ||
                    (original.Value["musicas"] as JsonObject)?.Any(p => p.Key.StartsWith("LICAO5.")) == true;
                Assert.AreEqual(hasRetired, historyLines.Count > 0);
            }

            var first = CatalogMigrationTests.Snapshot(root);
            Assert.IsFalse(await migration.EnsureCurrentAsync());
            CollectionAssert.AreEqual(first, CatalogMigrationTests.Snapshot(root), "migration(migration(estado)) deve ser idêntica ao estado após a primeira execução.");
            Assert.AreEqual(AppStartupDestination.Login, await startup.InitializeAsync());
            CollectionAssert.AreEqual(first, CatalogMigrationTests.Snapshot(root), "Novo startup não deve alterar nem duplicar arquivos.");
            course.SelectCourse(0);
            course.SelectLesson(4);
            Assert.IsTrue(course.GetExercises().Count > 0);
        }
        finally { Directory.Delete(root, true); }
    }

    [TestMethod]
    [DataRow("prepared")]
    [DataRow("history:ANA")]
    [DataRow("profile:ANA")]
    [DataRow("catalog:musicas")]
    [DataRow("catalog:musica_se_eu_quiser_falar_com_deus")]
    [DataRow("catalog:SENAI")]
    [DataRow("removed:curso_abnt2_basico")]
    [DataRow("removed:curso_abnt2_intermediario")]
    [DataRow("before-commit")]
    public async Task Every_publication_checkpoint_recovers_without_content_fallback_or_data_loss(string checkpoint)
    {
        string root = Path.Combine(Path.GetTempPath(), "DigitavoxPublicationFailures", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            FileSystem.AppDataDirectory = root;
            FileSystem.AppPackageDirectory = Path.Combine(AppContext.BaseDirectory, "Bundle");
            LegacyCatalogFixture.Write(root, "curso_abnt2_basico", "curso_abnt2_intermediario", "musicas", "musica_se_eu_quiser_falar_com_deus", "SENAI");
            var original = BuildUser("ANA", "multiple-profiles");
            Directory.CreateDirectory(Path.Combine(root, "Users"));
            File.WriteAllText(Path.Combine(root, "Users", "ANA.json"), original.ToJsonString());
            var broken = new CatalogMigration(root, FileSystem.OpenAppPackageFileAsync, point => { if (point == checkpoint) throw new IOException("Interrupção simulada"); });
            await Assert.ThrowsExactlyAsync<IOException>(() => broken.EnsureCurrentAsync());
            Assert.AreEqual(0, broken.ReadVersion());
            Assert.ThrowsExactly<InvalidOperationException>(() => new MauiCourseCatalogStore(broken).Load());
            var retry = new CatalogMigration(root, FileSystem.OpenAppPackageFileAsync);
            await retry.EnsureCurrentAsync();
            AssertPreservation(root, "ANA", original);
            Assert.AreEqual(3, new MauiCourseCatalogStore(retry).Load().Courses.Count);
            var after = CatalogMigrationTests.Snapshot(root);
            await retry.EnsureCurrentAsync();
            CollectionAssert.AreEqual(after, CatalogMigrationTests.Snapshot(root));
        }
        finally { Directory.Delete(root, true); }
    }

    [TestMethod]
    public async Task Completing_the_last_active_song_saves_statistics_without_unlocking_a_retired_lesson()
    {
        string root = Path.Combine(Path.GetTempPath(), "DigitavoxAdvanceAfterUpgrade", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            FileSystem.AppDataDirectory = root;
            FileSystem.AppPackageDirectory = Path.Combine(AppContext.BaseDirectory, "Bundle");
            Preferences.Default.Clear();
            Directory.CreateDirectory(Path.Combine(root, "Users"));
            File.WriteAllText(Path.Combine(root, "Users", "ANA.json"), new JsonObject
            {
                ["NOMEUSUARIO"] = "ANA", ["type"] = "USUARIO", ["musicas"] = new JsonObject { ["ULTIMACONCLUIDA"] = 3 }
            }.ToJsonString());
            var migration = new CatalogMigration(root, FileSystem.OpenAppPackageFileAsync);
            await migration.EnsureCurrentAsync();
            var course = new Course(new MauiCourseCatalogStore(migration));
            course.GetCoursesLists();
            course.SelectCourse(0);
            course.SelectLesson(4);
            var lesson = new CourseLesson();
            var progress = new UserProgress(lesson, course, new MauiUserProgressStore(), new MauiSettingsService());
            progress.UserRegistration("ANA");
            progress.CourseRegistration("musicas");
            progress.LessonRegistration("LICAO4");
            Assert.AreEqual(4, progress.LastAvailableLesson());
            Assert.AreEqual("Não", progress.LessonConcluded());
            var exercises = course.GetExercises();
            lesson.SetLessonChars(exercises, 1, 1, 97, 1, false, false);
            lesson.StartTimer(() => { });
            foreach (string exercise in exercises)
                foreach (char character in exercise + " ") lesson.CharPressed(character.ToString());
            lesson.StopTimer();
            progress.SaveStatistics();
            Assert.AreEqual(4, progress.LastAvailableLesson());
            Assert.AreEqual("Sim", progress.LessonConcluded());
            var saved = JsonNode.Parse(File.ReadAllText(Path.Combine(root, "Users", "ANA.json")));
            Assert.AreEqual(100, saved["musicas"]["LICAO4.1"]["PERCENTUALACERTO"].GetValue<int>());
            Assert.AreEqual("S", saved["musicas"]["LICAO4.1"]["CONCLUIU"].GetValue<string>());
            Assert.AreEqual(4, saved["musicas"]["ULTIMACONCLUIDA"].GetValue<int>());
            course.SelectLesson(progress.LastAvailableLesson());
            Assert.AreEqual("LICAO4", course.LessonId());
            Assert.ThrowsExactly<KeyNotFoundException>(() => course.SelectLesson(5));
            var after = CatalogMigrationTests.Snapshot(root);
            await migration.EnsureCurrentAsync();
            CollectionAssert.AreEqual(after, CatalogMigrationTests.Snapshot(root));
        }
        finally { Directory.Delete(root, true); }
    }

    [TestMethod]
    public async Task Files_corrupted_during_publication_are_not_certified_as_version_one()
    {
        string root = Path.Combine(Path.GetTempPath(), "DigitavoxPublicationIntegrity", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var migration = new CatalogMigration(root, UserProgressMigrationTests.OpenBundle, point =>
            {
                if (point == "before-commit") File.WriteAllText(Path.Combine(root, "Courses", "musicas.json"), "{}");
            });
            await Assert.ThrowsExactlyAsync<InvalidDataException>(() => migration.EnsureCurrentAsync());
            Assert.AreEqual(0, migration.ReadVersion());
            Assert.IsTrue(migration.IsPending);
            await new CatalogMigration(root, UserProgressMigrationTests.OpenBundle).EnsureCurrentAsync();
            Assert.AreEqual(1, migration.ReadVersion());
        }
        finally { Directory.Delete(root, true); }
    }

    [TestMethod]
    public async Task Restored_profiles_and_catalog_are_reconciled_despite_a_version_one_marker()
    {
        string root = Path.Combine(Path.GetTempPath(), "DigitavoxRestoredBackup", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            FileSystem.AppDataDirectory = root;
            FileSystem.AppPackageDirectory = Path.Combine(AppContext.BaseDirectory, "Bundle");
            var migration = new CatalogMigration(root, FileSystem.OpenAppPackageFileAsync);
            await migration.EnsureCurrentAsync();
            var original = BuildUser("ANONIMO", "multiple-profiles");
            Directory.CreateDirectory(Path.Combine(root, "Users"));
            File.WriteAllText(Path.Combine(root, "Users", "ANONIMO.json"), original.ToJsonString());
            LegacyCatalogFixture.Write(root, "curso_abnt2_basico", "musicas");
            Assert.AreEqual(1, migration.ReadVersion());
            await migration.EnsureCurrentAsync();
            AssertPreservation(root, "ANONIMO", original);
            Assert.IsFalse(File.Exists(Path.Combine(root, "Courses", "curso_abnt2_basico.json")));
            var after = CatalogMigrationTests.Snapshot(root);
            await migration.EnsureCurrentAsync();
            CollectionAssert.AreEqual(after, CatalogMigrationTests.Snapshot(root));
        }
        finally { Directory.Delete(root, true); }
    }

    private static void AssertPreservation(string root, string name, JsonObject original)
    {
        var active = JsonNode.Parse(File.ReadAllText(Path.Combine(root, "Users", name + ".json"))) as JsonObject;
        string historyPath = Path.Combine(root, "History", "Users", name + ".json");
        var history = File.Exists(historyPath) ? JsonNode.Parse(File.ReadAllText(historyPath)) : null;
        foreach (string id in new[] { "SENAI", "musica_se_eu_quiser_falar_com_deus" }) Assert.IsTrue(JsonNode.DeepEquals(original[id], active[id]));
        foreach (string id in new[] { "curso_abnt2_basico", "curso_abnt2_intermediario" })
        {
            Assert.IsNull(active[id]);
            if (original[id] is not null) Assert.IsTrue(JsonNode.DeepEquals(original[id], history["courses"][id]));
        }
        if (original["musicas"] is JsonObject music)
        {
            foreach (var attempt in music.Where(p => p.Key.StartsWith("LICAO")))
                Assert.IsTrue(JsonNode.DeepEquals(attempt.Value, attempt.Key.StartsWith("LICAO5.") ? history["courses"]["musicas"][attempt.Key] : active["musicas"][attempt.Key]), attempt.Key);
            int last = music["ULTIMACONCLUIDA"].GetValue<int>();
            Assert.AreEqual(Math.Min(last, 4), active["musicas"]["ULTIMACONCLUIDA"].GetValue<int>());
            if (last == 5) Assert.AreEqual(5, history["courses"]["musicas"]["ULTIMACONCLUIDA"].GetValue<int>());
        }
    }

    private static JsonObject BuildUser(string name, string scenario)
    {
        var user = new JsonObject { ["NOMEUSUARIO"] = name, ["type"] = "USUARIO" };
        if (scenario is "basic-completed" or "multiple-profiles" or "anonymous" or "utf8-bom") user["curso_abnt2_basico"] = UserProgressMigrationTests.CourseProgress(30);
        if (scenario is "intermediate-partial" or "multiple-profiles") user["curso_abnt2_intermediario"] = UserProgressMigrationTests.CourseProgress(6);
        if (scenario is "senai" or "multiple-profiles") user["SENAI"] = UserProgressMigrationTests.CourseProgress(12);
        if (scenario is "individual-song" or "multiple-profiles") user["musica_se_eu_quiser_falar_com_deus"] = UserProgressMigrationTests.CourseProgress(1);
        if (scenario is "music-through-four" or "retired-song-unlocked" or "retired-song-completed" or "retired-song-incomplete" or "multiple-profiles")
        {
            var music = new JsonObject { ["ULTIMACONCLUIDA"] = scenario is "retired-song-completed" or "multiple-profiles" ? 5 : 4 };
            for (int i = 1; i <= 4; i++) music["LICAO" + i + ".1"] = UserProgressMigrationTests.Attempt("S");
            if (scenario is "retired-song-completed" or "retired-song-incomplete" or "multiple-profiles") music["LICAO5.1"] = UserProgressMigrationTests.Attempt(scenario == "retired-song-incomplete" ? "N" : "S");
            if (scenario == "multiple-profiles") music["LICAO5.2"] = UserProgressMigrationTests.Attempt("N");
            user["musicas"] = music;
        }
        return user;
    }

    private sealed class Environment : IAppEnvironment
    {
        public AppPlatformKind Platform => AppPlatformKind.Android;
        public bool IsVirtualDevice => false;
        public bool IsScreenReaderEnabled => false;
        public bool ShouldUseLightForeground => false;
        public void RunOnMainThread(Action action) => action();
    }
}
