using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Digitavox.Helpers;
using Digitavox.Models;
using System.Text;

namespace Digitavox.ViewModels;

public partial class ThirdPartyLicensesViewModel : ObservableObject, IOnPageKeyPress
{
    private const string NoticesFileName = "THIRD-PARTY-NOTICES.txt";
    private const string SectionSeparator = "============================================================";
    private const int MaximumSpokenIntroductionLength = 2_000;

    private readonly DVViewModelFunctions dVViewModelFunctions;
    private readonly DVViewModelSpeak dVViewModelSpeak;
    private readonly FingerMapping fingerMapping;
    private readonly List<string> pressedKeys = new();
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
        FingerMapping fingerMapping)
    {
        this.dVViewModelFunctions = dVViewModelFunctions;
        this.dVViewModelSpeak = dVViewModelSpeak;
        this.fingerMapping = fingerMapping;
    }

    public async Task LoadAsync()
    {
        dVViewModelFunctions.SetCurrentPageIdentifier("na tela de licenças de terceiros");
        dVViewModelFunctions.ClearHelpOptions();
        TextSize = DVPersistence.Get<double>("fontSize");
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
        WeakReferenceMessenger.Default.Send(new DVMessage("BecomeFirstResponder"));
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
        string code = fingerMapping.mapKeyCode(keyCode);
        if (!pressedKeys.Contains(code))
        {
            pressedKeys.Add(code);
        }

        return true;
    }

    public bool OnPageKeyPress(int keyCode, int modifiers)
    {
        pressedKeys.Remove(fingerMapping.mapKeyCode(keyCode));
        var bean = fingerMapping.MapKey(keyCode, modifiers, pressedKeys);
        if (bean.code is not null)
        {
            dVViewModelSpeak.Skip();
            if (bean.code == "Escape" || (bean.code == "!" && DVDevice.IsVirtual()))
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
