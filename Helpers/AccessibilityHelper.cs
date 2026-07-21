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

namespace Digitavox.Helpers
{
    public static class AccessibilityHelper
    {
        public static bool IsHighContrastEnabled()
        {
#if WINDOWS
            var accessibilitySettings = new Windows.UI.ViewManagement.UISettings();
            var backgroundColor = accessibilitySettings.GetColorValue(Windows.UI.ViewManagement.UIColorType.Background);
            var isHighContrast = backgroundColor.ToString().Equals("#FF000000");
            return isHighContrast;
#else
            return false;
#endif
        }
    }
}
