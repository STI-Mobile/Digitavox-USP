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
            "musica_se_eu_quiser_falar_com_deus",
            "musicas",
            "SENAI"
        };
        FileSystem.AppPackageDirectory = Path.Combine(AppContext.BaseDirectory, "Bundle");

        await DVPersistence.CopyCourseFilesAsync();

        string courseDirectory = Path.Combine(testDirectory, "Courses");
        CollectionAssert.AreEquivalent(
            bundledCourses.Select(course => $"{course}.json").ToArray(),
            Directory.GetFiles(courseDirectory).Select(Path.GetFileName).ToArray());
    }

    [TestMethod]
    [DataRow("1,2,3,4,5")]
    [DataRow("3,4,5")]
    public void Numbered_catalog_accepts_gaps_without_renumbering(string numbers)
    {
        string directory = Path.Combine(testDirectory, "Courses");
        Directory.CreateDirectory(directory);
        int[] expected = numbers.Split(',').Select(int.Parse).ToArray();
        foreach (int number in expected.Reverse()) WriteCourse(directory, $"course-{number}.json", $"Curso {number}", number);

        var (json, files, names) = DVPersistence.ReadCourseFiles();

        CollectionAssert.AreEqual(expected, json.Select(c => c.GetProperty("CURSO").GetProperty("NUMEROCURSO").GetInt32()).ToArray());
        CollectionAssert.AreEqual(expected.Select(n => $"course-{n}.json").ToArray(), files);
        CollectionAssert.AreEqual(expected.Select(n => $"Curso {n}").ToArray(), names);
    }

    [TestMethod]
    public void Duplicate_numbers_are_rejected_instead_of_overwriting_a_course()
    {
        string directory = Path.Combine(testDirectory, "Courses");
        Directory.CreateDirectory(directory);
        WriteCourse(directory, "first.json", "Primeiro", 3);
        WriteCourse(directory, "duplicate.json", "Duplicado", 3);
        Assert.ThrowsExactly<InvalidDataException>(() => DVPersistence.ReadCourseFiles());
    }

    [TestMethod]
    [DataRow("{")]
    [DataRow("{\"type\":\"CURSO\"}")]
    [DataRow("{\"CURSO\":{\"NOMECURSO\":\"Incompleto\"}}")]
    [DataRow("[]")]
    public void Malformed_catalogs_are_rejected_with_diagnostic(string json)
    {
        string directory = Path.Combine(testDirectory, "Courses");
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, "broken.json"), json);
        var exception = Assert.ThrowsExactly<InvalidDataException>(() => DVPersistence.ReadCourseFiles());
        StringAssert.Contains(exception.Message, "broken.json");
    }

    [TestMethod]
    public void Auxiliary_files_are_not_loaded_as_courses()
    {
        string directory = Path.Combine(testDirectory, "Courses");
        Directory.CreateDirectory(directory);
        WriteCourse(directory, "real.json", "Real", 3);
        File.WriteAllText(Path.Combine(directory, "state.json"), "{\"catalogVersion\":1}");
        File.WriteAllText(Path.Combine(directory, "real.json.tmp"), "invalid temporary file");
        File.WriteAllText(Path.Combine(directory, "backup.txt"), "invalid text");
        CollectionAssert.AreEqual(new[] { "real.json" }, DVPersistence.ReadCourseFiles().Item2);
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
        string json = $$$"""
            {"type":"CURSO", "CURSO": {"NOMECURSO":"{{{courseName}}}"{{{numberProperty}}}, "QUANTIDADELICOES":1,
            "Present":{"APT":"Curso"},"Instruction":{"IST":"Digite"}},
            "LICAO1":{"Present":{"APT":"Lição"},"Instruction":{"IST":"Digite"},"EXER1":"abc",
            "REPETICOESEXER":1,"TEMPOPORCARACTER":1,"MEDIAEXER":90,"SOLETRAEXER":"nao","TUDO_EM_MAIUSCULO":"nao"}}
            """;
        File.WriteAllText(Path.Combine(directory, fileName), json);
    }
}
