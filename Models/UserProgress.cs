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
using System.Collections.Generic;

namespace Digitavox.Models
{
    public class UserProgress
    {
        private string courseKey;
        private string lessonKey;
        private bool oldLesson;
        private Dictionary<string, object> userDictionary;
        private Dictionary<string, object> lessonDictionary;
        private CourseLesson courseLesson;
        private Course course;
        public UserProgress(CourseLesson courseLesson, Course course)
        {
            this.courseLesson = courseLesson;
            this.course = course;
        }
        public void UserRegistration(string userName)
        {
            userDictionary = DVPersistence.LoadUserData(userName);
            if (!userDictionary.ContainsKey("NOMEUSUARIO")) userDictionary["NOMEUSUARIO"] = userName.ToUpper();
        }
        public void UserLogout()
        {
            userDictionary = new Dictionary<string, object>();
        }
        public void NewProgress()
        {
            userDictionary["type"] = "USUARIO";
            DVPersistence.SaveUserJson(userDictionary);
        }
        public void CourseRegistration(string course)
        {
            courseKey = course;
        }
        public void LessonRegistration(string lesson)
        {
            lessonKey = lesson;
        }
        public void AddLesson()
        {
            if (userDictionary.TryGetValue(courseKey, out object test) && test is Dictionary<string, object> courseDictionary)
            {
                var lessonRepetitions = courseDictionary.Keys.Where(key => key.StartsWith($"{lessonKey}."));
                courseDictionary[$"{lessonKey}.{lessonRepetitions.Count()+1}"] = lessonDictionary;
                if (courseLesson.GetStatistics().concluded)
                {
                    int last = int.Parse(lessonKey.Substring(5));
                    if (courseDictionary.ContainsKey("ULTIMACONCLUIDA"))
                    {
                        if (last > (int)courseDictionary["ULTIMACONCLUIDA"]) courseDictionary["ULTIMACONCLUIDA"] = last;
                    }
                    else
                    {
                        courseDictionary["ULTIMACONCLUIDA"] = 1;
                    }
                }
            }
            else
            {
                var courseValue = new Dictionary<string, object>()
                {
                    { $"{lessonKey}.1", lessonDictionary }
                };
                if (courseLesson.GetStatistics().concluded) courseValue["ULTIMACONCLUIDA"] = 1;
                userDictionary[courseKey] = courseValue;
            }
            DVPersistence.SaveUserJson(userDictionary);
        }
        public void GetLessonRepetitionData(int repetition)
        {
            oldLesson = true;
            if (userDictionary.TryGetValue(courseKey, out object test) && test is Dictionary<string, object> courseDictionary)
            {
                lessonDictionary = (Dictionary<string, object>)courseDictionary[$"{lessonKey}.{repetition}"];
            }
        }
        public int LastAvailableLesson()
        {
            int last = 0;
            if (userDictionary.TryGetValue(courseKey, out object test) && test is Dictionary<string, object> courseDictionary)
            {
                if (courseDictionary.ContainsKey("ULTIMACONCLUIDA"))
                {
                    last = (int)courseDictionary["ULTIMACONCLUIDA"];
                }
            }
            if (last < course.TotalLessons())
            {
                if (course.TotalLessons() == 1) return 1;
                return last + 1;
            }
            return last;
        }
        public void SaveStatistics()
        {
            oldLesson = false;
            int pressedChars = courseLesson.GetStatistics().correctChars + courseLesson.GetStatistics().incorrectChars;
            int charsByMinute = (courseLesson.GetStatistics().practiceTime == 0) ? 0 : pressedChars * 60 / courseLesson.GetStatistics().practiceTime;
            float timePercentual = (courseLesson.GetStatistics().totalTime == 0) ? 0 : courseLesson.GetStatistics().practiceTime * 100 / courseLesson.GetStatistics().totalTime;
            lessonDictionary = new Dictionary<string, object>()
            {
                { "DATAINICIO", courseLesson.GetStartTime()},
                { "DATAFIM", courseLesson.GetEndTime()},
                { "CONCLUIU", courseLesson.GetStatistics().concluded ? "S" : "N" },
                { "PERCENTUALACERTO", courseLesson.TotalCorrectPercent() },
                { "TOTALLETRASLICAO", courseLesson.TotalChars() },
                { "NLETRASACERTOU", courseLesson.GetStatistics().correctChars },
                { "NLETRASERROU",  courseLesson.GetStatistics().incorrectChars },
                { "PRATICALETRASLICAO", pressedChars },
                { "LETRASPORMINUTO", charsByMinute },
                { "PERCENTUALPALAVRASCORRETAS", courseLesson.TotalCorrectWordPercent() },
                { "TOTALPALAVRASLICAO", courseLesson.GetStatistics().totalWords },
                { "NPALAVRASACERTOU", courseLesson.GetStatistics().correctWords },
                { "NPALAVRASERROU", courseLesson.GetStatistics().incorrectWords },
                { "DIVISORTEMPO",  DVPersistence.Get<int>("timeDivider") },
                { "REPETICOESEXER", courseLesson.GetStatistics().exerciseRepetitions },
                { "TEMPOTOTALLICAO", courseLesson.GetStatistics().totalTime },
                { "TEMPOPRATICALICAO", courseLesson.GetStatistics().practiceTime },
                { "PERCENTUALTEMPO", timePercentual }
            };
            foreach (KeyValuePair<string, int> error in courseLesson.GetStatistics().errorsByChar)
            {
                lessonDictionary[$"ERROSEM_{error.Key}"] = error.Value;
            }
            AddLesson();
        }
        public object LessonKeyValue(string key)
        {
            return lessonDictionary[key];
        }
        public string LessonConcluded()
        {
            if (userDictionary.TryGetValue(courseKey, out object test) && test is Dictionary<string, object> courseDictionary)
            {
                if (courseDictionary.ContainsKey("ULTIMACONCLUIDA") && course.TotalLessons() == 1)
                {
                    return "Sim";
                }
            }
            return (int.Parse(lessonKey.Substring(5)) < LastAvailableLesson()) ? "Sim" : "Não";
        }
        public Dictionary<string, object>.KeyCollection GetKeys()
        {
            return lessonDictionary.Keys;
        }
        public string GetUserName()
        {
            return userDictionary["NOMEUSUARIO"].ToString();
        }
        public bool UserLogged()
        {
            return userDictionary != null;
        }
        public bool FirstLogin()
        {
            return !userDictionary.ContainsKey("type");
        }
        public List<string> GetLessonRepetitions()
        {
            List<string> lessonRepetitions = new List<string>
            {
                $"Lição {lessonKey.Substring(5)}"
            };
            if (userDictionary.TryGetValue(courseKey, out object test) && test is Dictionary<string, object> courseDictionary)
            {
                foreach (string lesson in courseDictionary.Keys.Where(key => key.StartsWith($"{lessonKey}.")).ToList())
                {
                    GetLessonRepetitionData(int.Parse(lesson.Split('.')[1]));
                    lessonRepetitions.Add($"{lessonDictionary["DATAFIM"]}");
                }
            }
            
            return lessonRepetitions;
        }
        public bool ConsultingOldLesson()
        {
            return oldLesson;
        }
    }
}
