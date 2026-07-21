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

using Digitavox.Helpers;
using Digitavox.Models;
using Digitavox.PlatformsImplementations;
using Microsoft.UI.Xaml;
using Windows.System;
using static Digitavox.Platforms.Windows.WinAPI;




namespace Digitavox.WinUI;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : MauiWinUIApplication
{
    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    private static int m_hHook = 0;
    private HookProc m_HookProcedure;
    public App()
	{
		this.InitializeComponent();
        DVSpeak.GetInstance().Init(this);
    }
    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs e)
    {
        base.OnLaunched(e);
        m_HookProcedure = new HookProc(HookProcedure);
        m_hHook = SetWindowsHookEx(WH_Keyboard, m_HookProcedure, (IntPtr)0, (int)GetCurrentThreadId());

    }

    private void HookProcedure(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode < 0) return;

        int keyFlags = HIWord(lParam);
        bool isKeyReleased = (keyFlags & KF_Up) == KF_Up;
        if (isKeyReleased)
        {
            OnKeyUp((uint)wParam);
        }
        else
        {
            OnKeyDown((uint)wParam);
        }

    }
    public bool OnKeyUp(uint keyCode)
    {
        Page p = Shell.Current.CurrentPage;
        bool IsShiftPressed = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);
        bool IsCtrlPressed = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Control).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);
        bool IsAltPressed = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Menu).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);
        bool IsCapsLockOn = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.CapitalLock).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Locked);
        bool IsNumLockOn = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.NumberKeyLock).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Locked);
        if (p is IOnPageKeyPress)
        {
            int keyModifiers = 0;
            if (IsCapsLockOn)
                DVKeyboard.SetModifier(Modifier.CapsLock, ref keyModifiers);
            if (IsCtrlPressed)
                DVKeyboard.SetModifier(Modifier.Ctrl, ref keyModifiers);
            if (IsAltPressed)
                DVKeyboard.SetModifier(Modifier.AltGr, ref keyModifiers);
            
            
            if (IsShiftPressed)
                DVKeyboard.SetModifier(Modifier.Shift, ref keyModifiers);
            if (IsNumLockOn)
                DVKeyboard.SetModifier(Modifier.NumLock, ref keyModifiers);

            bool handled = (p as IOnPageKeyPress).OnPageKeyPress((int)keyCode,
                    keyModifiers);
            if (handled) return true;
            else return false;
        }
        else return false;
    }
    public bool OnKeyDown(uint keyCode)
    {
        Page p = Shell.Current.CurrentPage;
        if (p is IOnPageKeyPress)
        {
            bool handled = (p as IOnPageKeyPress).OnPageKeyDown((int)keyCode);
            if (handled) return true;
            else return false;
        }
        else return false;
    }
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}

