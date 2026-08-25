using Digitavox.Helpers;
using Digitavox.PlatformsImplementations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Digitavox.PersistenceCharacterizationTests;

[TestClass]
public class DVPersistenceCharacterizationTests
{
    private string testDirectory;

    [TestInitialize]
    public void SetUp()
    {
        testDirectory = Path.Combine(
            Path.GetTempPath(),
            "DigitavoxPersistenceCharacterization",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(testDirectory);
        FileSystem.AppDataDirectory = testDirectory;
        FileSystem.AppPackageDirectory = Path.Combine(testDirectory, "Package");
        Directory.CreateDirectory(FileSystem.AppPackageDirectory);
        Preferences.Default.Clear();
        DVSpeak.GetInstance().Reset();
    }

    [TestCleanup]
    public void TearDown()
    {
        if (Directory.Exists(testDirectory))
        {
            Directory.Delete(testDirectory, recursive: true);
        }
    }

    [TestMethod]
    public void Missing_preferences_return_the_current_defaults()
    {
        Assert.AreEqual(1, DVPersistence.Get<int>("speakRate"));
        Assert.IsTrue(DVPersistence.Get<bool>("speakInput"));
        Assert.AreEqual(14d, DVPersistence.Get<double>("fontSize"));
        Assert.AreEqual(1, DVPersistence.Get<int>("timeDivider"));
        Assert.IsTrue(DVPersistence.Get<bool>("countRepetitions"));
        Assert.IsTrue(DVPersistence.Get<bool>("instructionsEnabled"));
        Assert.IsFalse(DVPersistence.Get<bool>("VoiceOverStatus"));
    }

    [TestMethod]
    public void Reset_configuration_restores_all_values_and_applies_speech_rate()
    {
        DVPersistence.Set("speakRate", 8);
        DVPersistence.Set("speakInput", false);
        DVPersistence.Set("fontSize", 28d);
        DVPersistence.Set("timeDivider", 5);
        DVPersistence.Set("countRepetitions", false);
        DVPersistence.Set("instructionsEnabled", false);

        DVPersistence.SetDefaulConfig();

        Assert.AreEqual(1, DVPersistence.Get<int>("speakRate"));
        Assert.IsTrue(DVPersistence.Get<bool>("speakInput"));
        Assert.AreEqual(14d, DVPersistence.Get<double>("fontSize"));
        Assert.AreEqual(1, DVPersistence.Get<int>("timeDivider"));
        Assert.IsTrue(DVPersistence.Get<bool>("countRepetitions"));
        Assert.IsTrue(DVPersistence.Get<bool>("instructionsEnabled"));
        Assert.AreEqual(1, DVSpeak.GetInstance().SpeechRate);
    }

    [TestMethod]
    public void User_progress_round_trips_through_the_current_json_shape()
    {
        Dictionary<string, object> lesson = new()
        {
            ["DATAFIM"] = "24/08/2026 17:00:00",
            ["CONCLUIU"] = "S",
            ["PERCENTUALACERTO"] = 87,
            ["PERCENTUALTEMPO"] = 42.5f
        };
        Dictionary<string, object> course = new()
        {
            ["ULTIMACONCLUIDA"] = 1,
            ["LICAO1.1"] = lesson
        };
        Dictionary<string, object> user = new()
        {
            ["NOMEUSUARIO"] = "ANA",
            ["type"] = "USUARIO",
            ["curso-teste"] = course
        };

        DVPersistence.SaveUserJson(user);
        Dictionary<string, object> loaded = DVPersistence.LoadUserData("ana");

        Assert.AreEqual("ANA", loaded["NOMEUSUARIO"]);
        Assert.AreEqual("USUARIO", loaded["type"]);
        Dictionary<string, object> loadedCourse =
            (Dictionary<string, object>)loaded["curso-teste"];
        Dictionary<string, object> loadedLesson =
            (Dictionary<string, object>)loadedCourse["LICAO1.1"];
        Assert.AreEqual(1, loadedCourse["ULTIMACONCLUIDA"]);
        Assert.AreEqual("S", loadedLesson["CONCLUIU"]);
        Assert.AreEqual(87, loadedLesson["PERCENTUALACERTO"]);
        Assert.AreEqual(42.5f, loadedLesson["PERCENTUALTEMPO"]);
    }

    [TestMethod]
    public void Course_files_are_sorted_by_declared_number_then_append_unnumbered_courses()
    {
        string courseDirectory = Path.Combine(testDirectory, "Courses");
        Directory.CreateDirectory(courseDirectory);
        WriteCourse(courseDirectory, "second.json", "Segundo", 2);
        WriteCourse(courseDirectory, "first.json", "Primeiro", 1);
        WriteCourse(courseDirectory, "extra.json", "Adicional", null);

        (List<System.Text.Json.JsonElement> json, List<string> files, List<string> names) =
            DVPersistence.ReadCourseFiles();

        CollectionAssert.AreEqual(
            new[] { "first.json", "second.json", "extra.json" },
            files);
        CollectionAssert.AreEqual(
            new[] { "Primeiro", "Segundo", "Adicional" },
            names);
        Assert.AreEqual(3, json.Count);
    }

    [TestMethod]
    public void Course_directory_existence_tracks_the_courses_folder()
    {
        Assert.IsFalse(DVPersistence.CourseDirectoryExists());

        Directory.CreateDirectory(Path.Combine(testDirectory, "Courses"));

        Assert.IsTrue(DVPersistence.CourseDirectoryExists());
    }

    [TestMethod]
    public async Task Copy_course_files_completes_only_after_all_assets_are_persisted()
    {
        string[] bundledCourses =
        {
            "curso_abnt2_basico",
            "curso_abnt2_intermediario",
            "musica_se_eu_quiser_falar_com_deus",
            "musicas",
            "SENAI"
        };
        foreach (string course in bundledCourses)
        {
            File.WriteAllText(
                Path.Combine(FileSystem.AppPackageDirectory, $"{course}.json"),
                course);
        }

        await DVPersistence.CopyCourseFilesAsync();

        string courseDirectory = Path.Combine(testDirectory, "Courses");
        CollectionAssert.AreEquivalent(
            bundledCourses.Select(course => $"{course}.json").ToArray(),
            Directory.GetFiles(courseDirectory).Select(Path.GetFileName).ToArray());
    }

    private static void WriteCourse(
        string directory,
        string fileName,
        string courseName,
        int? courseNumber)
    {
        string numberProperty = courseNumber.HasValue
            ? $", \"NUMEROCURSO\": {courseNumber.Value}"
            : string.Empty;
        string json =
            $"{{ \"CURSO\": {{ \"NOMECURSO\": \"{courseName}\"{numberProperty} }} }}";
        File.WriteAllText(Path.Combine(directory, fileName), json);
    }
}
