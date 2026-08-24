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
using Digitavox.Presentation.Input;

namespace Digitavox.ViewModels
{
    public partial class UserOptionsViewModel : ObservableObject, IOnPageKeyPress
    {
        int timeToCaptureNumber = 0;
        int totalOptions = 2;
        int introductionLines = 1;
        bool answerOption; 
        bool outcome;
        bool apresentationSkiped;
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
        private KeyboardInputProcessor keyboardInputProcessor;
        private UserProgress userProgress;
        private readonly ISettingsService settingsService;
        private readonly INavigationService navigationService;
        private readonly IAppEnvironment appEnvironment;
        public UserOptionsViewModel(DVViewModelSpeak dVViewModelSpeak,
                                    DVViewModelFunctions dVViewModelFunctions,
                                    KeyboardInputProcessor keyboardInputProcessor,
                                    UserProgress userProgress,
                                    ISettingsService settingsService,
                                    INavigationService navigationService,
                                    IAppEnvironment appEnvironment)
        {
            this.dVViewModelSpeak = dVViewModelSpeak;
            this.dVViewModelFunctions = dVViewModelFunctions;
            this.keyboardInputProcessor = keyboardInputProcessor;
            this.userProgress = userProgress;
            this.settingsService = settingsService;
            this.navigationService = navigationService;
            this.appEnvironment = appEnvironment;
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
                UserOn, ChangeUser
            };
        }
        public void OnPage()
        {
            dVViewModelFunctions.SetCurrentPageIdentifier("no menu de opções de usuário");
            Thread.Sleep(100);
            var textList = new List<string>();
            var speechList = new List<string>();
            if (settingsService.Get<bool>("instructionsEnabled"))
            {
                introductionLines = 2;
                textList.Add($"Use os números de 1 a {totalOptions}, tab e shift tab ou setas verticais para navegar entre as opções. Depois tecle enter para confirmar. Escape volta.");
                speechList.Add($"Use os números de 1 a {totalOptions}, tab e shift tab ou setas verticais para navegar entre as opções. Depois tecle êmter para confirmar. Esqueipe volta. ");
            }
            else
            {
                introductionLines = 1;
            }
            var defaultTextList = new List<string>()
            {
                "As opções são: ",
                "1 - Falar usuário atualmente logado",
                "2 - Alterar usuário logado",
                string.Empty
            };

            var defaultSpeechList = new List<string>()
            {
                "As opções são: ",
                "1 - Falar usuário atualmente logado",
                "2 - Alterar usuário logado",
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
                TextSize = settingsService.Get<double>("fontSize");
            });

            dVViewModelFunctions.SetFirstOptionLineNumber(dVViewModelSpeak.LineCount() - totalOptions - 1);
            dVViewModelFunctions.SetLastOptionLineNumber(dVViewModelSpeak.LineCount() - 2);
            dVViewModelFunctions.SetOptionNumberStart(dVViewModelSpeak.LineCount() - totalOptions - 2);
            dVViewModelFunctions.SetNumberCaptureTimeInterval(timeToCaptureNumber);
            answerOption = false;
            outcome = false;
            apresentationSkiped = false;
            dVViewModelSpeak.SpeakAll();
        }
        private void Enter()
        {
            outcome = true;
            apresentationSkiped = false;
            dVViewModelSpeak.Set("maintainTag", false);
            int selectNumber = dVViewModelFunctions.MapOptionToItem();
            pageFunctions[selectNumber]();
        }
        private void UserOn()
        {
            string userOutput = $"Usuário {userProgress.GetUserName()} logado.";
            dVViewModelSpeak.ChangeLine(userOutput, userOutput, introductionLines + totalOptions);
            dVViewModelSpeak.SpeakOneLine(introductionLines + totalOptions, () => { });
        }
        private void ChangeUser()
        {
            answerOption = true;
            string changeOutput = $"Você realmente deseja alterar o usuário logado? " +
                "Tecle S para confirmar ou N para negar.";
            dVViewModelSpeak.ChangeLine(changeOutput, changeOutput, introductionLines + totalOptions);
            dVViewModelSpeak.SpeakOneLine(introductionLines + totalOptions, () => { });
        }
        private void Accept()
        {
            dVViewModelSpeak.Speak("s", () => 
            {
                appEnvironment.RunOnMainThread(() =>
                {
                    _ = LogoutAsync();
                });
            });
        }
        private void RemoveOutcome()
        {
            outcome = false;
            dVViewModelSpeak.ChangeLine(string.Empty, string.Empty, introductionLines + totalOptions);
        }
        private async Task LogoutAsync()
        {
            userProgress.UserLogout();
            dVViewModelFunctions.LastLineIsText(false);
            
            await navigationService.GoBackAsync(levels: 2);
        }
        private void Reject()
        {
            dVViewModelSpeak.Speak("n", () => 
            {
                OnPage();
            });
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
                dVViewModelFunctions.LastLineIsText(outcome);
                if (answerOption)
                {
                    if (bean.code == "S" || bean.code == "s")
                    {
                        RemoveOutcome();
                        Accept();
                    }
                    else if (bean.code == "N" || bean.code == "n")
                    {
                        RemoveOutcome();
                        Reject();
                    }
                    else if (!DVKeyboard.IsModifierKey(bean.code))
                    {
                        dVViewModelFunctions.InvalidOption(bean.speakOnlyChar);
                    }
                }
                else
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
                    else if (pageKeyCodes.Contains(bean.code) || (bean.code == "!" && appEnvironment.IsVirtualDevice))
                    {
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
