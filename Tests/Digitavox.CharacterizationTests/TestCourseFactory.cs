using System.Text.Json;
using Digitavox.Helpers;
using Digitavox.Models;
using Digitavox.Core.Abstractions;

namespace Digitavox.CharacterizationTests;

internal static class TestCourseFactory
{
    public static int CourseCatalogLoadCalls { get; private set; }

    public static Course CreateSelectedCourse(int totalLessons = 2)
    {
        CourseCatalogLoadCalls = 0;
        string json = $$"""
        {
          "CURSO": {
            "Present": { "APT": "Apresentação do curso" },
            "Instruction": { "IST": "Instrução do curso" },
            "NOMECURSO": "Curso de caracterização",
            "QUANTIDADELICOES": {{totalLessons}}
          },
          "LICAO1": {
            "Present": { "APT": "Apresentação da lição" },
            "Instruction": { "IST": "Instrução da lição" },
            "EXER1": "abc",
            "IGNORAR": "não é exercício",
            "EXER2": "de f",
            "SOLETRAEXER": "não",
            "REPETICOESEXER": 1,
            "TEMPOPORCARACTER": 10,
            "MEDIAEXER": 80,
            "TUDO_EM_MAIUSCULO": "não"
          }
        }
        """;

        JsonElement root = JsonDocument.Parse(json).RootElement.Clone();
        CourseCatalogData catalog = new(
            new List<JsonElement> { root },
            new List<string> { "curso-teste.json" },
            new List<string> { "Curso de caracterização" });

        Course course = new(new TestCourseCatalogStore(catalog));
        course.GetCoursesLists();
        course.SelectCourse(0);
        return course;
    }

    private sealed class TestCourseCatalogStore : ICourseCatalogStore
    {
        private readonly CourseCatalogData catalog;

        public TestCourseCatalogStore(CourseCatalogData catalog)
        {
            this.catalog = catalog;
        }

        public CourseCatalogData Load()
        {
            CourseCatalogLoadCalls++;
            return catalog;
        }
    }
}
