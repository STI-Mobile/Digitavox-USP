using System.Text.Json.Nodes;

namespace Digitavox.PersistenceCharacterizationTests;

internal static class LegacyCatalogFixture
{
    // Exact legacy identities/topology with synthetic retired payload, not retained copyrighted material.
    public static void Write(string root, params string[] ids)
    {
        string directory = Path.Combine(root, "Courses");
        Directory.CreateDirectory(directory);
        foreach (string id in ids)
        {
            JsonObject definition;
            if (id is "curso_abnt2_basico" or "curso_abnt2_intermediario")
            {
                int total = id == "curso_abnt2_basico" ? 30 : 11;
                definition = new JsonObject
                {
                    ["type"] = "CURSO", ["CURSO"] = new JsonObject
                    {
                        ["NUMEROCURSO"] = id == "curso_abnt2_basico" ? 1 : 2,
                        ["NOMECURSO"] = id, ["QUANTIDADELICOES"] = total,
                        ["Present"] = new JsonObject { ["APT"] = "Curso legado sintético" },
                        ["Instruction"] = new JsonObject { ["IST"] = "Digite" }
                    }
                };
                for (int i = 1; i <= total; i++) definition["LICAO" + i] = SyntheticLesson();
            }
            else
            {
                definition = JsonNode.Parse(File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Bundle", id + ".json"))) as JsonObject;
                if (id == "musicas")
                {
                    definition["LICAO5"] = SyntheticLesson();
                    definition["CURSO"]["QUANTIDADELICOES"] = 5;
                    definition["CURSO"]["Present"]["APT"] = "Este curso é composto por cinco lições.";
                }
            }
            File.WriteAllText(Path.Combine(directory, id + ".json"), definition.ToJsonString());
        }
    }

    private static JsonObject SyntheticLesson() => new()
    {
        ["Present"] = new JsonObject { ["APT"] = "Lição legada retirada" }, ["Instruction"] = new JsonObject { ["IST"] = "Digite" },
        ["EXER1"] = "frase sintética", ["REPETICOESEXER"] = 1, ["TEMPOPORCARACTER"] = 1,
        ["MEDIAEXER"] = 90, ["SOLETRAEXER"] = "nao", ["TUDO_EM_MAIUSCULO"] = "nao"
    };
}
