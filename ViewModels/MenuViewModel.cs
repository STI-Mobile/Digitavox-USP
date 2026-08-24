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
using Digitavox.Models;
using Digitavox.Core.Abstractions;
using Digitavox.Presentation.Input;
using Digitavox.Helpers;
namespace Digitavox.ViewModels
{
    public partial class MenuViewModel : ObservableObject, IOnPageKeyPress
    {
        int timeToCaptureNumber = 0;
        int totalOptions = 7;
        List<string> pageKeyCodes;
        [ObservableProperty]
        private FormattedString pageFormattedLabel;
        [ObservableProperty]
        private string _pageLabel;
        [ObservableProperty]
        private double _textSize;
        private DVViewModelSpeak dVViewModelSpeak;
        private DVViewModelFunctions dVViewModelFunctions;
        private KeyboardInputProcessor keyboardInputProcessor;
        private readonly ISettingsService settingsService;
        private readonly IAppEnvironment appEnvironment;
        public MenuViewModel(DVViewModelSpeak dVViewModelSpeak,
                             DVViewModelFunctions dVViewModelFunctions,
                             KeyboardInputProcessor keyboardInputProcessor,
                             ISettingsService settingsService,
                             IAppEnvironment appEnvironment)
        {
            this.dVViewModelSpeak = dVViewModelSpeak;
            this.dVViewModelFunctions = dVViewModelFunctions;
            this.keyboardInputProcessor = keyboardInputProcessor;
            this.settingsService = settingsService;
            this.appEnvironment = appEnvironment;
            pageKeyCodes = new List<string>()
            {
                "Up", "Down", "Tab", "ShiftTab",
                "Enter"
            };
            for (int i = 1; i <= totalOptions; i++)
            {
                pageKeyCodes.Add($"{i}");
            }
        }
        public void OnPage()
        {
            dVViewModelFunctions.SetCurrentPageIdentifier("no menu inicial");
            dVViewModelFunctions.ClearHelpOptions();
            Thread.Sleep(100);
            var textList = new List<string>();
            var speechList = new List<string>();
            if (settingsService.Get<bool>("instructionsEnabled"))
            {
                textList.Add($"Use os números de 1 a {totalOptions}, tab e shift tab ou setas verticais para navegar entre as opções. Depois tecle Enter para confirmar. Escape volta.");
                speechList.Add($"Use os números de 1 a {totalOptions}, tab e shift tab ou setas verticais para navegar entre as opções. Depois tecle êmter para confirmar. Esqueipe volta.");
            }
            var defaultTextList = new List<string>()
            {
                "As opções são: ",
                "1 - Reconhecimento de teclado",
                "2 - Cursos de digitação",
                "3 - Opções de usuário",
                "4 - Configurações",
                "5 - Instruções de uso",
                "6 - Política de privacidade",
                "7 - Licenças de terceiros"
            };
            var defaultSpeechList = new List<string>()
            {
                "As opções são : ",
                "1 - Reconhecimento de teclado  ",
                "2 - Cursos de digitação  ",
                "3 - Opções de usuário  ",
                "4 - Configurações",
                "5 - Instruções de uso",
                "6 - Política de privacidade",
                "7 - Licenças de terceiros"
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


            dVViewModelFunctions.SetFirstOptionLineNumber(dVViewModelSpeak.LineCount() - totalOptions);
            dVViewModelFunctions.SetLastOptionLineNumber(dVViewModelSpeak.LineCount() - 1);
            dVViewModelFunctions.SetOptionNumberStart(dVViewModelSpeak.LineCount() - totalOptions - 1);
            dVViewModelFunctions.SetNumberCaptureTimeInterval(timeToCaptureNumber);
            dVViewModelFunctions.SetOption2PageList(new List<AppRoute>()
            {
                AppRoute.Keyboard, AppRoute.Courses, AppRoute.UserOptions, AppRoute.Config,
                AppRoute.Tutorial, AppRoute.PrivacyPolicy, AppRoute.ThirdPartyLicenses
            });
            dVViewModelSpeak.SpeakAll();
            WeakReferenceMessenger.Default.Send(new DVMessage("BecomeFirstResponder"));
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
                if (dVViewModelFunctions.KeysEnabled())
                {
                    if (bean.code == "Escape" || (bean.code == "!" && appEnvironment.IsVirtualDevice))
                    {
                        string exitText = "Você está no menu inicial.";
                        dVViewModelSpeak.Speak(exitText, () => { });
                    }
                    else
                    {
                        if (bean.code == " ")
                        {
                            OnPage();
                        }
                        else if (pageKeyCodes.Contains(bean.code))
                        {
                            dVViewModelFunctions.HandleKeyCode(bean.code);
                        }
                        else if (!DVKeyboard.IsModifierKey(bean.code))
                        {
                            dVViewModelFunctions.InvalidOption(bean.speakOnlyChar);
                        }
                    }
                }
            }
            return true;
        }
    }
}
