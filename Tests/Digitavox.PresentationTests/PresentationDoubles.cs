using Digitavox.Core.Abstractions;
using Microsoft.Maui.Controls;

namespace Digitavox.ViewModels
{
    public class DVViewModelSpeak
    {
        public List<string> Lines { get; private set; } = new();
        public DVViewModelSpeak SetTextAndSpeech(List<string> text, List<string> speech) { Lines = text; return this; }
        public void RegisterUpdateScreen(Action<FormattedString> update) => update(new FormattedString());
        public void SpeakAll() { }
        public void SpeakOneLine(int line, Action completed) => completed();
        public void Speak(string text, Action completed) => completed();
        public void ChangeLine(string text, string speech, int line) => Lines[line] = text;
        public void Skip() { }
        public void Set(string key, bool value) { }
        public int LineCount() => Lines.Count;
    }

    public class DVViewModelFunctions
    {
        public int Item { get; set; } = 2;
        public List<string> Keys { get; } = new();
        public bool ClearedHelp { get; private set; }
        public int MapOptionToItem() => Item;
        public void ClearHelpOptions() => ClearedHelp = true;
        public void SetCurrentPageIdentifier(string text) { }
        public string EditStringForVoiceOver(string text) => text;
        public void SetFirstOptionLineNumber(int line) { }
        public void SetLastOptionLineNumber(int line) { }
        public void SetOptionNumberStart(int line) { }
        public void SetNumberCaptureTimeInterval(int value) { }
        public void LastLineIsText(bool value) { }
        public void HandleKeyCode(string key) => Keys.Add(key);
        public void InvalidOption(string text) { }
    }
}

namespace Digitavox.Presentation.Input
{
    public class KeyboardInputProcessor
    {
        public bool KeyDown(int code) => true;
        public (string code, string speakOnlyChar) KeyUp(int code, int modifiers) => code switch
        {
            13 => ("Enter", "enter"), 27 => ("Escape", "escape"), 40 => ("Down", "baixo"), _ => ("3", "três")
        };
    }
}

namespace Digitavox.Helpers
{
    public static class DVKeyboard { public static bool IsModifierKey(string code) => false; }
}
