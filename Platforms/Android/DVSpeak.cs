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

using Android.Runtime;
using Android.Speech.Tts;
using Digitavox.Helpers;
using Java.Interop;
using Java.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextToSpeech = Android.Speech.Tts.TextToSpeech;

namespace Digitavox.PlatformsImplementations
{
    public partial class DVSpeak
    {
        
        private TextToSpeech textToSpeech;
        private MainActivity mainActivity;
        private bool initialized;
        private string initialText;
        private Action initialOnFinished;
        private Dictionary<string, Action> callbacks = new Dictionary<string, Action>();
        public partial void Cancel()
        {
            
            
            if (initialized) textToSpeech.PlaySilentUtterance(1, QueueMode.Flush, null);
        }
        public partial void Init(object obj)
        {
            
            mainActivity = (MainActivity)obj;
            textToSpeech = mainActivity.createTextToSpeech();
        }
        public partial void InitCompleted()
        {
            initialized = true;
            if (initialText != null)
            {
                string initialTextCopy = initialText;
                Action initialOnFinishedCopy = initialOnFinished;
                initialText = null;
                initialOnFinished = null;
                SpeakText(initialTextCopy, initialOnFinishedCopy);
            }
        }
        
        public partial void SpeakText(string text, Action onFinished)
        {
            
            if (initialized)
            {
                String utteranceId = UUID.RandomUUID().ToString();
                textToSpeech.Speak(text, QueueMode.Add, null, utteranceId);
                callbacks[utteranceId] = onFinished;
            } 
            else
            {
                initialText = text;
                initialOnFinished = onFinished;
            }
        }
        public partial void Completed(object id)
        {
            if (callbacks.ContainsKey((string)id))
            {
                callbacks[(string)id].Invoke();
                callbacks.Remove((string)id);
            }
        }
        public partial void SetSpeechRate(int speechRate)
        {
            if (textToSpeech != null)
            {
                textToSpeech.SetSpeechRate(speechRate * 0.8f + 0.2f);
            }
        }
    }
}
