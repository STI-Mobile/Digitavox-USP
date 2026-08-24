using Digitavox.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Digitavox.CharacterizationTests;

[TestClass]
public class CourseLessonCharacterizationTests
{
    [TestMethod]
    public void Setup_calculates_total_characters_words_and_allowed_time()
    {
        CourseLesson lesson = new();

        lesson.SetLessonChars(
            new List<string> { "ab cd" },
            exerciseRepetitions: 2,
            secondsPerChar: 10,
            targetPercent: 75,
            timeDivider: 2,
            spellingActive: false,
            ignoreLetterCase: false);

        Assert.AreEqual(12, lesson.TotalChars());
        Assert.AreEqual(4, lesson.GetStatistics().totalWords);
        Assert.AreEqual(60, lesson.GetStatistics().totalTime);
        Assert.AreEqual("1 minuto ", lesson.TotalTime());
    }

    [TestMethod]
    public void Correct_and_incorrect_input_update_character_word_and_error_statistics()
    {
        CourseLesson lesson = new();
        lesson.SetLessonChars(
            new List<string> { "ab" },
            exerciseRepetitions: 1,
            secondsPerChar: 10,
            targetPercent: 50,
            timeDivider: 1,
            spellingActive: false,
            ignoreLetterCase: false);
        lesson.StartTimer(() => { });

        lesson.CharPressed("a");
        lesson.CharPressed("x");
        lesson.CharPressed(" ");
        lesson.StopTimer();

        CourseLessonBean statistics = lesson.GetStatistics();
        Assert.AreEqual(2, statistics.correctChars);
        Assert.AreEqual(1, statistics.incorrectChars);
        Assert.AreEqual(0, statistics.correctWords);
        Assert.AreEqual(1, statistics.incorrectWords);
        Assert.AreEqual(1, statistics.errorsByChar["b"]);
        Assert.AreEqual(66, lesson.CorrectPercent());
        Assert.AreEqual(66, lesson.TotalCorrectPercent());
        Assert.IsTrue(statistics.concluded);
    }

    [TestMethod]
    public void Ignore_case_accepts_uppercase_input_for_lowercase_exercise()
    {
        CourseLesson lesson = new();
        lesson.SetLessonChars(
            new List<string> { "a" },
            exerciseRepetitions: 1,
            secondsPerChar: 10,
            targetPercent: 100,
            timeDivider: 1,
            spellingActive: true,
            ignoreLetterCase: true);
        lesson.StartTimer(() => { });

        lesson.CharPressed("A");
        lesson.CharPressed(" ");
        lesson.StopTimer();

        Assert.AreEqual(2, lesson.GetStatistics().correctChars);
        Assert.AreEqual(0, lesson.GetStatistics().incorrectChars);
        Assert.AreEqual(100, lesson.TotalCorrectPercent());
        Assert.IsTrue(lesson.GetStatistics().concluded);
    }

    [TestMethod]
    [DataRow(0, "0")]
    [DataRow(1, "1 segundo")]
    [DataRow(60, "1 minuto ")]
    [DataRow(61, "1 minuto 1 segundos")]
    [DataRow(3600, "1 hora ")]
    [DataRow(3661, "1 hora 1 minutos 1 segundos")]
    [DataRow(7200, "2 horas ")]
    public void Time_display_preserves_current_portuguese_output(int seconds, string expected)
    {
        CourseLesson lesson = new();

        Assert.AreEqual(expected, lesson.TimeDisplay(seconds));
    }
}
