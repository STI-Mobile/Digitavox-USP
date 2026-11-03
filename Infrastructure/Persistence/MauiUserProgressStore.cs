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

public sealed class MauiUserProgressStore : IUserProgressStore
{
    public Dictionary<string, object> Load(string userName) =>
        DVPersistence.LoadUserData(userName);

    public void Save(Dictionary<string, object> userData) =>
        DVPersistence.SaveUserJson(userData);
}
