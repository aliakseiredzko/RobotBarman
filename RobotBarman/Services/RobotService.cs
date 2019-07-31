using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FreshMvvm;
using RozumConnectionLib;

namespace RobotBarman.Services
{
    public class RobotService : IRobotService
    {
        private readonly int _askingPeriod = 200;
        private readonly IJsonDatabaseService _databaseService;

        public RobotService()
        {
            _databaseService = FreshIOC.Container.Resolve<IJsonDatabaseService>();           
            Robot = _databaseService.GetRobotData();          

            Speed = 50;
            IsRobotInRelax = true;
            IsRobotBusy = false;
            IsRobotConnected = false;

            GripperState = GripperState.Closed;
            CurrentPosition = new Position();
            CurrentPose = new Pose();
        }

        public bool IsRobotBusy { get; protected set; }
        public bool IsRobotInRelax { get; set; }
        public RealRobot Robot { get; set; }       
        public int Speed { get; set; }
        public bool IsRobotConnected { get; set; }

        public string Ip
        {
            get => Robot.URL;
            set
            {
                if (Robot.URL != value) Robot.URL = value;
            }
        }

        public Position CurrentPosition { get; set; }
        public Pose CurrentPose { get; set; }
        public string LastActionResult { get; protected set; }

        public async Task ChangeGripperStateAsync(int timeout = 1000)
        {
            switch (GripperState)
            {
                case GripperState.Opened:
                    await CloseGripperAsync();
                    break;
                case GripperState.Closed:
                    await OpenGripperAsync();
                    break;
            }
        }

        public GripperState GripperState { get; set; }

        public async Task<bool> ConnectToRobotAsync(string ip)
        {
            Robot = new RealRobot(ip, 8081);

            var connectionResult = await Robot.InitConnectionAsync();

            IsRobotConnected = Robot.IsConnected;
            _databaseService.SaveRobotData(Robot);

            return connectionResult;
        }      

        public async Task FreezeAsync()
        {
            LastActionResult = await Robot.SetModeAsync(RobotMode.Freeze);
            IsRobotInRelax = false;
        }

        public async Task RelaxAsync()
        {
            LastActionResult = await Robot.SetModeAsync(RobotMode.Relax);
            IsRobotInRelax = true;
        }

        public async Task RecoverAsync()
        {
            LastActionResult = await Robot.RecoverAsync();
        }

        public async Task OpenGripperAsync(int timeout = 1000)
        {
            LastActionResult = await Robot.OpenGripperAsync(timeout);
            GripperState = GripperState.Opened;
        }

        public async Task CloseGripperAsync(int timeout = 1000)
        {
            LastActionResult = await Robot.CloseGripperAsync(timeout);
            GripperState = GripperState.Closed;
        }

        public async Task RunPositionAsync(bool makePath, int speed, float maxSpeed, string positionName)
        {
            IsRobotInRelax = false;
            IsRobotBusy = true;

            LastActionResult = await Robot.SetPositionAsync(
                _databaseService.GetPosition(positionName), speed, MotionType.LINEAR, maxSpeed);

            await Robot.WaitMotionAsync(_askingPeriod);
            IsRobotBusy = false;
        }

        public async Task RunPositionAsync(bool makePath, int speed, float maxSpeed, Position position)
        {
            IsRobotInRelax = false;
            IsRobotBusy = true;

            LastActionResult = await Robot.SetPositionAsync(position, speed, MotionType.LINEAR, maxSpeed);

            await Robot.WaitMotionAsync(_askingPeriod);
            IsRobotBusy = false;
        }

