using System.Text.Json.Nodes;
using Digitavox.Core.Abstractions;
using Digitavox.Domain.Catalog;

namespace Digitavox.Infrastructure.Persistence;

public sealed class MauiUserHistoryStore : IUserHistoryStore
{
    public IReadOnlyList<string> LoadLines(string userName)
    {
        string path = Path.Combine(FileSystem.AppDataDirectory, "History", "Users", userName.ToUpper() + ".json");
        if (!File.Exists(path)) return Array.Empty<string>();
        var history = JsonNode.Parse(File.ReadAllBytes(path)) as JsonObject ?? throw new InvalidDataException("Histórico inválido.");
        return ArchivedProgressReader.Read(history);
    }
}
