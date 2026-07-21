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

using System.Speech.Synthesis;
using Digitavox.Helpers;

namespace Digitavox.PlatformsImplementations
{
    public partial class DVSpeak
    {
        private SpeechSynthesizer speechSynthesizer;
        private Dictionary<string, Action> callbacks = new Dictionary<string, Action>();
        public partial void Cancel()
        {
            speechSynthesizer.SpeakAsyncCancelAll();
        }
        public partial void Init(object obj)
        {
            speechSynthesizer = new SpeechSynthesizer();
            speechSynthesizer.SetOutputToDefaultAudioDevice();
            SetSpeechRate(DVPersistence.Get<int>("speakRate"));
        }
        public partial void InitCompleted()
        {
        }
        public partial void SpeakText(string text, Action onFinished)
        {
            string utteranceId = Guid.NewGuid().ToString();
            callbacks[utteranceId] = onFinished;
            SynthSpeakUtterance(text, utteranceId);
        }
        private void SynthSpeakUtterance(string text, string utteranceId)
        {
            var prompt = speechSynthesizer.SpeakAsync(text);
            speechSynthesizer.SpeakCompleted += (sender, args) =>
            {
                if (prompt.IsCompleted && callbacks.TryGetValue(utteranceId, out Action? value))
                {
                    value.Invoke();
                    callbacks.Remove(utteranceId);
                }
                speechSynthesizer.SpeakCompleted -= (s, a) => { };
            };
        }
        public partial void SetSpeechRate(int speechRate)
        {
            speechSynthesizer.Rate = speechRate;
        }
        public partial void Completed(object id)
        {
        }
    }
}
