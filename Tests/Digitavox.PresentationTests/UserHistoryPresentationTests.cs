using Digitavox.Core.Abstractions;
using Digitavox.Models;
using Digitavox.Presentation.Input;
using Digitavox.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Digitavox.PresentationTests;

[TestClass]
public class UserHistoryPresentationTests
{
    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task History_is_accessible_and_enter_cannot_start_retired_content(bool empty)
    {
        var (viewModel, speak, functions, navigation) = Create(empty ? Array.Empty<string>() : new[] { "Registro LICAO5.1", "Concluiu (S/N): N" });
        await viewModel.OnPageAsync();
        CollectionAssert.Contains(speak.Lines, "3 - Consultar histórico dos conteúdos retirados");
        viewModel.OnPageKeyPress(51, 0);
        viewModel.OnPageKeyPress(13, 0);
        Assert.IsTrue(speak.Lines.Any(l => l.StartsWith("Histórico")));
        Assert.IsTrue(functions.ClearedHelp);
        CollectionAssert.Contains(speak.Lines, empty ? "Nenhum registro histórico de conteúdo retirado." : "Registro LICAO5.1");
        functions.Keys.Clear();
        viewModel.OnPageKeyPress(40, 0);
        viewModel.OnPageKeyPress(13, 0);
        CollectionAssert.AreEqual(new[] { "Down" }, functions.Keys);
        Assert.AreEqual(0, navigation.Calls);
    }

    [TestMethod]
    public async Task Escape_returns_to_user_options_without_navigating_to_a_lesson()
    {
        var (viewModel, speak, _, navigation) = Create(new[] { "Registro LICAO5.1" });
        await viewModel.OnPageAsync();
        viewModel.OnPageKeyPress(51, 0);
        viewModel.OnPageKeyPress(13, 0);
        viewModel.OnPageKeyPress(27, 0);
        await viewModel.OnPageAsync();
        CollectionAssert.Contains(speak.Lines, "3 - Consultar histórico dos conteúdos retirados");
        Assert.AreEqual(0, navigation.Calls);
    }

    [TestMethod]
    public async Task Invalid_history_displays_an_error_without_resetting_or_starting_content()
    {
        var (viewModel, speak, _, navigation) = Create(null);
        await viewModel.OnPageAsync();
        viewModel.OnPageKeyPress(51, 0);
        viewModel.OnPageKeyPress(13, 0);
        CollectionAssert.Contains(speak.Lines, "Não foi possível consultar o histórico. Os dados foram preservados.");
        Assert.AreEqual(0, navigation.Calls);
    }

    private static (UserOptionsViewModel, DVViewModelSpeak, DVViewModelFunctions, Navigation) Create(IReadOnlyList<string> lines)
    {
        var settings = new Settings();
        var progress = new UserProgress(new CourseLesson(), new Course(new Catalog()), new Progress(), settings);
        progress.UserRegistration("ANA");
        var speak = new DVViewModelSpeak();
        var functions = new DVViewModelFunctions();
        var navigation = new Navigation();
        return (new UserOptionsViewModel(speak, functions, new KeyboardInputProcessor(), progress, settings, navigation, new Environment(), new History(lines)), speak, functions, navigation);
    }
    private sealed class History(IReadOnlyList<string> lines) : IUserHistoryStore
    {
        public IReadOnlyList<string> LoadLines(string name) => lines ?? throw new InvalidDataException("Inválido");
    }
    private sealed class Progress : IUserProgressStore
    {
        public Dictionary<string, object> Load(string name) => new() { ["NOMEUSUARIO"] = name };
        public void Save(Dictionary<string, object> data) => throw new AssertFailedException("Histórico não deve salvar/resetar progresso.");
    }
    private sealed class Catalog : ICourseCatalogStore
    {
        public CourseCatalogData Load() => throw new AssertFailedException("Histórico não deve carregar catálogo executável.");
    }
    private sealed class Settings : ISettingsService
    {
        public double DefaultFontSize => 14;
        public T Get<T>(string key) => (T)(object)(key == "instructionsEnabled" ? (object)true : 14d);
        public void Set<T>(string key, T value) { }
        public void ResetDefaults() { }
    }
    private sealed class Navigation : INavigationService
    {
        public int Calls { get; private set; }
        public Task GoToAsync(AppRoute route, int backLevels = 0) { Calls++; return Task.CompletedTask; }
        public Task GoBackAsync(int levels = 1) { Calls++; return Task.CompletedTask; }
    }
    private sealed class Environment : IAppEnvironment
    {
        public AppPlatformKind Platform => AppPlatformKind.Android;
        public bool IsVirtualDevice => false;
        public bool IsScreenReaderEnabled => false;
        public bool ShouldUseLightForeground => false;
        public void RunOnMainThread(Action action) => action();
    }
}
