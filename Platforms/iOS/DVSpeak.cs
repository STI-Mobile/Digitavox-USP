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
using Microsoft.Maui.ApplicationModel;

namespace Digitavox.PlatformsImplementations;

public partial class DVSpeak : AVSpeechSynthesizerDelegate
{
    private readonly Lazy<AVSpeechSynthesizer> lazySynthesizer = new(() => new AVSpeechSynthesizer());
    private AVSpeechSynthesizer Synthesizer => lazySynthesizer.Value;
    private readonly Dictionary<AVSpeechUtterance, Action> callbacks = new();
    private int speechGeneration;
    private float iosSpeechRate = AVSpeechUtterance.DefaultSpeechRate;

    public partial void Init(object obj)
    {
        Synthesizer.Delegate = this;
        // Let iOS route speech and handle interruptions independently of feedback sounds.
        Synthesizer.UsesApplicationAudioSession = false;
        RestoreSpeechRate();
    }

    public partial void InitCompleted() { }
    public partial void Completed(object id) { }

    public partial void Cancel()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            // Cancelled utterances must not navigate or restart an earlier page's speech.
            speechGeneration++;
            callbacks.Clear();
            Synthesizer.StopSpeaking(AVSpeechBoundary.Immediate);
            RestoreSpeechRate();
        });
    }

    private void RestoreSpeechRate()
    {
        float savedRate = NSUserDefaults.StandardUserDefaults.FloatForKey("speechRateKey");
        iosSpeechRate = savedRate != 0 ? savedRate : AVSpeechUtterance.DefaultSpeechRate;
    }

    public partial void SpeakText(string text, Action onFinished)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                onFinished();
                return;
            }

            var utterance = new AVSpeechUtterance(text)
            {
                Voice = AVSpeechSynthesisVoice.FromLanguage("pt-BR"),
                Rate = iosSpeechRate
            };
            // Register before speaking, including when iOS completes an utterance immediately.
            callbacks[utterance] = onFinished;
            Synthesizer.SpeakUtterance(utterance);
        });
    }

    public partial void SetSpeechRate(int speechRate)
    {
        iosSpeechRate = ((float)speechRate - 1) * 0.0625f + 0.5f;
        NSUserDefaults.StandardUserDefaults.SetFloat(iosSpeechRate, "speechRateKey");
    }

    public void EnqueueSpeech(string text) => SpeakText(text, () => { });

    public override void DidFinishSpeechUtterance(AVSpeechSynthesizer synthesizer, AVSpeechUtterance utterance)
    {
        if (callbacks.Remove(utterance, out var callback))
        {
            // Finish the native delegate call before a continuation stops or starts speech.
            int generation = speechGeneration;
            BeginInvokeOnMainThread(() =>
            {
                if (generation == speechGeneration) callback();
            });
        }
    }

    public override void DidCancelSpeechUtterance(AVSpeechSynthesizer synthesizer, AVSpeechUtterance utterance)
    {
        callbacks.Remove(utterance);
    }
}
