// Copyright 2024-2026 Universidade de São Paulo
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0

using Digitavox.Core.Abstractions;
using Digitavox.Helpers;

namespace Digitavox.Infrastructure.Settings;

public sealed class MauiSettingsService : ISettingsService
{
    public double DefaultFontSize => DVPersistence.GetFontSize();

    public T Get<T>(string key) => DVPersistence.Get<T>(key);

    public void Set<T>(string key, T value) => DVPersistence.Set(key, value);

    public void ResetDefaults() => DVPersistence.SetDefaulConfig();
}