        public async Task RunPositionsAsync(bool makePath, int speed, params string[] positionsName)
        {
            var positions = new List<Position>();
            foreach (var item in positionsName) positions.Add(_databaseService.GetPosition(item));

            IsRobotInRelax = false;
            IsRobotBusy = true;

            if (makePath)
            {
                LastActionResult = await Robot.RunPositionsAsync(positions, speed, MotionType.LINEAR);
                CurrentPosition = positions.Last();
                await Robot.WaitMotionAsync(_askingPeriod);
            }
            else
            {
                for (var i = 0; i < positions.Count; i++)
                {
                    Debug.WriteLine(positionsName[i]);
                    LastActionResult = await Robot.SetPositionAsync(positions[i], speed, MotionType.LINEAR);
                    CurrentPosition = positions[i];
                    await Robot.WaitMotionAsync(_askingPeriod);
                }
            }

            IsRobotBusy = false;
        }

        public async Task RunPositionsAsync(bool makePath, params Position[] positions)
        {           
            IsRobotInRelax = false;
            IsRobotBusy = true;

            if (makePath)
            {
                LastActionResult = await Robot.RunPositionsAsync(positions, Speed, MotionType.LINEAR);
                CurrentPosition = positions.Last();
                await Robot.WaitMotionAsync(_askingPeriod);
            }
            else
            {
                foreach (var position in positions)
                {
                    LastActionResult = await Robot.SetPositionAsync(position, Speed, MotionType.LINEAR);
                    CurrentPosition = position;
                    await Robot.WaitMotionAsync(_askingPeriod);
                }
            }

            IsRobotBusy = false;
        }

        public async Task RunPositionsAsync(bool makePath, params string[] positionsName)
        {
            var positions = new List<Position>();
            foreach (var item in positionsName) positions.Add(_databaseService.GetPosition(item));

            IsRobotInRelax = false;
            IsRobotBusy = true;

            if (makePath)
            {
                LastActionResult = await Robot.RunPositionsAsync(positions, Speed, MotionType.LINEAR);
                CurrentPosition = positions.Last();
                await Robot.WaitMotionAsync(_askingPeriod);
            }
            else
            {
                for (var i = 0; i < positions.Count; i++)
                {
                    Debug.WriteLine(positionsName[i]);
                    LastActionResult = await Robot.SetPositionAsync(positions[i], Speed, MotionType.LINEAR);
                    CurrentPosition = positions[i];
                    await Robot.WaitMotionAsync(_askingPeriod);
                }
            }

            IsRobotBusy = false;
        }

        public async Task RunPosesAsync(bool makePath, params string[] posesName)
        {
            var poses = new List<Pose>();
            foreach (var item in posesName) poses.Add(_databaseService.GetPose(item));

            IsRobotInRelax = false;
            IsRobotBusy = true;

            if (makePath)
            {
                LastActionResult = await Robot.RunPosesAsync(poses, Speed);
                CurrentPose = poses.Last();
                await Robot.WaitMotionAsync(_askingPeriod);
            }
            else
            {
                for (var i = 0; i < poses.Count; i++)
                {
                    Debug.WriteLine(posesName[i]);
                    LastActionResult = await Robot.SetPoseAsync(poses[i], Speed);
                    CurrentPose = poses[i];
                    await Robot.WaitMotionAsync(_askingPeriod);
                }
            }

            IsRobotBusy = false;
        }


        public async Task WaitForInputSignalAsync(int port, bool state, int askingPeriod = 50)
        {
            await Robot.WaitForInputSignalAsync(port, state, askingPeriod);
        }

        public async Task<bool> GetInputSignalAsync(int port)
        {
            return await Robot.GetDigitalInputAsync(port) == "HIGH";
        }

        public async Task SetToolAsync(string toolName)
        {
            LastActionResult = await Robot.SetToolAsync(_databaseService.GetGripper(toolName));
            await Robot.GetPositionAsync();
        }

        public async Task GoToBasePosition()
        {
            await RunPosesAsync(false, "Base1Position");
        }

        public async Task ParkAsync()
        {
            await CloseGripperAsync();
            await RunPosesAsync(true, "Base1Position", "PreParking", "Parking");
            await Robot.SetModeAsync(RobotMode.Relax);
        }

        public async Task UnparkAsync()
        {
            await RunPosesAsync(true, "Parking", "PreParking", "Base1Position");
            await OpenGripperAsync();
        }
    }
}