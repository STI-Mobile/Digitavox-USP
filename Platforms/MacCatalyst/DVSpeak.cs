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

using AVFoundation;
using Foundation;

namespace Digitavox.PlatformsImplementations {

  public partial class DVSpeak : AVSpeechSynthesizerDelegate {

    private AVSpeechSynthesizer synthesizer;
    private float iosSpeechRate;
    private Dictionary<AVSpeechUtterance, Action> callbacks = new();

    public partial void Cancel() {
      Console.WriteLine("Cancel Speech");
      synthesizer.StopSpeaking(AVSpeechBoundary.Immediate);
      float savedRate = NSUserDefaults.StandardUserDefaults.FloatForKey("speechRateKey");
      iosSpeechRate = savedRate != 0 ? savedRate : 0.5f;
    }

    public partial void Init(object obj) {
      Console.WriteLine("Init Speech");
      synthesizer = new AVSpeechSynthesizer();
      synthesizer.Delegate = this;
      synthesizer.UsesApplicationAudioSession = true;
    }

    public partial void InitCompleted() {
      Console.WriteLine("Init Completed");
    }

    public partial void SpeakText(string text, Action onFinished) {
      var utterance = new AVSpeechUtterance(text) {
        Voice = AVSpeechSynthesisVoice.FromLanguage("pt-BR"),
        Rate = iosSpeechRate 
      };
      synthesizer.SpeakUtterance(utterance);
      callbacks[utterance] = onFinished;
    }

    public partial void SetSpeechRate(int speechRate) {
      Console.WriteLine("Set Speech Rate");
      iosSpeechRate = ((float)speechRate - 1) * 0.0625f + 0.5f; 
      NSUserDefaults.StandardUserDefaults.SetFloat(iosSpeechRate, "speechRateKey");
    }

    public partial void Completed(object id) {
      Console.WriteLine("Completed Speech");
    }

    public override void DidFinishSpeechUtterance(AVFoundation.AVSpeechSynthesizer synthesizer, AVFoundation.AVSpeechUtterance utterance) {
      Console.WriteLine("Did Finish Speech Utterance");
      try {
        callbacks[utterance].Invoke();
        callbacks.Remove(utterance);
      }
      catch { }
    }

  } 
} 