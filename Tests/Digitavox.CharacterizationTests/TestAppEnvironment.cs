using Digitavox.Core.Abstractions;

namespace Digitavox.CharacterizationTests;

internal sealed class TestAppEnvironment : IAppEnvironment
{
    public TestAppEnvironment(AppPlatformKind platform = AppPlatformKind.Android)
    {
        Platform = platform;
    }

    public AppPlatformKind Platform { get; set; }
    public bool IsVirtualDevice => false;
    public bool IsScreenReaderEnabled { get; set; }
    public bool ShouldUseLightForeground => false;
    public void RunOnMainThread(Action action) => action();
}
