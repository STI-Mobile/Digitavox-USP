using System.Text.Json;

namespace Digitavox.Domain.Catalog;

public static class CourseCatalogValidator
{
    public static void Validate(JsonElement root, string fileName)
    {
        void Require(bool condition, string field)
        {
            if (!condition) throw new InvalidDataException($"Catálogo inválido em {fileName}: {field}.");
        }

        Require(root.ValueKind == JsonValueKind.Object, "objeto raiz");
        Require(root.TryGetProperty("CURSO", out var course) && course.ValueKind == JsonValueKind.Object, "CURSO");
        Require(HasText(course, "NOMECURSO"), "NOMECURSO");
        Require(HasText(course, "Present", "APT") && HasText(course, "Instruction", "IST"), "apresentação/instrução do curso");
        Require(TryInteger(course, "QUANTIDADELICOES", out int count) && count > 0, "QUANTIDADELICOES");
        if (course.TryGetProperty("NUMEROCURSO", out _))
            Require(TryInteger(course, "NUMEROCURSO", out int number) && number > 0, "NUMEROCURSO");

        Require(root.EnumerateObject().Count(p => p.Name.StartsWith("LICAO", StringComparison.Ordinal)) == count, "quantidade de lições");
        for (int i = 1; i <= count; i++)
        {
            Require(root.TryGetProperty($"LICAO{i}", out var lesson) && lesson.ValueKind == JsonValueKind.Object, $"LICAO{i}");
            Require(HasText(lesson, "Present", "APT") && HasText(lesson, "Instruction", "IST"), $"LICAO{i}: apresentação/instrução");
            var exercises = lesson.EnumerateObject().Where(p => p.Name.StartsWith("EXER", StringComparison.Ordinal)).ToList();
            Require(exercises.Count > 0 && exercises.All(p => p.Value.ValueKind == JsonValueKind.String && p.Value.GetString().Length > 0), $"LICAO{i}: exercícios");
            Require(TryInteger(lesson, "REPETICOESEXER", out int repetitions) && repetitions > 0, $"LICAO{i}: REPETICOESEXER");
            Require(TryInteger(lesson, "TEMPOPORCARACTER", out int seconds) && seconds > 0, $"LICAO{i}: TEMPOPORCARACTER");
            Require(TryInteger(lesson, "MEDIAEXER", out int target) && target >= 0 && target <= 100, $"LICAO{i}: MEDIAEXER");
            Require(HasText(lesson, "SOLETRAEXER") && HasText(lesson, "TUDO_EM_MAIUSCULO"), $"LICAO{i}: parâmetros");
        }
    }

    public static bool TryInteger(JsonElement parent, string key, out int value)
    {
        value = 0;
        return parent.TryGetProperty(key, out var property) && int.TryParse(property.ToString(), out value);
    }

    private static bool HasText(JsonElement parent, string key) =>
        parent.TryGetProperty(key, out var value) && value.ValueKind == JsonValueKind.String;

    private static bool HasText(JsonElement parent, string key, string child) =>
        parent.TryGetProperty(key, out var value) && value.ValueKind == JsonValueKind.Object && HasText(value, child);
}
