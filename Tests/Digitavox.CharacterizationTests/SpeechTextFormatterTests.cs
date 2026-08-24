using Digitavox.Core.Abstractions;
using Digitavox.Models;
using Digitavox.Presentation.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Digitavox.CharacterizationTests;

[TestClass]
public class SpeechTextFormatterTests
{
    [TestMethod]
    public void Screen_reader_terms_change_only_for_voice_over_on_ios()
    {
        FakeAppEnvironment environment = new();
        SpeechTextFormatter formatter = new(new FingerMapping(), environment);

        Assert.AreEqual("Escape volta.", formatter.AdaptForScreenReader("Escape volta."));

        environment.Platform = AppPlatformKind.Ios;
        environment.IsScreenReaderEnabled = true;

        Assert.AreEqual(
            "CTRL + Escape volta. control esqueipe volta.",
            formatter.AdaptForScreenReader("Escape volta. Esqueipe volta."));
    }

    [TestMethod]
    public void Keyboard_descriptions_are_kept_out_of_the_navigation_coordinator()
    {
        SpeechTextFormatter formatter = new(new FingerMapping(), new FakeAppEnvironment());

        Assert.AreEqual("a ", formatter.Spell("a "));
        Assert.AreEqual("a Interrogação ", formatter.DescribePunctuation("a?"));
        Assert.AreEqual("Interrogação abc", formatter.PreserveWordAfterLeadingPunctuation("?abc"));
    }

    private sealed class FakeAppEnvironment : IAppEnvironment
    {
        public AppPlatformKind Platform { get; set; } = AppPlatformKind.Android;
        public bool IsVirtualDevice => false;
        public bool IsScreenReaderEnabled { get; set; }
        public bool ShouldUseLightForeground => false;
        public void RunOnMainThread(Action action) => action();
    }
}
