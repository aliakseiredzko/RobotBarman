using FreshMvvm;
using RobotBarman.Services;
using Xamarin.Forms;
using Xamarin.Essentials;
using System.Linq;
using System.Diagnostics;
using System.Threading;

namespace RobotBarman
{
    internal class SettingsPageModel : FreshBasePageModel
    {        
        private IRobotService _robotService;
        private IBarmanService _barmanService;
        private IRobotFinder _robotFinder;
        private bool _isRobotInRelax;
        private bool _isIpSearching;
        private CancellationTokenSource _tokenSource;

        public SettingsPageModel()
        {
                 
        }

        public async override void Init(object initData)
        {
            _robotService = FreshIOC.Container.Resolve<IRobotService>();
            _barmanService = FreshIOC.Container.Resolve<IBarmanService>();
            _robotFinder = FreshIOC.Container.Resolve<IRobotFinder>();
            _isRobotInRelax = true;

            await _barmanService.InitDataAsync();
            await _robotService.InitDataAsync();

            base.Init(initData);
        }

        public Command BasePosition
        {
            get
            {
                return new Command(async () =>
                {
                    RaisePropertyChanged(nameof(IsRobotReady));
                    await _robotService.RunPosesAsync(false, "Base1Position");
                    RaisePropertyChanged(nameof(IsRobotReady));
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
                    RaisePropertyChanged(nameof(IsRobotReady));
                    await _robotService.ParkAsync();
                    RaisePropertyChanged(nameof(IsRobotReady));
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
                    RaisePropertyChanged(nameof(IsRobotReady));
                    await _robotService.UnparkAsync();
                    RaisePropertyChanged(nameof(IsRobotReady));
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
                            await _robotService.ConnectToRobotAsync(Ip);
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

        public Command FindRobot
        {
            get
            {
                return new Command(async () =>
                {                    
                    var profiles = Connectivity.ConnectionProfiles;
                    if (profiles.Contains(ConnectionProfile.WiFi))
                    {          
                        _tokenSource = new CancellationTokenSource();

                        CanFindIp = false;
                        ButtonFindText = "ПОИСК";                        
                        var ip = await _robotFinder.FindRobotIpAsync(_tokenSource);                                                
                        CanFindIp = true;
                        ButtonFindText = "НАЙТИ";

                        _tokenSource = null;                       

                        if(ip != null)
                        {
                            Ip = ip;
                            await CoreMethods.DisplayAlert("Успех", $"Робот найден с IP {Ip}", "Ура!");
                        }
                        else
                        {
                            await CoreMethods.DisplayAlert("SOS", $"Робот не найден :(", "Понятно");
                        }
                    }
                    else await CoreMethods.DisplayAlert("SOS", "Похоже, вы не подключены к Wi-Fi!", "Понятно");
                    
                });
            }
        }

        public Command CancelFindRobot
        {
            get
            {
                return new Command(() =>
                {                   
                    _tokenSource?.Cancel();                                        
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
        
        public string StartIpRange
        {
            get => _robotFinder.StartIpRange;
            set
            {
                if (_robotFinder.StartIpRange != value)
                {
                     _robotFinder.StartIpRange = value;
                    RaisePropertyChanged(nameof(StartIpRange));
                }
            }
        }

        public string LocalDeviceAddress
        {
            get => _robotFinder.LocalDeviceAddress;
            set
            {
                if (_robotFinder.LocalDeviceAddress != value)
                {
                    _robotFinder.LocalDeviceAddress = value;
                    RaisePropertyChanged(nameof(LocalDeviceAddress));
                }
            }
        }

        public bool CanFindIp
        {
            get => _isIpSearching;
            set
            {
                if (_isIpSearching == value) return;
                _isIpSearching = value;

                RaisePropertyChanged(nameof(CanFindIp));
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

        private string _buttonSearchText = "НАЙТИ";

        public string ButtonFindText
        {
            get { return _buttonSearchText; }
            set 
            { 
                if (_buttonSearchText == value) return;

                _buttonSearchText = value;
                RaisePropertyChanged(nameof(ButtonFindText));
            }
        }

    }
}