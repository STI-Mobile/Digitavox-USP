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

using Digitavox.PlatformsImplementations;
using Digitavox.Domain.Catalog;
using Digitavox.Infrastructure.Persistence;
using System.Text.Json;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace Digitavox.Helpers
{
    public static class DVPersistence
    {
        public static double GetFontSize()
        {
            return 14.0;
        }
        public static void SetDefaulConfig()
        {
            Set("speakRate", 1);
            Set("speakInput", true);
            Set("fontSize", GetFontSize());
            Set("timeDivider", 1);
            Set("countRepetitions", true);
            Set("instructionsEnabled", true);
            DVSpeak.GetInstance().SetSpeechRate(1);
        }
        public static T Get<T>(string key)
        {
            Dictionary<string, object> defaultValues = new Dictionary<string, object>()
            {
                {"speakRate", 1},
                {"speakInput", true},
                {"fontSize", GetFontSize()},
                {"timeDivider", 1},
                {"countRepetitions", true},
                {"instructionsEnabled", true},
                {"VoiceOverStatus", false}
            };
            return Preferences.Default.Get<T>(key, (T)defaultValues[key]);
        }
        public static void Set<T>(string key, T value)
        {
            Preferences.Default.Set(key, value);
        }
        private static string FilePath(string userInput)
        {
            string userDirectory = Path.Combine(FileSystem.AppDataDirectory, "Users");
            if (!Directory.Exists(userDirectory))
            {
                Directory.CreateDirectory(userDirectory);
            }
            string userName = userInput.ToUpper();
            return Path.Combine(userDirectory, $"{userName}.json");
        }
        public static Dictionary<string, object> LoadUserData(string userInput)
        {
            Dictionary<string, object> userDictionary = new Dictionary<string, object>();
            if (File.Exists(FilePath(userInput)))
            {
                using Stream fileStream = File.OpenRead(FilePath(userInput));
                using StreamReader reader = new StreamReader(fileStream);
                JsonElement rootJson = JsonDocument.Parse(reader.ReadToEnd()).RootElement;
                foreach (var prop in rootJson.EnumerateObject())
                {
                    if (prop.Value.ValueKind == JsonValueKind.String)
                    {
                        userDictionary[prop.Name] = prop.Value.GetString();
                    }
                    else
                    {
                        Dictionary<string, object> courseValue = new Dictionary<string, object>();
                        foreach (var prop2 in prop.Value.EnumerateObject())
                        {
                            if (prop2.Value.ValueKind == JsonValueKind.Number && prop2.Value.TryGetInt32(out int number2))
                            {
                                courseValue[prop2.Name] = number2;
                            }
                            else
                            {
                                Dictionary<string, object> lessonValue = new Dictionary<string, object>();
                                foreach (var prop3 in prop2.Value.EnumerateObject())
                                {
                                    if (prop3.Value.ValueKind == JsonValueKind.String)
                                    {
                                        lessonValue[prop3.Name] = prop3.Value.GetString();
                                    }
                                    else if (prop3.Value.ValueKind == JsonValueKind.Number && prop3.Value.TryGetInt32(out int number3))
                                    {
                                        lessonValue[prop3.Name] = number3;
                                    }
                                    else if (prop3.Value.ValueKind == JsonValueKind.Number && prop3.Value.TryGetSingle(out float number4))
                                    {
                                        lessonValue[prop3.Name] = number4;
                                    }
                                }
                                courseValue[prop2.Name] = lessonValue;
                            }
                        }
                        userDictionary[prop.Name] = courseValue;
                    }
                }
            }
            return userDictionary;
        }
        public static void SaveUserJson(Dictionary<string, object> userData)
        {
            if (userData.TryGetValue("NOMEUSUARIO", out object keyvalue) && keyvalue is string userName)
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                byte[] jsonUtf8Bytes = JsonSerializer.SerializeToUtf8Bytes(userData, options);
                File.WriteAllBytes(FilePath(userName), jsonUtf8Bytes);
            }
        }
        public static bool CourseDirectoryExists()
        {
            string coursesDirectory = Path.Combine(FileSystem.AppDataDirectory, "Courses");
            return Directory.Exists(coursesDirectory);
        }
        public static async Task CopyCourseFilesAsync()
        {
            await new CatalogMigration(FileSystem.AppDataDirectory, FileSystem.OpenAppPackageFileAsync).EnsureCurrentAsync();
        }
        public static (List<JsonElement>, List<string>, List<string>) ReadCourseFiles(IEnumerable<string> allowedCourseIds = null)
        {
            var numbered = new SortedDictionary<int, (JsonElement Json, string File, string Name)>();
            List<JsonElement> jsonList = new List<JsonElement>();
            List<string> fileNameList = new List<string>();
            List<JsonElement> addedJsonList = new List<JsonElement>();
            List<string> addedFileNameList = new List<string>();
            List<string> courseNameList = new List<string>();
            string courseDirectory = Path.Combine(FileSystem.AppDataDirectory, "Courses");
            string[] courseFiles = Directory.GetFiles(courseDirectory, "*.json").OrderBy(Path.GetFileName, StringComparer.Ordinal).ToArray();
            if (allowedCourseIds is not null)
            {
                var allowed = allowedCourseIds.ToHashSet(StringComparer.Ordinal);
                courseFiles = courseFiles.Where(p => allowed.Contains(Path.GetFileNameWithoutExtension(p))).ToArray();
            }
            foreach (string courseFile in courseFiles)
            {
                JsonElement rootJson;
                try
                {
                    using Stream fileStream = File.OpenRead(courseFile);
                    using var document = JsonDocument.Parse(fileStream);
                    rootJson = document.RootElement.Clone();
                }
                catch (JsonException exception)
                {
                    throw new InvalidDataException($"JSON de catálogo inválido: {Path.GetFileName(courseFile)}.", exception);
                }
                // Auxiliary JSON objects are not course definitions. Version metadata lives outside Courses.
                if (rootJson.ValueKind == JsonValueKind.Object && !rootJson.TryGetProperty("CURSO", out _) &&
                    (!rootJson.TryGetProperty("type", out var type) || type.ToString() != "CURSO")) continue;
                CourseCatalogValidator.Validate(rootJson, Path.GetFileName(courseFile));
                if (rootJson.GetProperty("CURSO").TryGetProperty("NUMEROCURSO", out var numberProp))
                {
                    int number = int.Parse(numberProp.ToString());
                    if (!numbered.TryAdd(number, (rootJson, Path.GetFileName(courseFile), rootJson.GetProperty("CURSO").GetProperty("NOMECURSO").GetString())))
                        throw new InvalidDataException($"NUMEROCURSO duplicado: {number} em {Path.GetFileName(courseFile)}.");
                }
                else
                {
                    addedJsonList.Add(rootJson);
                    addedFileNameList.Add(Path.GetFileName(courseFile));
                }
            }
            foreach (var courseObjects in numbered.Values)
            {
                jsonList.Add(courseObjects.Json);
                fileNameList.Add(courseObjects.File);
                courseNameList.Add(courseObjects.Name);
            }
            for (int i = 0; i < addedJsonList.Count; i++)
            {
                jsonList.Add(addedJsonList[i]);
                fileNameList.Add(addedFileNameList[i]);
                courseNameList.Add(addedJsonList[i].GetProperty("CURSO").GetProperty("NOMECURSO").ToString());
            }
            return (jsonList, fileNameList, courseNameList);
        }
    }
}
