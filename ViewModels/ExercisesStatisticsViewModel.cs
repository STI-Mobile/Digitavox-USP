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
using Digitavox.Helpers;
using Digitavox.Models;

namespace Digitavox.ViewModels
{
    public partial class ExercisesStatisticsViewModel : ObservableObject, IOnPageKeyPress
    {
        List<string> pressedKeys = new List<string>();
        private int instructionLines = 1;
        private int lessonNumber;
        List<string> pageKeyCodes;
        [ObservableProperty]
        private FormattedString pageFormattedLabel;
        [ObservableProperty]
        private string _pageLabel;
        [ObservableProperty]
        private double _textSize;
        private Course course;
        private DVViewModelSpeak dVViewModelSpeak;
        private DVViewModelFunctions dVViewModelFunctions;
        private FingerMapping fingerMapping;
        private UserProgress userProgress;
        private CourseLesson courseLesson;
        public ExercisesStatisticsViewModel(Course course,
                                            DVViewModelSpeak dVViewModelSpeak,
                                            DVViewModelFunctions dVViewModelFunctions,
                                            FingerMapping fingerMapping,
                                            UserProgress userProgress,
                                            CourseLesson courseLesson)
        {
            this.course = course;
            this.dVViewModelSpeak = dVViewModelSpeak;
            this.dVViewModelFunctions = dVViewModelFunctions;
            this.fingerMapping = fingerMapping;
            this.userProgress = userProgress;
            this.courseLesson = courseLesson;
            pageKeyCodes = new List<string>()
            {
                "Up", "Down", "Tab", "ShiftTab"
            };
        }
        public void OnPage()
        {
            dVViewModelFunctions.SetCurrentPageIdentifier("na tela de estatísticas de exercícios");
            Thread.Sleep(100);
            dVViewModelFunctions.ClearHelpOptions();
            dVViewModelFunctions.SetFirstOptionLineNumber(1);
            dVViewModelFunctions.SetOptionNumberStart(0);
            var textList = new List<string>();
            var speechList = new List<string>();
            if (DVPersistence.Get<bool>("instructionsEnabled"))
            {
                string instructionsText = userProgress.ConsultingOldLesson() ? "Escape volta." : "Aperte enter para fazer a próxima lição disponível. Escape volta para o menu de lições.";
                string instructionsSpeech = userProgress.ConsultingOldLesson() ? "Esqueipe volta." : "Aperte êmter para fazer a próxima lição disponível. Esqueipe volta para o menu de lições.";
                textList.Add($"Use tab e shift tab ou as setas verticais para navegar pelos itens. {instructionsText}");
                speechList.Add($"Use tab e shift tab ou as setas verticais para navegar pelos itens. {instructionsSpeech}");
            }
            List<string> defaultTextList = new List<string>()
            {
                "Lição: " + course.LessonNumber(),
                "Concluída: " + (userProgress.LessonKeyValue("CONCLUIU").Equals("S") ? "Sim" : "Não"),
                "Percentual de acertos: " + userProgress.LessonKeyValue("PERCENTUALACERTO") + " %",
                "Letras por minuto: " + userProgress.LessonKeyValue("LETRASPORMINUTO"),
                "Tempo para terminar a lição: " + courseLesson.TimeDisplay((int)userProgress.LessonKeyValue("TEMPOTOTALLICAO")),
                "Tempo de digitação: " + courseLesson.TimeDisplay((int)userProgress.LessonKeyValue("TEMPOPRATICALICAO")),
                "Percentual de tempo gasto: " + userProgress.LessonKeyValue("PERCENTUALTEMPO") + " %",
                "Total de caracteres na lição: " + userProgress.LessonKeyValue("TOTALLETRASLICAO"),
                "Total de caracteres digitados: " + userProgress.LessonKeyValue("PRATICALETRASLICAO"),
                "Total de caracteres digitados corretos: " + userProgress.LessonKeyValue("NLETRASACERTOU")
            };
            List<string> defaultSpeechList = new List<string>()
            {
                "Lição: " + course.LessonNumber(),
                "Concluída: " + (userProgress.LessonKeyValue("CONCLUIU").Equals("S") ? "Sim" : "Não"),
                "Percentual de acertos: " + userProgress.LessonKeyValue("PERCENTUALACERTO") + " %",
                "Letras por minuto: " + userProgress.LessonKeyValue("LETRASPORMINUTO"),
                "Tempo para terminar a lição: " + courseLesson.TimeDisplay((int) userProgress.LessonKeyValue("TEMPOTOTALLICAO")),
                "Tempo de digitação: " + courseLesson.TimeDisplay((int) userProgress.LessonKeyValue("TEMPOPRATICALICAO")),
                "Percentual de tempo gasto: " + userProgress.LessonKeyValue("PERCENTUALTEMPO") + " %",
                "Total de caracteres na lição: " + userProgress.LessonKeyValue("TOTALLETRASLICAO"),
                "Total de caracteres digitados: " + userProgress.LessonKeyValue("PRATICALETRASLICAO"),
                "Total de caracteres digitados corretos: " + userProgress.LessonKeyValue("NLETRASACERTOU")
            };
            if (defaultSpeechList.Count == defaultTextList.Count)
            {
                for (int i = 0; i < defaultTextList.Count; i++)
                {
                    textList.Add(defaultTextList[i]);
                    speechList.Add(defaultSpeechList[i]);
                }
            }
            if ((int)userProgress.LessonKeyValue("TOTALPALAVRASLICAO") > 0)
            {
                textList.Add("Total de palavras na lição: " + userProgress.LessonKeyValue("TOTALPALAVRASLICAO"));
                textList.Add("Total de palavras digitadas: " + ((int)userProgress.LessonKeyValue("NPALAVRASACERTOU") + (int)userProgress.LessonKeyValue("NPALAVRASERROU")));
                textList.Add("Total de palavras digitadas corretas: " + userProgress.LessonKeyValue("NPALAVRASACERTOU"));
                textList.Add("Percentual de palavras corretas: " + userProgress.LessonKeyValue("PERCENTUALPALAVRASCORRETAS") + " %");
                speechList.Add("Total de palavras na lição: " + userProgress.LessonKeyValue("TOTALPALAVRASLICAO"));
                speechList.Add("Total de palavras digitadas: " + ((int)userProgress.LessonKeyValue("NPALAVRASACERTOU") + (int)userProgress.LessonKeyValue("NPALAVRASERROU")));
                speechList.Add("Total de palavras digitadas corretas: " + userProgress.LessonKeyValue("NPALAVRASACERTOU"));
                speechList.Add("Percentual de palavras corretas: " + userProgress.LessonKeyValue("PERCENTUALPALAVRASCORRETAS") + " %");
            }
            bool firstMatch = true;
            foreach (string key in userProgress.GetKeys())
            {
                string searchKey = "ERROSEM_";
                if (key.Contains(searchKey))
                {
                    if (firstMatch)
                    {
                        textList.Add("Erros: ");
                        speechList.Add("Erros: ");
                        firstMatch = false;
                    }
                    string displayKeyError = key.Substring(searchKey.Length);
                    if (displayKeyError == " ")
                    {
                        displayKeyError = "Barra de espaços";
                    }
                    textList.Add(displayKeyError + ": " + userProgress.LessonKeyValue(key));
                    speechList.Add(displayKeyError + ": " + userProgress.LessonKeyValue(key));
                }
            }

            textList = textList.Select(text => dVViewModelFunctions.EditStringForVoiceOver(text)).ToList();
            speechList = speechList.Select(speech => dVViewModelFunctions.EditStringForVoiceOver(speech)).ToList();

            dVViewModelSpeak.SetTextAndSpeech(textList, speechList).RegisterUpdateScreen((text) =>
            {
                
                
                PageFormattedLabel = text;
                TextSize = DVPersistence.Get<double>("fontSize");
            });

            lessonNumber = userProgress.LastAvailableLesson();

            dVViewModelFunctions.SetFirstOptionLineNumber(instructionLines);
            dVViewModelFunctions.SetLastOptionLineNumber(dVViewModelSpeak.LineCount() - 1);
            dVViewModelFunctions.SetOptionNumberStart(instructionLines - 1);
            dVViewModelSpeak.SpeakAll();
        }
        private async void NavigateBack()
        {
            string backRoute = userProgress.ConsultingOldLesson() ? ".." : "../..";
            await Shell.Current.GoToAsync(backRoute);
        }
        private async void GoToNextLesson()
        {
            await Shell.Current.GoToAsync("../../Exercises");
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
        public bool OnPageKeyPress(int keyCode, int keyModifiers)
        {
            pressedKeys.Remove(fingerMapping.mapKeyCode(keyCode));
            var bean = fingerMapping.MapKey(keyCode, keyModifiers, pressedKeys);
            if (bean.code != null)
            {
                dVViewModelSpeak.Skip();
                if (bean.code == " ")
                {
                    OnPage();
                }
                else if (bean.code == "Escape" || (bean.code == "!" && DVDevice.IsVirtual()))
                {
                    NavigateBack();
                }
                else if (bean.code == "Enter" && !userProgress.ConsultingOldLesson())
                {
                    course.SelectLesson(lessonNumber);
                    userProgress.LessonRegistration($"LICAO{lessonNumber}");
                    GoToNextLesson();
                }
                else if (pageKeyCodes.Contains(bean.code))
                {
                    dVViewModelFunctions.HandleKeyCode(bean.code);
                }
                else if (!DVKeyboard.IsModifierKey(bean.code))
                {
                    dVViewModelFunctions.InvalidOption(bean.speakOnlyChar);
                }
            }
            return true;
        }
    }
}
