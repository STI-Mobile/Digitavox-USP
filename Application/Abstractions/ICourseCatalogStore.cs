// Copyright 2024-2026 Universidade de São Paulo
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0

using System.Text.Json;

namespace Digitavox.Core.Abstractions;

public sealed record CourseCatalogData(
    List<JsonElement> Courses,
    List<string> FileNames,
    List<string> CourseNames);

public interface ICourseCatalogStore
{
    CourseCatalogData Load();
}
