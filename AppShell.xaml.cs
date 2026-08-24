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

using Digitavox.Core.Abstractions;
using Digitavox.Views;

namespace Digitavox;

public partial class AppShell : Shell
{
    private readonly IAppStartupService appStartupService;
    private DataTemplate _firstView;

    public DataTemplate FirstView
    {
        get { return _firstView; }
        set
        {
            if (_firstView != value)
            {
                _firstView = value;
                OnPropertyChanged(nameof(FirstView));
            }
        }
    }

    public AppShell(IAppStartupService appStartupService)
	{
        this.appStartupService = appStartupService;
        InitializeComponent();
        FirstView = new DataTemplate(() => new ContentPage());
        BindingContext = this;

        Routing.RegisterRoute(AppRoute.Alert.ToString(), typeof(AlertView));
        Routing.RegisterRoute(AppRoute.Tutorial.ToString(), typeof(TutorialView));
        Routing.RegisterRoute(AppRoute.Login.ToString(), typeof(LoginView));
        Routing.RegisterRoute(AppRoute.Menu.ToString(), typeof(MenuView));
        Routing.RegisterRoute(AppRoute.Keyboard.ToString(), typeof(KeyboardView));
        Routing.RegisterRoute(AppRoute.Courses.ToString(), typeof(CoursesView));
        Routing.RegisterRoute(AppRoute.CoursesHelp.ToString(), typeof(CoursesHelpView));
        Routing.RegisterRoute(AppRoute.Lessons.ToString(), typeof(LessonsView));
        Routing.RegisterRoute(AppRoute.LessonsHelp.ToString(), typeof(LessonsHelpView));
        Routing.RegisterRoute(AppRoute.Exercises.ToString(), typeof(ExercisesView));
        Routing.RegisterRoute(AppRoute.ExercisesHelp.ToString(), typeof(ExercisesHelpView));
        Routing.RegisterRoute(AppRoute.ExercisesStatistics.ToString(), typeof(ExercisesStatisticsView));
        Routing.RegisterRoute(AppRoute.UserOptions.ToString(), typeof(UserOptionsView));
        Routing.RegisterRoute(AppRoute.Config.ToString(), typeof(ConfigView));
        Routing.RegisterRoute(AppRoute.SecondHelp.ToString(), typeof(SecondHelpView));
        Routing.RegisterRoute(AppRoute.PrivacyPolicy.ToString(), typeof(PrivacyPolicyView));
        Routing.RegisterRoute(AppRoute.ThirdPartyLicenses.ToString(), typeof(ThirdPartyLicensesView));
    }

    public async Task InitializeAsync()
    {
        AppStartupDestination destination = await appStartupService.InitializeAsync();
        Type firstViewType = destination == AppStartupDestination.Login
            ? typeof(LoginView)
            : typeof(TutorialView);
        FirstView = new DataTemplate(firstViewType);
    }
}
