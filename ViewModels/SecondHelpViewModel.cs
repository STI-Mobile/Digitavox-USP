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
    public partial class SecondHelpViewModel : ObservableObject, IOnPageKeyPress
    {
        List<string> pressedKeys = new List<string>();
        string statistcsOptionKey;
        List<string> pageKeyCodes;
        [ObservableProperty]
        private FormattedString pageFormattedLabel;
        [ObservableProperty]
        private string _pageLabel;
        [ObservableProperty]
        private double _textSize;
        private DVViewModelSpeak dVViewModelSpeak;
        private DVViewModelFunctions dVViewModelFunctions;
        private FingerMapping fingerMapping;
        private UserProgress userProgress;
        private Course course;
        public SecondHelpViewModel(DVViewModelSpeak dVViewModelSpeak,
                                   DVViewModelFunctions dVViewModelFunctions, 
                                   FingerMapping fingerMapping,
                                   UserProgress userProgress,
                                   Course course)
        {
            this.dVViewModelSpeak = dVViewModelSpeak;
            this.dVViewModelFunctions = dVViewModelFunctions;
            this.fingerMapping = fingerMapping;
            this.userProgress = userProgress;
            this.course = course;
            statistcsOptionKey = dVViewModelFunctions.LessonStatisticsCode();
        }
        public void OnPage()
        {
            bool statisticsNext = dVViewModelSpeak.Get<string>("optionTitle").StartsWith(statistcsOptionKey);
            string instructionText = statisticsNext ? "Use tab e shift tab ou as setas verticais para navegar pelos itens. Depois tecle Enter para confirmar. Escape volta." : "Use tab e shift tab ou as setas verticais para navegar pelos itens. Escape volta.";
            string instructionSpeak = statisticsNext ? "Use tab e shift tab ou as setas verticais para navegar pelos itens. Depois tecle êmter para confirmar. Esqueipe volta." : "Use tab e shift tab ou as setas verticais para navegar pelos itens. Esqueipe volta.";
            pageKeyCodes = new List<string>()
            {
                "Up", "Down", "Tab", "ShiftTab",
                "Escape"
            };
            dVViewModelFunctions.ClearHelpOptions();
            Thread.Sleep(100);
            List<string> textList = new List<string>()
            {
                
                dVViewModelSpeak.Get<string>("optionTitle")
            };
            List<string> speechList = new List<string>()
            {
                string.Empty
            };
            if (DVPersistence.Get<bool>("instructionsEnabled"))
            {
                textList.Add(instructionText);
                speechList.Add(instructionSpeak);
            }
            int originalTextListCount = textList.Count;
            foreach (string line in dVViewModelSpeak.Get<List<string>>("optionList"))
            {
                textList.Add(line);
                speechList.Add(line);
            }
            if (dVViewModelSpeak.Get<string>("optionTitle").StartsWith(statistcsOptionKey))
            {
                dVViewModelFunctions.SetFirstOptionLineNumber(originalTextListCount + 1);
                dVViewModelFunctions.SetOptionNumberStart(originalTextListCount);
                dVViewModelFunctions.SetLastOptionLineNumber(textList.Count - 1);
                dVViewModelFunctions.SetOption2PageList(new List<string>()
                {
                    "ExercisesStatistics"
                });
                pageKeyCodes.Add("Enter");
            }
            else
            {
                dVViewModelFunctions.SetFirstOptionLineNumber(originalTextListCount);
                dVViewModelFunctions.SetOptionNumberStart(originalTextListCount - 1);
                for (int i = 1; i <= course.GetExercises().Count; i++)
                {
                    textList.Add($"Exercício {i}: {course.GetExercises()[i - 1]}");
                    speechList.Add($"Exercício {i}: {(string.Equals(course.LessonProperty("SOLETRAEXER"), "sim", StringComparison.OrdinalIgnoreCase) ? dVViewModelFunctions.SpellWord(course.GetExercises()[i - 1]) : dVViewModelFunctions.SpeakPunctuation(course.GetExercises()[i - 1]))}");
                }
            }

            textList = textList.Select(text => dVViewModelFunctions.EditStringForVoiceOver(text)).ToList();
            speechList = speechList.Select(speech => dVViewModelFunctions.EditStringForVoiceOver(speech)).ToList();

            dVViewModelSpeak.SetTextAndSpeech(textList, speechList).RegisterUpdateScreen((text) =>
            {
                
                
                PageFormattedLabel = text;
                TextSize = DVPersistence.Get<double>("fontSize");
            });
            dVViewModelFunctions.SetLastOptionLineNumber(dVViewModelSpeak.LineCount() - 1);
            dVViewModelSpeak.SpeakAll();
            WeakReferenceMessenger.Default.Send(new DVMessage("BecomeFirstResponder"));
        }
        private void Option2Lesson()
        {
            int selectNumber = dVViewModelFunctions.MapOptionToItem() + 1;
            userProgress.GetLessonRepetitionData(selectNumber);
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
                if (bean.code == "Enter")
                {
                    if (dVViewModelSpeak.Get<string>("optionTitle").StartsWith(statistcsOptionKey)) Option2Lesson();
                }
                if (bean.code == " ")
                {
                    OnPage();
                }
                else if (pageKeyCodes.Contains(bean.code) || (bean.code == "!" && DVDevice.IsVirtual()))
                {
                    dVViewModelFunctions.HandleKeyCode(bean.code);
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
