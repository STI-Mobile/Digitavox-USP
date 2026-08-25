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

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Digitavox.Helpers;
using Digitavox.Models;
using Digitavox.Core.Abstractions;
using Digitavox.Core.Messages;
using Digitavox.Presentation.Input;

namespace Digitavox.ViewModels
{
    public partial class CoursesViewModel : ObservableObject, IKeyboardInputHandler
    {
        int timeToCaptureNumber = 500;
        int textLineCount;
        int speakFromHelp;
        List<string> courseList;
        List<string> textList;
        List<string> speechList;
        List<string> pageKeyCodes;
        [ObservableProperty]
        private FormattedString pageFormattedLabel;
        [ObservableProperty]
        private string _pageLabel;
        [ObservableProperty]
        private double _textSize;
        private Course course;
        private DVViewModelSpeak dVViewModelSpeak;
        private DVViewModelFunctions dVViewModelFunctions;
        private KeyboardInputProcessor keyboardInputProcessor;
        private UserProgress userProgress;
        private readonly ISettingsService settingsService;
        private readonly IAppEnvironment appEnvironment;
        public CoursesViewModel(Course course, 
                                DVViewModelSpeak dVViewModelSpeak,
                                DVViewModelFunctions dVViewModelFunctions,
                                KeyboardInputProcessor keyboardInputProcessor,
                                UserProgress userProgress,
                                ISettingsService settingsService,
                                IAppEnvironment appEnvironment)
        {
            this.course = course;
            this.dVViewModelSpeak = dVViewModelSpeak;
            this.dVViewModelFunctions = dVViewModelFunctions;
            this.keyboardInputProcessor = keyboardInputProcessor;
            this.userProgress = userProgress;
            this.settingsService = settingsService;
            this.appEnvironment = appEnvironment;
            course.GetCoursesLists();
            pageKeyCodes = new List<string>()
            {
                "Up", "Down", "Tab", "ShiftTab",
                "Left", "Right",
                "Escape", "Enter", "F1",
                "F2", "F3", "F4", "F5", "F6"
            };
            for (int i = 0; i < 10; i++)
            {
                pageKeyCodes.Add($"{i}");
            }
        }
        public async Task OnPageAsync()
        {
            dVViewModelFunctions.SetCurrentPageIdentifier("no menu de cursos");
            speakFromHelp = dVViewModelFunctions.GetUpdateSpeakFromHelp();
            await Task.Delay(100);
            courseList = course.CourseNameList();
            UpdateTextSpeakLists();
            dVViewModelFunctions.SetFirstOptionLineNumber(dVViewModelSpeak.LineCount() - courseList.Count);
            dVViewModelFunctions.SetLastOptionLineNumber(dVViewModelSpeak.LineCount() - 1);
            dVViewModelFunctions.SetOptionNumberStart(dVViewModelSpeak.LineCount() - courseList.Count - 1);
            dVViewModelFunctions.SetNextPageRoute(AppRoute.CoursesHelp);
            dVViewModelFunctions.SetNumberCaptureTimeInterval(timeToCaptureNumber);
            dVViewModelFunctions.SetOption2PageList(new List<AppRoute>()
            {
                AppRoute.Lessons
            });
            if (speakFromHelp == -1)
            {
                textLineCount = textList.Count;
                dVViewModelSpeak.SpeakAll();
            }
            else
            {
                dVViewModelSpeak.SpeakOneLine(speakFromHelp, () => { });
                dVViewModelFunctions.SetFirstOptionLineNumber(dVViewModelSpeak.LineCount() - courseList.Count - 1);
                dVViewModelFunctions.SetLastOptionLineNumber(dVViewModelSpeak.LineCount() - 2);
                dVViewModelFunctions.SetOptionNumberStart(dVViewModelSpeak.LineCount() - courseList.Count - 2 + course.CourseNumber());
            }
            WeakReferenceMessenger.Default.Send(new RequestFirstResponderMessage());
        }
        private void UpdateTextSpeakLists()
        {
            textList = new List<string>()
            {
                
                "Cursos de digitação"
            };
            speechList = new List<string>()
            {
                "Cursos de digitação"
            };
            if (settingsService.Get<bool>("instructionsEnabled"))
            {
                textList.Add($"Use os números de 1 a {courseList.Count}, tab e shift tab ou setas verticais para navegar entre as opções. Depois tecle enter para confirmar. Escape volta e ao navegar pelas opções tecle F1 para ajuda.");
                speechList.Add($"Use os números de 1 a {courseList.Count}, tab e shift tab ou setas verticais para navegar entre as opções. Depois tecle êmter para confirmar. Esqueipe volta e ao navegar pelas opções tecle F1 para ajuda.");
            }
            else
            {
                textList.Add(string.Empty);
                speechList.Add(string.Empty);
            }
            textList.Add("As opções são: ");
            speechList.Add("As opções são: ");
            for (int index = 1; index <= courseList.Count; index++)
            {
                textList.Add($"{index} - {courseList[index - 1]}");
                speechList.Add($"{index} - {courseList[index - 1]}");
            }
            if (speakFromHelp == 0 || speakFromHelp == 1)
            {
                
                
                List<string> courseTexts = new List<string>()
                {
                    course.CourseApresentation(),
                    course.CourseInstruction()
                };
                textList.Add(courseTexts[speakFromHelp]);
                speechList.Add(courseTexts[speakFromHelp]);
                speakFromHelp = textList.Count - 1;
                dVViewModelSpeak.Set("maintainTag", false);
            }

            textList = textList.Select(text => dVViewModelFunctions.EditStringForVoiceOver(text)).ToList();
            speechList = speechList.Select(speech => dVViewModelFunctions.EditStringForVoiceOver(speech)).ToList();

            dVViewModelSpeak.SetTextAndSpeech(textList, speechList)
                            .RegisterUpdateScreen((text) =>
                            {
                                
                                
                                PageFormattedLabel = text;
                                TextSize = settingsService.Get<double>("fontSize");
                            });
        }
        private void Option2Course()
        {
            int selectNumber = dVViewModelFunctions.MapOptionToItem();
            course.SelectCourse(selectNumber);
            userProgress.CourseRegistration(course.CourseId());
            dVViewModelFunctions.CourseHelpOptions();
        }
        public bool OnPageKeyDown(int keyCode)
        {
            return keyboardInputProcessor.KeyDown(keyCode);
        }
        public bool OnPageKeyPress(int keyCode, int modifiers)
        {
            var bean = keyboardInputProcessor.KeyUp(keyCode, modifiers);
            if (bean.code != null)
            {
                dVViewModelSpeak.Skip();
                Option2Course();
                dVViewModelFunctions.LastLineIsText(dVViewModelSpeak.LineCount() > textLineCount);
                UpdateTextSpeakLists();
                if (bean.code == " ")
                {
                    _ = OnPageAsync();
                }
                else if (pageKeyCodes.Contains(bean.code) || ((bean.code == "!" || bean.code == "@") && appEnvironment.IsVirtualDevice))
                {
                    dVViewModelFunctions.HandleKeyCode(bean.code);
                    if (dVViewModelFunctions.GetSpeakFromHelp() != -1) _ = OnPageAsync();
                }
                else if (!DVKeyboard.IsModifierKey(bean.code))
                {
                    dVViewModelFunctions.InvalidOption(bean.speakOnlyChar);
                }
            }
            return true;
        }
    }
}
