using System.Text.Json;
using Digitavox.Core.Abstractions;
using Digitavox.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Digitavox.CharacterizationTests;

[TestClass]
public class CourseContentAvailabilityTests
{
    [TestMethod]
    public void Retired_course_ids_are_never_exposed_even_by_a_legacy_store()
    {
        var store = new LegacyStore();
        var course = new Course(store);
        course.GetCoursesLists();
        CollectionAssert.AreEqual(new[] { "Músicas" }, course.CourseNameList());
        course.SelectCourse(0);
        Assert.AreEqual("musicas", course.CourseId());
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => course.SelectCourse(1));
    }

    [TestMethod]
    public void Retired_lesson_cannot_be_selected_read_or_executed_from_legacy_json()
    {
        var course = new Course(new LegacyStore());
        course.GetCoursesLists();
        course.SelectCourse(0);
        Assert.AreEqual(4, course.TotalLessons());
        Assert.ThrowsExactly<KeyNotFoundException>(() => course.SelectLesson(5));
        Assert.ThrowsExactly<InvalidOperationException>(() => course.GetExercises());
        Assert.ThrowsExactly<InvalidOperationException>(() => course.LessonApresentation());
        Assert.ThrowsExactly<InvalidOperationException>(() => course.LessonInstruction());
        course.SelectLesson(1);
        CollectionAssert.AreEqual(new[] { "ativa" }, course.GetExercises());
    }

    [TestMethod]
    public void Reset_invalidates_json_selection_and_reloads_catalog()
    {
        var store = new LegacyStore();
        var course = new Course(store);
        course.GetCoursesLists();
        course.SelectCourse(0);
        course.SelectLesson(1);
        course.ResetCatalog();
        Assert.ThrowsExactly<InvalidOperationException>(() => course.GetExercises());
        Assert.ThrowsExactly<InvalidOperationException>(() => course.CourseApresentation());
        course.GetCoursesLists();
        Assert.AreEqual(2, store.Loads);
    }

    private sealed class LegacyStore : ICourseCatalogStore
    {
        public int Loads { get; private set; }
        public CourseCatalogData Load()
        {
            Loads++;
            using var document = JsonDocument.Parse("""
                {"CURSO":{"NOMECURSO":"Músicas","QUANTIDADELICOES":5,"Present":{"APT":"Curso"},"Instruction":{"IST":"Digite"}},
                "LICAO1":{"EXER1":"ativa","Present":{"APT":"Lição ativa"},"Instruction":{"IST":"Digite"}},
                "LICAO5":{"EXER1":"retirada","Present":{"APT":"Apresentação retirada"},"Instruction":{"IST":"Instrução retirada"}}}
                """);
            var root = document.RootElement.Clone();
            return new(new() { root, root, root }, new() { "curso_abnt2_basico.json", "curso_abnt2_intermediario.json", "musicas.json" }, new() { "Básico", "Intermediário", "Músicas" });
        }
    }
}
