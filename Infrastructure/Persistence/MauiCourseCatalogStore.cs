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
    public CourseCatalogData Load()
    {
        var (courses, fileNames, courseNames) = DVPersistence.ReadCourseFiles();
        return new CourseCatalogData(courses, fileNames, courseNames);
    }
}
