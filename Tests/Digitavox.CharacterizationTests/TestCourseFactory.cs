using System.Text.Json;
using Digitavox.Helpers;
using Digitavox.Models;

namespace Digitavox.CharacterizationTests;

internal static class TestCourseFactory
{
    public static Course CreateSelectedCourse(int totalLessons = 2)
    {
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
        DVPersistence.CourseFiles = (
            new List<JsonElement> { root },
            new List<string> { "curso-teste.json" },
            new List<string> { "Curso de caracterização" });

        Course course = new();
        course.GetCoursesLists();
        course.SelectCourse(0);
        return course;
    }
}
