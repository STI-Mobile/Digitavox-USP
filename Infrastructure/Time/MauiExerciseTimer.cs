// Copyright 2024-2026 Universidade de São Paulo
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0

using Digitavox.Core.Abstractions;

namespace Digitavox.Infrastructure.Time;

public sealed class MauiExerciseTimer : IExerciseTimer
{
    private IDispatcherTimer timer;
    private Action elapsed;

    public void Start(TimeSpan interval, Action elapsed)
    {
        this.elapsed = elapsed;
        timer ??= CreateTimer();
        timer.Stop();
        timer.Interval = interval;
        timer.Start();
    }

    public void Stop() => timer?.Stop();

    private IDispatcherTimer CreateTimer()
    {
        IDispatcherTimer dispatcherTimer =
            Microsoft.Maui.Controls.Application.Current.Dispatcher.CreateTimer();
        dispatcherTimer.Tick += (_, _) => elapsed?.Invoke();
        return dispatcherTimer;
    }
}
