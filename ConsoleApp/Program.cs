using FreshMvvm;
using Mobile.Communication.Client;
using Mobile.Communication.Common;
using Newtonsoft.Json;
using RobotBarman;
using RobotBarman.Services;
using RobotBarman.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("e".Contains("e"));
            var client = new HttpClient();            
            //Tts();
            //TestBarman().Wait();
        }

        static async Task Tts()
        {
            try
            {
                 HttpClient client = new HttpClient();

                const string oAuthToken = "AQAAAAAi6x98AATuwXTszgvWMU2OoUqroplSDHI";

                string iamToken = await GetIamToken(oAuthToken);
                Console.WriteLine(iamToken);
                string folderId = "b1ggaocl30s8q028gl78";
                
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + iamToken);
                var values = new Dictionary<string, string>
                {
                    { "text", "Здравствуйте" },
                    { "lang", "ru-RU" },
                    { "folderId", folderId }
                };
                var content = new FormUrlEncodedContent(values);
                var response = await client.PostAsync("https://tts.api.cloud.yandex.net/speech/v1/tts:synthesize", content);
                var responseBytes = await response.Content.ReadAsByteArrayAsync();
                File.WriteAllBytes("speech.ogg", responseBytes);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        static async Task<string> GetIamToken(string oAuthToken)
        {
            using (var client = new HttpClient())
            {
                var httpContent = new StringContent(
                    "{\"yandexPassportOauthToken\":\"" + $"{oAuthToken}"+ "\"}",
                    Encoding.UTF8, "application/json");
                var response = await client.PostAsync("https://iam.api.cloud.yandex.net/iam/v1/tokens", httpContent);
                var content = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<Dictionary<string, string>>(content)["iamToken"];                
            }
        }

        static async Task TestBarman()
        {
            FreshIOC.Container.Register<IAgvService, AgvService>();
            FreshIOC.Container.Register<IJsonDatabaseService, JsonDatabaseService>();
            FreshIOC.Container.Register<ISoundService, SoundService>();
            FreshIOC.Container.Register<IRobotService, RobotService>();
            FreshIOC.Container.Register<IBarmanService, BarmanService>();           

            var barman = FreshIOC.Container.Resolve<IBarmanService>();
            var robot = FreshIOC.Container.Resolve<IRobotService>();
            var database = FreshIOC.Container.Resolve<IJsonDatabaseService>();           
            var sounds = FreshIOC.Container.Resolve<ISoundService>();
            var agv = FreshIOC.Container.Resolve<IAgvService>();

            agv.Ip = "192.168.1.102";
            agv.Port = 7171;
            agv.Password = "Omron4you";
            await agv.ConnectAsync();
            await agv.GoToGoalAsync("BarmanGoal");
            Console.WriteLine("Done");

            //barman.PutCupsToAgv().Wait();


        }
    }
}
