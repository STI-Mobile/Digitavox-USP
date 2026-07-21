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

using Digitavox.Helpers;
using Digitavox.Models;
using Digitavox.Views;
using Plugin.Maui.Audio;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text.RegularExpressions;
using Digitavox.PlatformsImplementations;

namespace Digitavox.ViewModels
{
    public class DVViewModelFunctions
    {
        private int numberCaptureInterval;
        private string numberConcat;
        private string nextPageRoute;
        private string lastHelpPageRoute = string.Empty;
        private string currentPageIdentifier = string.Empty;
        private int optionNumber;
        private int firstOptionLineNumber;
        private int lastOptionLineNumber;
        private int speakFromHelp;
        private bool lastLineIsText;
        private bool alertControl = false;
        private bool keysEnabled = true;
        private List<string> pageRouteStack = new List<string>();
        private List<string> pageListEnterFunction;
        private List<string> courseHelpOptions;
        private List<string> lessonHelpOptions;
        private List<string> exerciseHelpOptions;
        private Dictionary<string, Action> commonFunctions;
        private Dictionary<string, Action> helpFunctions = new Dictionary<string, Action>();
        private Dictionary<string, string> onlySpokenOptions = new Dictionary<string, string>();
        private string buzzSound = "buzz.wav";
        private IAudioPlayer player1;
        private System.Timers.Timer timer = new System.Timers.Timer();
        private Course course;
        private CourseLesson courseLesson;
        private UserProgress userProgress;
        private DVViewModelSpeak dVViewModelSpeak;
        private FingerMapping fingerMapping;
        private readonly IAudioManager audioManager;
        public DVViewModelFunctions(Course course,
                               CourseLesson courseLesson,
                               UserProgress userProgress,
                               DVViewModelSpeak dVViewModelSpeak,
                               FingerMapping fingerMapping,
                               IAudioManager audioManager)
        {
            this.course = course;
            this.courseLesson = courseLesson;
            this.userProgress = userProgress;
            this.dVViewModelSpeak = dVViewModelSpeak;
            this.fingerMapping = fingerMapping;
            this.audioManager = audioManager;
            commonFunctions = new Dictionary<string, Action>()
            {
                { "Enter", Enter},
                { "!", NavigateBack},
                { "Down",  DownArrow},
                { "Tab",  DownArrow},
                { "Up",  UpArrow},
                { "ShiftTab",  UpArrow},
                { "Escape", NavigateBack},
                { "F1", GoToNextPage},
                { "@", GoToNextPage}
            };
            courseHelpOptions = new List<string>()
            {
                "Left", "Right", "Enter", "F2", "F3", "F4", "F5", "F6", "Escape"
            };
            lessonHelpOptions = new List<string>()
            {
                "Left", "Right", "Enter", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "Escape"
            };
            exerciseHelpOptions = new List<string>()
            {
                "Left", "Ctrl+Left", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "Escape"
            };
            numberConcat = string.Empty;
            nextPageRoute = "Menu";
            speakFromHelp = -1;
            lastLineIsText = false;
            optionNumber = 0;
            firstOptionLineNumber = 0;
        }
        public void CourseHelpOptions()
        {
            helpFunctions = new Dictionary<string, Action>()
            {
                {courseHelpOptions[0], CourseSelectedApresentation},
                {courseHelpOptions[1], CourseSelectedInstruction},
                {courseHelpOptions[2], StartCourse},
            };
            onlySpokenOptions = new Dictionary<string, string>()
            {
                {courseHelpOptions[3], $"Total de {course.TotalLessons()} lições."},
                {courseHelpOptions[4], $"Última concluída: lição {userProgress.LastAvailableLesson() - 1}."},
                {courseHelpOptions[5], $"Usuário {userProgress.GetUserName()} logado."},
                {courseHelpOptions[6], $"O divisor de tempo definido é {DVPersistence.Get<int>("timeDivider")}"},
                {courseHelpOptions[7], $"Curso número {course.CourseNumber()} de {course.CourseNameList().Count}"}
            };
        }
        private void CourseSelectedApresentation()
        {
            OptionBasedOnCurrentPage("CoursesHelp", 0);
        }
        private void CourseSelectedInstruction()
        {
            OptionBasedOnCurrentPage("CoursesHelp", 1);
        }
        public bool CoursesEnterOptionSelected()
        {
            return optionNumber == courseHelpOptions.IndexOf("Enter") - 2;
        }
        private void StartCourse()
        {
            nextPageRoute = "Lessons";
            if (pageRouteStack[pageRouteStack.Count - 1] == "CoursesHelp")
            {
                lastHelpPageRoute = pageRouteStack[pageRouteStack.Count - 1];
                pageRouteStack.RemoveAt(pageRouteStack.Count - 1);
            }
            GoToNextPage();
        }
        public void LessonHelpOptions()
        {
            helpFunctions = new Dictionary<string, Action>()
            {
                {lessonHelpOptions[0], LessonSelectedApresentation},
                {lessonHelpOptions[1], LessonSelectedInstruction},
                {lessonHelpOptions[2], StartLesson},
                {lessonHelpOptions[3], LessonData},
                {lessonHelpOptions[9], LessonStatistics},
                {lessonHelpOptions[10], CourseApresentation},
                {lessonHelpOptions[11], CourseInstruction},
                {lessonHelpOptions[12], NavigateBack}
            };
            string exercisesString = string.Empty;
            for (int i = 0; i < course.GetExercises().Count; i++)
            {
                exercisesString += string.Equals(course.LessonProperty("SOLETRAEXER"), "sim", StringComparison.OrdinalIgnoreCase) ? SpellWord(course.GetExercises()[i]) : SpeakPunctuation(course.GetExercises()[i]);
            }
            onlySpokenOptions = new Dictionary<string, string>()
            {
                {lessonHelpOptions[4], exercisesString},
                {lessonHelpOptions[5], $"Usuário {userProgress.GetUserName()} logado."},
                {lessonHelpOptions[6], $"O divisor de tempo definido é {DVPersistence.Get<int>("timeDivider")}"},
                {lessonHelpOptions[7], $"Lição número {course.LessonNumber()} de {course.TotalLessons()}"},
                {lessonHelpOptions[8], $"{course.CourseProperty("NOMECURSO")}"}
            };
        }
        public string LessonDataCode()
        {
            return lessonHelpOptions[3];
        }
        public string LessonStatisticsCode()
        {
            return lessonHelpOptions[9];
        }
        public bool LessonsEnterOptionSelected()
        {
            return optionNumber == lessonHelpOptions.IndexOf("Enter") - 2;
        }
        private void StartLesson()
        {
            nextPageRoute = "Exercises";
            if (pageRouteStack[pageRouteStack.Count - 1] == "LessonsHelp")
            {
                lastHelpPageRoute = pageRouteStack[pageRouteStack.Count - 1];
                pageRouteStack.RemoveAt(pageRouteStack.Count - 1);
            }
            GoToNextPage();
        }
        private void CourseApresentation()
        {
            OptionBasedOnCurrentPage("LessonsHelp", 0);
        }
        private void CourseInstruction()
        {
            OptionBasedOnCurrentPage("LessonsHelp", 1);
        }
        private void LessonSelectedApresentation()
        {
            OptionBasedOnCurrentPage("LessonsHelp", 2);
        }
        private void LessonSelectedInstruction()
        {
            OptionBasedOnCurrentPage("LessonsHelp", 3);
        }
        private void LessonData()
        {
            courseLesson.SetLessonChars(course.GetExercises(), int.Parse(course.LessonProperty("REPETICOESEXER")), int.Parse(course.LessonProperty("TEMPOPORCARACTER")),
                int.Parse(course.LessonProperty("MEDIAEXER")), DVPersistence.Get<int>("timeDivider"), string.Equals(course.LessonProperty("SOLETRAEXER"), "sim", StringComparison.OrdinalIgnoreCase), string.Equals(course.LessonProperty("TUDO_EM_MAIUSCULO"), "sim", StringComparison.OrdinalIgnoreCase));
            List<string> statisticsList = new List<string>()
            {
                $"Lição: {course.LessonNumber()}",
                $"Concluída: {userProgress.LessonConcluded()}",
                $"Repetições: {course.LessonProperty("REPETICOESEXER")}",
                $"Tempo por caractere em segundos: {course.LessonProperty("TEMPOPORCARACTER")}",
                $"Total de caracteres: {courseLesson.TotalChars()}",
                $"Tempo máximo para concluir: {courseLesson.TotalTime()}",
                $"Percentual de acertos mínimo: {course.LessonProperty("MEDIAEXER")} %",
                $"Maiúsculas dispensadas: {course.LessonProperty("TUDO_EM_MAIUSCULO")}",
                $"Quantidade de exercícios: {course.GetExercises().Count}",
            };
            NextPageContent($"{LessonDataCode()} - Apresenta dados da lição", statisticsList);
        }
        private void LessonStatistics()
        {
            List<string> textList = userProgress.GetLessonRepetitions();
            if (textList.Count == 1)
            {
                dVViewModelSpeak.Speak("Nenhuma tentativa registrada", () => { });
                if (pageRouteStack[pageRouteStack.Count - 1] == "LessonsHelp")
                {
                    optionNumber = lessonHelpOptions.IndexOf("F8") + firstOptionLineNumber;
                    dVViewModelSpeak.BoldLine(optionNumber);
                }
            }
            else NextPageContent($"{LessonStatisticsCode()} - Apresenta estatísticas da lição", textList);
        }
        public void ExerciseHelpOptions()
        {
            helpFunctions = new Dictionary<string, Action>()
            {
                {exerciseHelpOptions[6], LessonApresentation},
                {exerciseHelpOptions[7], LessonInstruction},
                {exerciseHelpOptions[9], TimeStatistics},
                {exerciseHelpOptions[10], NavigateBack},
                {"Ctrl+Up", LessonApresentation},
                {"Ctrl+Down", LessonInstruction},
            };
            onlySpokenOptions = new Dictionary<string, string>()
            {
                {exerciseHelpOptions[0], $"Repetição {courseLesson.CurrentRepetition()} de {courseLesson.GetStatistics().exerciseRepetitions}"},
                {exerciseHelpOptions[1], $"{CharsTypedCorrectPercentual()}%" },
                {exerciseHelpOptions[2], $"{fingerMapping.Code2Speak(courseLesson.CurrentCharacter())} {fingerMapping.Code2Finger(courseLesson.CurrentCharacter())}"},
                {exerciseHelpOptions[3], SpellWord(courseLesson.CurrentExercise().Substring(courseLesson.NextCharacterIndex()))},
                {exerciseHelpOptions[4], (string.Equals(course.LessonProperty("SOLETRAEXER"), "sim", StringComparison.OrdinalIgnoreCase)) ? SpellWord(courseLesson.CurrentExercise().Substring(courseLesson.NextCharacterIndex())) : SpeakPunctuation(SpeakRemainder(courseLesson.CurrentExercise().Substring(courseLesson.NextCharacterIndex())))},
                {exerciseHelpOptions[5], (string.Equals(course.LessonProperty("SOLETRAEXER"), "sim", StringComparison.OrdinalIgnoreCase)) ? SpellWord(courseLesson.CurrentExercise()) : SpeakPunctuation(courseLesson.CurrentExercise())},
                {exerciseHelpOptions[8], DateTime.Now.ToString().Substring(11)},
                {"Up", (string.Equals(course.LessonProperty("SOLETRAEXER"), "sim", StringComparison.OrdinalIgnoreCase)) ? SpellWord(courseLesson.CurrentExercise()) : SpeakPunctuation(courseLesson.CurrentExercise())},
                {"Right", (string.Equals(course.LessonProperty("SOLETRAEXER"), "sim", StringComparison.OrdinalIgnoreCase)) ? SpellWord(courseLesson.CurrentExercise().Substring(courseLesson.NextCharacterIndex())) : SpeakPunctuation(SpeakRemainder(courseLesson.CurrentExercise().Substring(courseLesson.NextCharacterIndex())))},
                {"Down", $"{fingerMapping.Code2Speak(courseLesson.CurrentCharacter())} {fingerMapping.Code2Finger(courseLesson.CurrentCharacter())}"},
                {"Ctrl+Right", SpellWord(courseLesson.CurrentExercise().Substring(courseLesson.NextCharacterIndex()))},
            };
        }
        public void ClearHelpOptions()
        {
            onlySpokenOptions.Clear();
            helpFunctions.Clear();
        }
        public string SpellWord(string word)
        {
            string exerciseSpelled = string.Empty;
            if (word.Last() == ' ' && word.Length > 1) word = word.Remove(word.Length - 1, 1);
            foreach (char letter in word)
            {
                exerciseSpelled += fingerMapping.Code2Speak($"{letter}") + " ";
            }
            return exerciseSpelled;
        }
        public string SpeakPunctuation(string word)
        {
            string speakString = string.Empty;
            foreach (char character in word)
            {
                if (IsPunctuation(character)) speakString += " " + fingerMapping.Code2Speak($"{character}") + " ";
                else speakString += character;
            }
            return speakString;
        }
        static bool IsPunctuation(char character)
        {
            string punctuationChars = ".,;:?!";

            return punctuationChars.Contains(character);
        }
        private string SpeakRemainder(string word)
        {
            string speakString = fingerMapping.Code2Speak($"{word.First()}") + " ";
            if (char.IsLetter(word.First()))
            {
                return word;
            }
            if (word.Length > 1)
            {
                speakString += SpeakRemainder(word.Substring(1));
            }
            return speakString;
        }
        private int CharsTypedCorrectPercentual()
        {
            int totalCharsTyped = courseLesson.GetStatistics().correctChars + courseLesson.GetStatistics().incorrectChars;
            if (totalCharsTyped == 0) return 0;
            return (100 * courseLesson.GetStatistics().correctChars / totalCharsTyped); 
        }
        private void LessonApresentation()
        {
            OptionBasedOnCurrentPage("ExercisesHelp", 0);
        }
        private void LessonInstruction()
        {
            OptionBasedOnCurrentPage("ExercisesHelp", 1);
        }
        private void TimeStatistics()
        {
            OptionBasedOnCurrentPage("ExercisesHelp", 2);
        }
        public async void CreatePlayers()
        {
            if (player1 == null)
            {
                player1 = audioManager.CreatePlayer(await FileSystem.OpenAppPackageFileAsync(buzzSound));
            }
        }
        public void PlayBuzzSound()
        {
            player1.Play();
        }
        private void ShortCode2Speak(string code)
        {
            if (!courseLesson.ExerciseRunnig() || courseLesson.ExercisePaused())
            {
                dVViewModelSpeak.Speak(onlySpokenOptions[code], () => { });
            }
            else 
            {
                int spellingSpeakRate = DVPersistence.Get<int>("speakRate") - 3;
                if (spellingSpeakRate < 1)
                {
                    spellingSpeakRate = 1;
                }
                DVSpeak.GetInstance().SetSpeechRate(spellingSpeakRate);
                courseLesson.PauseTimer();
                dVViewModelSpeak.Speak(onlySpokenOptions[code], () => {
                    DVSpeak.GetInstance().SetSpeechRate(DVPersistence.Get<int>("speakRate"));
                    courseLesson.ContinueTimer();
                });
            }
            int number = -1;
            if (pageRouteStack[pageRouteStack.Count - 1] == "CoursesHelp")
            {
                number = courseHelpOptions.IndexOf(code);
            }
            else if (pageRouteStack[pageRouteStack.Count - 1] == "LessonsHelp")
            {
                number = lessonHelpOptions.IndexOf(code);
            }
            else if (pageRouteStack[pageRouteStack.Count - 1] == "ExercisesHelp")
            {
                List<string> arrow_function = new List<string>
                {
                    "Down", "Ctrl+Right", "Right", "Up", "Ctrl+Up", "Ctrl+Down" 
                };
                number = exerciseHelpOptions.IndexOf(code);
                if ((pageRouteStack[pageRouteStack.Count - 1] == "ExercisesHelp") && arrow_function.Contains(code))
                {
                    number = exerciseHelpOptions.IndexOf("F2") + arrow_function.IndexOf(code);
                }
            }
            if (number != -1)
            {
                optionNumber = number + firstOptionLineNumber;
                dVViewModelSpeak.BoldLine(optionNumber);
            }
        }
        public void SetOptionNumberStart(int optionStart)
        {
            optionNumber = optionStart;
        }
        public void SetFirstOptionLineNumber(int lineNumber)
        {
            firstOptionLineNumber = lineNumber;
        }
        public void SetLastOptionLineNumber(int lineNumber)
        {
            lastOptionLineNumber = lineNumber;
        }
        public void SetNumberCaptureTimeInterval(int interval)
        {
            numberCaptureInterval = interval;
        }
        public void SetNextPageRoute(string route)
        {
            nextPageRoute = route;
        }
        public void SetOption2PageList(List<string> pagesRoutes)
        {
            pageListEnterFunction = pagesRoutes;
        }
        private async void NextPageContent(string title, List<string> lines)
        {
            dVViewModelSpeak.Set("optionTitle", title);
            dVViewModelSpeak.Set("optionList", lines);
            pageRouteStack.Add(nextPageRoute);
            dVViewModelSpeak.CurrentIsExercisePage(false);
            dVViewModelSpeak.CurrentIsLessonsPage(false);
            await Shell.Current.GoToAsync("SecondHelp");
        }
        private void UpArrow()
        {
            if (optionNumber > firstOptionLineNumber) optionNumber = optionNumber - 1;
            else optionNumber = lastOptionLineNumber;
            dVViewModelSpeak.SpeakOneLine(optionNumber, () => { });
        }
        private void DownArrow()
        {
            if (optionNumber < lastOptionLineNumber) optionNumber = optionNumber + 1;
            else optionNumber = firstOptionLineNumber;
            dVViewModelSpeak.SpeakOneLine(optionNumber, () => { });
        }
        private void NumberPressed(string numberString)
        {
            numberConcat = string.Empty;
            int number = int.Parse(numberString);
            if (number <= dVViewModelSpeak.LineCount() - firstOptionLineNumber)
            {
                optionNumber = number + firstOptionLineNumber - 1;
                dVViewModelSpeak.SpeakOneLine(optionNumber, () => { });
            }
            else
            {
                dVViewModelSpeak.Speak($"{numberString}. Opção não encontrada.", () => { });
            }
        }
        private void Enter()
        {
            if (optionNumber >= firstOptionLineNumber && optionNumber < dVViewModelSpeak.LineCount())
            {
                nextPageRoute = (pageListEnterFunction.Count > 1) ? pageListEnterFunction[optionNumber - firstOptionLineNumber] : pageListEnterFunction[0];
                if (nextPageRoute != "PrivacyPolicy")
                {
                    GoToNextPage();
                }
                else
                {
                    keysEnabled = false;
                    if (DVVoiceOverHelper.IsVoiceOverEnabled())
                        dVViewModelSpeak.Speak("Para escutar a política de privacidade as teclas de setas e esqueipe devem ser devolvidas para o controle do vóice ôver, para tanto pressione as teclas de seta para direita e esquerda ao mesmo tempo. Para sair pressione as teclas control e esqueipe juntas e depois pressione as teclas de seta para direita e esquerda ao mesmo tempo novamente.", () =>
                        {
                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                GoToNextPage();
                            });
                        });
                    else if (DVDevice.IsIos()) 
                        dVViewModelSpeak.Speak("Para escutar a política de privacidade o vóice ôver deve ser ativado e as teclas de setas e esqueipe devem estar configuradas para o seu uso. Para sair pressione as teclas control e esqueipe juntas e depois pressione as teclas de seta para direita e esquerda ao mesmo tempo novamente.", () =>
                        {
                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                GoToNextPage();
                            });
                        });
                    else
                    {
                        dVViewModelSpeak.Speak("Para escutar a política de privacidade ative o leitor de tela e para sair pressione a tecla esqueipe.", () =>
                        {
                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                GoToNextPage();
                            });
                        });
                    }
                }
            }
        }
        public bool KeysEnabled()
        {
            bool keysEnabledStored = keysEnabled;
            if (!keysEnabled) keysEnabled = true;
            return keysEnabledStored;
        }
        public async void GoToNextPage()
        {
            lastLineIsText = false;
            keysEnabled = true;
            pageRouteStack.Add(nextPageRoute);
            dVViewModelSpeak.CurrentIsExercisePage(false);
            dVViewModelSpeak.CurrentIsLessonsPage(false);
            dVViewModelSpeak.ClearStyleDictionary();
            await Shell.Current.GoToAsync(nextPageRoute);
        }
        public async void DisplayAlert()
        {
            if (!alertControl)
            {
                alertControl = true;
                await Shell.Current.GoToAsync("Alert");
            }
        }
        public async void DismissAlert()
        {
            if (alertControl)
            {
                alertControl = false;
                await Shell.Current.GoToAsync("..");
            }
        }
        public bool OnAlert()
        {
            return alertControl;
        }
        private async void NavigateBack()
        {
            lastLineIsText = false;
            
            string currentPageRoute = string.Empty;
            string navigateBackRoute = "..";
            if (pageRouteStack.Count > 0)
            {
                currentPageRoute = pageRouteStack[pageRouteStack.Count - 1];
                pageRouteStack.RemoveAt(pageRouteStack.Count - 1);
            }
            if ((currentPageRoute == "Exercises" && lastHelpPageRoute == "LessonsHelp") || (currentPageRoute == "Lessons" && lastHelpPageRoute == "CoursesHelp"))
            {
                navigateBackRoute = "../..";
                lastHelpPageRoute = string.Empty;
            }
            dVViewModelSpeak.CurrentIsExercisePage(false);
            dVViewModelSpeak.CurrentIsLessonsPage(false);
            dVViewModelSpeak.ClearStyleDictionary();
            await Shell.Current.GoToAsync(navigateBackRoute);
        }
        public void InvalidOption(string key)
        {
            if (!(key == "Aumenta volume" || key == "Diminui volume"))
            {
                if (optionNumber >= firstOptionLineNumber && optionNumber <= dVViewModelSpeak.LineCount() - 1) dVViewModelSpeak.Speak($"{key}. Opção inválida", () => { });
                else SkipPageApresentation();
            }
        }
        private void OptionBasedOnCurrentPage(string route, int helpNumber)
        {
            speakFromHelp = helpNumber;
            if (pageRouteStack[pageRouteStack.Count - 1] == route)
            {
                NavigateBack();
            }
        }
        public int GetUpdateSpeakFromHelp()
        {
            int storedValue = speakFromHelp;
            speakFromHelp = -1;
            return storedValue;
        }
        public int GetSpeakFromHelp()
        {
            return speakFromHelp;
        }
        public int MapOptionToItem()
        {
            if (optionNumber < firstOptionLineNumber) return 0;
            else if (optionNumber > dVViewModelSpeak.LineCount() - 1) return dVViewModelSpeak.LineCount() - firstOptionLineNumber;
            return optionNumber - firstOptionLineNumber;
        }
        public void HandleKeyCode(string code)
        {
            if (!keysEnabled) keysEnabled = true;
            if (!dVViewModelSpeak.Get<bool>("maintainTag"))
            {
                dVViewModelSpeak.Set("maintainTag", true);
            }
            if (int.TryParse(code, out _))
            {
                if (numberCaptureInterval > 0)
                {
                    if (numberConcat.Length > 0)
                    {
                        timer.Stop();
                        timer.Start();
                    }
                    else
                    {
                        timer.Interval = numberCaptureInterval;
                        timer.Start();
                    }
                    numberConcat += code;
                    timer.Elapsed += (sender, e) =>
                    {
                        timer.Stop();
                        NumberPressed(numberConcat);
                    };
                }
                else
                {
                    NumberPressed(code);
                }
            }
            else if ((optionNumber >= firstOptionLineNumber) && (optionNumber <= dVViewModelSpeak.LineCount() - 1) && (code != "Escape") && !lastLineIsText)
            {
                List <string> arrow_navigation = new List<string>
                {
                    "Up", "Down"
                };
                if (helpFunctions.ContainsKey(code))
                {
                    helpFunctions[code]();
                }
                else if (onlySpokenOptions.ContainsKey(code) && (((pageRouteStack[pageRouteStack.Count - 1] == "ExercisesHelp") && !arrow_navigation.Contains(code)) || (pageRouteStack[pageRouteStack.Count - 1] != "ExercisesHelp")))
                {
                    ShortCode2Speak(code);
                }
                else
                {
                    if (!((code == "!" || code == "@") && (!DVDevice.IsVirtual()))) commonFunctions[code]();
                }
            }
            else if (code == "Escape") 
            {
                NavigateBack();
            }
            else
            {
                SkipPageApresentation();
            }
        }
        public void LastLineIsText(bool lineText)
        {
            lastLineIsText = lineText;
        }
        private void SkipPageApresentation()
        {
            if (optionNumber < firstOptionLineNumber)
            {
                optionNumber = firstOptionLineNumber;
                dVViewModelSpeak.SpeakOneLine(optionNumber, () => { });
            } 
            else if (optionNumber > dVViewModelSpeak.LineCount() - 1)
            {
                optionNumber = dVViewModelSpeak.LineCount() - 1;
                dVViewModelSpeak.SpeakOneLine(optionNumber, () => { });
            }
            else if (lastLineIsText)
            {
                speakFromHelp = -1;
                lastLineIsText = false;
                dVViewModelSpeak.SpeakOneLine(optionNumber, () => { });
            }
        }


        private static readonly Dictionary<string, string> termModifications = new Dictionary<string, string>
        {
            { "Escape", "CTRL + Escape" },
            { "ESCAPE", "CTRL + ESCAPE" },
            { "esqueipe", "control esqueipe" },
            { "Esqueipe", "control esqueipe" },
            { "[ESC]", "[CTRL] + [ESC]" },
        };

        public string EditStringForVoiceOver(string inputString)
        {
            if (DeviceInfo.Platform == DevicePlatform.iOS && DVVoiceOverHelper.IsVoiceOverEnabled())
            {
                foreach (var term in termModifications.Keys)
                {
                    if (inputString.Contains(term))
                    {
                        inputString = inputString.Replace(term, termModifications[term]);
                    }
                }
            }

            return inputString;
        }
        public void SetCurrentPageIdentifier(string currentPageIdentifier)
        {
            this.currentPageIdentifier = currentPageIdentifier;
        }
        public string CurrentPageIdentifier()
        {
            if (currentPageIdentifier.Length > 0)
            {
                return currentPageIdentifier;
            }
            return string.Empty;
        }
    }
}