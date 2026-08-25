// Copyright 2024-2026 Universidade de São Paulo
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0

using Digitavox.Core.Abstractions;
using Digitavox.Helpers;

namespace Digitavox.Infrastructure.Platform;

public sealed class MauiAppEnvironment : IAppEnvironment
{
    public AppPlatformKind Platform
    {
        get
        {
            if (DeviceInfo.Platform == DevicePlatform.Android) return AppPlatformKind.Android;
            if (DeviceInfo.Platform == DevicePlatform.iOS) return AppPlatformKind.Ios;
            if (DeviceInfo.Platform == DevicePlatform.MacCatalyst) return AppPlatformKind.MacCatalyst;
            if (DeviceInfo.Platform == DevicePlatform.WinUI) return AppPlatformKind.Windows;
            return AppPlatformKind.Unknown;
        }
    }

    public bool IsVirtualDevice => DeviceInfo.Current.DeviceType == DeviceType.Virtual;

    public bool IsScreenReaderEnabled => DVVoiceOverHelper.IsVoiceOverEnabled();

    public bool ShouldUseLightForeground =>
        AccessibilityHelper.IsHighContrastEnabled() ||
        Microsoft.Maui.Controls.Application.Current.RequestedTheme != AppTheme.Light;

    public void RunOnMainThread(Action action) => MainThread.BeginInvokeOnMainThread(action);
}
