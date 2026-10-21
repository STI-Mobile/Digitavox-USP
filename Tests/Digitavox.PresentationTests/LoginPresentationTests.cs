using Digitavox.Core.Abstractions;
using Digitavox.Models;
using Digitavox.Presentation.Input;
using Digitavox.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Digitavox.PresentationTests;

[TestClass]
public class LoginPresentationTests
{
    [TestMethod]
    public void Name_can_be_entered_when_the_title_speech_never_completes()
    {
        var (viewModel, speech, progress, navigation) = Create();
        viewModel.OnPage();
        viewModel.OnPageKeyPress('A', 0);
        viewModel.OnPageKeyPress('N', 0);
        viewModel.OnPageKeyPress('A', 0);
        Assert.AreEqual("ANA", speech.Lines[2]);

        speech.CompleteSpeechImmediately = true;
        viewModel.OnPageKeyPress(13, 0);

        Assert.AreEqual("ANA", progress.LoadedName);
        Assert.AreEqual(AppRoute.Menu, navigation.LastRoute);
    }

    [TestMethod]
    public void Anonymous_confirmation_works_when_the_title_speech_never_completes()
    {
        var (viewModel, speech, progress, navigation) = Create();
        viewModel.OnPage();
        viewModel.OnPageKeyPress(13, 0);
        StringAssert.Contains(speech.Lines[3], "deseja entrar como anônimo");

        speech.CompleteSpeechImmediately = true;
        viewModel.OnPageKeyPress('S', 0);

        Assert.AreEqual("anonimo", progress.LoadedName);
        Assert.AreEqual(AppRoute.Menu, navigation.LastRoute);
    }

    [TestMethod]
    public void Late_title_completion_does_not_restart_instructions_after_typing()
    {
        var (viewModel, speech, _, _) = Create();
        viewModel.OnPage();
        Action finishTitle = speech.PendingSpeech[0];
        viewModel.OnPageKeyPress('A', 0);
        finishTitle();

        CollectionAssert.AreEqual(new[] { 0 }, speech.SpokenLines);
        Assert.AreEqual("A", speech.Lines[2]);
    }

    [TestMethod]
    public void Anonymous_confirmation_can_be_cancelled_and_replaced_by_a_name()
    {
        var (viewModel, speech, progress, navigation) = Create();
        viewModel.OnPage();
        viewModel.OnPageKeyPress(13, 0);
        speech.CompleteSpeechImmediately = true;
        viewModel.OnPageKeyPress('N', 0);
        viewModel.OnPageKeyPress('A', 0);
        viewModel.OnPageKeyPress(13, 0);

        Assert.AreEqual("A", progress.LoadedName);
        Assert.AreEqual(AppRoute.Menu, navigation.LastRoute);
    }

    [TestMethod]
    public void Repeated_enter_does_not_cancel_the_speech_that_completes_login()
    {
        var (viewModel, speech, _, navigation) = Create();
        speech.CompleteSpeechImmediately = true;
        viewModel.OnPage();
        viewModel.OnPageKeyPress('A', 0);
        speech.CompleteSpeechImmediately = false;
        viewModel.OnPageKeyPress(13, 0);
        int skipCalls = speech.SkipCalls;
        viewModel.OnPageKeyPress(13, 0);
        Assert.AreEqual(skipCalls, speech.SkipCalls);

        speech.PendingSpeech[0]();
        viewModel.OnPageKeyPress(13, 0);
        Assert.AreEqual(skipCalls, speech.SkipCalls);
        speech.PendingSpeech[1]();
        Assert.AreEqual(AppRoute.Menu, navigation.LastRoute);
    }

    private static (LoginViewModel, DVViewModelSpeak, Progress, Navigation) Create()
    {
        var settings = new Settings();
        var store = new Progress();
        var speech = new DVViewModelSpeak { CompleteSpeechImmediately = false };
        var navigation = new Navigation();
        var viewModel = new LoginViewModel(speech, new KeyboardInputProcessor(),
            new UserProgress(null, null, store, settings), new DVViewModelFunctions(),
            settings, navigation, new Environment());
        return (viewModel, speech, store, navigation);
    }

    private sealed class Progress : IUserProgressStore
    {
        public string LoadedName { get; private set; }
        public Dictionary<string, object> Load(string name)
        {
            LoadedName = name;
            return new() { ["NOMEUSUARIO"] = name.ToUpper(), ["type"] = "USUARIO" };
        }
        public void Save(Dictionary<string, object> data) { }
    }
    private sealed class Settings : ISettingsService
    {
        public double DefaultFontSize => 14;
        public T Get<T>(string key) => (T)(object)14d;
        public void Set<T>(string key, T value) { }
        public void ResetDefaults() { }
    }
    private sealed class Navigation : INavigationService
    {
        public AppRoute? LastRoute { get; private set; }
        public Task GoToAsync(AppRoute route, int backLevels = 0) { LastRoute = route; return Task.CompletedTask; }
        public Task GoBackAsync(int levels = 1) => Task.CompletedTask;
    }
    private sealed class Environment : IAppEnvironment
    {
        public AppPlatformKind Platform => AppPlatformKind.Ios;
        public bool IsVirtualDevice => true;
        public bool IsScreenReaderEnabled => false;
        public bool ShouldUseLightForeground => true;
        public void RunOnMainThread(Action action) => action();
    }
}
