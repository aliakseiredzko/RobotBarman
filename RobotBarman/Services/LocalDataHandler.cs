using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace RobotBarman.Services
{
    public class LocalDataHandler
    {
        public static void SaveTextData(string filename, string data)
        {
            var fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                filename);
            File.WriteAllText(fileName, data);
        }

        public static string GetTextData(string filename)
        {
            var fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                filename);
            return File.Exists(fileName) ? File.ReadAllText(fileName) : "";
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

        public static string GetTextFromAssembly(string fileName)
        {
            var assembly = typeof(App).GetTypeInfo().Assembly;
            var stream = assembly.GetManifestResourceStream($"RobotBarman.{fileName}");            
            var text = "";
            using (var reader = new StreamReader(stream))
            {
                text = reader.ReadToEnd();
            }

            return text;
        }
    }
}