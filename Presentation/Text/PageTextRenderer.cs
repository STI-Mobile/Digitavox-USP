// Copyright 2024-2026 Universidade de São Paulo
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0

using Digitavox.Core.Abstractions;

namespace Digitavox.Presentation.Text;

public sealed class PageTextRenderer
{
    private readonly ISettingsService settingsService;
    private readonly IAppEnvironment appEnvironment;

    public PageTextRenderer(ISettingsService settingsService, IAppEnvironment appEnvironment)
    {
        this.settingsService = settingsService;
        this.appEnvironment = appEnvironment;
    }

    public FormattedString Render(
        IReadOnlyList<string> lines,
        int boldLineIndex,
        bool isExercisePage,
        IReadOnlyDictionary<int, Tuple<string, int>> characterStyles)
    {
        FormattedString result = new();
        foreach ((string line, int index) in lines.Select((line, index) => (line, index)))
        {
            if (isExercisePage && characterStyles.TryGetValue(index, out Tuple<string, int> style))
            {
                AddStyledLine(result, line, index, boldLineIndex, isExercisePage, style);
            }
            else
            {
                result.Spans.Add(CreateSpan(line, index, boldLineIndex, isExercisePage, string.Empty));
            }

            result.Spans.Add(new Span { Text = "\n" });
            if (isExercisePage && index <= 4)
            {
                result.Spans.Add(new Span { Text = "\n" });
            }
        }
        return result;
    }

    private void AddStyledLine(
        FormattedString result,
        string line,
        int lineIndex,
        int boldLineIndex,
        bool isExercisePage,
        Tuple<string, int> style)
    {
        int characterIndex = style.Item2;
        if (characterIndex < 0 || characterIndex >= line.Length)
        {
            result.Spans.Add(CreateSpan(line, lineIndex, boldLineIndex, isExercisePage, string.Empty));
            return;
        }

        result.Spans.Add(CreateSpan(line[..characterIndex], lineIndex, boldLineIndex, isExercisePage, string.Empty));
        result.Spans.Add(CreateSpan(line[characterIndex].ToString(), lineIndex, boldLineIndex, isExercisePage, style.Item1));
        result.Spans.Add(CreateSpan(line[(characterIndex + 1)..], lineIndex, boldLineIndex, isExercisePage, string.Empty));
    }

    private Span CreateSpan(
        string text,
        int lineIndex,
        int boldLineIndex,
        bool isExercisePage,
        string characterStyle)
    {
        double fontSize = settingsService.Get<double>("fontSize");
        if (isExercisePage && lineIndex is >= 2 and <= 4)
        {
            fontSize += 4.0;
        }

        FontAttributes fontAttributes = boldLineIndex == lineIndex || characterStyle == "bold"
            ? FontAttributes.Bold
            : FontAttributes.None;
        Color color = appEnvironment.ShouldUseLightForeground ? Colors.White : Colors.Black;
        if (characterStyle == "green") color = Colors.Green;
        if (characterStyle == "red") color = Colors.Red;

        return new Span
        {
            Text = text,
            FontSize = fontSize,
            FontAttributes = fontAttributes,
            TextColor = color
        };
    }
}
