// Copyright 2024-2026 Universidade de São Paulo
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Digitavox.Helpers;

namespace Digitavox.Models
{
    public class FingerMapping
    {
        private JsonElement fingers;
        private JsonElement keysAndFingers;
        private JsonElement keysIos2Android;
        private JsonElement keysWindows2Android;
        private List<string> releasedKeysPressedTogether = new List<string>();
        private bool altGrPressedOnPreviousKey;
        private bool shiftPressedOnPreviousKey;
        private bool controlPressedOnPreviousKey;
        private bool functionPressedOnPreviousKey;
        private bool acuteAccentOnPreviousKey;
        private bool circumflexAccentOnPreviousKey;
        private bool graveAccentOnPreviousKey;
        private bool tildeAccentOnPreviousKey;
        private List<string> charsWithCaps = new List<string>(new string[] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "ç" });
        private List<string> keysWithCaps = new List<string>();
        public string mapKeyCode(int intKey)
        {
            var key = intKey.ToString();
            if ((DVDevice.IsIos() || DVDevice.IsMac()) && keysIos2Android.TryGetProperty(key, out var mapping2))
            {
                key = mapping2.GetString();
            }
            else if (DVDevice.IsWindows() && keysWindows2Android.TryGetProperty(key, out var mapping3))
            {
                key = mapping3.GetString();
            }
            if (keysAndFingers.TryGetProperty(key, out var mapping))
            {
                return mapping.GetProperty("code").GetString();
            }
            return "";
        }
        public FingerMappingBean MapKey(int intKey, int modifiers, List<string> pressedKeys)
        {
            var key = intKey.ToString();
            if ((DVDevice.IsIos() || DVDevice.IsMac()) && keysIos2Android.TryGetProperty(key, out var mapping4))
            {
                key = mapping4.GetString();
            }
            else if (DVDevice.IsWindows() && keysWindows2Android.TryGetProperty(key, out var mapping8))
            {
                key = mapping8.GetString();
            }
            var shiftPressed = DVKeyboard.IsModifierSet(Modifier.Shift, modifiers) || releasedKeysPressedTogether.Contains("Shift");
            if (DVKeyboard.IsModifierSet(Modifier.CapsLock, modifiers) && keysWithCaps.Contains(key))
            {
                shiftPressed = !shiftPressed;
            }
            var numLockOn = DVKeyboard.IsModifierSet(Modifier.NumLock, modifiers);
            if ((DVKeyboard.IsModifierSet(Modifier.Ctrl, modifiers) || releasedKeysPressedTogether.Contains("Ctrl")) && keysAndFingers.TryGetProperty(key + "+c", out var mapping6))
            {
                key += "+c";
            } 
            else if (shiftPressed && keysAndFingers.TryGetProperty(key + "+s", out var mapping2))
            {
                key += "+s";
            }
            else if (!numLockOn && keysAndFingers.TryGetProperty(key + "-nl", out var mapping3))
            {
                key += "-nl";
            }
            else if ((DVKeyboard.IsModifierSet(Modifier.AltGr, modifiers) || releasedKeysPressedTogether.Contains("AltGr")) && keysAndFingers.TryGetProperty(key + "+ag", out var mapping7))
            {
                key += "+ag";
            }
            var result = new FingerMappingBean("tecla não mapeada " + key, key, key, key, key, key);
            if (keysAndFingers.TryGetProperty(key, out var mapping))
            {
                var code = mapping.GetProperty("code").GetString();
                bool withModifiers = "Shift".Equals(code) || "Ctrl".Equals(code) || "Function".Equals(code) || "AltGr".Equals(code);
                bool onlyModifiers = true;
                foreach (var pressedKey in pressedKeys)
                {
                    if ("Shift".Equals(pressedKey) || "Ctrl".Equals(pressedKey) || "Function".Equals(pressedKey) || "AltGr".Equals(pressedKey))
                    {
                        withModifiers = true;
                    } else
                    {
                        onlyModifiers = false;
                    }
                }
                
                if (pressedKeys.Count > 0 && withModifiers && !onlyModifiers)
                {
                    releasedKeysPressedTogether.Add(code);
                    result = new FingerMappingBean(null, null, null, null, null, null);
                }
                else if (!(("Shift".Equals(code) && shiftPressedOnPreviousKey) 
                        || ("Ctrl".Equals(code) && controlPressedOnPreviousKey)
                        || ("Function".Equals(code) && functionPressedOnPreviousKey)
                        || ("AltGr".Equals(code) && altGrPressedOnPreviousKey)
                        || ("´".Equals(code) && !acuteAccentOnPreviousKey)
                        || ("^".Equals(code) && !circumflexAccentOnPreviousKey)
                        || ("`".Equals(code) && !graveAccentOnPreviousKey)
                        || ("~".Equals(code) && !tildeAccentOnPreviousKey)))
                {
                    releasedKeysPressedTogether.Clear();
                    if (acuteAccentOnPreviousKey && ("A".Equals(code.ToUpper()) || "E".Equals(code.ToUpper()) || "I".Equals(code.ToUpper()) || "O".Equals(code.ToUpper()) || "U".Equals(code.ToUpper())))
                    {
                        key += "+aa";
                        if (keysAndFingers.TryGetProperty(key, out var mapping5))
                        {
                            mapping = mapping5;
                        }
                    }
                    else if (circumflexAccentOnPreviousKey && ("A".Equals(code) || "E".Equals(code) || "O".Equals(code) || "a".Equals(code) || "e".Equals(code) || "o".Equals(code)))
                    {
                        key += "+ca";
                        if (keysAndFingers.TryGetProperty(key, out var mapping5))
                        {
                            mapping = mapping5;
                        }
                    }
                    else if (graveAccentOnPreviousKey && ("A".Equals(code.ToUpper())))
                    {
                        key += "+ga";
                        if (keysAndFingers.TryGetProperty(key, out var mapping5))
                        {
                            mapping = mapping5;
                        }
                    }
                    else if (tildeAccentOnPreviousKey && ("A".Equals(code.ToUpper()) || "O".Equals(code.ToUpper())))
                    {
                        key += "+ta";
                        if (keysAndFingers.TryGetProperty(key, out var mapping5))
                        {
                            mapping = mapping5;
                        }
                    }
                    code = mapping.GetProperty("code").GetString();
                    var show = mapping.GetProperty("code").GetString();
                    if (mapping.TryGetProperty("screen", out var newShow))
                    {
                        show = newShow.GetString();
                    }
                    var showOnlyChar = show;
                    var speakOnlyChar = mapping.GetProperty("speak").GetString();
                    var speak = mapping.GetProperty("speak").GetString() + " " + fingers.GetProperty(mapping.GetProperty("finger").GetString()).GetString();
                    if (mapping.TryGetProperty("explanation", out var explanation))
                    {
                        speak += " " + explanation;
                        show += " " + explanation;
                    }
                    result = new FingerMappingBean(speak, show, code, speakOnlyChar, showOnlyChar, key);
                } 
                else
                {
                    releasedKeysPressedTogether.Clear();
                    result = new FingerMappingBean(null, null, null, null, null, null);
                }
                shiftPressedOnPreviousKey = DVKeyboard.IsModifierSet(Modifier.Shift, modifiers) && !"Shift".Equals(code);
                controlPressedOnPreviousKey = DVKeyboard.IsModifierSet(Modifier.Ctrl, modifiers) && !"Ctrl".Equals(code);
                functionPressedOnPreviousKey = DVKeyboard.IsModifierSet(Modifier.Fn, modifiers) && !"Function".Equals(code);
                altGrPressedOnPreviousKey = DVKeyboard.IsModifierSet(Modifier.AltGr, modifiers) && !"AltGr".Equals(code);
                acuteAccentOnPreviousKey = "´".Equals(code);
                if (!"Shift".Equals(code))
                {
                    circumflexAccentOnPreviousKey = "^".Equals(code);
                    graveAccentOnPreviousKey = "`".Equals(code);
                }
                tildeAccentOnPreviousKey = "~".Equals(code);
            }
            return result;
        }
        public string Code2Key(string key,string code)
        {
            bool foundKey;
            string keyValue = string.Empty;
            foreach (var topKeyValue in keysAndFingers.EnumerateObject())
            {
                foundKey = false;
                foreach (var baseKeyValue in topKeyValue.Value.EnumerateObject())
                {
                    if (baseKeyValue.NameEquals("code") && baseKeyValue.Value.ValueEquals(code))
                    {
                        foundKey = true;
                    }
                    else if (baseKeyValue.NameEquals(key))
                    {
                        keyValue = baseKeyValue.Value.ToString();
                    }
                }
                if (foundKey)
                {
                    return keyValue;
                }
            }
            return string.Empty;
        }
        public string Code2Speak(string code)
        {
            return Code2Key("speak", code);
        }
        public string Code2Finger(string code)
        {
            return fingers.GetProperty(Code2Key("finger", code)).GetString();
        }
        public FingerMapping()
        {
            Initialize();
        }
        private async void Initialize()
        {
            
            var stream = await FileSystem.OpenAppPackageFileAsync("Fingers.json");
            fingers = JsonDocument.Parse(new StreamReader(stream).ReadToEnd()).RootElement;
            
            stream = await FileSystem.OpenAppPackageFileAsync("Keys2Fingers.json");
            keysAndFingers = JsonDocument.Parse(new StreamReader(stream).ReadToEnd()).RootElement;
            
            stream = await FileSystem.OpenAppPackageFileAsync("KeysIos2Android.json");
            keysIos2Android = JsonDocument.Parse(new StreamReader(stream).ReadToEnd()).RootElement;
            stream = await FileSystem.OpenAppPackageFileAsync("KeysWindows2Android.json");
            keysWindows2Android = JsonDocument.Parse(new StreamReader(stream).ReadToEnd()).RootElement;
            foreach (var obj in keysAndFingers.EnumerateObject())
            {
                if (charsWithCaps.Contains(obj.Value.GetProperty("code").GetString()))
                {
                    keysWithCaps.Add(obj.Name);
                }
            }
        }
    }
}
