namespace Digitavox.Core.Abstractions;

public interface IUserHistoryStore
{
    IReadOnlyList<string> LoadLines(string userName);
}
