using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace RobotBarman.Services
{
    public class LocalDataHandler
    {
        public static Task<bool> ExistsAsync(string filename)
        {
            string filepath = GetFilePath(filename);
            bool exists = File.Exists(filepath);
            return Task<bool>.FromResult(exists);
        }

        private static string GetFilePath(string filename)
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                filename);
        }

        public static async Task SaveTextDataAsync(string filename, string data)
        {
            var fileName = GetFilePath(filename);
            using (StreamWriter writer = File.CreateText(fileName))
            {
                await writer.WriteAsync(data);
            }

            //File.WriteAllText(fileName, data);
        }

        public static async Task<string> GetTextDataAsync(string filename)
        {
            var fileName = GetFilePath(filename);

            if (!File.Exists(fileName)) return "";

            using (StreamReader reader = File.OpenText(fileName))
            {
                return await reader.ReadToEndAsync();
            }

            //return File.Exists(fileName) ? File.ReadAllText(fileName) : "";
        }

        public static Stream GetSoundStream(string soundName)
        {
            var assembly = typeof(App).GetTypeInfo().Assembly;
            return assembly.GetManifestResourceStream($"{soundName}");
        }

        public static string GetSoundPath(string soundName)
        {
            var assembly = typeof(App).GetTypeInfo().Assembly;
            var stream = assembly.GetManifestResourceStream($"RobotBarman.{soundName}");
            foreach (var manifestResourceName in assembly.GetManifestResourceNames())
            {
                Debug.WriteLine(manifestResourceName);
            }
            return assembly.GetManifestResourceInfo("RobotBarman.Sounds.select.wav")?.FileName;
        }        

        public static async Task<string> GetTextFromAssemblyAsync(string fileName)
        {
            var assembly = typeof(App).GetTypeInfo().Assembly;
            var stream = assembly.GetManifestResourceStream($"RobotBarman.{fileName}");            
            var text = "";
            using (var reader = new StreamReader(stream))
            {
                text = await reader.ReadToEndAsync();
            }

            return text;
        }
    }
}