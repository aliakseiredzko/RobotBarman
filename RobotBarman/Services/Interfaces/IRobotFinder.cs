using System.Threading;
using System.Threading.Tasks;

namespace RobotBarman.Services
{
    public interface IRobotFinder
    {
        int PingTimeout { get; set; }
        int RequestTimeout { get; set; }
        string StartIpRange { get; set; }

        Task<string> FindRobotIpAsync(CancellationTokenSource tokenSource = null);
    }
}