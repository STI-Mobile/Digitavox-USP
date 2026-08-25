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
using Digitavox.ViewModels;
using Digitavox.Core.Abstractions;
using Digitavox.Models;
using Digitavox.Core.Messages;
using Digitavox.Views;
using Microsoft.Extensions.Logging;

namespace Digitavox;

public partial class App : Application
{
    private readonly IAppStartupService appStartupService;
    private readonly CourseLesson courseLesson;
    private readonly DVViewModelSpeak dVViewModelSpeak;
    private readonly ICurrentPageContext currentPageContext;
    private readonly ILogger<App> logger;

    public App(
        IAppStartupService appStartupService,
        DVViewModelSpeak dVViewModelSpeak,
        DVViewModelFunctions dVViewModelFunctions,
        ICurrentPageContext currentPageContext,
        CourseLesson courseLesson,
        ILogger<App> logger)
    {
        this.appStartupService = appStartupService;
        this.courseLesson = courseLesson;
        this.dVViewModelSpeak = dVViewModelSpeak;
        this.currentPageContext = currentPageContext;
        this.logger = logger;
        InitializeComponent();

        WeakReferenceMessenger.Default.Register<ShowSpeechCompatibilityAlertMessage>(this, (r, m) =>
        {
            this.dVViewModelSpeak.Skip();
            _ = dVViewModelFunctions.DisplayAlertAsync();
        });
        WeakReferenceMessenger.Default.Register<HideSpeechCompatibilityAlertMessage>(this, (r, m) =>
        {
            _ = dVViewModelFunctions.DismissAlertAsync();
        });
    }

    protected override Window CreateWindow(IActivationState activationState)
    {
        Window window = new Window(new StartupView());
        window.Created += OnWindowCreated;
        window.Activated += (s, e) =>
        {
            if (window.Page is AppShell)
            {
                this.courseLesson.ContinueTimer();
            }
        };
        window.Deactivated += (s, e) =>
        {
            this.courseLesson.PauseTimer();
        };
        window.Stopped += (s, e) =>
        {
            this.courseLesson.PauseTimer();
#if __ANDROID__
            dVViewModelSpeak.Skip();
#endif
        };
        window.Resumed += (s, e) =>
        {
            if (window.Page is not AppShell)
            {
                return;
            }

            this.courseLesson.ContinueTimer();
            string currentPageMessage = $"Você está {this.currentPageContext.Identifier}";
            this.dVViewModelSpeak.Skip();
            this.dVViewModelSpeak.Speak(currentPageMessage, () => { });
        };
        window.Destroying += (s, e) =>
        {
            this.courseLesson.PauseTimer();
        };
        return window;
    }

    private async void OnWindowCreated(object sender, EventArgs e)
    {
        if (sender is not Window window)
        {
            return;
        }

        try
        {
            AppStartupDestination destination = await appStartupService.InitializeAsync();
            window.Page = new AppShell(destination);
            courseLesson.ContinueTimer();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Falha ao inicializar o aplicativo.");

            if (window.Page is StartupView startupView)
            {
                startupView.ShowInitializationError();
            }
        }
    }

}
