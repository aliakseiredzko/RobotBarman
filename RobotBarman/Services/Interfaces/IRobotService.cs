using System.Threading.Tasks;
using RozumConnectionLib;

namespace RobotBarman.Services
{
    public interface IRobotService
    {
        GripperState GripperState { get; }
        bool IsRobotConnected { get; set; }
        RealRobot Robot { get; set; }        
        int Speed { get; set; }
        string Ip { get; set; }
        Position CurrentPosition { get; set; }
        Pose CurrentPose { get; set; }
        string LastActionResult { get; }
        bool IsRobotBusy { get; }
        bool IsRobotInRelax { get; set; }
        void ConnectToRobot(string ip);        
        Task SetToolAsync(string toolName);
        Task RecoverAsync();
        Task FreezeAsync();
        Task RelaxAsync();
        Task RunPosesAsync(bool makePath, params string[] posesName);
        Task RunPositionsAsync(bool makePath, params string[] positionsName);
        Task RunPositionsAsync(bool makePath, params Position[] positions);
        Task OpenGripperAsync(int timeout = 1000);
        Task CloseGripperAsync(int timeout = 1000);
        Task ChangeGripperStateAsync(int timeout = 1000);
        Task WaitForInputSignalAsync(int port, bool state, int askingPeriod = 50);
        Task RunPositionsAsync(bool makePath, int speed, params string[] positionsName);
        Task RunPositionAsync(bool makePath, int speed, float maxSpeed, string positionName);
        Task<bool> GetInputSignalAsync(int port);
        Task RunPositionAsync(bool makePath, int speed, float maxSpeed, Position position);
        Task GoToBasePosition();
        Task ParkAsync();
        Task UnparkAsync();
    }

    public enum GripperState
    {
        Opened,
        Closed
    }
}