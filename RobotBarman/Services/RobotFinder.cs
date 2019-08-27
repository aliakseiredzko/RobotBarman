using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace RobotBarman.Services
{
    public class RobotFinder : IRobotFinder
    {
        public int PingTimeout { get; set; }
        public int RequestTimeout { get; set; }
        public string StartIpRange { get; set; }

        public RobotFinder()
        {
            PingTimeout = 200;
            RequestTimeout = 200;
            StartIpRange = GetLocalAddress();
        }

        public async Task<string> FindRobotIpAsync(CancellationTokenSource tokenSource = null)
        {
            var startRange = Regex.Match(StartIpRange, "^\\d{1,3}.\\d{1,3}.\\d{1,3}.").Value;
            var ip = "";
            var startValue = int.Parse(Regex.Match(StartIpRange, "\\d{1,3}$").Value);

            var ping = new Ping();
            var client = new HttpClient();
            client.Timeout = TimeSpan.FromMilliseconds(RequestTimeout);

            for (int i = startValue; i < 255; i++)
            {
                if (tokenSource.IsCancellationRequested) return null;

                ip = startRange + i;
                try
                {
                    var reply = await ping.SendPingAsync(IPAddress.Parse(ip), PingTimeout);

                    if (reply.Status == IPStatus.Success)
                    {
                        var response = await client.GetAsync($"http://{ip}:8081/status/motion");
                        if (response.IsSuccessStatusCode)
                        {
                            Debug.WriteLine($"Connected to {ip}");
                            return ip;
                        }
                    }
                }
                catch
                {
                    Debug.WriteLine($"Can't connect to {ip}");
                }
            }
            return null;
        }


        private string GetLocalAddress()
        {
            var IpAddress = Dns.GetHostAddresses(Dns.GetHostName()).FirstOrDefault();
            if(IpAddress == null)
                return "192.168.1.1";

            return IpAddress.ToString();
        }
    }
}
