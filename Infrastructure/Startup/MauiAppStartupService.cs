// Copyright 2024-2026 Universidade de São Paulo
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0

using Digitavox.Core.Abstractions;
using Digitavox.Models;
using Digitavox.Infrastructure.Persistence;

namespace Digitavox.Infrastructure.Startup;

public sealed class MauiAppStartupService : IAppStartupService
{
    private readonly FingerMapping fingerMapping;
    private readonly CatalogMigration catalogMigration;
    private readonly Course course;

    public MauiAppStartupService(FingerMapping fingerMapping, CatalogMigration catalogMigration, Course course)
    {
        this.fingerMapping = fingerMapping;
        this.catalogMigration = catalogMigration;
        this.course = course;
    }

    public async Task<AppStartupDestination> InitializeAsync()
    {
        await fingerMapping.InitializeAsync();

        bool firstInstallation = await catalogMigration.EnsureCurrentAsync();
        course.ResetCatalog();
        course.GetCoursesLists();
        return firstInstallation ? AppStartupDestination.Tutorial : AppStartupDestination.Login;
    }
}
