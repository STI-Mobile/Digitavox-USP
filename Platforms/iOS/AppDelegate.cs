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

using Foundation;
using UIKit;
using Digitavox.ViewModels;
using Digitavox.Platforms.iOS;
using Microsoft.Extensions.DependencyInjection;

namespace Digitavox;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    private DVViewModelSpeak viewModelSpeak;

    protected override MauiApp CreateMauiApp()
    {
        MauiApp mauiApp = MauiProgram.CreateMauiApp();
        viewModelSpeak = mauiApp.Services.GetRequiredService<DVViewModelSpeak>();
        return mauiApp;
    }

    public override void OnActivated(UIApplication application)
    {
        base.OnActivated(application);
        AccessibilityHelper.PersistVoiceOverStatusAndNotify();
    }

    public override void OnResignActivation(UIApplication application)
    {
        base.OnResignActivation(application);
        viewModelSpeak.Skip();
        AccessibilityHelper.CheckVoiceOverStatusChangeAndNotify();
    }


}
