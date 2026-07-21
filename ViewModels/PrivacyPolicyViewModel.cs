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

namespace Digitavox.ViewModels
{
    public partial class PrivacyPolicyViewModel : IOnPageKeyPress
    {
        List<string> pressedKeys = new List<string>();
        List<string> pageKeyCodes;
        private FingerMapping fingerMapping;
        private DVViewModelFunctions dVViewModelFunctions;
        public PrivacyPolicyViewModel(FingerMapping fingerMapping,
                                      DVViewModelFunctions dVViewModelFunctions)
        {
            this.fingerMapping = fingerMapping;
            this.dVViewModelFunctions = dVViewModelFunctions;
            pageKeyCodes = new List<string>()
            {
                "Escape"
            };
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
                if (pageKeyCodes.Contains(bean.code) || (bean.code == "!" && DVDevice.IsVirtual()))
                {
                    dVViewModelFunctions.HandleKeyCode(bean.code);
                }
                
                
                
                
            }
            return true;
        }
    }
}
