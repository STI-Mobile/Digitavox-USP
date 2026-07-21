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
    public partial class LessonsHelpViewModel : ObservableObject, IOnPageKeyPress 
    {
        List<string> pressedKeys = new List<string>();
        int totalOptions = 13;
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
        private Course course;
        public LessonsHelpViewModel(DVViewModelSpeak dVViewModelSpeak,
                                    DVViewModelFunctions dVViewModelFunctions,
                                    FingerMapping fingerMapping,
                                    Course course)
        {
            this.dVViewModelSpeak = dVViewModelSpeak;
            this.dVViewModelFunctions = dVViewModelFunctions;
            this.fingerMapping = fingerMapping;
            this.course = course;
            pageKeyCodes = new List<string>()
            {
                "Up", "Down", "Tab", "ShiftTab",
                "Right", "Left", "Enter",
                "F2", "F3", "F4", "F5", "F6",
                "F7", "F8", "F9", "F10", "Escape"
            };
        }
        public void OnPage()
        {
            dVViewModelFunctions.SetCurrentPageIdentifier("no menu de ajuda de lições");
            Thread.Sleep(100);

            var textList = new List<string>()
            {
                
                $"Lição {course.LessonNumber()}",
                "As opções são: ",
                "← - Falar apresentação da lição",
                "→ - Falar instrução da lição",
                "↵ - Entrar na lição",
                "F2 - Apresentar dados da lição",
                "F3 - Falar o que vai ser digitado na sequência do exercício",
                "F4 - Falar usuário logado",
                "F5 - Falar desafio do tempo",
                "F6 - Falar qual a lição do total disponível",
                "F7 - Falar nome do curso",
                "F8 - Apresentar estatísticas da lição",
                "F9 - Falar apresentação do curso",
                "F10 - Falar instrução do curso",
                "[ESC] - Sair"
            };

            var speechList = new List<string>()
            {
                $"Lição {course.LessonNumber()}",
                "As opções são: ",
                "Seta para esquerda - Falar apresentação da lição",
                "Seta para direita - Falar instrução da lição",
                "Enter - Entrar na lição",
                "F2 - Apresentar dados da lição",
                "F3 - Falar o que vai ser digitado na sequência do exercício",
                "F4 - Falar usuário logado",
                "F5 - Falar desafio do tempo",
                "F6 - Falar qual a lição do total disponível",
                "F7 - Falar nome do curso",
                "F8 - Apresentar estatísticas da lição",
                "F9 - Falar apresentação do curso",
                "F10 - Falar instrução do curso",
                "Esqueipe - Sair"
            };

            textList = textList.Select(text => dVViewModelFunctions.EditStringForVoiceOver(text)).ToList();
            speechList = speechList.Select(speech => dVViewModelFunctions.EditStringForVoiceOver(speech)).ToList();

            dVViewModelSpeak.SetTextAndSpeech(textList, speechList).RegisterUpdateScreen((text) =>
            {
                
                
                PageFormattedLabel = text;
                TextSize = DVPersistence.Get<double>("fontSize");
            });

            dVViewModelFunctions.SetFirstOptionLineNumber(dVViewModelSpeak.LineCount() - totalOptions);
            dVViewModelFunctions.SetLastOptionLineNumber(dVViewModelSpeak.LineCount() - 1);
            dVViewModelFunctions.SetOptionNumberStart(dVViewModelSpeak.LineCount() - totalOptions - 1);
            dVViewModelFunctions.SetNextPageRoute("SecondHelp");
            dVViewModelFunctions.SetOption2PageList(new List<string>()
            {
                "SecondHelp"
            });
            dVViewModelSpeak.SpeakAll();
            WeakReferenceMessenger.Default.Send(new DVMessage("BecomeFirstResponder"));
        }
        private void Enter()
        {
            List<string> optionsKeys = new List<string>();
            for (int i = pageKeyCodes.Count - totalOptions; i < pageKeyCodes.Count; i++)
            {
                optionsKeys.Add(pageKeyCodes[i]);
            }
            int selectNumber = dVViewModelFunctions.MapOptionToItem();
            if (pageKeyCodes.Contains(optionsKeys[selectNumber]))
            {
                dVViewModelFunctions.HandleKeyCode(optionsKeys[selectNumber]);
            }
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
                dVViewModelFunctions.LessonHelpOptions();
                if (bean.code == " ")
                {
                    OnPage();
                }
                else if (bean.code == "Enter")
                {
                    if (dVViewModelFunctions.LessonsEnterOptionSelected()) dVViewModelFunctions.HandleKeyCode("Enter");
                    else Enter();
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