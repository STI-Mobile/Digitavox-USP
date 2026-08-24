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
using Digitavox.Core.Abstractions;
using Digitavox.Presentation.Input;

namespace Digitavox.ViewModels
{
    public partial class PrivacyPolicyViewModel : IKeyboardInputHandler
    {
        List<string> pageKeyCodes;
        private KeyboardInputProcessor keyboardInputProcessor;
        private DVViewModelFunctions dVViewModelFunctions;
        private readonly IAppEnvironment appEnvironment;
        public PrivacyPolicyViewModel(KeyboardInputProcessor keyboardInputProcessor,
                                      DVViewModelFunctions dVViewModelFunctions,
                                      IAppEnvironment appEnvironment)
        {
            this.keyboardInputProcessor = keyboardInputProcessor;
            this.dVViewModelFunctions = dVViewModelFunctions;
            this.appEnvironment = appEnvironment;
            pageKeyCodes = new List<string>()
            {
                "Escape"
            };
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
                if (pageKeyCodes.Contains(bean.code) || (bean.code == "!" && appEnvironment.IsVirtualDevice))
                {
                    dVViewModelFunctions.HandleKeyCode(bean.code);
                }
                
                
                
                
            }
            return true;
        }
    }
}
