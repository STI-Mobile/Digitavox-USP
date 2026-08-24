// Copyright 2024-2026 Universidade de São Paulo
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Digitavox.Helpers;
using Digitavox.Models;
using Digitavox.Core.Abstractions;
using Digitavox.Core.Messages;
using Digitavox.Presentation.Input;
using System.Text;

namespace Digitavox.ViewModels;

public partial class ThirdPartyLicensesViewModel : ObservableObject, IOnPageKeyPress
{
    private const string NoticesFileName = "THIRD-PARTY-NOTICES.txt";
    private const string SectionSeparator = "============================================================";
    private const int MaximumSpokenIntroductionLength = 2_000;

    private readonly DVViewModelFunctions dVViewModelFunctions;
    private readonly DVViewModelSpeak dVViewModelSpeak;
    private readonly KeyboardInputProcessor keyboardInputProcessor;
    private readonly ISettingsService settingsService;
    private readonly IAppEnvironment appEnvironment;
    private bool isLoaded;
    private List<string> introductionParagraphs = new();

    [ObservableProperty]
    private FormattedString pageFormattedLabel = new();

    [ObservableProperty]
    private IReadOnlyList<string> noticeSections = Array.Empty<string>();

    [ObservableProperty]
    private double textSize;

    public ThirdPartyLicensesViewModel(
        DVViewModelFunctions dVViewModelFunctions,
        DVViewModelSpeak dVViewModelSpeak,
        KeyboardInputProcessor keyboardInputProcessor,
        ISettingsService settingsService,
        IAppEnvironment appEnvironment)
    {
        this.dVViewModelFunctions = dVViewModelFunctions;
        this.dVViewModelSpeak = dVViewModelSpeak;
        this.keyboardInputProcessor = keyboardInputProcessor;
        this.settingsService = settingsService;
        this.appEnvironment = appEnvironment;
    }

    public async Task LoadAsync()
    {
        dVViewModelFunctions.SetCurrentPageIdentifier("na tela de licenças de terceiros");
        dVViewModelFunctions.ClearHelpOptions();
        TextSize = settingsService.Get<double>("fontSize");
        if (!isLoaded)
        {
            try
            {
                await using Stream stream = await FileSystem.OpenAppPackageFileAsync(NoticesFileName);
                using StreamReader reader = new(stream);
                string notices = reader.ReadToEnd();
                if (TrySplitNotice(notices, out string introduction, out string licenseNotices))
                {
                    introductionParagraphs = SplitIntoParagraphs(introduction);
                    NoticeSections = SplitIntoSections(licenseNotices);
                }
                else
                {
                    introductionParagraphs.Clear();
                    NoticeSections = SplitIntoSections(notices);
                }
                isLoaded = true;
            }
            catch (Exception)
            {
                introductionParagraphs = new()
                {
                    "Não foi possível carregar os avisos de licenças de terceiros."
                };
                NoticeSections = Array.Empty<string>();
            }
        }

        SpeakIntroduction();
            WeakReferenceMessenger.Default.Send(new RequestFirstResponderMessage());
    }

    private static bool TrySplitNotice(
        string notices,
        out string introduction,
        out string licenseNotices)
    {
        int separatorIndex = notices.IndexOf(SectionSeparator, StringComparison.Ordinal);
        if (separatorIndex > 0 && separatorIndex <= MaximumSpokenIntroductionLength)
        {
            introduction = notices[..separatorIndex].Trim();
            licenseNotices = notices[separatorIndex..];
            return true;
        }

        introduction = string.Empty;
        licenseNotices = notices;
        return false;
    }

    private static List<string> SplitIntoParagraphs(string introduction)
    {
        return introduction
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Split("\n\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(paragraph => string.Join(
                " ",
                paragraph.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)))
            .ToList();
    }

    private void SpeakIntroduction()
    {
        if (introductionParagraphs.Count > 0)
        {
            dVViewModelSpeak.Skip();
            dVViewModelSpeak
                .SetTextAndSpeech(
                    new List<string>(introductionParagraphs),
                    new List<string>(introductionParagraphs))
                .RegisterUpdateScreen(formattedText => PageFormattedLabel = formattedText);
            dVViewModelSpeak.SpeakAll();
        }
    }

    private static IReadOnlyList<string> SplitIntoSections(string notices)
    {
        const int linesPerSection = 100;
        List<string> sections = new();
        StringBuilder section = new();
        int lineCount = 0;

        using StringReader reader = new(notices);
        while (reader.ReadLine() is { } line)
        {
            section.AppendLine(line);
            lineCount++;

            if (lineCount == linesPerSection)
            {
                sections.Add(section.ToString());
                section.Clear();
                lineCount = 0;
            }
        }

        if (section.Length > 0)
        {
            sections.Add(section.ToString());
        }

        return sections;
    }

    private void NavigateBack()
    {
        dVViewModelFunctions.HandleKeyCode("Escape");
    }

    public bool OnPageKeyDown(int keyCode)
    {
        return keyboardInputProcessor.KeyDown(keyCode);
    }

    public bool OnPageKeyPress(int keyCode, int modifiers)
    {
        var bean = keyboardInputProcessor.KeyUp(keyCode, modifiers);
        if (bean.code is not null)
        {
            dVViewModelSpeak.Skip();
            if (bean.code == "Escape" || (bean.code == "!" && appEnvironment.IsVirtualDevice))
            {
                NavigateBack();
            }
            else if (bean.code == " ")
            {
                SpeakIntroduction();
            }
        }

        return true;
    }
}
