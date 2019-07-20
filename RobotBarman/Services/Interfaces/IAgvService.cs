using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RobotBarman.Services
{
    public interface IAgvService
    {
        string Ip { get; set; }
        int Port { get; set; }
        string Password { get; set; }
        bool IsConnected { get; }
        string LastActionResult { get; }
        IEnumerable<string> Goals { get; }
        IEnumerable<string> Routes { get; }
        string PutCupsGoal { get; set; }
        string IntermediatePutCupsGoal { get; set; }
        string RouteToPatrol { get; set; }
        bool PatrolRouteForever { get; set; }
        Task<ConnectionDetails> ConnectAsync();
        Task<bool> RefreshGoalsAsync();
        Task<bool> RefreshRoutesAsync();
        Task<bool> GoToGoalAsync(string goal);
        Task<bool> PatrolRouteAsync(string route, bool forever);
        Task<bool> SayAsync(string speech);
        Task<bool> PutCupsToAgv();
        Task<bool> ResetAsync();
    }
}
