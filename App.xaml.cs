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

using CommunityToolkit.Mvvm.Messaging;
using Digitavox.Helpers;
using Digitavox.ViewModels;
using Digitavox.Views;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Digitavox;

public partial class App : Application
{
	public App(DVViewModelSpeak dVViewModelSpeak, DVViewModelFunctions dVViewModelFunctions)
	{
		InitializeComponent();

        WeakReferenceMessenger.Default.Register<DVMessage>(this, (r, m) => 
        {
            if (m.Value == "WindowStopped")
            {
            #if __ANDROID__
                dVViewModelSpeak.Skip();
            #endif
            

            }
            else if (m.Value == "WindowResumed")
            {
                string currentPageMessage = $"Você está {dVViewModelFunctions.CurrentPageIdentifier()}";
                dVViewModelSpeak.Skip();
                dVViewModelSpeak.Speak(currentPageMessage, () => { });
            }
            if (m.Value == "DisplayAlertDialog")
            {
                dVViewModelSpeak.Skip();
                
                dVViewModelFunctions.DisplayAlert();
            }
            else if (m.Value == "DismissAlertDialog")
            {
                dVViewModelFunctions.DismissAlert();
            }
        });

        

    }
    
    
    
    
    
    protected override Window CreateWindow(IActivationState? activationState)
    {
        Window window = new Window(new AppShell());
        window.Created += (s, e) =>
        {
            WeakReferenceMessenger.Default.Send(new DVMessage("WindowCreated"));
        };
        window.Activated += (s, e) =>
        {
            WeakReferenceMessenger.Default.Send(new DVMessage("WindowActivated"));
        };
        window.Deactivated += (s, e) =>
        {
            WeakReferenceMessenger.Default.Send(new DVMessage("WindowDeactivated"));
        };
        window.Stopped += (s, e) =>
        {
            WeakReferenceMessenger.Default.Send(new DVMessage("WindowStopped"));
        };
        window.Resumed += (s, e) =>
        {
            WeakReferenceMessenger.Default.Send(new DVMessage("WindowResumed"));
        };
        window.Destroying += (s, e) =>
        {
            WeakReferenceMessenger.Default.Send(new DVMessage("WindowDestroying"));
        };
        return window;
    }

    private void OnRequestedThemeChanged(object sender, AppThemeChangedEventArgs e)
    {
        Dispatcher.Dispatch(() =>
        {
            var theme = e.RequestedTheme;
            if (theme == AppTheme.Dark)
            {
                Resources["DynamicTextColor"] = Resources["TextColorDark"];
            }
            else
            {
                Resources["DynamicTextColor"] = Resources["TextColorLight"];
            }
        });
    }

    protected override void OnStart()
    {
        base.OnStart();
    }

    
    protected override void OnSleep()
    {
        base.OnSleep();
        
    }

    
    protected override void OnResume()
    {
        base.OnResume();
        
    }

}
