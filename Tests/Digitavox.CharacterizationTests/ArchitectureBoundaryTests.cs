// Copyright 2024-2026 Universidade de São Paulo
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Digitavox.CharacterizationTests;

[TestClass]
public sealed class ArchitectureBoundaryTests
{
    private static readonly string[] CoreDirectories =
    [
        "Application",
        "Domain",
        "Models",
        "Presentation",
        "ViewModels",
        "Views"
    ];

    private static readonly string[] ForbiddenCoreReferences =
    [
        "Digitavox.Infrastructure",
        "Digitavox.PlatformsImplementations",
        "DVPersistence",
        "DVSpeak.GetInstance",
        "Shell.Current",
        "Application.Current"
    ];

    private static readonly string[] TransientViewModels =
    [
        "AlertViewModel",
        "ConfigViewModel",
        "CoursesHelpViewModel",
        "CoursesViewModel",
        "ExercisesHelpViewModel",
        "ExercisesStatisticsViewModel",
        "ExercisesViewModel",
        "KeyboardViewModel",
        "LessonsHelpViewModel",
        "LessonsViewModel",
        "LoginViewModel",
        "MenuViewModel",
        "PrivacyPolicyViewModel",
        "SecondHelpViewModel",
        "ThirdPartyLicensesViewModel",
        "TutorialViewModel",
        "UserOptionsViewModel"
    ];

    private static readonly string[] SessionServices =
    [
        "Course",
        "CourseLesson",
        "FingerMapping",
        "UserProgress",
        "DVViewModelSpeak",
        "DVViewModelFunctions"
    ];

    private static readonly Lazy<string> RepositoryRoot = new(FindRepositoryRoot);

    [TestMethod]
    public void CoreLayersDoNotReferenceInfrastructureOrPlatformDetails()
    {
        var violations = SourceFiles(CoreDirectories)
            .SelectMany(file => ForbiddenCoreReferences
                .Where(reference => File.ReadAllText(file).Contains(reference, StringComparison.Ordinal))
                .Select(reference => $"{Relative(file)} -> {reference}"))
            .ToList();

        AssertHasNoViolations(violations);
    }

    [TestMethod]
    public void ViewsDependOnlyOnViewModelsAndPresentationContracts()
    {
        string[] forbiddenViewUsings =
        [
            "using Digitavox.Core",
            "using Digitavox.Domain",
            "using Digitavox.Helpers",
            "using Digitavox.Infrastructure",
            "using Digitavox.Models",
            "using Digitavox.Platforms"
        ];

        var violations = SourceFiles(["Views"])
            .SelectMany(file => forbiddenViewUsings
                .Where(reference => File.ReadAllText(file).Contains(reference, StringComparison.Ordinal))
                .Select(reference => $"{Relative(file)} -> {reference}"))
            .ToList();

        AssertHasNoViolations(violations);
    }

    [TestMethod]
    public void EveryViewUsesCompiledBindings()
    {
        var xamlFiles = Directory
            .EnumerateFiles(Path.Combine(RepositoryRoot.Value, "Views"), "*.xaml", SearchOption.TopDirectoryOnly)
            .Append(Path.Combine(RepositoryRoot.Value, "AppShell.xaml"));

        var violations = xamlFiles
            .Where(file => !File.ReadAllText(file).Contains("x:DataType=", StringComparison.Ordinal))
            .Select(file => $"{Relative(file)} -> x:DataType ausente")
            .ToList();

        AssertHasNoViolations(violations);
    }

    [TestMethod]
    public void ViewModelsDoNotBlockOnAsynchronousWork()
    {
        string[] blockingCalls =
        [
            "async void",
            "Thread.Sleep",
            ".GetAwaiter().GetResult()",
            ".Wait()",
            ".Result"
        ];

        var violations = SourceFiles(["ViewModels"])
            .SelectMany(file => blockingCalls
                .Where(call => File.ReadAllText(file).Contains(call, StringComparison.Ordinal))
                .Select(call => $"{Relative(file)} -> {call}"))
            .ToList();

        AssertHasNoViolations(violations);
    }

