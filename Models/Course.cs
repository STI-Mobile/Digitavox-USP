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

using Digitavox.Core.Abstractions;
using System.Text.Json;
using Digitavox.Domain.Catalog;

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
        private readonly ICourseCatalogStore courseCatalogStore;
        public Course(ICourseCatalogStore courseCatalogStore)
        {
            this.courseCatalogStore = courseCatalogStore;
        }
        public void GetCoursesLists()
        {
            if (jsonList.Count == 0)
            {
                CourseCatalogData catalog = courseCatalogStore.Load();
                if (catalog.Courses.Count != catalog.FileNames.Count || catalog.Courses.Count != catalog.CourseNames.Count)
                    throw new InvalidDataException("Listas de catálogo inconsistentes.");
                var indices = Enumerable.Range(0, catalog.Courses.Count)
                    .Where(i => !ContentAvailability.IsRemoved(Path.GetFileNameWithoutExtension(catalog.FileNames[i]))).ToArray();
                jsonList = indices.Select(i => catalog.Courses[i]).ToList();
                courseIdList = indices.Select(i => catalog.FileNames[i]).ToList();
                courseNameList = indices.Select(i => catalog.CourseNames[i]).ToList();
            }
        }
        public void SelectCourse(int fileNumber)
        {
            if (fileNumber < 0 || fileNumber >= jsonList.Count)
                throw new ArgumentOutOfRangeException(nameof(fileNumber), "Curso inexistente no catálogo.");
            fileRoot = jsonList[fileNumber];
            courseRoot = fileRoot.GetProperty("CURSO");
            courseId = courseIdList[fileNumber];
            courseNumber = fileNumber + 1;
            lessonId = null;
            lessonRoot = default;
        }
        public void SelectLesson(int lessonNumber)
        {
            string selectedId = $"LICAO{lessonNumber}";
            if (fileRoot.ValueKind != JsonValueKind.Object || ContentAvailability.IsRemoved(CourseId(), selectedId) || lessonNumber < 1 || lessonNumber > TotalLessons() ||
                !fileRoot.TryGetProperty(selectedId, out var selectedLesson) || selectedLesson.ValueKind != JsonValueKind.Object)
                throw new KeyNotFoundException($"Lição inexistente no catálogo: {selectedId}.");
            lessonId = selectedId;
            lessonRoot = selectedLesson;
        }
        public string CourseApresentation()
        {
            EnsureAvailable();
            return courseRoot.GetProperty("Present").GetProperty("APT").ToString();
        }
        public string CourseInstruction()
        {
            EnsureAvailable();
            return courseRoot.GetProperty("Instruction").GetProperty("IST").ToString();
        }
        public int TotalLessons()
        {
            EnsureAvailable();
            int total = int.Parse(courseRoot.GetProperty("QUANTIDADELICOES").ToString());
            return CourseId() == "musicas" ? Math.Min(total, 4) : total;
        }
        public string CourseProperty(string property)
        {
            EnsureAvailable();
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
            EnsureAvailable(requireLesson: true);
            return lessonRoot.GetProperty("Present").GetProperty("APT").ToString();
        }
        public string LessonInstruction()
        {
            EnsureAvailable(requireLesson: true);
            return lessonRoot.GetProperty("Instruction").GetProperty("IST").ToString();
        }
        public string LessonProperty(string property)
        {
            EnsureAvailable(requireLesson: true);
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
            EnsureAvailable(requireLesson: true);
            List<string> exercisesList = new List<string>();
            var exercises = lessonRoot.EnumerateObject()
                              .Where(it => it.Name.StartsWith("EXER"));
            foreach (var exercise in exercises)
            {
                exercisesList.Add(exercise.Value.ToString());
            }
            return exercisesList;
        }
        public void ResetCatalog()
        {
            jsonList.Clear();
            courseIdList.Clear();
            courseNameList.Clear();
            fileRoot = courseRoot = lessonRoot = default;
            courseId = lessonId = null;
            courseNumber = 0;
        }
        private void EnsureAvailable(bool requireLesson = false)
        {
            if (courseId is null || ContentAvailability.IsRemoved(CourseId(), requireLesson ? lessonId : null) ||
                (requireLesson && lessonRoot.ValueKind != JsonValueKind.Object))
                throw new InvalidOperationException("Conteúdo indisponível para execução.");
        }
    }
}
