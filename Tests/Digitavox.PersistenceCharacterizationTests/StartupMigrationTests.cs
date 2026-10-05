using Digitavox.Core.Abstractions;
using Digitavox.Infrastructure.Persistence;
using Digitavox.Infrastructure.Startup;
using Digitavox.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Digitavox.PersistenceCharacterizationTests;

[TestClass]
public class StartupMigrationTests
{
    private string root;

    [TestInitialize]
    public void SetUp()
    {
        root = Path.Combine(Path.GetTempPath(), "DigitavoxStartupTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        FileSystem.AppDataDirectory = root;
        FileSystem.AppPackageDirectory = Path.Combine(AppContext.BaseDirectory, "Bundle");
    }
    [TestCleanup]
    public void TearDown() => Directory.Delete(root, true);

    private (MauiAppStartupService Startup, Course Course, CatalogMigration Migration) Create(Action<string> checkpoint = null)
    {
        var migration = new CatalogMigration(root, FileSystem.OpenAppPackageFileAsync, checkpoint);
        var course = new Course(new MauiCourseCatalogStore(migration));
        return (new MauiAppStartupService(new FingerMapping(new Environment()), migration, course), course, migration);
    }

    [TestMethod]
    public async Task Clean_startup_publishes_before_runtime_and_opens_tutorial_only_once()
    {
        var (startup, course, migration) = Create();
        Assert.ThrowsExactly<InvalidOperationException>(() => course.GetCoursesLists());
        Assert.AreEqual(AppStartupDestination.Tutorial, await startup.InitializeAsync());
        course.GetCoursesLists();
        Assert.AreEqual(3, course.CourseNameList().Count);
        var before = CatalogMigrationTests.Snapshot(root);
        Assert.AreEqual(AppStartupDestination.Login, await startup.InitializeAsync());
        CollectionAssert.AreEqual(before, CatalogMigrationTests.Snapshot(root));
        Assert.AreEqual(3, course.CourseNameList().Count);
        Assert.ThrowsExactly<InvalidOperationException>(() => course.CourseApresentation(), "A seleção anterior deve ser invalidada.");
        Assert.AreEqual(1, migration.ReadVersion());
    }

    [TestMethod]
    [DataRow("empty")]
    [DataRow("partial")]
    [DataRow("legacy")]
    [DataRow("missing-active-file")]
    public async Task Existing_or_incomplete_courses_directory_does_not_skip_migration(string kind)
    {
        Directory.CreateDirectory(Path.Combine(root, "Courses"));
        if (kind == "partial") LegacyCatalogFixture.Write(root, "curso_abnt2_basico");
        if (kind == "legacy") LegacyCatalogFixture.Write(root, "curso_abnt2_basico", "curso_abnt2_intermediario", "musicas", "musica_se_eu_quiser_falar_com_deus", "SENAI");
        if (kind == "missing-active-file")
        {
            await Create().Startup.InitializeAsync();
            File.Delete(Path.Combine(root, "Courses", "musicas.json"));
        }
        var (startup, course, migration) = Create();
        Assert.AreEqual(AppStartupDestination.Login, await startup.InitializeAsync());
        Assert.AreEqual(1, migration.ReadVersion());
        course.GetCoursesLists();
        Assert.AreEqual(3, course.CourseNameList().Count);
        course.SelectCourse(0);
        Assert.AreEqual("musicas", course.CourseId());
        Assert.AreEqual(4, course.TotalLessons());
        Assert.ThrowsExactly<KeyNotFoundException>(() => course.SelectLesson(5));
        Assert.IsFalse(File.Exists(Path.Combine(root, "Courses", "curso_abnt2_basico.json")));
        Assert.IsFalse(File.Exists(Path.Combine(root, "Courses", "curso_abnt2_intermediario.json")));
    }

    [TestMethod]
    public async Task Failure_blocks_runtime_and_retry_preserves_the_first_launch_destination()
    {
        var interrupted = Create(point => { if (point == "catalog:musicas") throw new IOException("Interrupção"); });
        await Assert.ThrowsExactlyAsync<IOException>(() => interrupted.Startup.InitializeAsync());
        Assert.ThrowsExactly<InvalidOperationException>(() => interrupted.Course.GetCoursesLists());
        var retry = Create();
        Assert.AreEqual(AppStartupDestination.Tutorial, await retry.Startup.InitializeAsync());
        retry.Course.GetCoursesLists();
        Assert.AreEqual(3, retry.Course.CourseNameList().Count);
    }

    [TestMethod]
    public async Task Unknown_local_files_cannot_become_executable_courses()
    {
        var (startup, course, _) = Create();
        await startup.InitializeAsync();
        File.WriteAllText(Path.Combine(root, "Courses", "arbitrary.json"), "{invalid unknown file}");
        course.ResetCatalog();
        course.GetCoursesLists();
        Assert.AreEqual(3, course.CourseNameList().Count);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => course.SelectCourse(3));
    }

    [TestMethod]
    public async Task Restored_legacy_music_under_a_current_marker_is_not_loaded()
    {
        var (startup, course, _) = Create();
        await startup.InitializeAsync();
        LegacyCatalogFixture.Write(root, "musicas");
        course.ResetCatalog();
        Assert.ThrowsExactly<InvalidDataException>(() => course.GetCoursesLists());
        await startup.InitializeAsync();
        course.GetCoursesLists();
        course.SelectCourse(0);
        Assert.AreEqual(4, course.TotalLessons());
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
