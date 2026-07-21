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

namespace Digitavox.ViewModels
{
    public partial class CoursesViewModel : ObservableObject, IOnPageKeyPress
    {
        List<string> pressedKeys = new List<string>();
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
        private FingerMapping fingerMapping;
        private UserProgress userProgress;
        public CoursesViewModel(Course course, 
                                DVViewModelSpeak dVViewModelSpeak,
                                DVViewModelFunctions dVViewModelFunctions,
                                FingerMapping fingerMapping,
                                UserProgress userProgress)
        {
            this.course = course;
            this.dVViewModelSpeak = dVViewModelSpeak;
            this.dVViewModelFunctions = dVViewModelFunctions;
            this.fingerMapping = fingerMapping;
            this.userProgress = userProgress;
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
        public void OnPage()
        {
            dVViewModelFunctions.SetCurrentPageIdentifier("no menu de cursos");
            speakFromHelp = dVViewModelFunctions.GetUpdateSpeakFromHelp();
            Thread.Sleep(100);
            courseList = course.CourseNameList();
            UpdateTextSpeakLists();
            dVViewModelFunctions.SetFirstOptionLineNumber(dVViewModelSpeak.LineCount() - courseList.Count);
            dVViewModelFunctions.SetLastOptionLineNumber(dVViewModelSpeak.LineCount() - 1);
            dVViewModelFunctions.SetOptionNumberStart(dVViewModelSpeak.LineCount() - courseList.Count - 1);
            dVViewModelFunctions.SetNextPageRoute("CoursesHelp");
            dVViewModelFunctions.SetNumberCaptureTimeInterval(timeToCaptureNumber);
            dVViewModelFunctions.SetOption2PageList(new List<string>()
            {
                "Lessons"
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
            WeakReferenceMessenger.Default.Send(new DVMessage("BecomeFirstResponder"));
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
            if (DVPersistence.Get<bool>("instructionsEnabled"))
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
                                TextSize = DVPersistence.Get<double>("fontSize");
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
            string code = fingerMapping.mapKeyCode(keyCode);
            if (!pressedKeys.Contains(code))
            {
                pressedKeys.Add(code);
            }
            return true;
        }
        public bool OnPageKeyPress(int keyCode, int modifiers)
        {
            pressedKeys.Remove(fingerMapping.mapKeyCode(keyCode));
            var bean = fingerMapping.MapKey(keyCode, modifiers, pressedKeys);
            if (bean.code != null)
            {
                dVViewModelSpeak.Skip();
                Option2Course();
                dVViewModelFunctions.LastLineIsText(dVViewModelSpeak.LineCount() > textLineCount);
                UpdateTextSpeakLists();
                if (bean.code == " ")
                {
                    OnPage();
                }
                else if (pageKeyCodes.Contains(bean.code) || ((bean.code == "!" || bean.code == "@") && DVDevice.IsVirtual()))
                {
                    dVViewModelFunctions.HandleKeyCode(bean.code);
                    if (dVViewModelFunctions.GetSpeakFromHelp() != -1) OnPage();
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
