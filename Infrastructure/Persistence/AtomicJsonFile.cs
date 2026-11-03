using System.Text;
using System.Text.Json.Nodes;

namespace Digitavox.Infrastructure.Persistence;

internal static class AtomicJsonFile
{
    public static byte[] Encode(JsonNode node) => Encoding.UTF8.GetBytes(node.ToJsonString(new() { WriteIndented = true }));

    public static bool Matches(string path, byte[] bytes) => File.Exists(path) && File.ReadAllBytes(path).SequenceEqual(bytes);

    public static void Write(string path, byte[] bytes)
    {
        if (Matches(path, bytes)) return;
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        string temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                stream.Write(bytes);
                stream.Flush(flushToDisk: true);
            }
            File.Move(temporary, path, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }
}
