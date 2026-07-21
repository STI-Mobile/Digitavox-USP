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
using System.Text.Json;

namespace Digitavox.Models
{
    public class Course
    {
        JsonElement fileRoot;
        JsonElement courseRoot;
        JsonElement lessonRoot;
        List<JsonElement> jsonList = new List<JsonElement>();
        List<string> courseIdList = new List<string>();
        List<string> courseNameList = new List<string>();
        private string lessonId;
        private string courseId;
        private int courseNumber;
        public void GetCoursesLists()
        {
            if (jsonList.Count == 0)
            {
                var temp = DVPersistence.ReadCourseFiles();
                jsonList = temp.Item1;
                courseIdList = temp.Item2;
                courseNameList = temp.Item3;
            }
        }
        public void SelectCourse(int fileNumber)
        {
            fileRoot = jsonList[fileNumber];
            courseRoot = fileRoot.GetProperty("CURSO");
            courseId = courseIdList[fileNumber];
            courseNumber = fileNumber + 1;
        }
        public void SelectLesson(int lessonNumber)
        {
            lessonId = $"LICAO{lessonNumber}";
            lessonRoot = fileRoot.GetProperty(lessonId);
        }
        public string CourseApresentation()
        {
            return courseRoot.GetProperty("Present").GetProperty("APT").ToString();
        }
        public string CourseInstruction()
        {
            return courseRoot.GetProperty("Instruction").GetProperty("IST").ToString();
        }
        public int TotalLessons()
        {
            return int.Parse(courseRoot.GetProperty("QUANTIDADELICOES").ToString());
        }
        public string CourseProperty(string property)
        {
            return courseRoot.GetProperty(property).ToString();
        }
        public int CourseNumber()
        {
            return courseNumber;
        }
        public string LessonId()
        {
            return lessonId;
        }
        public string LessonApresentation()
        {
            return lessonRoot.GetProperty("Present").GetProperty("APT").ToString();
        }
        public string LessonInstruction()
        {
            return lessonRoot.GetProperty("Instruction").GetProperty("IST").ToString();
        }
        public string LessonProperty(string property)
        {
            return lessonRoot.GetProperty(property).ToString();
        }
        public int LessonNumber()
        {
            return int.Parse(lessonId.Substring(5));
        }
        public string CourseId()
        {
            return courseId.Replace(".json", string.Empty);
        }
        public List<string> CourseNameList()
        {
            return courseNameList;
        }
        public List<string> GetExercises()
        {
            List<string> exercisesList = new List<string>();
            var exercises = lessonRoot.EnumerateObject()
                              .Where(it => it.Name.StartsWith("EXER"));
            foreach (var exercise in exercises)
            {
                exercisesList.Add(exercise.Value.ToString());
            }
            return exercisesList;
        }
    }
}
