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
    public partial class LessonsViewModel : ObservableObject, IOnPageKeyPress
    {
        List<string> pressedKeys = new List<string>();
        int timeToCaptureNumber = 1000;
        int textLineCount;
        int speakFromHelp;
        bool speakLessons;
        bool lessonTextOption;
        int lessonNumber;
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
        public LessonsViewModel(Course course, 
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
            pageKeyCodes = new List<string>()
            {
                "Up", "Down", "Tab", "ShiftTab",
                "Escape", "Enter", "F1", "Right", "Left",
                "F2", "F3", "F4", "F5", "F6",
                "F7", "F8", "F9", "F10"
            };
            for (int i = 0; i < 10; i++)
            {
                pageKeyCodes.Add($"{i}");
            }
            lessonTextOption = false;
        }
        public void OnPage()
        {
            dVViewModelFunctions.SetCurrentPageIdentifier("no menu de lições");
            dVViewModelSpeak.CurrentIsLessonsPage(true);
            speakFromHelp = dVViewModelFunctions.GetUpdateSpeakFromHelp();
            speakLessons = false;
            lessonNumber = userProgress.LastAvailableLesson();
            UpdateTextSpeakLists();
            Thread.Sleep(100);
            dVViewModelFunctions.SetNextPageRoute("LessonsHelp");
            dVViewModelFunctions.SetNumberCaptureTimeInterval(timeToCaptureNumber);
            dVViewModelFunctions.SetOption2PageList(new List<string>()
            {
                "Exercises"
            });
            dVViewModelFunctions.SetFirstOptionLineNumber(textList.Count - lessonNumber);
            dVViewModelFunctions.SetLastOptionLineNumber(dVViewModelSpeak.LineCount() - 1);
            dVViewModelFunctions.SetOptionNumberStart(textList.Count);
            if (speakFromHelp == -1)
            {
                textLineCount = textList.Count;
                dVViewModelSpeak.SpeakAll();
                lessonTextOption = false;
            }
            else
            {
                dVViewModelSpeak.SpeakOneLine(speakFromHelp, () => { });
                if (lessonNumber != 1 || lessonTextOption)
                {
                    dVViewModelFunctions.SetFirstOptionLineNumber(dVViewModelSpeak.LineCount() - lessonNumber - 1);
                    dVViewModelFunctions.SetLastOptionLineNumber(dVViewModelSpeak.LineCount() - 2);
                    dVViewModelFunctions.SetOptionNumberStart(dVViewModelSpeak.LineCount() - lessonNumber - 2 + course.LessonNumber());
                    lessonTextOption = false;
                }
            }
            WeakReferenceMessenger.Default.Send(new DVMessage("BecomeFirstResponder"));
        }
        private void UpdateTextSpeakLists()
        {
            List<string> courseTexts = new List<string>()
            {
                course.CourseApresentation(),
                course.CourseInstruction(),
                course.CourseProperty("NOMECURSO")
            };
            if (lessonNumber == 1)
            {
                textList = new List<string>()
                {
                    $"{courseTexts[0]}",
                    string.Empty,
                    $"{courseTexts[1]}",
                    string.Empty,
                    $"Lições do curso: {courseTexts[2]}"
                };
                speechList = new List<string>()
                {
                    courseTexts[0],
                    string.Empty,
                    courseTexts[1],
                    string.Empty,
                    $"Lições do curso: {courseTexts[2]}"
                };
            }
            else
            {
                textList = new List<string>()
                {
                    $"Lições do curso: {courseTexts[2]}"
                };
                speechList = new List<string>()
                {
                    $"Lições do curso: {courseTexts[2]}"
                };
            }
            if (DVPersistence.Get<bool>("instructionsEnabled"))
            {
                textList.Add("Use os números, tab e shift tab ou setas verticais para navegar entre as opções. Depois tecle enter para confirmar. Escape volta e F1 ajuda.");
                speechList.Add("Use os números, tab e shift tab ou setas verticais para navegar entre as opções. Depois tecle êmter para confirmar. Esqueipe volta e F1 ajuda.");
            }
            else
            {
                textList.Add(string.Empty);
                speechList.Add(string.Empty);
            }
            for (int i = 1; i <= lessonNumber; i++)
            {
                textList.Add($"Lição {i}");
                if (speakLessons) speechList.Add($"Lição {i}");
                else speechList.Add((i == lessonNumber) ? $"Lição {i}" : string.Empty);
            }
            if (lessonNumber != 1 && (speakFromHelp == 0 || speakFromHelp == 1))
            {
                textList.Add(courseTexts[speakFromHelp]);
                speechList.Add(courseTexts[speakFromHelp]);
                speakFromHelp = textList.Count - 1;
                dVViewModelSpeak.Set("maintainTag", false);
            }
            else if (speakFromHelp == 2 || speakFromHelp == 3)
            {
                List<string> lessonTexts = new List<string>()
                {
                    course.LessonApresentation(),
                    course.LessonInstruction()
                };
                lessonTextOption = true;
                textList.Add(lessonTexts[speakFromHelp % 2]);
                speechList.Add(lessonTexts[speakFromHelp % 2]);
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
        private void Option2Lesson()
        {
            int selectNumber = dVViewModelFunctions.MapOptionToItem() + 1;
            if (selectNumber > course.TotalLessons()) selectNumber = course.TotalLessons();
            course.SelectLesson(selectNumber);
            userProgress.LessonRegistration($"LICAO{selectNumber}");
            dVViewModelFunctions.LessonHelpOptions();
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
                if (bean.code == "F2" || bean.code == "F8")
                {
                    dVViewModelFunctions.SetNextPageRoute("SecondHelp");
                }
                dVViewModelSpeak.Skip();
                Option2Lesson();
                speakLessons = true;
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