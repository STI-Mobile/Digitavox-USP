using System.Text.Json;

namespace CommunityToolkit.Mvvm.Messaging
{
    public sealed class WeakReferenceMessenger
    {
        public static WeakReferenceMessenger Default { get; } = new();

        public void Register<TMessage>(object recipient, Action<object, TMessage> handler)
        {
            // Os testes caracterizam o modelo, não o mensageiro global.
        }
    }
}

namespace Digitavox.Helpers
{
    public sealed class DVMessage
    {
        public DVMessage(string value)
        {
            Value = value;
        }

        public string Value { get; }
    }

    public enum Modifier
    {
        CapsLock = 0b_0000_0001,
        Shift = 0b_0000_0010,
        Ctrl = 0b_0000_0100,
        Alt = 0b_0000_1000,
        Window = 0b_0001_0000,
        NumLock = 0b_0010_0000,
        AltGr = 0b_0100_0000,
        Fn = 0b_1000_0000,
        Option = Alt,
        Command = Window
    }

    public static class DVKeyboard
    {
        public static void SetModifier(Modifier modifier, ref int keyModifier)
        {
            keyModifier |= (int)modifier;
        }

        public static void ResetModifier(Modifier modifier, ref int keyModifier)
        {
            keyModifier &= ~(int)modifier;
        }

        public static bool IsModifierSet(Modifier modifier, int keyModifier)
        {
            return (keyModifier & (int)modifier) == (int)modifier;
        }

        public static bool IsModifierKey(string code)
        {
            return Enum.IsDefined(typeof(Modifier), code);
        }
    }

    public enum TestPlatform
    {
        Android,
        Ios,
        Mac,
        Windows
    }

    public static class DVDevice
    {
        public static TestPlatform Platform { get; set; } = TestPlatform.Android;

        public static bool IsAndroid() => Platform == TestPlatform.Android;
        public static bool IsVirtual() => false;
        public static bool IsIos() => Platform == TestPlatform.Ios;
        public static bool IsMac() => Platform == TestPlatform.Mac;
        public static bool IsWindows() => Platform == TestPlatform.Windows;
    }

    public static class DVPersistence
    {
        private static readonly Dictionary<string, Dictionary<string, object>> Users = new();
        private static readonly Dictionary<string, object> Settings = new();

        public static (List<JsonElement>, List<string>, List<string>) CourseFiles { get; set; }
            = (new List<JsonElement>(), new List<string>(), new List<string>());

        public static int ReadCourseFilesCalls { get; private set; }
        public static int SaveUserCalls { get; private set; }
        public static Dictionary<string, object> LastSavedUser { get; private set; }

        public static void Reset()
        {
            Users.Clear();
            Settings.Clear();
            CourseFiles = (new List<JsonElement>(), new List<string>(), new List<string>());
            ReadCourseFilesCalls = 0;
            SaveUserCalls = 0;
            LastSavedUser = null;
        }

        public static void SeedUser(string userName, Dictionary<string, object> data)
        {
            Users[userName] = data;
        }

        public static void SetSetting<T>(string key, T value)
        {
            Settings[key] = value;
        }

        public static T Get<T>(string key)
        {
            return (T)Settings[key];
        }

        public static Dictionary<string, object> LoadUserData(string userName)
        {
            if (!Users.TryGetValue(userName, out Dictionary<string, object> data))
            {
                data = new Dictionary<string, object>();
                Users[userName] = data;
            }

            return data;
        }

        public static void SaveUserJson(Dictionary<string, object> userData)
        {
            LastSavedUser = userData;
            SaveUserCalls++;
        }

        public static (List<JsonElement>, List<string>, List<string>) ReadCourseFiles()
        {
            ReadCourseFilesCalls++;
            return CourseFiles;
        }
    }
}

namespace Digitavox.Models
{
    public interface IDispatcherTimer
    {
        TimeSpan Interval { get; set; }
        event EventHandler Tick;
        void Start();
        void Stop();
    }

    public sealed class FakeDispatcherTimer : IDispatcherTimer
    {
        public TimeSpan Interval { get; set; }
        public bool IsRunning { get; private set; }
        public event EventHandler Tick;

        public void Start() => IsRunning = true;
        public void Stop() => IsRunning = false;
        public void RaiseTick() => Tick?.Invoke(this, EventArgs.Empty);
    }

    public sealed class FakeDispatcher
    {
        public FakeDispatcherTimer LastTimer { get; private set; }

        public IDispatcherTimer CreateTimer()
        {
            LastTimer = new FakeDispatcherTimer();
            return LastTimer;
        }
    }

    public sealed class Application
    {
        public static Application Current { get; } = new();
        public FakeDispatcher Dispatcher { get; } = new();
    }

    public static class FileSystem
    {
        public static Task<Stream> OpenAppPackageFileAsync(string fileName)
        {
            string path = Path.Combine(AppContext.BaseDirectory, "Fixtures", fileName);
            return Task.FromResult<Stream>(File.OpenRead(path));
        }
    }
}
