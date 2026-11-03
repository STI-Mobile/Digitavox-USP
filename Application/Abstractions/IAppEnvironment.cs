// Copyright 2024-2026 Universidade de São Paulo
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0

namespace Digitavox.Core.Abstractions;

public enum AppPlatformKind
{
    Unknown,
    Android,
    Ios,
    MacCatalyst,
    Windows
}

public interface IAppEnvironment
{
    AppPlatformKind Platform { get; }

    bool IsVirtualDevice { get; }

    bool IsScreenReaderEnabled { get; }

    bool ShouldUseLightForeground { get; }

    void RunOnMainThread(Action action);
}
