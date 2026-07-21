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
using Digitavox.Views;
using System.ComponentModel;
using System.IO.IsolatedStorage;

namespace Digitavox;

public partial class AppShell : Shell, INotifyPropertyChanged
{
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

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    public AppShell()
	{
        InitializeComponent();

        if (DVPersistence.CourseDirectoryExists())
        {
            FirstView = new DataTemplate(typeof(LoginView));
        }
        else
        {
            DVPersistence.CopyCourseFiles();
            FirstView = new DataTemplate(typeof(TutorialView));
        }
        BindingContext = this;

        Routing.RegisterRoute("Alert", typeof(AlertView));
        Routing.RegisterRoute("Tutorial", typeof(TutorialView));
        Routing.RegisterRoute("Login", typeof(LoginView));
        Routing.RegisterRoute("Menu", typeof(MenuView));
        Routing.RegisterRoute("Keyboard", typeof(KeyboardView));
        Routing.RegisterRoute("Courses", typeof(CoursesView));
        Routing.RegisterRoute("CoursesHelp", typeof(CoursesHelpView));
        Routing.RegisterRoute("Lessons", typeof(LessonsView));
        Routing.RegisterRoute("LessonsHelp", typeof(LessonsHelpView));
        Routing.RegisterRoute("Exercises", typeof(ExercisesView));
        Routing.RegisterRoute("ExercisesHelp", typeof(ExercisesHelpView));
        Routing.RegisterRoute("ExercisesStatistics", typeof(ExercisesStatisticsView));
        Routing.RegisterRoute("UserOptions", typeof(UserOptionsView));
        Routing.RegisterRoute("Config", typeof(ConfigView));
        Routing.RegisterRoute("SecondHelp", typeof(SecondHelpView));
        Routing.RegisterRoute("PrivacyPolicy", typeof(PrivacyPolicyView));
        Routing.RegisterRoute("ThirdPartyLicenses", typeof(ThirdPartyLicensesView));
    }
}
