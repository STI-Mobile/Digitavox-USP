using System.Text.Json.Nodes;

namespace Digitavox.Domain.Catalog;

public sealed record MigratedUserProgress(JsonObject Progress, JsonObject History, bool Changed);

/// <summary>Pure, lossless transformation of legacy identities. History contains statistics, never catalog payload.</summary>
public static class UserProgressMigration
{
    public static MigratedUserProgress ToVersionOne(JsonObject original, JsonObject existingHistory = null)
    {
        if (original["NOMEUSUARIO"] is not JsonValue name || !name.TryGetValue<string>(out _))
            throw new InvalidDataException("Perfil sem NOMEUSUARIO válido; nenhum progresso foi resetado.");
        var progress = (JsonObject)original.DeepClone();
        var history = existingHistory is null
            ? new JsonObject { ["catalogVersion"] = 1, ["courses"] = new JsonObject() }
            : (JsonObject)existingHistory.DeepClone();
        if (history["catalogVersion"] is not JsonValue version || !version.TryGetValue<int>(out int number) || number != 1 || history["courses"] is not JsonObject courses)
            throw new InvalidDataException("Histórico incompatível; recuperação necessária.");

        foreach (string id in new[] { "curso_abnt2_basico", "curso_abnt2_intermediario" })
        {
            if (!progress.TryGetPropertyValue(id, out var removed)) continue;
            if (removed is not JsonObject course) throw new InvalidDataException($"Progresso inválido de {id}.");
            Archive(courses, id, course);
            progress.Remove(id);
        }

        if (progress.TryGetPropertyValue("musicas", out var musicNode))
        {
            if (musicNode is not JsonObject music) throw new InvalidDataException("Progresso inválido de musicas.");
            int? frontier = null;
            if (music.TryGetPropertyValue("ULTIMACONCLUIDA", out var value))
            {
                if (value is not JsonValue integer || !integer.TryGetValue<int>(out int last) || last < 0 || last > 5)
                    throw new InvalidDataException("ULTIMACONCLUIDA de musicas inválida; nenhum progresso foi resetado.");
                frontier = last;
            }
            var retired = new JsonObject();
            foreach (string key in music.Select(p => p.Key).Where(k => k.StartsWith("LICAO5.", StringComparison.Ordinal)).ToArray())
            {
                if (!int.TryParse(key.AsSpan(7), out int attempt) || attempt < 1 || music[key] is not JsonObject)
                    throw new InvalidDataException($"Tentativa inválida: {key}.");
                retired[key] = music[key].DeepClone();
                music.Remove(key);
            }
            if (retired.Count > 0 || frontier == 5)
            {
                if (frontier.HasValue) retired["ULTIMACONCLUIDA"] = frontier.Value;
                Archive(courses, "musicas", retired);
            }
            if (frontier.HasValue) music["ULTIMACONCLUIDA"] = Math.Min(frontier.Value, 4);
        }

        return new(progress, history, !JsonNode.DeepEquals(original, progress));
    }

    private static void Archive(JsonObject courses, string id, JsonObject incoming)
    {
        if (!courses.TryGetPropertyValue(id, out var existing))
        {
            courses[id] = incoming.DeepClone();
            return;
        }
        if (existing is not JsonObject archived) throw new InvalidDataException($"Histórico inválido de {id}.");
        foreach (var field in incoming)
        {
            if (!archived.TryGetPropertyValue(field.Key, out var previous))
                archived[field.Key] = field.Value?.DeepClone();
            else if (field.Key == "ULTIMACONCLUIDA" && previous is JsonValue oldValue && field.Value is JsonValue newValue &&
                oldValue.TryGetValue<int>(out int oldLast) && newValue.TryGetValue<int>(out int newLast))
                archived[field.Key] = Math.Max(oldLast, newLast);
            else if (!JsonNode.DeepEquals(previous, field.Value))
                throw new InvalidDataException($"Conflito de histórico em {id}/{field.Key}; nenhum registro foi sobrescrito.");
        }
    }
}
