using System.Text.Json.Nodes;

namespace Digitavox.Domain.Catalog;

/// <summary>History is presented independently of executable course/lesson objects.</summary>
public static class ArchivedProgressReader
{
    private static readonly (string Key, string Label)[] Statistics =
    {
        ("DATAINICIO", "Início"), ("DATAFIM", "Fim"), ("CONCLUIU", "Concluiu (S/N)"),
        ("PERCENTUALACERTO", "Percentual de acertos"), ("LETRASPORMINUTO", "Letras por minuto"),
        ("TOTALLETRASLICAO", "Caracteres da lição"), ("NLETRASACERTOU", "Caracteres corretos"),
        ("NLETRASERROU", "Caracteres incorretos"), ("PRATICALETRASLICAO", "Caracteres digitados"),
        ("PERCENTUALPALAVRASCORRETAS", "Percentual de palavras corretas"), ("TOTALPALAVRASLICAO", "Palavras da lição"),
        ("NPALAVRASACERTOU", "Palavras corretas"), ("NPALAVRASERROU", "Palavras incorretas"),
        ("DIVISORTEMPO", "Divisor de tempo"), ("REPETICOESEXER", "Repetições"),
        ("TEMPOTOTALLICAO", "Tempo total (segundos)"), ("TEMPOPRATICALICAO", "Tempo de prática (segundos)"),
        ("PERCENTUALTEMPO", "Percentual de tempo")
    };

    public static IReadOnlyList<string> Read(JsonObject history)
    {
        var lines = new List<string>();
        if (history?["catalogVersion"] is not JsonValue version || !version.TryGetValue<int>(out int number) || number != 1 || history["courses"] is not JsonObject courses)
            throw new InvalidDataException("Histórico incompatível.");
        foreach (var course in courses.OrderBy(c => c.Key, StringComparer.Ordinal))
        {
            string title = course.Key switch
            {
                "curso_abnt2_basico" => "Curso básico",
                "curso_abnt2_intermediario" => "Curso intermediário",
                "musicas" => "Músicas: lição 5 retirada",
                _ => course.Key
            };
            if (course.Value is not JsonObject records) throw new InvalidDataException("Histórico inválido.");
            lines.Add(title + " — conteúdo indisponível para execução");
            if (records["ULTIMACONCLUIDA"] is JsonValue frontier)
                lines.Add("Última lição concluída registrada antes da retirada: " + frontier.ToString());
            foreach (var record in records.Where(r => r.Key.StartsWith("LICAO", StringComparison.Ordinal)))
            {
                if (record.Value is not JsonObject attempt) throw new InvalidDataException("Tentativa histórica inválida.");
                lines.Add("Registro " + record.Key);
                foreach (var statistic in Statistics)
                    if (attempt[statistic.Key] is JsonValue value) lines.Add(statistic.Label + ": " + value.ToString());
                foreach (var error in attempt.Where(e => e.Key.StartsWith("ERROSEM_", StringComparison.Ordinal)))
                    lines.Add("Erros no caractere " + error.Key.Substring("ERROSEM_".Length) + ": " + error.Value?.ToString());
            }
        }
        return lines;
    }
}
