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

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Speech.Tts;
using Android.Views;
using Android.Views.Accessibility;
using CommunityToolkit.Mvvm.Messaging;
using Digitavox.Helpers;
using Digitavox.Models;
using Digitavox.PlatformsImplementations;
using Java.Util;
using Locale = Java.Util.Locale;
using TextToSpeech = Android.Speech.Tts.TextToSpeech;

namespace Digitavox;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]

public class MainActivity : MauiAppCompatActivity, TextToSpeech.IOnInitListener, TextToSpeech.IOnUtteranceCompletedListener
{
    private TextToSpeech textToSpeech;
    private AccessibilityManager am;
    private bool active;

    protected override void OnCreate(Bundle savedInstanceState) {
        base.OnCreate(savedInstanceState);
        active = true;
        Window.SetStatusBarColor(Android.Graphics.Color.Rgb(253, 180, 32));
        am = (AccessibilityManager)GetSystemService(Context.AccessibilityService);
        WeakReferenceMessenger.Default.Register<DVMessage>(this, (r, m) => {
            if (m.Value == "WindowActivated")
            {
                
            }
        });
       
    }
    private void CheckTalkbackHandler(object sender, EventArgs e)
    {
        if (active) CheckTalkback();
    }
    private void CheckTalkback()
    {
        if (am.IsEnabled && am.IsTouchExplorationEnabled)
        {
            WeakReferenceMessenger.Default.Send(new DVMessage("DisplayAlertDialog"));
        }
        else
        {
            WeakReferenceMessenger.Default.Send(new DVMessage("DismissAlertDialog"));
        }
    }
    protected override void OnPause()
    {
        base.OnPause();
        active = false;
    }
    protected override void OnResume()
    {
        DVSpeak.GetInstance().Init(this);
        base.OnResume();
        active = true;
        
        
        FullScreen();
    }
    public TextToSpeech createTextToSpeech()
    {
        textToSpeech = new TextToSpeech(this, this);
        textToSpeech.SetSpeechRate(DVPersistence.Get<int>("speakRate"));
        return textToSpeech;
    }
    public void OnInit([GeneratedEnum] OperationResult status)
    {
        if (status == OperationResult.Success)
        {
            textToSpeech.SetLanguage(new Locale("pt", "BR"));
            textToSpeech.SetOnUtteranceCompletedListener(this);
            DVSpeak.GetInstance().InitCompleted();
        }
    }
    public void OnUtteranceCompleted(string utteranceId)
    {
        DVSpeak.GetInstance().Completed(utteranceId);
    }
    public override bool OnKeyDown([GeneratedEnum] Keycode keyCode, KeyEvent e)
    {
        Page p = Shell.Current.CurrentPage;
        if (p is IOnPageKeyPress)
        {
            bool handled = (p as IOnPageKeyPress).OnPageKeyDown((int)keyCode);
            if (handled) return true;
            else return base.OnKeyDown(keyCode, e);
        }
        else return base.OnKeyDown(keyCode, e);
    }
    public override bool OnKeyUp([GeneratedEnum] Keycode keyCode, KeyEvent e)
    {
        Page p = Shell.Current.CurrentPage;
        if (p is IOnPageKeyPress)
        {
          int keyModifiers = 0;
          if (e.IsCapsLockOn)
            DVKeyboard.SetModifier(Modifier.CapsLock, ref keyModifiers);
          if (e.IsCtrlPressed)
            DVKeyboard.SetModifier(Modifier.Ctrl, ref keyModifiers);
          if (e.IsAltPressed)
            DVKeyboard.SetModifier(Modifier.AltGr, ref keyModifiers);
          if (e.IsFunctionPressed)
            DVKeyboard.SetModifier(Modifier.Fn, ref keyModifiers);
          if (e.IsShiftPressed)
            DVKeyboard.SetModifier(Modifier.Shift, ref keyModifiers);
          if (e.IsNumLockOn)
            DVKeyboard.SetModifier(Modifier.NumLock, ref keyModifiers);
          
          
          bool handled = (p as IOnPageKeyPress).OnPageKeyPress((int)keyCode,
                  keyModifiers);
          if (handled) return true;
              else return base.OnKeyUp(keyCode, e);
          }
          else return base.OnKeyUp(keyCode, e);
    }
    private void FullScreen()
    {
        
        
        
        this.Window.DecorView.SystemUiVisibility = (StatusBarVisibility)
            (SystemUiFlags.HideNavigation | SystemUiFlags.Immersive);
    }
}