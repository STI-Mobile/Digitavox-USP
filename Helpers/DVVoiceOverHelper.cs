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

using System;
using Microsoft.Maui;
#if __IOS__
using UIKit;
#endif

namespace Digitavox.Helpers
{
    public static class DVVoiceOverHelper
    {
        public static bool IsVoiceOverEnabled()
        {
            #if __IOS__
                return UIKit.UIAccessibility.IsVoiceOverRunning;
            #else
                return false;
            #endif
        }

        public static bool HasNotch()
        {
            #if __IOS__
                var keyWindow = UIApplication.SharedApplication.Windows[0];
                var bottomSafeArea = keyWindow.SafeAreaInsets.Bottom;
                return bottomSafeArea > 0;
            #else
                return false;
            #endif
        }
    }

}

