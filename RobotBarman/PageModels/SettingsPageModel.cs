using FreshMvvm;
using RobotBarman.Services;
using Xamarin.Forms;
using Xamarin.Essentials;
using System.Linq;

namespace RobotBarman
{
    internal class SettingsPageModel : FreshBasePageModel
    {        
        private readonly IRobotService _robotService;
        private readonly IBarmanService _barmanService;
        private readonly IAgvService _agvService;
        private bool _isRobotInRelax;

        public SettingsPageModel()
        {
            _robotService = FreshIOC.Container.Resolve<IRobotService>();            
            _barmanService = FreshIOC.Container.Resolve<IBarmanService>();
            _agvService = FreshIOC.Container.Resolve<IAgvService>();
            _isRobotInRelax = true;
        }

        public Command BasePosition
        {
            get
            {
                return new Command(async () =>
                {
                    await _robotService.RunPosesAsync(false, "Base1Position");
                    await CoreMethods.DisplayAlert("", $"State: {_robotService.LastActionResult}", "OK");
                });
            }
        }

        public Command Reset
        {
            get
            {
                return new Command(async () =>
                {
                    _barmanService.ResetSpill();
                    while (_robotService.IsRobotBusy)
                    {
                        await _robotService.FreezeAsync();
                    }
                });
            }
        }

        public Command Gripper
        {
            get
            {
                return new Command(async () =>
                {
                    await _robotService.ChangeGripperStateAsync();
                    await CoreMethods.DisplayAlert("", $"Gripper state: {_robotService.GripperState}", "OK");
                });
            }
        }

        public Command Recover
        {
            get
            {
                return new Command(async () =>
                {
                    await _robotService.RecoverAsync();
                    await CoreMethods.DisplayAlert("", $"Recover state: {_robotService.LastActionResult}", "OK");
                });
            }
        }

        public Command Parking
        {
            get
            {
                return new Command(async () =>
                {
                    await _robotService.ParkAsync();
                    await CoreMethods.DisplayAlert("", $"Parking state: {_robotService.LastActionResult}", "OK");
                });
            }
        }

        public Command Unparking
        {
            get
            {
                return new Command(async () =>
                {
                    await _robotService.UnparkAsync();
                    await CoreMethods.DisplayAlert("", $"BasePosition state: {_robotService.LastActionResult}", "OK");
                });
            }
        }

        public Command CheckConnection
        {
            get
            {
                return new Command(async () =>
                {
                    var profiles = Connectivity.ConnectionProfiles;
                    if (profiles.Contains(ConnectionProfile.WiFi))
                    {
                        if (!string.IsNullOrEmpty(Ip))
                        {
                            _robotService.ConnectToRobot(Ip);
                            RaisePropertyChanged(nameof(IsRobotConnected));
                            RaisePropertyChanged(nameof(IsRobotReady));
                            await CoreMethods.DisplayAlert("", $"CheckConnection state: {_robotService.IsRobotConnected}",
                                "OK");
                        }
                    }
                    else await CoreMethods.DisplayAlert("SOS", "Похоже, вы не подключены к Wi-Fi!", "Понятно");
                    
                });
            }
        }

        public string Ip
        {
            get => _robotService.Ip;
            set
            {
                if (_robotService.Ip != value)
                {
                    _robotService.Ip = value;
                    RaisePropertyChanged(nameof(Ip));
                }
            }
        }       

        public bool IsRobotConnected => _robotService.IsRobotConnected;

        public bool IsRobotFree => !_robotService.IsRobotBusy;

        public bool IsRobotReady => !_robotService.IsRobotBusy && _robotService.IsRobotConnected;

        public bool IsRobotInRelax
        {
            get => _isRobotInRelax;
            set
            {
                if (_isRobotInRelax == value) return;
                _isRobotInRelax = value;

                if (value)
                    _robotService.RelaxAsync();
                else _robotService.FreezeAsync();

                RaisePropertyChanged(nameof(IsRobotInRelax));
            }
        }

        public int Speed
        {
            get => _robotService.Speed;
            set
            {
                if (_robotService.Speed != value)
                {
                    _robotService.Speed = value;
                    RaisePropertyChanged(nameof(Speed));
                }
            }
        }
    }
}