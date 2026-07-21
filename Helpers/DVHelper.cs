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

using CommunityToolkit.Mvvm.Messaging.Messages;

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

  ///
  /// <summary>
  ///   Funções para tratar modificadores de teclas
  ///
  ///   public static void SetXxx(ref int keyModifiers)
  ///   public static void ResetXxx(ref int keyModifiers)
  ///   public static bool IsXxxSet(int keyModifiers)
  ///
  ///   Xxx = CapsLock, Shift, Ctrl, OpttionAlt, Command, NumLock, AltGr, Fn
  ///
  ///   public static void SetModifier(Modifier modifier, ref int keyModifier)
  ///   public static void ResetModifier(Modifier modifier, ref int keyModifier)
  ///   public static bool IsModifierSet(Modifier modifier, int keyModifier)
  /// </summary>
  /// 
  /// <param name="modifier">
  ///   enum Modifier {CapsLock, Shift, Ctrl, Alt, Window, NumLock, AltGr, Fn,
  ///                  Option, Command}
  /// </param>
  ///
  /// <param name="keyModifier">
  ///   bit0 - CapsLock
  ///   bit1 - Shift
  ///   bit2 - Ctrl
  ///   bit3 - Option / Alt
  ///   bit4 - Command / Window
  ///   bit5 - NumLock
  ///   bit6 - AltGr
  ///   bit7 - Fn
  /// </param>
  ///
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

  /// <summary>
  ///   Funções para detectar tipo de plataforma
  /// </summary>
  /// <returns>
  ///   TRUE se corresponde à plataforma
  ///   FALSE se não corresponde à plataforma
  /// </returns>
  public static class DVDevice {

    public static bool IsAndroid() =>
      DeviceInfo.Current.Platform == DevicePlatform.Android;

    public static bool IsVirtual() =>
      DeviceInfo.Current.DeviceType == DeviceType.Virtual;

    public static bool IsIos() =>
      DeviceInfo.Current.Platform == DevicePlatform.iOS;

    public static bool IsMac() =>
      DeviceInfo.Current.Platform == DevicePlatform.MacCatalyst;

    public static bool IsWindows() =>
      DeviceInfo.Current.Platform == DevicePlatform.WinUI;

  } 

  /// <summary>
  ///   Classe auxiliar para mensagens para usar com WeakReferenceMessenger
  /// </summary>
  public class DVMessage : ValueChangedMessage<string> {

    public DVMessage(string value) : base(value) { }

  } 

} 

