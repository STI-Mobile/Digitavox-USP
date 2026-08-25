// Copyright 2024-2026 Universidade de São Paulo
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0

using Digitavox.Core.Abstractions;
using Digitavox.PlatformsImplementations;

namespace Digitavox.Infrastructure.Speech;

public sealed class MauiSpeechService : ISpeechService
{
    public void Cancel() => DVSpeak.GetInstance().Cancel();

    public void Speak(string text, Action onCompleted) =>
        DVSpeak.GetInstance().SpeakText(text, onCompleted);

    public void SetRate(int rate) => DVSpeak.GetInstance().SetSpeechRate(rate);
}
