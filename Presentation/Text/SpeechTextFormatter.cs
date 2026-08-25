// Copyright 2024-2026 Universidade de São Paulo
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0

using Digitavox.Core.Abstractions;
using Digitavox.Models;

namespace Digitavox.Presentation.Text;

public sealed class SpeechTextFormatter : ISpeechTextFormatter
{
    private static readonly IReadOnlyDictionary<string, string> ScreenReaderTerms =
        new Dictionary<string, string>
        {
            { "Escape", "CTRL + Escape" },
            { "ESCAPE", "CTRL + ESCAPE" },
            { "esqueipe", "control esqueipe" },
            { "Esqueipe", "control esqueipe" },
            { "[ESC]", "[CTRL] + [ESC]" }
        };

    private readonly FingerMapping fingerMapping;
    private readonly IAppEnvironment appEnvironment;

    public SpeechTextFormatter(FingerMapping fingerMapping, IAppEnvironment appEnvironment)
    {
        this.fingerMapping = fingerMapping;
        this.appEnvironment = appEnvironment;
    }

    public string AdaptForScreenReader(string text)
    {
        if (appEnvironment.Platform != AppPlatformKind.Ios || !appEnvironment.IsScreenReaderEnabled)
        {
            return text;
        }

        foreach ((string term, string replacement) in ScreenReaderTerms)
        {
            text = text.Replace(term, replacement);
        }
        return text;
    }

    public string Spell(string text)
    {
        if (text.Length > 1 && text[^1] == ' ')
        {
            text = text[..^1];
        }

        return string.Concat(text.Select(character => $"{fingerMapping.Code2Speak(character.ToString())} "));
    }

    public string DescribePunctuation(string text)
    {
        return string.Concat(text.Select(character =>
            IsPunctuation(character)
                ? $" {fingerMapping.Code2Speak(character.ToString())} "
                : character.ToString()));
    }

    public string PreserveWordAfterLeadingPunctuation(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        if (char.IsLetter(text[0])) return text;

        string firstCharacter = $"{fingerMapping.Code2Speak(text[0].ToString())} ";
        return text.Length == 1
            ? firstCharacter
            : firstCharacter + PreserveWordAfterLeadingPunctuation(text[1..]);
    }

    private static bool IsPunctuation(char character) => ".,;:?!".Contains(character);
}
