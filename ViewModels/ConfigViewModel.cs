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
using Digitavox.Helpers;
using Digitavox.Models;
using Digitavox.PlatformsImplementations;

namespace Digitavox.ViewModels
{
    public partial class ConfigViewModel : ObservableObject, IOnPageKeyPress
    {
        List<string> pressedKeys = new List<string>();
        private int currentSpeakRate = -1;
        private double prevFontSize = -1;
        int timeToCaptureNumber = 0;
        int introductionLines;
        int totalOptions = 7;
        int answerController = -1; 
        bool updateText = true;
        bool storedText = false;
        bool answerOption;
        bool outcome;
        bool apresentationSkiped;
        string answerLetter;
        List<string> pageKeyCodes;
        List<Action> pageFunctions;
        [ObservableProperty]
        private FormattedString pageFormattedLabel;
        [ObservableProperty]
        private string _pageLabel;
        [ObservableProperty]
        private double _textSize;
        private DVViewModelSpeak dVViewModelSpeak;
        private DVViewModelFunctions dVViewModelFunctions;
        private FingerMapping fingerMapping;
        public ConfigViewModel(DVViewModelSpeak dVViewModelSpeak,
                               DVViewModelFunctions dVViewModelFunctions,
                               FingerMapping fingerMapping)
        {
            this.dVViewModelSpeak = dVViewModelSpeak;
            this.dVViewModelFunctions = dVViewModelFunctions;
            this.fingerMapping = fingerMapping;
            pageKeyCodes = new List<string>()
            {
                "Up", "Down", "Tab", "ShiftTab",
                "Escape"
            };
            for (int i = 1; i <= totalOptions; i++)
            {
                pageKeyCodes.Add($"{i}");
            }
            pageFunctions = new List<Action>()
            {
                SpeakRate, TypingToggle, ChangeFont, TimeDivider, CountRepetitions, EnableInstructions, ResetConfig
            };
        }
        public void OnPage()
        {
            currentSpeakRate = -1;
            prevFontSize = -1;
            dVViewModelFunctions.SetCurrentPageIdentifier("no menu de configurações");
            dVViewModelFunctions.ClearHelpOptions();
            Thread.Sleep(100);
            var textList = new List<string>();
            var speechList = new List<string>();
            if ((DVPersistence.Get<bool>("instructionsEnabled") && updateText) || storedText)
            {
                introductionLines = 2;
                storedText = true;
                textList.Add($"Use os números de 1 a {totalOptions}, tab e shift tab ou setas verticais para navegar entre as opções. Depois tecle Enter para confirmar. Escape volta.");
                speechList.Add($"Use os números de 1 a {totalOptions}, tab e shift tab ou setas verticais para navegar entre as opções. Depois tecle êmter para confirmar. Esqueipe volta.");
            }
            else
            {
                introductionLines = 1;
            }
            updateText = false;
            var defaultTextList = new List<string>() {
                "As opções são: ",
                "1 - Alterar velocidade de fala",
                "2 - Falar teclagem",
                "3 - Mudar tamanho do texto",
                "4 - Mudar divisor de tempo",
                "5 - Falar repetições na lição",
                "6 - Apresentar instruções de navegação",
                "7 - Restaurar configurações originais",
                string.Empty
            };
            var defaultSpeechList = new List<string>() {
                "As opções são: ",
                "1 - Alterar velocidade de fala",
                "2 - Falar teclagem",
                "3 - Mudar tamanho do texto",
                "4 - Mudar divisor de tempo",
                "5 - Falar repetições na lição",
                "6 - Apresentar instruções de navegação",
                "7 - Restaurar configurações originais",
                string.Empty
            };
            if (defaultSpeechList.Count == defaultTextList.Count)
            {
                for (int i = 0; i < defaultTextList.Count; i++)
                {
                    textList.Add(defaultTextList[i]);
                    speechList.Add(defaultSpeechList[i]);
                }
            }
            textList = textList.Select(text => dVViewModelFunctions.EditStringForVoiceOver(text)).ToList();
            speechList = speechList.Select(speech => dVViewModelFunctions.EditStringForVoiceOver(speech)).ToList();

            dVViewModelSpeak.SetTextAndSpeech(textList, speechList).RegisterUpdateScreen((text) =>
            {
                
                
                PageFormattedLabel = text;
                TextSize = DVPersistence.Get<double>("fontSize");
            });


            dVViewModelFunctions.SetFirstOptionLineNumber(dVViewModelSpeak.LineCount() - totalOptions - 1);
            dVViewModelFunctions.SetLastOptionLineNumber(introductionLines + totalOptions - 1);
            dVViewModelFunctions.SetOptionNumberStart(dVViewModelSpeak.LineCount() - totalOptions - 2);
            dVViewModelFunctions.SetNumberCaptureTimeInterval(timeToCaptureNumber);
            answerController = -1;
            answerOption = true;
            outcome = false;
            apresentationSkiped = false;
            dVViewModelSpeak.SpeakAll();
        }
        private void Enter()
        {
            outcome = true;
            answerOption = false;
            apresentationSkiped = false;
            dVViewModelSpeak.Set("maintainTag", false);
            int selectNumber = dVViewModelFunctions.MapOptionToItem();
            pageFunctions[selectNumber]();
        }
        private void SpeakRate()
        {
            answerController = pageFunctions.IndexOf(SpeakRate);
            string speakRateOutput = $"A velocidade de fala atualmente definida é {DVPersistence.Get<int>("speakRate")}.\n" +
                "Digite um número de 1 a 9 para definir a velocidade de fala.\n" +
                "Escape volta.";
            string textSpeak = $"A velocidade de fala atualmente definida é {DVPersistence.Get<int>("speakRate")}. " +
                "Digite um número de 1 a 9 para definir a velocidade de fala. " +
                "Esqueipe volta.";
            DisplayOutcome(speakRateOutput, textSpeak);
        }
        private void ChangeSpeakRate()
        {
            
            outcome = true;
            
            DVSpeak.GetInstance().SetSpeechRate(currentSpeakRate);
            string speakRateOutputText = $"A velocidade de fala definida é {currentSpeakRate}. Aperte enter para salvar ou escape para descartar a alteração.";
            string speakRateOutputSpeech = $"A velocidade de fala definida é {currentSpeakRate}. Aperte êmter para salvar ou esqueipe para descartar a alteração.";
            DisplayOutcome(speakRateOutputText, speakRateOutputSpeech);
            
        }
        private void ConfirmSpeakRate()
        {
            answerController = -1;
            DVPersistence.Set("speakRate", currentSpeakRate);
            answerOption = true;
            OnPage();
        }
        private void TypingToggle()
        {
            answerController = pageFunctions.IndexOf(TypingToggle);
            string speakInputString = (DVPersistence.Get<bool>("speakInput")) ? "habilitada" : "desabilitada";
            string typingOutput = $"A fala de teclagem está atualmente {speakInputString}. Para habilitá-la digite S ou para desabilitá-la digite N.";
            DisplayOutcome(typingOutput, typingOutput);
        }
        private void ChangeTyping()
        {
            outcome = true;
            answerController = -1;
            string speakInputString = (DVPersistence.Get<bool>("speakInput")) ? "habilitada" : "desabilitada";
            string typingOutput = $"Fala de teclagem {speakInputString}.";
            DisplayOutcome(typingOutput, typingOutput);
            answerOption = true;
        }
        private void ChangeFont()
        {
            answerController = pageFunctions.IndexOf(ChangeFont);
            string fontSizeOutput = $"O tamanho da fonte atualmente definido é {(int)((DVPersistence.Get<double>("fontSize") - (DVPersistence.GetFontSize() / 2)) / (DVPersistence.GetFontSize() / 2))}.\n" +
                "Digite um número de 1 a 3 para definir o tamanho.\n" +
                "Escape volta.";
            string textSpeak = $"O tamanho da fonte atualmente definido é {(int)((DVPersistence.Get<double>("fontSize") - (DVPersistence.GetFontSize() / 2)) / (DVPersistence.GetFontSize() / 2))}.\n" +
                "Digite um número de 1 a 3 para definir o tamanho.\n" +
                "Esqueipe volta.";
            DisplayOutcome(fontSizeOutput, textSpeak);
        }
        private void ChangeTextSize()
        {
            
            outcome = true; 
            string fontSizeOutputText = $"O tamanho de texto definido é {(int)((DVPersistence.Get<double>("fontSize") - (DVPersistence.GetFontSize() / 2)) / (DVPersistence.GetFontSize() / 2))}. Aperte enter para salvar ou escape para descartar a alteração.";
            string fontSizeOutputSpeech = $"O tamanho de texto definido é {(int)((DVPersistence.Get<double>("fontSize") - (DVPersistence.GetFontSize() / 2)) / (DVPersistence.GetFontSize() / 2))}. Aperte êmter para salvar ou esqueipe para descartar a alteração.";
            DisplayOutcome(fontSizeOutputText, fontSizeOutputSpeech);
            
        }
        private void ConfirmTextSize()
        {
            answerController = -1;
            answerOption = true;
            OnPage();
        }
        private void TimeDivider()
        {
            answerController = pageFunctions.IndexOf(TimeDivider);
            string timeDividerOutput = $"O divisor de tempo é utilizado para diminuir o tempo máximo para realizar uma lição. O divisor de tempo atualmente definido é {DVPersistence.Get<int>("timeDivider")}.\n" +
                "Digite um número de 1 a 5 para definir o valor do novo divisor.\n" +
                "Escape volta.";
            string textSpeak = $"O divisor de tempo é utilizado para diminuir o tempo máximo para realizar uma lição. O divisor de tempo atualmente definido é {DVPersistence.Get<int>("timeDivider")}.\n" +
                "Digite um número de 1 a 5 para definir o valor do novo divisor.\n" +
                "Esqueipe volta.";
            DisplayOutcome(timeDividerOutput, textSpeak);
        }
        private void ChangeTimeDivider()
        {
            answerController = -1;
            outcome = true;
            string timeDividerOutput = $"O novo divisor de tempo definido é {DVPersistence.Get<int>("timeDivider")}.";
            DisplayOutcome(timeDividerOutput, timeDividerOutput);
            answerOption = true;
        }
        private void CountRepetitions()
        {
            answerController = pageFunctions.IndexOf(CountRepetitions);
            string countRepetitionsString = (DVPersistence.Get<bool>("countRepetitions")) ? "habilitada" : "desabilitada";
            string repetitionsOutput = $"A fala do número de repetições está atualmente {countRepetitionsString}. Para habilitá-la digite S ou para desabilitá-la digite N.";
            DisplayOutcome(repetitionsOutput, repetitionsOutput);
        }
        private void ChangeRepetitions()
        {
            outcome = true;
            answerController = -1;
            string repetitionsString = (DVPersistence.Get<bool>("countRepetitions")) ? "habilitada" : "desabilitada";
            string repetitionsOutput = $"Fala de repetições {repetitionsString}.";
            DisplayOutcome(repetitionsOutput, repetitionsOutput);
            answerOption = true;
        }
        private void EnableInstructions()
        {
            answerController = pageFunctions.IndexOf(EnableInstructions);
            string enableInstructionsString = (DVPersistence.Get<bool>("instructionsEnabled")) ? "habilitada" : "desabilitada";
            string enableInstructionsOutput = $"A apresentação de instruções de navegação nas telas está atualmente {enableInstructionsString}. Para habilitá-la digite S ou para desabilitá-la digite N.";
            DisplayOutcome(enableInstructionsOutput, enableInstructionsOutput);
        }
        private void ChangeInstructions()
        {
            outcome = true;
            answerController = -1;
            string repetitionsString = (DVPersistence.Get<bool>("instructionsEnabled")) ? "habilitada" : "desabilitada";
            string repetitionsOutput = $"Apresentação de instruções de navegação {repetitionsString}.";
            DisplayOutcome(repetitionsOutput, repetitionsOutput);
            answerOption = true;
        }
        private void ResetConfig()
        {
            answerController = pageFunctions.IndexOf(ResetConfig);
            string restoreOutput = "Você realmente deseja restaurar as configurações do Digitavox USP para as originais? " +
                "Tecle S para confirmar ou N para negar." +
                "Depois tecle Enter para confirmar.";
            string textSpeak = "Você realmente deseja restaurar as configurações do Digitavóx USP para as originais? " +
                "Tecle s para confirmar ou n para negar. " +
                "Depois tecle êmter para confirmar.";
            DisplayOutcome(restoreOutput , textSpeak);
        }
        private void ResetConfigAccepted()
        {
            outcome = true;
            answerController = -1;
            string restoreOutput = "As configurações foram restauradas.";
            DisplayOutcome(restoreOutput, restoreOutput);
            answerOption = true;
        }
        private void DisplayOutcome(string text, string speak)
        {
            string modifiedText = dVViewModelFunctions.EditStringForVoiceOver(text);
            string modifiedSpeak = dVViewModelFunctions.EditStringForVoiceOver(speak);

            dVViewModelSpeak.ChangeLine(modifiedText, modifiedSpeak, totalOptions + introductionLines);
            dVViewModelSpeak.SpeakOneLine(totalOptions + introductionLines, () => { });
        }
        private void RemoveOutcome()
        {
            outcome = false;
            dVViewModelSpeak.ChangeLine(string.Empty, string.Empty, totalOptions + introductionLines);
        }
        private void Confirm()
        {
            if (answerController == pageFunctions.IndexOf(ResetConfig) && answerLetter == "S")
            {
                DVPersistence.SetDefaulConfig();
                ResetConfigAccepted();
            }
            else if (answerController == pageFunctions.IndexOf(ResetConfig) && answerLetter == "N")
            {
                OnPage();
            }
            else if (answerController == pageFunctions.IndexOf(TypingToggle))
            {
                ChangeTyping();
            }
            else if (answerController == pageFunctions.IndexOf(CountRepetitions))
            {
                ChangeRepetitions();
            }
            else if (answerController == pageFunctions.IndexOf(EnableInstructions))
            {
                ChangeInstructions();
            }
        }
        private void Reject(string key)
        {
            dVViewModelSpeak.Speak(key, () =>
            {
                OnPage();
            });
        }
        private void Select(string key, Action onCompleted)
        {
            dVViewModelSpeak.Speak($"{key}", onCompleted.Invoke);
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
                dVViewModelFunctions.LastLineIsText(outcome);
                if (answerController == pageFunctions.IndexOf(SpeakRate))
                {
                    if (bean.code == "1" || bean.code == "2" || bean.code == "3" || bean.code == "4" || bean.code == "5" || bean.code == "6" || bean.code == "7" || bean.code == "8" || bean.code == "9")
                    {
                        
                        currentSpeakRate = int.Parse(bean.code);
                        Select(bean.speakOnlyChar, () =>
                        {
                            ChangeSpeakRate();
                        });
                    }
                    else if (bean.code == "Escape")
                    {
                        if (currentSpeakRate != -1)
                        {
                            DVSpeak.GetInstance().SetSpeechRate(DVPersistence.Get<int>("speakRate"));
                        }
                        Reject(bean.speakOnlyChar);
                    }
                    else if (bean.code == "Enter" && currentSpeakRate != -1)
                    {
                        ConfirmSpeakRate();
                    }
                    else if (!DVKeyboard.IsModifierKey(bean.code))
                    {
                        dVViewModelFunctions.InvalidOption(bean.speakOnlyChar);
                    }
                }
                else if (answerController == pageFunctions.IndexOf(ChangeFont))
                {
                    if (bean.code == "1" || bean.code == "2" || bean.code == "3")
                    {
                        prevFontSize = DVPersistence.Get<double>("fontSize");
                        DVPersistence.Set("fontSize", (DVPersistence.GetFontSize() / 2) + (DVPersistence.GetFontSize() / 2) * int.Parse(bean.code));
                        Select(bean.speakOnlyChar, () =>
                        {
                            ChangeTextSize();
                        });
                    }
                    else if (bean.code == "Escape")
                    {
                        if (prevFontSize != -1)
                        {
                            DVPersistence.Set("fontSize", prevFontSize);
                        }
                        Reject(bean.speakOnlyChar);
                    }
                    else if (bean.code == "Enter" && prevFontSize != -1)
                    {
                        ConfirmTextSize();
                    }
                    else if (!DVKeyboard.IsModifierKey(bean.code))
                    {
                        dVViewModelFunctions.InvalidOption(bean.speakOnlyChar);
                    }
                }
                else if (answerController == pageFunctions.IndexOf(TimeDivider))
                {
                    if (bean.code == "1" || bean.code == "2" || bean.code == "3" || bean.code == "4" || bean.code == "5")
                    {
                        DVPersistence.Set("timeDivider", int.Parse(bean.code));
                        Select(bean.speakOnlyChar, () =>
                        {
                            ChangeTimeDivider();
                        });
                    }
                    else if (bean.code == "Escape")
                    {
                        Reject(bean.speakOnlyChar);
                    }
                    else if (!DVKeyboard.IsModifierKey(bean.code))
                    {
                        dVViewModelFunctions.InvalidOption(bean.speakOnlyChar);
                    }
                }
                else if (answerController == pageFunctions.IndexOf(TypingToggle))
                {
                    if (bean.code == "S" || bean.code == "s")
                    {
                        answerLetter = "S";
                        DVPersistence.Set("speakInput", true);
                        Select(bean.speakOnlyChar, () =>
                        {
                            Confirm();
                        });
                    }
                    else if (bean.code == "N" || bean.code == "n")
                    {
                        answerLetter = "N";
                        DVPersistence.Set("speakInput", false);
                        Select(bean.speakOnlyChar, () =>
                        {
                            Confirm();
                        });
                    }
                    else if (bean.code == "Escape")
                    {
                        Reject(bean.speakOnlyChar);
                    }
                    else if (!DVKeyboard.IsModifierKey(bean.code))
                    {
                        dVViewModelFunctions.InvalidOption(bean.speakOnlyChar);
                    }
                }
                else if (answerController == pageFunctions.IndexOf(CountRepetitions))
                {
                    if (bean.code == "S" || bean.code == "s")
                    {
                        answerLetter = "S";
                        DVPersistence.Set("countRepetitions", true);
                        Select(bean.speakOnlyChar, () =>
                        {
                            Confirm();
                        });
                    }
                    else if (bean.code == "N" || bean.code == "n")
                    {
                        answerLetter = "N";
                        DVPersistence.Set("countRepetitions", false);
                        Select(bean.speakOnlyChar, () =>
                        {
                            Confirm();
                        });
                    }
                    else if (bean.code == "Escape")
                    {
                        Reject(bean.speakOnlyChar);
                    }
                    else if (!DVKeyboard.IsModifierKey(bean.code))
                    {
                        dVViewModelFunctions.InvalidOption(bean.speakOnlyChar);
                    }
                }
                else if (answerController == pageFunctions.IndexOf(EnableInstructions))
                {
                    if (bean.code == "S" || bean.code == "s")
                    {
                        answerLetter = "S";
                        DVPersistence.Set("instructionsEnabled", true);
                        Select(bean.speakOnlyChar, () =>
                        {
                            Confirm();
                        });
                    }
                    else if (bean.code == "N" || bean.code == "n")
                    {
                        answerLetter = "N";
                        DVPersistence.Set("instructionsEnabled", false);
                        Select(bean.speakOnlyChar, () =>
                        {
                            Confirm();
                        });
                    }
                    else if (bean.code == "Escape")
                    {
                        Reject(bean.speakOnlyChar);
                    }
                    else if (!DVKeyboard.IsModifierKey(bean.code))
                    {
                        dVViewModelFunctions.InvalidOption(bean.speakOnlyChar);
                    }
                }
                else if (answerController == pageFunctions.IndexOf(ResetConfig))
                {
                    if (bean.code == "S" || bean.code == "s")
                    {
                        answerLetter = "S";
                        Select(bean.speakOnlyChar, () =>
                        {
                            Confirm();
                        });
                    }
                    else if (bean.code == "N" || bean.code == "n")
                    {
                        answerLetter = "N";
                        Select(bean.speakOnlyChar, () =>
                        {
                            Confirm();
                        });
                    }
                    else if (bean.code == "Escape")
                    {
                        Reject(bean.speakOnlyChar);
                    }
                    else if (!DVKeyboard.IsModifierKey(bean.code))
                    {
                        dVViewModelFunctions.InvalidOption(bean.speakOnlyChar);
                    }
                }
                else if (answerOption)
                {
                    if (outcome) RemoveOutcome();
                    if (bean.code == " ")
                    {
                        OnPage();
                    }
                    else if (bean.code == "Enter" && apresentationSkiped)
                    {
                        Enter();
                    }
                    else if (pageKeyCodes.Contains(bean.code) || (bean.code == "!" && DVDevice.IsVirtual()))
                    {
                        if (bean.code == "Escape" || (bean.code == "!" && DVDevice.IsVirtual()))
                        {
                            updateText = true;
                            storedText = false;
                        }
                        dVViewModelFunctions.HandleKeyCode(bean.code);
                        apresentationSkiped = true;
                    }
                    else if (!DVKeyboard.IsModifierKey(bean.code))
                    {
                        dVViewModelFunctions.InvalidOption(bean.speakOnlyChar);
                        apresentationSkiped = true;
                    }
                }
            }
            return true;
        }
    }
}