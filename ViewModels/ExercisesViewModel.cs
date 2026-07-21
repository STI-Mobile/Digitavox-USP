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

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Digitavox.Helpers;
using Digitavox.Models;
using Plugin.Maui.Audio;
using System;

namespace Digitavox.ViewModels
{
    public partial class ExercisesViewModel : ObservableObject, IOnPageKeyPress
    {
        List<string> pressedKeys = new List<string>();
        int escPressed = 0;
        bool updateLine;
        string exerciseSpeak;
        string wordInput;
        string exercise;
        bool ignoreLetterCase;
        bool endLesson;
        int exerciseNumber;
        int consecutiveErrors;
        List<string> exercisesList;
        
        string color;
        string lastInput = " ";
        string spellingActive;
        string exerciseDisplay;
        string inputDisplay;
        int repetitionsTotal;
        bool startExercise;
        int speakFromHelp;
        List<string> pageKeyCodes;
        [ObservableProperty]
        private FormattedString pageFormattedLabel;
        [ObservableProperty]
        private string _pageLabel;
        [ObservableProperty]
        private double _textSize;
        private Course course;
        private CourseLesson courseLesson;
        private DVViewModelSpeak dVViewModelSpeak;
        private DVViewModelFunctions dVViewModelFunctions;
        private FingerMapping fingerMapping;
        private UserProgress userProgress;
        public ExercisesViewModel(Course course,
                                  CourseLesson courseLesson, 
                                  DVViewModelSpeak dVViewModelSpeak,
                                  DVViewModelFunctions dVViewModelFunctions,
                                  FingerMapping fingerMapping,
                                  UserProgress userProgress)
        {
            this.course = course;
            this.courseLesson = courseLesson;
            this.dVViewModelSpeak = dVViewModelSpeak;
            this.dVViewModelFunctions = dVViewModelFunctions;
            this.fingerMapping = fingerMapping;
            this.userProgress = userProgress;
            pageKeyCodes = new List<string>()
            {
                "F1", "Up", "Escape", "Left", "Right", "Down",
                "Ctrl+Left", "Ctrl+Right", "Ctrl+Up", "Ctrl+Down",
                "F2", "F3", "F4", "F5", "F6",
                "F7", "F8", "F9",
            };
            repetitionsTotal = 0;
            consecutiveErrors = 0;
        }
        public void OnPage()
        {
            escPressed = 0;
            dVViewModelFunctions.SetCurrentPageIdentifier("na tela de exercícios");
            dVViewModelSpeak.CurrentIsExercisePage(true);
            updateLine = true;
            endLesson = false;
            startExercise = true;
            int exerciseLine = 3;
            dVViewModelFunctions.SetFirstOptionLineNumber(exerciseLine);
            dVViewModelFunctions.SetLastOptionLineNumber(exerciseLine);
            dVViewModelFunctions.SetOptionNumberStart(exerciseLine);
            dVViewModelFunctions.SetNextPageRoute("ExercisesHelp");
            speakFromHelp = dVViewModelFunctions.GetUpdateSpeakFromHelp();
            Thread.Sleep(100);
            if (speakFromHelp == -1)
            {
                if (courseLesson.ExerciseRunnig())
                {
                    UpdateTextSpeakLists();
                    courseLesson.ContinueTimer();
                }
                else
                {
                    BeginExercise();
                    UpdateTextSpeakLists();
                    dVViewModelSpeak.SpeakAll(() =>
                    {
                        if (updateLine && escPressed == 0)
                        {
                            dVViewModelSpeak.ChangeLine(exercise, exerciseSpeak, 3);
                            dVViewModelSpeak.SpeakOneLine(3, () =>
                            {
                                courseLesson.ContinueTimer();
                                Thread.Sleep(200);
                                if (updateLine) dVViewModelSpeak.ChangeLine(exerciseDisplay, exerciseSpeak, 3);
                                startExercise = true;
                            });
                        }
                    });
                }
            }
            else if (speakFromHelp != 2)
            {
                dVViewModelSpeak.Set("maintainTag", false);
                courseLesson.PauseTimer();
                UpdateTextSpeakLists();
                dVViewModelSpeak.SpeakOneLine(speakFromHelp, () => 
                {
                    courseLesson.ContinueTimer();
                });
            }
            else
            {
                dVViewModelSpeak.Set("maintainTag", false);
                courseLesson.PauseTimer();
                UpdateTextSpeakLists();
                dVViewModelSpeak.SpeakAll(() =>
                {
                    courseLesson.ContinueTimer();
                });
            }
            WeakReferenceMessenger.Default.Send(new DVMessage("BecomeFirstResponder"));

        }
        public void BeginExercise()
        {
            startExercise = false;
            dVViewModelFunctions.CreatePlayers();
            wordInput = "";
            exercisesList = course.GetExercises();
            exerciseNumber = 0;
            exercise = exercisesList[0];
            exerciseDisplay = exercise;
            inputDisplay = string.Empty;
            spellingActive = course.LessonProperty("SOLETRAEXER");
            if (string.Equals(spellingActive, "sim", StringComparison.OrdinalIgnoreCase)) exerciseSpeak = dVViewModelFunctions.SpellWord(exercise);
            else exerciseSpeak = dVViewModelFunctions.SpeakPunctuation(exercise);
            ignoreLetterCase = string.Equals(course.LessonProperty("TUDO_EM_MAIUSCULO"), "sim", StringComparison.OrdinalIgnoreCase);
            repetitionsTotal = int.Parse(course.LessonProperty("REPETICOESEXER"));
            courseLesson.SetLessonChars(exercisesList, repetitionsTotal, int.Parse(course.LessonProperty("TEMPOPORCARACTER")),
                int.Parse(course.LessonProperty("MEDIAEXER")), DVPersistence.Get<int>("timeDivider"), string.Equals(spellingActive, "sim", StringComparison.OrdinalIgnoreCase), ignoreLetterCase);
            courseLesson.StartTimer(() =>
            {
                dVViewModelSpeak.Skip();
                dVViewModelSpeak.Speak("Tempo expirado", () =>
                {
                    userProgress.SaveStatistics();
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        dVViewModelFunctions.SetNextPageRoute("ExercisesStatistics");
                        dVViewModelFunctions.GoToNextPage();
                    });
                });
            });
        }
        private void UpdateTextSpeakLists()
        {
            List<string> exercisesTexts = new List<string>()
            {
                course.LessonApresentation(),
                course.LessonInstruction()
            };
            List<string> textList = new List<string>()
            {
                exercisesTexts[0],
                exercisesTexts[1],
                $"Repetição: {courseLesson.CurrentRepetition()} / {repetitionsTotal}",
                exercise,
                wordInput
            };
            List<string> speakList = new List<string>()
            {
                exercisesTexts[0],
                exercisesTexts[1],
                string.Empty,
                string.Empty,
                string.Empty
            };
            if (speakFromHelp == 2)
            {
                speakList = new List<string>()
                {
                    string.Empty, string.Empty, string.Empty, string.Empty, string.Empty
                };
                List<string> statisticsList = new List<string>()
                {
                    $"Tempo decorrido: {courseLesson.PracticeTime()}",
                    $"Tempo disponível: {courseLesson.TotalTime()}",
                    $"Percentual de tempo gasto: {(float)(100 * courseLesson.GetStatistics().practiceTime / courseLesson.GetStatistics().totalTime)} %",
                };
                foreach (string line in statisticsList)
                {
                    textList.Add(line);
                    speakList.Add(line);
                }
                speakFromHelp = -1;
            }

            textList = textList.Select(text => dVViewModelFunctions.EditStringForVoiceOver(text)).ToList();
            speakList = speakList.Select(speech => dVViewModelFunctions.EditStringForVoiceOver(speech)).ToList();

            dVViewModelSpeak.SetTextAndSpeech(textList, speakList)
                            .RegisterUpdateScreen((text) =>
                            {
                                
                                PageFormattedLabel = text;
                                TextSize = DVPersistence.Get<double>("fontSize");
                            });
        }
        private void ControlExercises()
        {
            bool updateRepetition = false;
            if (exerciseNumber < exercisesList.Count - 1)
            {
                exerciseNumber++;
                exerciseSpeak = dVViewModelFunctions.SpeakPunctuation(exercisesList[exerciseNumber]);
            }
            else
            {
                exerciseNumber = 0;
                exerciseSpeak = (exercisesList.Count > 1) ? dVViewModelFunctions.SpeakPunctuation(exercisesList[exerciseNumber]) : string.Empty;
                if (courseLesson.CurrentRepetition() == repetitionsTotal)
                {
                    EndExercise();
                    return;
                }
                else
                {
                    courseLesson.IncrementRepetition();
                    updateRepetition = true;
                }
            }
            exercise = exercisesList[exerciseNumber];
            DisplayExercise(0);
            if (string.Equals(spellingActive, "sim", StringComparison.OrdinalIgnoreCase))
            {
                if (exerciseSpeak.Length > 0)
                {
                    exerciseSpeak = dVViewModelFunctions.SpellWord(exerciseSpeak);
                }
            }
            string repetitionsCount = $"Repetição: {courseLesson.CurrentRepetition()} / {repetitionsTotal}";
            string repetitionsSpeak = (DVPersistence.Get<bool>("countRepetitions") && updateRepetition) ? $"Repetição {courseLesson.CurrentRepetition()} de {repetitionsTotal}" : string.Empty;
            dVViewModelSpeak.ChangeLine(repetitionsCount, repetitionsSpeak, 2);
            dVViewModelSpeak.ChangeLine(exercise, exerciseSpeak, 3);
            SpeakNextIteration();
        }
        private void SpeakNextIteration()
        {
            wordInput = string.Empty;
            courseLesson.PauseTimer();
            dVViewModelSpeak.SpeakRecursive(2, () =>
            {
                
                courseLesson.ContinueTimer();
            });
        }
        private void DisplayExercise(int index)
        {
            if (index == 0)
            {
                exerciseDisplay = exercise;
                inputDisplay = string.Empty;
            }
            dVViewModelSpeak.AttributeStyle("bold", 3, index);
            if (index > 0)
            {
                if (!ignoreLetterCase && (wordInput[index - 1] == exercise[index - 1]))
                {
                    color = "green";
                    consecutiveErrors = 0;
                }
                else if (ignoreLetterCase && (wordInput.ToLower()[index - 1] == exercise.ToLower()[index - 1]))
                {
                    color = "green";
                    consecutiveErrors = 0;
                }
                else
                {
                    color = "red";
                    CountConsecutiveErrors();
                    dVViewModelFunctions.PlayBuzzSound();
                }
                dVViewModelSpeak.AttributeStyle(color, 4, index - 1);
                inputDisplay += wordInput[index - 1];
            }
            
            string centerInput = inputDisplay;
            
            dVViewModelSpeak.ChangeLine(centerInput, string.Empty, 4);
        }
        private void CountConsecutiveErrors()
        {
            consecutiveErrors += 1;
            if (consecutiveErrors >= 4)
            {
                dVViewModelSpeak.Speak("Excesso de erros. Tecle seta para direita para falar o restante do exercício ou F1 para ajuda.", () => { });
            }
        }
        private void WriteWord()
        {
            
            if (DVPersistence.Get<bool>("speakInput"))
            {
                
                dVViewModelSpeak.Speak(lastInput, () => {
                    
                });
            }
            DisplayExercise(wordInput.Length);
        }
        private void EndExercise()
        {
            endLesson = true;
            courseLesson.StopTimer();
            userProgress.SaveStatistics();
            dVViewModelFunctions.SetNextPageRoute("ExercisesStatistics");
            dVViewModelSpeak.Speak("Fim da lição", () =>
            {
                Thread.Sleep(100);
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    endLesson = false;
                    dVViewModelFunctions.GoToNextPage();
                });
                
            });
        }
        private void CountEsc()
        {
            escPressed += 1; 
            dVViewModelSpeak.Skip();
            if (escPressed == 2)
            {
                updateLine = false;
                if (courseLesson.ExerciseRunnig()) courseLesson.StopTimer();
                userProgress.SaveStatistics();
                dVViewModelFunctions.HandleKeyCode("Escape");
            }
            else
            {
                dVViewModelSpeak.Speak("Aperte esqueipe novamente para sair.", () => { });
            }
        }
        public bool OnPageKeyDown(int keyCode)
        {
            string code = fingerMapping.mapKeyCode(keyCode);
            if (!pressedKeys.Contains(code))
            {
                pressedKeys.Add(code);
            }
            return true;
        }
        public bool OnPageKeyPress(int keyCode, int modifiers)
        {
            pressedKeys.Remove(fingerMapping.mapKeyCode(keyCode));
            var bean = fingerMapping.MapKey(keyCode, modifiers, pressedKeys);
            if (bean.code != null && !endLesson)
            {
                dVViewModelFunctions.ExerciseHelpOptions();
                if (bean.code == "Escape" || (bean.code == "!" && DVDevice.IsVirtual()))
                {
                    CountEsc();
                }
                else
                {
                    escPressed = 0;
                    if (!DVKeyboard.IsModifierKey(bean.code)) dVViewModelSpeak.Skip();
                    UpdateTextSpeakLists();
                    if (startExercise)
                    {
                        updateLine = false;
                        if (pageKeyCodes.Contains(bean.code) || ((bean.code == "!" || bean.code == "@") && DVDevice.IsVirtual()))
                        {
                            dVViewModelFunctions.HandleKeyCode(bean.code);
                            if (dVViewModelFunctions.GetSpeakFromHelp() != -1) OnPage();
                        }
                        else if (bean.code.Length == 1)
                        {
                            if (wordInput.Length == exercise.Length)
                            {
                                courseLesson.CharPressed(bean.code);
                                if (bean.code != " ")
                                {
                                    CountConsecutiveErrors();
                                    dVViewModelFunctions.PlayBuzzSound();
                                    
                                }
                                else
                                {
                                    consecutiveErrors = 0;
                                }
                                ControlExercises();
                                
                            }
                            else
                            {
                                wordInput = wordInput + $"{bean.code}";
                                lastInput = bean.speakOnlyChar;
                                WriteWord();
                                courseLesson.CharPressed(bean.code);
                            }
                        }
                    }
                    else
                    {
                        startExercise = true;
                    }
                }
            }
            return true;
        }
    }
}