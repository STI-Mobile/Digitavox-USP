// Copyright 2024-2026 Universidade de São Paulo
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0

using Digitavox.Core.Abstractions;

namespace Digitavox.Infrastructure.Navigation;

public sealed class ShellNavigationService : INavigationService
{
    public Task GoToAsync(AppRoute route, int backLevels = 0) =>
        Shell.Current.GoToAsync(BuildRoute(route.ToString(), backLevels));

    public Task GoBackAsync(int levels = 1) =>
        Shell.Current.GoToAsync(BuildRoute(null, levels));

    private static string BuildRoute(string route, int backLevels)
    {
        if (backLevels < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(backLevels));
        }

        string backPath = string.Join('/', Enumerable.Repeat("..", backLevels));
        if (string.IsNullOrEmpty(backPath)) return route ?? string.Empty;
        if (string.IsNullOrEmpty(route)) return backPath;
        return $"{backPath}/{route}";
    }
}
