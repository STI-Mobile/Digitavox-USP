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

using Microsoft.Extensions.Logging;
using Digitavox.PlatformsImplementations;
using Digitavox.Views;
using Digitavox.ViewModels;
using Digitavox.Models;
using Plugin.Maui.Audio;
using Digitavox.Core.Abstractions;
using Digitavox.Infrastructure.Navigation;
using Digitavox.Infrastructure.Platform;
using Digitavox.Infrastructure.Settings;
using Digitavox.Infrastructure.Speech;
using Digitavox.Infrastructure.Startup;
using Digitavox.Presentation.Text;
using Digitavox.Presentation.State;
using Digitavox.Infrastructure.Audio;
using Digitavox.Presentation.Input;
using Digitavox.Domain.Time;
using Digitavox.Infrastructure.Time;
using Digitavox.Infrastructure.Persistence;

namespace Digitavox;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();
        builder.Services.AddSingleton<Course>();
        builder.Services.AddSingleton<CourseLesson>();
        builder.Services.AddSingleton<FingerMapping>();
        builder.Services.AddSingleton<UserProgress>();
        builder.Services.AddSingleton<ISettingsService, MauiSettingsService>();
        builder.Services.AddSingleton<IClock, SystemClock>();
        builder.Services.AddSingleton<IExerciseTimer, MauiExerciseTimer>();
        builder.Services.AddSingleton<IUserProgressStore, MauiUserProgressStore>();
        builder.Services.AddSingleton<ICourseCatalogStore, MauiCourseCatalogStore>();
        builder.Services.AddSingleton<INavigationService, ShellNavigationService>();
        builder.Services.AddSingleton<ISpeechService, MauiSpeechService>();
        builder.Services.AddSingleton<IAppEnvironment, MauiAppEnvironment>();
        builder.Services.AddSingleton<IAppStartupService, MauiAppStartupService>();
        builder.Services.AddSingleton<ISpeechTextFormatter, SpeechTextFormatter>();
        builder.Services.AddSingleton<PageTextRenderer>();
        builder.Services.AddSingleton<ICurrentPageContext, CurrentPageContext>();
        builder.Services.AddSingleton<IFeedbackSoundService, MauiFeedbackSoundService>();
        builder.Services.AddTransient<KeyboardInputProcessor>();
        builder.Services.AddSingleton<DVViewModelSpeak>();
        builder.Services.AddSingleton<DVViewModelFunctions>();
        builder.Services.AddTransient<ConfigViewModel>();
        builder.Services.AddTransient<CoursesHelpViewModel>();
        builder.Services.AddTransient<CoursesViewModel>();
        builder.Services.AddTransient<ExercisesHelpViewModel>();
        builder.Services.AddTransient<ExercisesStatisticsViewModel>();
        builder.Services.AddTransient<ExercisesViewModel>();
        builder.Services.AddTransient<KeyboardViewModel>();
        builder.Services.AddTransient<LessonsHelpViewModel>();
        builder.Services.AddTransient<LessonsViewModel>();
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<TutorialViewModel>();
        builder.Services.AddTransient<MenuViewModel>();
        builder.Services.AddTransient<UserOptionsViewModel>();
        builder.Services.AddTransient<SecondHelpViewModel>();
        builder.Services.AddTransient<PrivacyPolicyViewModel>();
        builder.Services.AddTransient<ThirdPartyLicensesViewModel>();
        builder.Services.AddTransient<AlertViewModel>();
        builder.Services.AddTransient<AlertView>();
        builder.Services.AddTransient<ConfigView>();
        builder.Services.AddTransient<CoursesHelpView>();
        builder.Services.AddTransient<CoursesView>();
        builder.Services.AddTransient<ExercisesHelpView>();
        builder.Services.AddTransient<ExercisesStatisticsView>();
        builder.Services.AddTransient<ExercisesView>();
        builder.Services.AddTransient<KeyboardView>();
        builder.Services.AddTransient<LessonsHelpView>();
        builder.Services.AddTransient<LessonsView>();
        builder.Services.AddTransient<LoginView>();
        builder.Services.AddTransient<TutorialView>();
        builder.Services.AddTransient<MenuView>();
        builder.Services.AddTransient<UserOptionsView>();
        builder.Services.AddTransient<SecondHelpView>();
        builder.Services.AddTransient<PrivacyPolicyView>();
        builder.Services.AddTransient<ThirdPartyLicensesView>();

        builder.Services.AddSingleton(AudioManager.Current);

#if IOS || MACCATALYST
    builder.ConfigureMauiHandlers(handlers => handlers.AddHandler(typeof(VerticalStackLayout), typeof(DVLayoutHandler)));
	builder.ConfigureMauiHandlers(handlers => handlers.AddHandler(typeof(Grid), typeof(DVLayoutHandler))); 
	DVSpeak.GetInstance().Init(null);
#endif

#if DEBUG
        builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
