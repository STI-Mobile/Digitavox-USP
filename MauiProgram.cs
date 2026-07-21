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
using static Microsoft.Maui.ApplicationModel.Permissions;
using Digitavox.Views;
using Digitavox.ViewModels;
using Digitavox.Models;
using Digitavox.Helpers;
using Plugin.Maui.Audio;

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
        builder.Services.AddSingleton<DVViewModelSpeak>();
        builder.Services.AddSingleton<DVViewModelFunctions>();
        builder.Services.AddSingleton<ConfigViewModel>();
        builder.Services.AddSingleton<CoursesHelpViewModel>();
        builder.Services.AddSingleton<CoursesViewModel>();
        builder.Services.AddSingleton<ExercisesHelpViewModel>();
        builder.Services.AddSingleton<ExercisesStatisticsViewModel>();
        builder.Services.AddSingleton<ExercisesViewModel>();
        builder.Services.AddSingleton<KeyboardViewModel>();
        builder.Services.AddSingleton<LessonsHelpViewModel>();
        builder.Services.AddSingleton<LessonsViewModel>();
        builder.Services.AddSingleton<LoginViewModel>();
        builder.Services.AddSingleton<TutorialViewModel>();
        builder.Services.AddSingleton<MenuViewModel>();
        builder.Services.AddSingleton<UserOptionsViewModel>();
        builder.Services.AddSingleton<SecondHelpViewModel>();
        builder.Services.AddSingleton<PrivacyPolicyViewModel>();
        builder.Services.AddSingleton<ThirdPartyLicensesViewModel>();
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
