// Copyright 2024-2026 Universidade de São Paulo
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0

using Digitavox.Models;

namespace Digitavox.Presentation.Input;

public sealed class KeyboardInputProcessor
{
    private readonly FingerMapping fingerMapping;
    private readonly KeyboardInputState state = new();
    private readonly List<string> pressedKeys = new();

    public KeyboardInputProcessor(FingerMapping fingerMapping)
    {
        this.fingerMapping = fingerMapping;
    }

    public bool KeyDown(int keyCode)
    {
        string code = fingerMapping.mapKeyCode(keyCode);
        if (!pressedKeys.Contains(code))
        {
            pressedKeys.Add(code);
        }
        return true;
    }

    public FingerMappingBean KeyUp(int keyCode, int modifiers)
    {
        pressedKeys.Remove(fingerMapping.mapKeyCode(keyCode));
        return fingerMapping.MapKey(keyCode, modifiers, pressedKeys, state);
    }

    public string DescribeKey(string code) => fingerMapping.Code2Speak(code);
}
