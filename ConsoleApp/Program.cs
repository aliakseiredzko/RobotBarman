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
using System.Text.RegularExpressions;
using System.Threading;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using RozumConnectionLib;

namespace ConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            FreshIOC.Container.Register<IAgvService, AgvService>();
            FreshIOC.Container.Register<IJsonDatabaseService, JsonDatabaseService>();
            FreshIOC.Container.Register<ISoundService, SoundService>();
            FreshIOC.Container.Register<IRobotService, RobotService>();
            FreshIOC.Container.Register<IBarmanService, BarmanService>();
            var robotService = FreshIOC.Container.Resolve<IRobotService>();
            var database = FreshIOC.Container.Resolve<IJsonDatabaseService>();
            await robotService.ConnectToRobotAsync("10.162.0.181");


            var robot = new RealRobot("10.162.0.181");
            await robot.InitConnectionAsync();
            if (robot.IsConnected)
            {
                Console.WriteLine("Robot connected");
                var result = await robot.SetBasePositionAsync(new[] { 0, 0, 0, 0, 0, 1.57 });
                Console.WriteLine(result);

                result = await robot.GetBasePositionAsync();
                Console.WriteLine(result);
                Console.WriteLine(robot.BasePosition);

                await robot.GetPositionAsync();
                var pos = robot.Position.Clone();

                Console.WriteLine(pos);

                await robotService.RunPositionAsync(false, 2, 1, "Base1UpPosition");
                Console.WriteLine(robotService.LastActionResult);
            }

            Console.ReadKey();

            await robotService.FreezeAsync();
        }

        public static void DisplayDnsAddresses()
        {
            NetworkInterface[] adapters  = NetworkInterface.GetAllNetworkInterfaces();
            
            foreach (NetworkInterface adapter in adapters)
            {
        
                IPInterfaceProperties adapterProperties = adapter.GetIPProperties();
                IPAddressCollection dnsServers = adapterProperties.DnsAddresses;
                if (dnsServers.Count > 0)
                {
                    Console.WriteLine(adapter.Description);
                    foreach (IPAddress dns in dnsServers)
                    {
                        Console.WriteLine("  DNS Servers ............................. : {0}", 
                            dns.ToString());
                    }
                    Console.WriteLine();
                }
            }
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
            await agv.GoToGoalAsync("BarmanGoal", false);
            Console.WriteLine("Done");

            //barman.PutCupsToAgv().Wait();


        }
    }
}
