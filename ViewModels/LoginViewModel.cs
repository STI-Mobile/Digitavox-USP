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
using Microsoft.Maui.Controls;

namespace Digitavox.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        List<string> pressedKeys = new List<string>();
        bool enterName;
        bool titleOutput;
        string userName;
        bool enterAnswer;
        int accept = -1;
        [ObservableProperty]
        private FormattedString pageFormattedLabel;
        [ObservableProperty]
        private string _pageLabel;
        [ObservableProperty]
        private double _textSize;
        public List<string> alphabetLetters = new List<string>
        {
            "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M",
            "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z",
            "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m",
            "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z",
            "Ç", "ç"
        };
        List<string> changeVowels = new List<string>
        {
            "á", "ó", "é", "í", "ú",
            "à", "ò", "è", "ì", "ù",
            "â", "ô", "ê", "î", "û",
            "ä", "ö", "ë", "ï", "ü",
            "ã", "õ"
        };
        public List<string> speakUserName = new List<string>();
        private DVViewModelSpeak dVViewModelSpeak;
        private FingerMapping fingerMapping;
        private UserProgress userProgress;
        private DVViewModelFunctions dVViewModelFunctions;
        public LoginViewModel(DVViewModelSpeak dVViewModelSpeak, 
                              FingerMapping fingerMapping,
                              UserProgress userProgress,
                              DVViewModelFunctions dVViewModelFunctions)
        {
            this.dVViewModelSpeak = dVViewModelSpeak;
            this.fingerMapping = fingerMapping;
            this.userProgress = userProgress;
            this.dVViewModelFunctions = dVViewModelFunctions;
            
        }
        public void OnPage()
        {
            dVViewModelFunctions.SetCurrentPageIdentifier("na tela de identificação");
            var textList = new List<string>()
            {
                
                "Digitavox USP - Cursos de Digitação",
                "Digite seu nome depois tecle enter para entrar, ou tecle enter para entrar como anônimo",
                string.Empty,
                string.Empty
            };

            var speechList = new List<string>()
            {
                "Digitavóx uspe - Cursos de digitação",
                "Digite seu nome depois tecle êmter para entrar, ou tecle êmter para entrar como anônimo",
                string.Empty,
                string.Empty
            };

            textList = textList.Select(text => dVViewModelFunctions.EditStringForVoiceOver(text)).ToList();
            speechList = speechList.Select(speech => dVViewModelFunctions.EditStringForVoiceOver(speech)).ToList();

            dVViewModelSpeak.SetTextAndSpeech(textList, speechList).RegisterUpdateScreen((text) =>
            {
                
                
                PageFormattedLabel = text;
                TextSize = DVPersistence.Get<double>("fontSize");
            });

            titleOutput = true;
            enterAnswer = false;
            userName = string.Empty;
            speakUserName.Clear();
            dVViewModelSpeak.Set("maintainTag", false);
            dVViewModelSpeak.SpeakOneLine(0, () =>
            {
                enterName = true;
                
                titleOutput = false;
                dVViewModelSpeak.SpeakOneLine(1, () =>
                {
                    dVViewModelSpeak.Set("maintainTag", true);
                });
            });
            WeakReferenceMessenger.Default.Send(new DVMessage("BecomeFirstResponder"));
        }

        private void WriteName(string letter)
        {
            dVViewModelSpeak.ChangeLine(userName, userName, 2);
            dVViewModelSpeak.Speak(letter, () => { });
        }
        private void User()
        {
            enterName = false;
            dVViewModelSpeak.Set("maintainTag", false);
            if (userName.Equals(string.Empty))
            {
                accept = 1;
                enterAnswer = true;
                EnterAnonymous();
            }
            else
            {
                dVViewModelSpeak.SpeakOneLine(2, () =>
                {
                    GetRegistrationStatus();
                });
            }
        }
        private void GetRegistrationStatus()
        {
            userProgress.UserRegistration(userName);
            if (userProgress.FirstLogin())
            {
                accept = 0;
                enterAnswer = true;
                NewLogin();
            }
            else
            {
                LoginConcluded();
            }
        }
        private void LoginConcluded()
        {
            string userNameSpeak = userName;
            if (userName == "anonimo") userNameSpeak = "anônimo";
            string text = $"Bem-vindo {userName}, o Digitavox USP vai te ajudar a desenvolver habilidades no teclado do computador.";
            string speech = $"Bem-vindo {userNameSpeak}, o Digitavóx uspe vai te ajudar a desenvolver habilidades no teclado do computador.";
            dVViewModelSpeak.ChangeLine(text, speech, 3);
            dVViewModelSpeak.SpeakOneLine(3, () =>
            {
                if (!dVViewModelFunctions.OnAlert())
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        GoToMenuPage();
                    });
                }
            });
        }
        private void NewLogin()
        {
            string userNameSpeak = userName;
            if (userName == "anonimo") userNameSpeak = "anônimo";
            string text = $"Usuário {userName} não cadastrado. Deseja cadastrá-lo agora? Tecle S para confirmar ou N para cancelar.";
            string speech = $"Usuário {userNameSpeak} não cadastrado. Deseja cadastrá-lo agora? Tecle s para confirmar ou n para cancelar.";
            dVViewModelSpeak.ChangeLine(text, speech, 3);
            dVViewModelSpeak.SpeakOneLine(3, () => { });
        }
        private void EnterAnonymous()
        {
            string text = "Sem nome, deseja entrar como anônimo? Tecle S para confirmar ou N para cancelar.";
            string speech = "Sem nome, deseja entrar como anônimo? Tecle s para confirmar ou n para cancelar.";
            dVViewModelSpeak.ChangeLine(text, speech, 3);
            dVViewModelSpeak.SpeakOneLine(3, () => { });
        }
        private void RepeatRegisterText()
        {
            string e = string.Empty;
            dVViewModelSpeak.ChangeLine(e, e, 2);
            dVViewModelSpeak.ChangeLine(e, e, 3);
            userName = string.Empty;
            speakUserName.Clear();
            enterName = true;
            enterAnswer = false;
            dVViewModelSpeak.SpeakOneLine(1, () => { });
        }
        private void Accept()
        {
            dVViewModelSpeak.Speak("s", () =>
            {
                if (accept == 0)
                {
                    userProgress.NewProgress();
                    
                    LoginConcluded();
                }
                else if (accept == 1)
                {
                    userName = "anonimo";
                    accept = 0;
                    GetRegistrationStatus();
                }
            });
        }
        private void Reject()
        {
            accept = -1;
            dVViewModelSpeak.Speak("n", () =>
            {
                RepeatRegisterText();
            });
        }
        private async void GoToMenuPage()
        {
            await Shell.Current.GoToAsync("Menu");
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
        public void HandleUnexpectedKey(string keySpeak)
        {
            if (keySpeak == "Esqueipe")
                dVViewModelSpeak.Speak("Você está na tela de identificação", () => { });
            else
                dVViewModelFunctions.InvalidOption(keySpeak);
        }
        public bool OnPageKeyPress(int keyCode, int modifiers)
        {
            pressedKeys.Remove(fingerMapping.mapKeyCode(keyCode));
            var bean = fingerMapping.MapKey(keyCode, modifiers, pressedKeys);
            if (bean.code != null)
            {
                if (!titleOutput)
                {
                    if (bean.code != null) dVViewModelSpeak.Skip();
                    if (enterName)
                    {
                        if (bean.code == "Enter")
                        {
                            User();
                        }
                        else if (bean.code == "Backspace")
                        {
                            if (userName.Length > 1)
                            {
                                string last;
                                userName = userName.Substring(0, userName.Length - 1);
                                speakUserName.RemoveAt(speakUserName.Count - 1);
                                last = speakUserName.Last();
                                WriteName(last);
                            }
                            else
                            {
                                userName = string.Empty;
                                WriteName(string.Empty);
                            }
                        }
                        else
                        {
                            if (alphabetLetters.Contains(bean.code))
                            {
                                string letter = bean.speakOnlyChar;
                                userName = userName + bean.code;
                                speakUserName.Add(letter);
                                WriteName(letter);
                            }
                            else if (changeVowels.Contains(bean.code))
                            {
                                List<string> newVowel = new List<string>()
                                {
                                    "a", "o", "e", "i", "u"
                                };
                                int index = changeVowels.IndexOf(bean.code) % 5;
                                string letter = fingerMapping.Code2Speak(newVowel[index]);
                                userName = userName + newVowel[index];
                                speakUserName.Add(letter);
                                WriteName(letter);
                            }
                            else if (!DVKeyboard.IsModifierKey(bean.code))
                            {
                                HandleUnexpectedKey(bean.speakOnlyChar);
                            }
                        }
                    }
                    else if (enterAnswer)
                    {
                        if (bean.code == "S" || bean.code == "s")
                        {
                            enterAnswer = false;
                            Accept();
                        }
                        else if (bean.code == "N" || bean.code == "n")
                        {
                            enterAnswer = false;
                            if (accept >= 0) Reject();
                        }
                        else if (!DVKeyboard.IsModifierKey(bean.code))
                        {
                            HandleUnexpectedKey(bean.speakOnlyChar);
                        }
                    }
                }
            }
            return true;
        }
    }
}