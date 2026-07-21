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

    private static Lazy<AVSpeechSynthesizer> lazySynthesizer = new Lazy<AVSpeechSynthesizer>(() => new AVSpeechSynthesizer());
    private AVSpeechSynthesizer synthesizer => lazySynthesizer.Value;
    
    private float iosSpeechRate;
    private Dictionary<string, Action> callbacks = new Dictionary<string, Action>();
    private Dictionary<AVSpeechUtterance, string> utteranceToIdMap = new Dictionary<AVSpeechUtterance, string>();

    public partial void Cancel() {
      
      synthesizer.StopSpeaking(AVSpeechBoundary.Immediate);
      float savedRate = NSUserDefaults.StandardUserDefaults.FloatForKey("speechRateKey");
      iosSpeechRate = savedRate != 0 ? savedRate : 0.5f;
    }

    public partial void Init(object obj) {
      
      synthesizer.Delegate = this;
      synthesizer.UsesApplicationAudioSession = true;
    }

    public partial void InitCompleted() {
      
    }

    public partial void SpeakText(string text, Action onFinished) {
        var utteranceId = Guid.NewGuid().ToString();
        var utterance = new AVSpeechUtterance(text) {
            Voice = AVSpeechSynthesisVoice.FromLanguage("pt-BR"),
            Rate = iosSpeechRate
        };
        synthesizer.SpeakUtterance(utterance);
        callbacks[utteranceId] = onFinished;
        utteranceToIdMap[utterance] = utteranceId;
    }

    public partial void SetSpeechRate(int speechRate) {
      
      iosSpeechRate = ((float)speechRate - 1) * 0.0625f + 0.5f; 
      NSUserDefaults.StandardUserDefaults.SetFloat(iosSpeechRate, "speechRateKey");
    }

    public partial void Completed(object id) {
      
    }


    private Queue<string> speechQueue = new Queue<string>();

    public void EnqueueSpeech(string text)
    {
        speechQueue.Enqueue(text);
        if (!synthesizer.Speaking)
        {
            SpeakNextInQueue();
        }
    }

    private void SpeakNextInQueue()
    {
        if (speechQueue.Count > 0)
        {
            string nextSpeech = speechQueue.Dequeue();
            SpeakText(nextSpeech, () => {
                
            });
        }
    }

    public override void DidFinishSpeechUtterance(AVFoundation.AVSpeechSynthesizer synthesizer, AVFoundation.AVSpeechUtterance utterance)
    {
        
        try
        {
            var utteranceId = utteranceToIdMap[utterance];
            if (callbacks.TryGetValue(utteranceId, out var callback))
            {
                callback.Invoke();
                callbacks.Remove(utteranceId);
                utteranceToIdMap.Remove(utterance);
            }
            SpeakNextInQueue(); 
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in DidFinishSpeechUtterance: {ex.Message}");
        }
    }


    } 
} 