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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Digitavox.PlatformsImplementations
{
    public partial class DVSpeak
    {
        private static readonly Lazy<DVSpeak> instance = new Lazy<DVSpeak>(() => new DVSpeak());
        public partial void Cancel();
        public partial void SpeakText(string text, Action onFinished);
        public partial void Init(object obj);
        public partial void InitCompleted();
        public partial void SetSpeechRate(int speechRate);
        public partial void Completed(object id);
        public static DVSpeak GetInstance()
        {
            return instance.Value;
        }
        private DVSpeak()
        {
        }
    }
}
