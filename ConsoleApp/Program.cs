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

namespace ConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string startIP = "192.168.1.";
            string endIP = "192.168.1.254";
            //Console.WriteLine(Regex.Match(startIP, "\\d{1,3}$").Value);
            //Console.WriteLine(Regex.Matches(startIP, "\\d{1,3}")[1]);

            Console.WriteLine("Start devices scan");
            var ping = new Ping();
            var tcpClient = new TcpClient();
            tcpClient.SendTimeout = 100;
            tcpClient.ReceiveTimeout = 100;
                
            for (int i = 1; i < 255; i++)
            {
                
                try
                {  
                    var reply = ping.Send(IPAddress.Parse(startIP+i), 500);

                    if(reply.Status == IPStatus.Success)
                    {
                        await tcpClient.ConnectAsync(IPAddress.Parse(startIP + i), 8081);
                        if (tcpClient.Connected)
                        {
                            Console.WriteLine($"{startIP + i} connected");
                        }
                        //await client.GetAsync("http://192.168.1.1/get/status");  
                    }

                    else Console.WriteLine($"{startIP+i} isn't connected");
                    
                }
                catch
                {
                    Console.WriteLine($"{startIP+i} isn't connected");
                    tcpClient.Close();
                }
            }
            Console.WriteLine("End devices scan");
      
            try
            {
                
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Operation was cancelled");
            }

            DisplayDnsAddresses();
           
            

            Console.ReadKey();
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
