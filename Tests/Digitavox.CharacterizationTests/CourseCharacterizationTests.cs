using Digitavox.Helpers;
using Digitavox.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Digitavox.CharacterizationTests;

[TestClass]
public class CourseCharacterizationTests
{
    [TestMethod]
    [DataRow(-1)]
    [DataRow(1)]
    public void Missing_course_selection_does_not_change_current_course(int index)
    {
        Course course = TestCourseFactory.CreateSelectedCourse();
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => course.SelectCourse(index));
        Assert.AreEqual("curso-teste", course.CourseId());
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(2)]
    [DataRow(3)]
    public void Missing_lesson_selection_does_not_change_current_lesson(int number)
    {
        Course course = TestCourseFactory.CreateSelectedCourse();
        course.SelectLesson(1);
        Assert.ThrowsExactly<KeyNotFoundException>(() => course.SelectLesson(number));
        Assert.AreEqual("LICAO1", course.LessonId());
        CollectionAssert.AreEqual(new[] { "abc", "de f" }, course.GetExercises());
    }

    [TestInitialize]
    public void SetUp()
    {
        DVPersistence.Reset();
    }

    [TestMethod]
    public void Course_data_is_loaded_once_and_selected_by_index()
    {
        Course course = TestCourseFactory.CreateSelectedCourse();

        course.GetCoursesLists();

        Assert.AreEqual(1, TestCourseFactory.CourseCatalogLoadCalls);
        Assert.AreEqual(1, course.CourseNumber());
        Assert.AreEqual("curso-teste", course.CourseId());
        Assert.AreEqual("Curso de caracterização", course.CourseNameList().Single());
        Assert.AreEqual("Apresentação do curso", course.CourseApresentation());
        Assert.AreEqual("Instrução do curso", course.CourseInstruction());
        Assert.AreEqual(2, course.TotalLessons());
    }

    [TestMethod]
    public void Lesson_selection_exposes_properties_and_ordered_exercises()
    {
        Course course = TestCourseFactory.CreateSelectedCourse();

        course.SelectLesson(1);

        Assert.AreEqual("LICAO1", course.LessonId());
        Assert.AreEqual(1, course.LessonNumber());
        Assert.AreEqual("Apresentação da lição", course.LessonApresentation());
        Assert.AreEqual("Instrução da lição", course.LessonInstruction());
        CollectionAssert.AreEqual(new[] { "abc", "de f" }, course.GetExercises());
    }
}
