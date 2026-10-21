// Copyright 2024-2026 Universidade de São Paulo
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0

using Digitavox.Core.Abstractions;
using Digitavox.Helpers;

namespace Digitavox.Infrastructure.Persistence;

public sealed class MauiCourseCatalogStore : ICourseCatalogStore
{
    private readonly CatalogMigration migration;
    public MauiCourseCatalogStore(CatalogMigration migration) => this.migration = migration;

    public CourseCatalogData Load()
    {
        if (migration.ReadVersion() != CatalogMigration.CurrentVersion || migration.IsPending)
            throw new InvalidOperationException("Catálogo indisponível até a conclusão da migração.");
        var (courses, fileNames, courseNames) = DVPersistence.ReadCourseFiles(CatalogMigration.ActiveCourseIds);
        if (!fileNames.SequenceEqual(CatalogMigration.ActiveCourseIds.Select(id => id + ".json")) ||
            !courses.Select(c => c.GetProperty("CURSO").GetProperty("NUMEROCURSO").ToString()).SequenceEqual(new[] { "3", "4", "5" }) ||
            courses[0].GetProperty("CURSO").GetProperty("QUANTIDADELICOES").ToString() != "4" || courses[0].TryGetProperty("LICAO5", out _))
            throw new InvalidDataException("Catálogo incompleto ou incompatível; reinicie para recuperar a migração.");
        return new CourseCatalogData(courses, fileNames, courseNames);
    }
}
