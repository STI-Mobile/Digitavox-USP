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
using Digitavox.Presentation.Text;

namespace Digitavox.ViewModels
{
    public class DVViewModelSpeak
    {
        private readonly ISpeechService speechService;
        private readonly PageTextRenderer pageTextRenderer;
        private int boldLineIndex = -1;
        private bool isExercisePage = false;
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

        public DVViewModelSpeak(
            ISpeechService speechService,
            PageTextRenderer pageTextRenderer)
        {
            this.speechService = speechService;
            this.pageTextRenderer = pageTextRenderer;
        }

        public void Skip()
        {
            if (!stopRecursive)
                stopRecursive = true;

            speechService.Cancel();
        }
        private void CancelCurrentExecution()
        {
            Skip();
        }
        public void Speak(string text, Action onCompleted)
        {
            speechService.Speak(text, onCompleted);
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
            speechService.Cancel();
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
            updateScreen.Invoke(pageTextRenderer.Render(
                textList,
                boldLineIndex,
                isExercisePage,
                letterStyleDictionary));
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
    }
}
