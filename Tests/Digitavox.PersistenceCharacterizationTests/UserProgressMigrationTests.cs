using System.Text.Json.Nodes;
using Digitavox.Domain.Catalog;
using Digitavox.Helpers;
using Digitavox.Infrastructure.Persistence;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Digitavox.PersistenceCharacterizationTests;

[TestClass]
public class UserProgressMigrationTests
{
    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    [DataRow(4)]
    [DataRow(5)]
    public void Music_frontier_is_bounded_without_changing_surviving_statistics(int last)
    {
        var user = User(last);
        var before = user.DeepClone();
        var result = UserProgressMigration.ToVersionOne(user);
        Assert.AreEqual(Math.Min(last, 4), result.Progress["musicas"]["ULTIMACONCLUIDA"].GetValue<int>());
        Assert.IsTrue(JsonNode.DeepEquals(before["musicas"]["LICAO4.1"], result.Progress["musicas"]["LICAO4.1"]));
        Assert.IsTrue(JsonNode.DeepEquals(before["SENAI"], result.Progress["SENAI"]));
        Assert.IsTrue(JsonNode.DeepEquals(before["musica_se_eu_quiser_falar_com_deus"], result.Progress["musica_se_eu_quiser_falar_com_deus"]));
        Assert.IsTrue(JsonNode.DeepEquals(before, user), "A transformação pura não deve modificar a entrada.");
        if (last == 5) Assert.AreEqual(5, result.History["courses"]["musicas"]["ULTIMACONCLUIDA"].GetValue<int>());
    }

    [TestMethod]
    [DataRow("S")]
    [DataRow("N")]
    public void Retired_courses_and_all_music_attempts_are_archived_losslessly(string concluded)
    {
        var user = User(5);
        user["curso_abnt2_basico"] = CourseProgress(30);
        user["curso_abnt2_intermediario"] = CourseProgress(6);
        user["musicas"]["LICAO5.1"] = Attempt(concluded);
        user["musicas"]["LICAO5.3"] = Attempt("N"); // Preserve identities even if attempt numbers have gaps.
        var result = UserProgressMigration.ToVersionOne(user);
        Assert.IsNull(result.Progress["curso_abnt2_basico"]);
        Assert.IsNull(result.Progress["curso_abnt2_intermediario"]);
        Assert.IsNull(result.Progress["musicas"]["LICAO5.1"]);
        foreach (string id in new[] { "curso_abnt2_basico", "curso_abnt2_intermediario" })
            Assert.IsTrue(JsonNode.DeepEquals(user[id], result.History["courses"][id]));
        foreach (string key in new[] { "LICAO5.1", "LICAO5.3" })
            Assert.IsTrue(JsonNode.DeepEquals(user["musicas"][key], result.History["courses"]["musicas"][key]));
        var twice = UserProgressMigration.ToVersionOne(result.Progress, result.History);
        Assert.IsFalse(twice.Changed);
        Assert.IsTrue(JsonNode.DeepEquals(result.Progress, twice.Progress));
        Assert.IsTrue(JsonNode.DeepEquals(result.History, twice.History));
        // Retrying from original after an archive-only commit must not duplicate or alter records either.
        var retry = UserProgressMigration.ToVersionOne(user, result.History);
        Assert.IsTrue(JsonNode.DeepEquals(result.History, retry.History));
        Assert.IsTrue(JsonNode.DeepEquals(result.Progress, retry.Progress));
    }

    [TestMethod]
    public void Empty_progress_and_unknown_root_fields_are_preserved()
    {
        var user = JsonNode.Parse("{\"NOMEUSUARIO\":\"ANA\",\"type\":\"USUARIO\",\"extension\":{\"arbitrary\":[1,true,\"value\"]}}") as JsonObject;
        var result = UserProgressMigration.ToVersionOne(user);
        Assert.IsFalse(result.Changed);
        Assert.IsTrue(JsonNode.DeepEquals(user, result.Progress));
    }

    [TestMethod]
    [DataRow("-1")]
    [DataRow("6")]
    [DataRow("1.5")]
    [DataRow("\"5\"")]
    [DataRow("null")]
    public void Invalid_music_frontier_is_not_silently_reset(string json)
    {
        var user = User(0);
        user["musicas"]["ULTIMACONCLUIDA"] = JsonNode.Parse(json);
        string original = user.ToJsonString();
        Assert.ThrowsExactly<InvalidDataException>(() => UserProgressMigration.ToVersionOne(user));
        Assert.AreEqual(original, user.ToJsonString());
    }

    [TestMethod]
    public void Conflicting_history_is_not_overwritten()
    {
        var user = User(5);
        user["musicas"]["LICAO5.1"] = Attempt("S");
        var first = UserProgressMigration.ToVersionOne(user);
        first.History["courses"]["musicas"]["LICAO5.1"]["NLETRASACERTOU"] = 999;
        string before = first.History.ToJsonString();
        Assert.ThrowsExactly<InvalidDataException>(() => UserProgressMigration.ToVersionOne(user, first.History));
        Assert.AreEqual(before, first.History.ToJsonString());
    }

