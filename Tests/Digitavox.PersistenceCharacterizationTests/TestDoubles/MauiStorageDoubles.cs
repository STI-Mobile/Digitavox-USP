namespace Digitavox.PlatformsImplementations
{
    public sealed class DVSpeak
    {
        private static readonly DVSpeak Instance = new();

        public static DVSpeak GetInstance() => Instance;

        public int SpeechRate { get; private set; }

        public void SetSpeechRate(int rate)
        {
            SpeechRate = rate;
        }

        public void Reset()
        {
            SpeechRate = 0;
        }
    }
}

namespace Digitavox.Helpers
{
    public sealed class TestPreferences
    {
        private readonly Dictionary<string, object> values = new();

        public T Get<T>(string key, T defaultValue)
        {
            return values.TryGetValue(key, out object value) ? (T)value : defaultValue;
        }

        public void Set<T>(string key, T value)
        {
            values[key] = value;
        }

        public void Clear()
        {
            values.Clear();
        }
    }

    public static class Preferences
    {
        public static TestPreferences Default { get; } = new();
    }

    public static class FileSystem
    {
        public static string AppDataDirectory { get; set; }

        public static string AppPackageDirectory { get; set; }

        public static Task<Stream> OpenAppPackageFileAsync(string fileName)
        {
            Stream stream = File.OpenRead(Path.Combine(AppPackageDirectory, fileName));
            return Task.FromResult(stream);
        }
    }
}
