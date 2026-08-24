using Digitavox.Helpers;
using Digitavox.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Digitavox.CharacterizationTests;

[TestClass]
public class UserProgressCharacterizationTests
{
    [TestInitialize]
    public void SetUp()
    {
        DVPersistence.Reset();
        DVPersistence.SetSetting("timeDivider", 2);
    }

    [TestMethod]
    public void New_user_is_normalized_and_becomes_registered_after_first_save()
    {
        Course course = TestCourseFactory.CreateSelectedCourse();
        UserProgress progress = new(new CourseLesson(), course);

        progress.UserRegistration("ana");

        Assert.IsTrue(progress.UserLogged());
        Assert.IsTrue(progress.FirstLogin());
        Assert.AreEqual("ANA", progress.GetUserName());

        progress.NewProgress();

        Assert.IsFalse(progress.FirstLogin());
        Assert.AreEqual("USUARIO", DVPersistence.LastSavedUser["type"]);
        Assert.AreEqual(1, DVPersistence.SaveUserCalls);
    }

    [TestMethod]
    public void Existing_user_resumes_at_the_lesson_after_the_last_completed_one()
    {
        Course course = TestCourseFactory.CreateSelectedCourse(totalLessons: 3);
        Dictionary<string, object> courseProgress = new()
        {
            ["ULTIMACONCLUIDA"] = 1
        };
        DVPersistence.SeedUser("bia", new Dictionary<string, object>
        {
            ["NOMEUSUARIO"] = "BIA",
            ["type"] = "USUARIO",
            [course.CourseId()] = courseProgress
        });
        UserProgress progress = new(new CourseLesson(), course);

        progress.UserRegistration("bia");
        progress.CourseRegistration(course.CourseId());

        Assert.IsFalse(progress.FirstLogin());
        Assert.AreEqual(2, progress.LastAvailableLesson());
    }

    [TestMethod]
    public void Completed_exercise_is_saved_with_statistics_and_unlocks_next_lesson()
    {
        Course course = TestCourseFactory.CreateSelectedCourse(totalLessons: 2);
        CourseLesson lesson = new();
        lesson.SetLessonChars(
            new List<string> { "a" },
            exerciseRepetitions: 1,
            secondsPerChar: 10,
            targetPercent: 100,
            timeDivider: 2,
            spellingActive: false,
            ignoreLetterCase: false);
        lesson.StartTimer(() => { });
        lesson.CharPressed("a");
        lesson.CharPressed(" ");
        lesson.StopTimer();

        UserProgress progress = new(lesson, course);
        progress.UserRegistration("caio");
        progress.NewProgress();
        progress.CourseRegistration(course.CourseId());
        progress.LessonRegistration("LICAO1");

        progress.SaveStatistics();

        Dictionary<string, object> savedCourse =
            (Dictionary<string, object>)DVPersistence.LastSavedUser[course.CourseId()];
        Dictionary<string, object> savedLesson =
            (Dictionary<string, object>)savedCourse["LICAO1.1"];

        Assert.AreEqual("S", savedLesson["CONCLUIU"]);
        Assert.AreEqual(100, savedLesson["PERCENTUALACERTO"]);
        Assert.AreEqual(2, savedLesson["TOTALLETRASLICAO"]);
        Assert.AreEqual(2, savedLesson["NLETRASACERTOU"]);
        Assert.AreEqual(2, savedLesson["DIVISORTEMPO"]);
        Assert.AreEqual(1, savedCourse["ULTIMACONCLUIDA"]);
        Assert.AreEqual(2, progress.LastAvailableLesson());
        Assert.AreEqual("Sim", progress.LessonConcluded());
    }
}
