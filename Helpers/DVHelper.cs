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

namespace Digitavox.Helpers {

  public enum Modifier {
    CapsLock = 0b_0000_0001,
    Shift    = 0b_0000_0010,
    Ctrl     = 0b_0000_0100,
    Alt      = 0b_0000_1000,
    Window   = 0b_0001_0000,
    NumLock  = 0b_0010_0000,
    AltGr    = 0b_0100_0000,
    Fn       = 0b_1000_0000,
    Option   = Alt,
    Command  = Window
  }

  /// <summary>
  /// Funções para compor, remover e consultar modificadores de teclado
  /// representados como uma máscara de bits.
  /// </summary>
  public static class DVKeyboard {

    public static void SetModifier(Modifier modifier, ref int keyModifier) {
      keyModifier |= (int)modifier;
    }

    public static void ResetModifier(Modifier modifier, ref int keyModifier) {
      keyModifier &= ~(int)modifier;
    }

    public static bool IsModifierSet(Modifier modifier, int keyModifier) {
      return ((keyModifier & (int)modifier) == (int)modifier);
    }

    public static bool IsModifierKey(string code)
        {
            return Enum.IsDefined(typeof(Modifier), code);
        }

  } 

} 
