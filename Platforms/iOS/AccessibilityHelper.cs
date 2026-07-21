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
using AVFoundation;
using UIKit;
using Digitavox.PlatformsImplementations;
using Digitavox.Helpers;
using Digitavox.ViewModels;

namespace Digitavox.Platforms.iOS
{
    public static class AccessibilityHelper
    {
        private static void Speak(string message)
        {
            var speechSynthesizer = DVSpeak.GetInstance();
            speechSynthesizer.EnqueueSpeech(message);
        }

        public static void PersistVoiceOverStatusAndNotify()
        {
            bool isVoiceOverRunning = UIAccessibility.IsVoiceOverRunning;
            DVPersistence.Set("VoiceOverStatus", isVoiceOverRunning);

            if (isVoiceOverRunning)
            {
                System.Diagnostics.Debug.WriteLine("VoiceOver ON");
                Speak("Para navegar no app com o VoiceOver, pressione simultaneamente as teclas de seta para a direita e esquerda");
            } else {
                System.Diagnostics.Debug.WriteLine("VoiceOver OFF");
            }
        }

        public static void CheckVoiceOverStatusChangeAndNotify()
        {
            bool initialVoiceOverStatus = DVPersistence.Get<bool>("VoiceOverStatus");
            bool currentVoiceOverStatus = UIAccessibility.IsVoiceOverRunning;

            if (initialVoiceOverStatus && currentVoiceOverStatus)
            {
                System.Diagnostics.Debug.WriteLine("VoiceOver estava ON");
                Speak("Para voltar as teclas de setas para o controle do VoiceOver, pressione as setas para a direita e esquerda simultaneamente.");
            }
        }

    }
}
