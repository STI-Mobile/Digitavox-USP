// Copyright 2024-2026 Universidade de São Paulo
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0

using Digitavox.Core.Abstractions;
using Plugin.Maui.Audio;

namespace Digitavox.Infrastructure.Audio;

public sealed class MauiFeedbackSoundService : IFeedbackSoundService
{
    private const string ErrorSoundFileName = "buzz.wav";
    private readonly IAudioManager audioManager;
    private IAudioPlayer player;

    public MauiFeedbackSoundService(IAudioManager audioManager)
    {
        this.audioManager = audioManager;
    }

    public async Task PrepareAsync()
    {
        if (player != null) return;
        Stream stream = await FileSystem.OpenAppPackageFileAsync(ErrorSoundFileName);
        player = audioManager.CreatePlayer(stream);
    }

    public void PlayError() => player?.Play();
}
