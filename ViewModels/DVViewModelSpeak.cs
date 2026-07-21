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

using Digitavox.PlatformsImplementations;
using Microsoft.Maui;
using Digitavox.Helpers;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.Controls;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace Digitavox.ViewModels
{
    public class DVViewModelSpeak
    {
        private int boldLineIndex = -1;
        private bool isExercisePage = false;
        private bool isLessonsPage = false;
        private string letterStyle = string.Empty;
        private int letterIndex = -1;
        private Dictionary<int, Tuple<string, int>> letterStyleDictionary = new Dictionary<int, Tuple<string, int>>();
        private bool stopRecursive;
        private List<string> textList = new List<string>();
        private List<string> speechList = new List<string>();
        private Action<FormattedString> updateScreen;
        private string currentExecutionId = Guid.NewGuid().ToString();

        private Dictionary<string, object> key2Value = new Dictionary<string, object>()
        {
            {"maintainTag", false},
            {"optionTitle", string.Empty},
            {"optionList", new List<string>()}
        };
        public void Skip()
        {
            if (!stopRecursive)
                stopRecursive = true;

            DVSpeak.GetInstance().Cancel();
        }
        private void CancelCurrentExecution()
        {
            Skip();
        }
        public void Speak(string text, Action onCompleted)
        {
            DVSpeak.GetInstance().SpeakText(text, onCompleted);
        }
        public void SpeakAll(Action onCompleted)
        {
            currentExecutionId = Guid.NewGuid().ToString();
            string thisExecutionId = currentExecutionId;

            stopRecursive = false;
            key2Value["maintainTag"] = false;
            RecursiveCalls(textList.Count, currentExecutionId, onCompleted);
        }
        public void SpeakAll()
        {
            CancelCurrentExecution();
            SpeakAll(() => { });
        }
        public int LineCount()
        {
            return textList.Count;
        }
        public void SpeakOneLine(int index, Action onCompleted)
        {
            DVSpeak.GetInstance().Cancel();
            if (index >= 0 && index < textList.Count)
            {
                BoldLine(index);
                Speak(speechList[index], () =>
                {
                    if (!(bool)key2Value["maintainTag"]) CallUpdateScreen();
                    onCompleted.Invoke();
                });
            }
        }
        private void RecursiveCalls(int total, string executionId, Action onCompleted)
        {
            if (currentExecutionId != executionId)
            {
                return;
            }
            if (total > 0 && total <= textList.Count)
            {
                SpeakOneLine(textList.Count - total, () =>
                {
                    if (!stopRecursive)
                    {
                        RecursiveCalls(total - 1, executionId, onCompleted);
                    }
                    else
                    {
                        onCompleted.Invoke();
                    }
                });
            }
            else
            {
                onCompleted.Invoke();
            }
        }
        public void SpeakRecursive(int lines, Action onCompleted)
        {
            currentExecutionId = Guid.NewGuid().ToString();
            string thisExecutionId = currentExecutionId;

            stopRecursive = false;

            RecursiveCalls(textList.Count - lines, thisExecutionId, onCompleted);
        }
        public void BoldLine(int index)
        {
            if (speechList[index].Length > 0)
            {
                boldLineIndex = index;
            }
            CallUpdateScreen();
            boldLineIndex = -1;
        }
        public void RegisterUpdateScreen(Action<FormattedString> updateScreen)
        {
            this.updateScreen = updateScreen;
        }
        public DVViewModelSpeak SetTextAndSpeech(List<string> textList, List<string> speechList)
        {
            this.textList = textList;
            this.speechList = speechList;
            return this;
        }
        public T Get<T>(string key)
        {
            return (T)key2Value[key];
        }
        public void Set<T>(string key, T value)
        {
            key2Value[key] = value;
        }
        public void ChangeLine(string textLine, string speechLine, int index)
        {
            if (index <= textList.Count)
            {
                textList[index] = textLine;
                speechList[index] = speechLine;
                CallUpdateScreen();
            }
        }
        private void CallUpdateScreen()
        {
            List<string> textListCopy = textList;
            FormattedString result = new FormattedString();
            for (int index = 0; index < textList.Count; index++)
            {
                string line = textListCopy[index];
                Dictionary<int, Tuple<string, int>> letterStyleCopy = new Dictionary<int, Tuple<string, int>>(letterStyleDictionary);
                if (isExercisePage && letterStyleCopy.ContainsKey(index))
                {
                    letterIndex = letterStyleCopy[index].Item2;
                    if (letterIndex >= 0 && line.Length > letterIndex)
                    {
                        string firstPart = line.Substring(0, letterIndex);
                        string letter = line[letterIndex].ToString();
                        string secondPart = line.Substring(letterIndex + 1);
                        result.Spans.Add(DefineSpan(firstPart, index));
                        letterStyle = letterStyleCopy[index].Item1;
                        result.Spans.Add(DefineSpan(letter, index));
                        letterStyle = string.Empty;
                        result.Spans.Add(DefineSpan(secondPart, index));
                    }
                    else
                    {
                        result.Spans.Add(DefineSpan(line, index));
                    }
                    
                }
                else
                {
                    result.Spans.Add(DefineSpan(line, index));
                }
                result.Spans.Add(new Span { Text = "\n" });
                if (isExercisePage && index <= 4)
                {
                    result.Spans.Add(new Span { Text = "\n" });
                }
            }
            updateScreen.Invoke(result);
        }
        private Span DefineSpan(string text, int index)
        {
            double fontSize = DVPersistence.Get<double>("fontSize");
            FontAttributes fontAttributes = FontAttributes.None;

            Color color;
            if (AccessibilityHelper.IsHighContrastEnabled())
            {
                color = Colors.White; 
            }
            else
            {
                AppTheme currentTheme = Application.Current.RequestedTheme;
                color = (currentTheme == AppTheme.Light) ? Colors.Black : Colors.White;
            }

            if (isExercisePage && index >= 2 && index <= 4)
            {
                fontSize += 4.0;
            }

            if (boldLineIndex == index || (letterStyle == "bold" && text.Length == 1))
            {
                fontAttributes = FontAttributes.Bold;
            }
            else if (letterStyle == "green" && text.Length == 1)
            {
                color = Colors.Green;
            }
            else if (letterStyle == "red" && text.Length == 1)
            {
                color = Colors.Red;
            }

            return new Span { Text = text, FontSize = fontSize, FontAttributes = fontAttributes, TextColor = color };
        }
        public void AttributeStyle(string letterStyle, int lineIndex, int letterIndex)
        {
            
            letterStyleDictionary[lineIndex] = new Tuple<string, int>(letterStyle, letterIndex);
        }
        public void ClearStyleDictionary()
        {
            letterStyleDictionary.Clear(); 
        }
        public void CurrentIsExercisePage(bool isExercisePage)
        {
            this.isExercisePage = isExercisePage;
        }
        public void CurrentIsLessonsPage(bool isLessonsPage)
        {
            this.isLessonsPage = isLessonsPage;
        }

    }
}
