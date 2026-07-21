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

using CommunityToolkit.Mvvm.Messaging;
using Digitavox.Helpers;

namespace Digitavox.Models
{
    public class CourseLesson
    {
        private DateTime startTime;
        private DateTime timeCheckpoint;
        private DateTime endTime;
        private bool running;
        private bool paused;
        private int previousMilliseconds;
        private List<string> lessonChars;
        private int nextIndexToType;
        private int currentPhrase;
        private int targetPercent;
        private int currentRepetition;
        private bool currentWordError;
        private bool ignoreLetterCase;
        private CourseLessonBean bean;
        private IDispatcherTimer timer;
        private Action timeIsOver;
        public CourseLesson()
        {
            WeakReferenceMessenger.Default.Register<DVMessage>(this, (r, m) => {
                if (m.Value == "WindowCreated" || m.Value == "WindowActivated" || m.Value == "WindowResumed")
                {
                    ContinueTimer();
                }
                else if (m.Value == "WindowDeactivated" || m.Value == "WindowStopped" || m.Value == "WindowDestroying")
                {
                    PauseTimer();
                }
            });
        }
        public void SetLessonChars(List<string> lessonChars, int exerciseRepetitions, int secondsPerChar, int targetPercent, int timeDivider, bool spellingActive, bool ignoreLetterCase)
        {
            this.lessonChars = lessonChars;
            this.targetPercent = targetPercent;
            this.ignoreLetterCase = ignoreLetterCase;
            bean.totalWords = 0;
            if (!spellingActive)
            {
                foreach (string lessonChar in lessonChars)
                {
                    bean.totalWords += lessonChar.Split(' ').Length;
                }
            }
            bean.exerciseRepetitions = exerciseRepetitions;
            if (bean.exerciseRepetitions > 1)
            {
                bean.totalWords *= bean.exerciseRepetitions;
                
                
                
                
                
                
            }
            List<string> newLessonChars = new List<string>();
            foreach (string lessonChar in lessonChars)
            {
                newLessonChars.Add(lessonChar + " ");
            }
            this.lessonChars = newLessonChars;
            
            bean.totalTime = secondsPerChar * TotalChars() / timeDivider;
            this.previousMilliseconds = 0;
        }
        public void StartTimer(Action timeIsOver)
        {
            running = true;
            paused = true;
            startTime = DateTime.Now;
            timeCheckpoint = startTime;
            bean.correctChars = 0;
            bean.incorrectChars = 0;
            bean.correctWords = 0;
            bean.incorrectWords = 0;
            currentWordError = false;
            nextIndexToType = 0;
            currentPhrase = 0;
            currentRepetition = 1;
            bean.errorsByChar = new Dictionary<string, int>();
            this.timeIsOver = timeIsOver;
            timer = Application.Current.Dispatcher.CreateTimer();
            timer.Tick += (sender, e) =>
            {
                StopTimer();
                timeIsOver.Invoke();
            };
        }
        public void CharPressed(string c)
        {
            ContinueTimer();
            string correct = lessonChars[currentPhrase].ToCharArray()[nextIndexToType].ToString();
            if (correct.Equals(" ") && bean.totalWords > 0)
            {
                if (!currentWordError)
                {
                    bean.correctWords++;
                }
                else
                {
                    bean.incorrectWords++;
                    currentWordError = false;
                }
            }
            if (!ignoreLetterCase && correct.Equals(c))
            {
                bean.correctChars += 1;
            }
            else if (ignoreLetterCase && correct.ToLower().Equals(c.ToLower()))
            {
                bean.correctChars += 1;
            }
            else
            {
                bean.incorrectChars += 1;
                currentWordError = true;
                if (!bean.errorsByChar.ContainsKey(correct))
                {
                    bean.errorsByChar.Add(correct, 0);
                }
                bean.errorsByChar[correct] += 1;
            }
            nextIndexToType++;
            if (nextIndexToType >= lessonChars[currentPhrase].Length)
            {
                nextIndexToType = 0;
                if (lessonChars.Count() > 1)
                {
                    currentPhrase = (currentPhrase + 1) % lessonChars.Count();
                }
            }
        }
        public void StopTimer()
        {
            running = false;
            endTime = DateTime.Now;
            this.previousMilliseconds += (int)(endTime - timeCheckpoint).TotalMilliseconds;
            bean.practiceTime = this.previousMilliseconds / 1000;
            if (timer != null) timer.Stop();
            bean.concluded = TotalCorrectPercent() >= targetPercent;
        }
        public void PauseTimer()
        {
            if (running && !paused)
            {
                this.previousMilliseconds += (int)(DateTime.Now - timeCheckpoint).TotalMilliseconds;
                bean.practiceTime = this.previousMilliseconds / 1000;
                paused = true;
                if (timer != null) timer.Stop();
            }
        }
        public void ContinueTimer()
        {
            if (running && paused)
            {
                timeCheckpoint = DateTime.Now;
                paused = false;
                
                timer.Interval = TimeSpan.FromMilliseconds(bean.totalTime * 1000 - previousMilliseconds);
                
                timer.Start();
            }
        }
        public CourseLessonBean GetStatistics()
        {
            return bean;
        }
        public int TotalChars()
        {
            return lessonChars.Select(x => x.Length).Aggregate(0, (a, b) => a + b) * bean.exerciseRepetitions;
        }
        
        
        
        
        public int CorrectPercent()
        {
            if (bean.correctChars + bean.incorrectChars > 0)
            {
                return bean.correctChars * 100 / (bean.correctChars + bean.incorrectChars);
            } 
            else
            {
                return 0;
            }
        }
        public int TotalCorrectPercent()
        {
            if (TotalChars() > 0)
            {
                return bean.correctChars * 100 / TotalChars();
            }
            else
            {
                return 0;
            }
        }
        public int TotalCorrectWordPercent()
        {
            if (bean.correctWords + bean.incorrectWords > 0)
            {
                return bean.correctWords * 100 / (bean.correctWords + bean.incorrectWords);
            }
            else
            {
                return 0;
            }
        }
        
        
        
        
        public string CurrentExercise()
        {
            return lessonChars[currentPhrase];
        }
        public string CurrentCharacter()
        {
            return lessonChars[currentPhrase].ToCharArray()[nextIndexToType].ToString();
        }
        public int NextCharacterIndex()
        {
            return nextIndexToType;
        }
        public int CurrentRepetition()
        {
            return (running || currentRepetition == bean.exerciseRepetitions) ? currentRepetition : 1;
        }
        public void IncrementRepetition()        {
            currentRepetition++;
        }
        public string GetStartTime()
        { 
            return startTime.ToString(); 
        }
        public string GetEndTime()
        {
            return endTime.ToString();
        }
        public bool ExerciseRunnig()
        {
            return running;
        }
        public bool ExercisePaused()
        {
            return paused;
        }
        public string PracticeTime()
        {
            return TimeDisplay(bean.practiceTime);
        }
        public string TotalTime()
        {
            return TimeDisplay(bean.totalTime);
        }
        public string TimeDisplay(int timeInSeconds)
        {
            string hourString = (timeInSeconds / 3600 == 0) ? string.Empty : $"{timeInSeconds / 3600} horas ";
            string minuteString = ((timeInSeconds % 3600) / 60 == 0) ? string.Empty : $"{(timeInSeconds % 3600) / 60} minutos ";
            string secondString = ((timeInSeconds % 60) == 0) ? string.Empty : $"{timeInSeconds % 60} segundos";
            if (timeInSeconds >= 3600 && timeInSeconds < 7200)
            {
                hourString = "1 hora ";
            }
            else if ((timeInSeconds % 3600) >= 60 && (timeInSeconds % 3600) < 120)
            {
                minuteString = "1 minuto ";
            }
            else if (timeInSeconds % 60 == 1)
            {
                secondString = "1 segundo";
            }
            string time = hourString + minuteString + secondString;
            return (time.Length > 0) ? time : "0";
        }
    }
}
