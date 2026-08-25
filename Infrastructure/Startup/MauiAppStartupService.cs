// Copyright 2024-2026 Universidade de São Paulo
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0

using Digitavox.Core.Abstractions;
using Digitavox.Helpers;
using Digitavox.Models;

namespace Digitavox.Infrastructure.Startup;

public sealed class MauiAppStartupService : IAppStartupService
{
    private readonly FingerMapping fingerMapping;

    public MauiAppStartupService(FingerMapping fingerMapping)
    {
        this.fingerMapping = fingerMapping;
    }

    public async Task<AppStartupDestination> InitializeAsync()
    {
        await fingerMapping.InitializeAsync();

        if (DVPersistence.CourseDirectoryExists())
        {
            return AppStartupDestination.Login;
        }

        await DVPersistence.CopyCourseFilesAsync();
        return AppStartupDestination.Tutorial;
    }
}