    [TestMethod]
    public void History_can_be_read_without_a_catalog_or_exposing_unknown_payload()
    {
        var user = User(5);
        var attempt = Attempt("N");
        attempt["EXER1"] = "payload que não pode ser executado";
        user["musicas"]["LICAO5.1"] = attempt;
        var result = UserProgressMigration.ToVersionOne(user);
        var lines = ArchivedProgressReader.Read(result.History);
        Assert.IsTrue(lines.Contains("Registro LICAO5.1"));
        Assert.IsTrue(lines.Contains("Concluiu (S/N): N"));
        Assert.IsTrue(lines.Contains("Percentual de acertos: 87"));
        Assert.IsTrue(lines.Contains("Erros no caractere a: 3"));
        Assert.IsFalse(lines.Any(l => l.Contains("payload")));
    }

    [TestMethod]
    [DataRow("history:ANA")]
    [DataRow("profile:ANA")]
    public async Task All_profiles_including_anonymous_are_recovered_without_duplicate_history(string stop)
    {
        string root = Path.Combine(Path.GetTempPath(), "DigitavoxProgressTests", Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(Path.Combine(root, "Users"));
            foreach (string name in new[] { "ANA", "BIA", "ANONIMO" })
            {
                var user = User(5);
                user["NOMEUSUARIO"] = name;
                user["curso_abnt2_basico"] = CourseProgress(30);
                user["musicas"]["LICAO5.1"] = Attempt("N");
                File.WriteAllText(Path.Combine(root, "Users", name + ".json"), user.ToJsonString());
            }
            var migration = new CatalogMigration(root, OpenBundle, point => { if (point == stop) throw new IOException("Interrupção"); });
            await Assert.ThrowsExactlyAsync<IOException>(() => migration.EnsureCurrentAsync());
            Assert.AreEqual(0, migration.ReadVersion());
            await new CatalogMigration(root, OpenBundle).EnsureCurrentAsync();
            foreach (string name in new[] { "ANA", "BIA", "ANONIMO" })
            {
                var active = JsonNode.Parse(File.ReadAllText(Path.Combine(root, "Users", name + ".json")));
                var history = JsonNode.Parse(File.ReadAllText(Path.Combine(root, "History", "Users", name + ".json")));
                Assert.IsNull(active["curso_abnt2_basico"]);
                Assert.AreEqual(4, active["musicas"]["ULTIMACONCLUIDA"].GetValue<int>());
                Assert.AreEqual("N", history["courses"]["musicas"]["LICAO5.1"]["CONCLUIU"].GetValue<string>());
                Assert.AreEqual(5, history["courses"]["musicas"]["ULTIMACONCLUIDA"].GetValue<int>());
            }
            var before = CatalogMigrationTests.Snapshot(root);
            await new CatalogMigration(root, OpenBundle).EnsureCurrentAsync();
            CollectionAssert.AreEqual(before, CatalogMigrationTests.Snapshot(root));
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }

    [TestMethod]
    public async Task Malformed_profile_prevents_all_writes_and_preserves_original_bytes()
    {
        string root = Path.Combine(Path.GetTempPath(), "DigitavoxInvalidProfile", Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(Path.Combine(root, "Users"));
            File.WriteAllText(Path.Combine(root, "Users", "A.json"), User(5).ToJsonString());
            File.WriteAllText(Path.Combine(root, "Users", "Z.json"), "{");
            var before = CatalogMigrationTests.Snapshot(root);
            await Assert.ThrowsExactlyAsync<InvalidDataException>(() => new CatalogMigration(root, OpenBundle).EnsureCurrentAsync());
            CollectionAssert.AreEqual(before, CatalogMigrationTests.Snapshot(root));
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }

    internal static Task<Stream> OpenBundle(string name) => Task.FromResult<Stream>(File.OpenRead(Path.Combine(AppContext.BaseDirectory, "Bundle", name)));
    internal static JsonObject User(int last) => new()
    {
        ["NOMEUSUARIO"] = "ANA", ["type"] = "USUARIO",
        ["musicas"] = new JsonObject { ["ULTIMACONCLUIDA"] = last, ["LICAO4.1"] = Attempt("S") },
        ["SENAI"] = CourseProgress(12), ["musica_se_eu_quiser_falar_com_deus"] = CourseProgress(1)
    };
    internal static JsonObject CourseProgress(int last) => new() { ["ULTIMACONCLUIDA"] = last, ["LICAO1.1"] = Attempt("S") };
    internal static JsonObject Attempt(string concluded) => new()
    {
        ["DATAINICIO"] = "2026-08-24 17:00:00", ["DATAFIM"] = "2026-08-24 17:05:00", ["CONCLUIU"] = concluded,
        ["PERCENTUALACERTO"] = 87, ["TOTALLETRASLICAO"] = 100, ["NLETRASACERTOU"] = 87, ["NLETRASERROU"] = 13,
        ["LETRASPORMINUTO"] = 20, ["PERCENTUALTEMPO"] = 42.5m, ["ERROSEM_a"] = 3
    };
}
