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
    public partial class ExercisesHelpViewModel : ObservableObject, IOnPageKeyPress
    {
        List<string> pressedKeys = new List<string>();
        int totalOptions = 11;
        List<string> functionKeyCodes;
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
        public ExercisesHelpViewModel(DVViewModelSpeak dVViewModelSpeak,
                                      DVViewModelFunctions dVViewModelFunctions,
                                      FingerMapping fingerMapping)
        {
            this.dVViewModelSpeak = dVViewModelSpeak;
            this.dVViewModelFunctions = dVViewModelFunctions;
            this.fingerMapping = fingerMapping;
            functionKeyCodes = new List<string>()
            {
                "Left", "Right", "F2",
                "F3", "F4", "F5", "F6",
                "F7", "F8", "F9", "Escape"
            };
            pageKeyCodes = new List<string>()
            {
                "Up", "Down", "Tab", "ShiftTab",
                "Ctrl+Left", "Ctrl+Right", "Ctrl+Up", "Ctrl+Down"
            };
            foreach (string key in functionKeyCodes)
            {
                pageKeyCodes.Add(key);
            }
        }
        public void OnPage()
        {
            dVViewModelFunctions.SetCurrentPageIdentifier("no menu de ajuda de exercícios");
            Thread.Sleep(100);

            var textList = new List<string>()
{
                "Nesse menu a seta para cima e seta para baixo são utilizadas apenas para navegar entre os itens. As opções são: ",
                "← - Falar qual a repetição atual e qual o total de repetições para o exercício",
                "[Ctrl] ← - Falar o percentual de teclas digitadas corretamente",
                $"{functionKeyCodes[2]} ou ↓ - Falar qual a próxima tecla a digitar e com qual dedo",
                $"{functionKeyCodes[3]} ou [Ctrl] → - Soletrar o que deve ser digitado no restante do exercício",
                $"{functionKeyCodes[4]} ou → - Falar o que deve ser digitado no restante do exercício",
                $"{functionKeyCodes[5]} ou ↑ - Falar o que deve ser digitado no exercício",
                $"{functionKeyCodes[6]} ou [Ctrl] ↑ - Falar apresentação da lição",
                $"{functionKeyCodes[7]} ou [Ctrl] ↓ - Falar instrução da lição",
                $"{functionKeyCodes[8]} - Falar a hora atual",
                $"{functionKeyCodes[9]} - Falar o tempo decorrido, o tempo total disponível para conclusão da lição e o percentual já gasto",
                "[ESC] - Sair"
            };

            var speechList = new List<string>()
            {
                "Nesse menu a seta para cima e seta para baixo são utilizadas apenas para navegar entre os itens. As opções são: ",
                "Seta para esquerda - Falar qual a repetição atual e qual o total de repetições para o exercício",
                "Control seta para esquerda - Falar o percentual de teclas digitadas corretamente",
                $"{functionKeyCodes[2]} ou seta para baixo - Falar qual a próxima tecla a digitar e com qual dedo",
                $"{functionKeyCodes[3]} ou control seta para direita - Soletrar o que deve ser digitado no restante do exercício",
                $"{functionKeyCodes[4]} ou seta para direita - Falar o que deve ser digitado no restante do exercício",
                $"{functionKeyCodes[5]} ou seta para cima - Falar o que deve ser digitado no exercício",
                $"{functionKeyCodes[6]} ou control seta para cima - Falar apresentação da lição",
                $"{functionKeyCodes[7]} ou control seta para baix - Falar instrução da lição",
                $"{functionKeyCodes[8]} - Falar a hora atual",
                $"{functionKeyCodes[9]} - Falar o tempo decorrido, o tempo total disponível para conclusão da lição e o percentual já gasto",
                "Esqueipe - Sair"
            };

            textList = textList.Select(text => dVViewModelFunctions.EditStringForVoiceOver(text)).ToList();
            speechList = speechList.Select(speech => dVViewModelFunctions.EditStringForVoiceOver(speech)).ToList();

            dVViewModelSpeak.SetTextAndSpeech(textList, speechList)
                            .RegisterUpdateScreen((text) =>
                            {
                                
                                
                                PageFormattedLabel = text;
                                TextSize = DVPersistence.Get<double>("fontSize");
                            });


            dVViewModelFunctions.SetFirstOptionLineNumber(dVViewModelSpeak.LineCount() - totalOptions);
            dVViewModelFunctions.SetLastOptionLineNumber(dVViewModelSpeak.LineCount() - 1);
            dVViewModelFunctions.SetOptionNumberStart(dVViewModelSpeak.LineCount() - totalOptions - 1);
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
                dVViewModelFunctions.ExerciseHelpOptions();
                if (bean.code == " ")
                {
                    OnPage();
                }
                else if (bean.code == "Enter")
                {
                    Enter();
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