    [TestMethod]
    public void PlatformAdaptersDoNotConstructViewModelsDirectly()
    {
        var viewModelTypes = TransientViewModels
            .Concat(["DVViewModelSpeak", "DVViewModelFunctions"])
            .ToArray();

        var violations = SourceFiles(["Platforms"])
            .SelectMany(file => viewModelTypes
                .Where(type => File.ReadAllText(file).Contains($"new {type}(", StringComparison.Ordinal))
                .Select(type => $"{Relative(file)} -> new {type}(...)"))
            .ToList();

        AssertHasNoViolations(violations);
    }

    [TestMethod]
    public void DependencyInjectionPreservesScreenAndSessionLifetimes()
    {
        string compositionRoot = File.ReadAllText(Path.Combine(RepositoryRoot.Value, "MauiProgram.cs"));
        var violations = new List<string>();

        violations.AddRange(TransientViewModels
            .Where(type => !compositionRoot.Contains($"AddTransient<{type}>()", StringComparison.Ordinal))
            .Select(type => $"{type} deve ser transient"));

        violations.AddRange(SessionServices
            .Where(type => !compositionRoot.Contains($"AddSingleton<{type}>()", StringComparison.Ordinal))
            .Select(type => $"{type} deve ser singleton de sessão"));

        AssertHasNoViolations(violations);
    }

    [TestMethod]
    public void StartupResolvesDestinationBeforeCreatingAppShell()
    {
        string appSource = File.ReadAllText(Path.Combine(RepositoryRoot.Value, "App.xaml.cs"));
        string shellSource = File.ReadAllText(Path.Combine(RepositoryRoot.Value, "AppShell.xaml.cs"));
        string shellMarkup = File.ReadAllText(Path.Combine(RepositoryRoot.Value, "AppShell.xaml"));

        int initializeIndex = appSource.IndexOf(
            "await appStartupService.InitializeAsync()",
            StringComparison.Ordinal);
        int shellCreationIndex = appSource.IndexOf(
            "new AppShell(destination)",
            StringComparison.Ordinal);

        Assert.IsTrue(
            appSource.Contains("new Window(new StartupView())", StringComparison.Ordinal),
            "A janela deve iniciar com uma View transitória enquanto o startup assíncrono executa.");
        Assert.IsTrue(initializeIndex >= 0, "O startup assíncrono deve ser executado pela composição do App.");
        Assert.IsTrue(
            shellCreationIndex > initializeIndex,
            "O AppShell só deve ser criado depois que o destino inicial for resolvido.");
        Assert.IsFalse(
            shellSource.Contains("new ContentPage()", StringComparison.Ordinal),
            "O AppShell não deve voltar a materializar uma página vazia como placeholder.");
        Assert.IsFalse(
            shellMarkup.Contains("{Binding FirstView}", StringComparison.Ordinal),
            "O conteúdo inicial do Shell não deve depender de um DataTemplate mutável.");
    }

    [TestMethod]
    public void KeyboardInputContractBelongsToPresentationLayer()
    {
        string contract = Path.Combine(RepositoryRoot.Value, "Presentation", "Input", "IKeyboardInputHandler.cs");
        string legacyContract = Path.Combine(RepositoryRoot.Value, "Models", "IOnPageKeyPress.cs");

        Assert.IsTrue(File.Exists(contract), "O contrato de entrada deve existir em Presentation/Input.");
        Assert.IsFalse(File.Exists(legacyContract), "O contrato de entrada não deve voltar para Models.");
    }

    private static IEnumerable<string> SourceFiles(IEnumerable<string> directories) =>
        directories.SelectMany(directory => Directory.EnumerateFiles(
            Path.Combine(RepositoryRoot.Value, directory),
            "*.cs",
            SearchOption.AllDirectories));

    private static string Relative(string path) => Path.GetRelativePath(RepositoryRoot.Value, path);

    private static void AssertHasNoViolations(IReadOnlyCollection<string> violations) =>
        Assert.AreEqual(0, violations.Count, string.Join(Environment.NewLine, violations));

    private static string FindRepositoryRoot()
    {
        foreach (string startingPath in new[] { AppContext.BaseDirectory, Directory.GetCurrentDirectory() })
        {
            var directory = new DirectoryInfo(startingPath);
            while (directory is not null)
            {
                if (File.Exists(Path.Combine(directory.FullName, "Digitavox.csproj")))
                {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }
        }

        throw new DirectoryNotFoundException("Não foi possível localizar a raiz do repositório.");
    }
}
