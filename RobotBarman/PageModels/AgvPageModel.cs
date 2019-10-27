using FreshMvvm;
using RobotBarman.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using RozumConnectionLib;
using Xamarin.Forms;

namespace RobotBarman
{
    internal class AgvPageModel : FreshBasePageModel
    {        
        private readonly IRobotService _robotService;
        private readonly IBarmanService _barmanService;
        private readonly IAgvService _agvService;

        public AgvPageModel()
        {
            _robotService = FreshIOC.Container.Resolve<IRobotService>();            
            _barmanService = FreshIOC.Container.Resolve<IBarmanService>();
            _agvService = FreshIOC.Container.Resolve<IAgvService>();
        }
        
        public string AgvIp
        {
            get => _agvService.Ip;
            set
            {
                if (_agvService.Ip != value)
                {
                    _agvService.Ip = value;
                    RaisePropertyChanged(nameof(AgvIp));
                }
            }

        }

        public int AgvPort
        {
            get => _agvService.Port;
            set
            {
                if (_agvService.Port != value)
                {
                    _agvService.Port = value;
                    RaisePropertyChanged(nameof(AgvPort));
                }
            }
        }

        public string AgvPassword
        {
            get => _agvService.Password;
            set
            {
                if (_agvService.Password != value)
                {
                    _agvService.Password = value;
                    RaisePropertyChanged(nameof(AgvPassword));
                }
            }
        }

        private string _textToSay;

        public string TextToSay
        {
            get => _textToSay;
            set
            {
                if (_textToSay!=value)
                {
                    _textToSay = value;
                    RaisePropertyChanged(nameof(TextToSay));
                }
                
            }
        }

        public Command ConnectToAgv
        {
            get
            {
                return new Command(async () =>
                {
                    string resultToShow = "";
                    var result = await _agvService.ConnectAsync();
                    switch (result)
                    {
                        case ConnectionDetails.Ok:
                            resultToShow = "Успешно";
                            await GetData();
                            RaisePropertyChanged(nameof(IsAgvConnected));
                            IsAgvReady = true;
                            break;
                        case ConnectionDetails.WrongPassword:
                            resultToShow = "Неверный пароль";
                            break;
                        case ConnectionDetails.ConnectionError:
                            resultToShow = "Проблема с подключением к тележке";
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    await CoreMethods.DisplayAlert("Подключение к тележке", resultToShow, "ОК");
                });
            }
        }

        public Command PutCups
        {
            get
            {
                return new Command(async () =>
                {
                    IsAgvReady = false;
                    if (!await _agvService.PutCupsToAgv()) 
                        await CoreMethods.DisplayAlert("Ошибка", "Тележка не приедет", "ОК");
                    IsAgvReady = true;
                });
            }
        }

        public Command GoToCupGoal
        {
            get
            {
                return new Command(async () =>
                {
                    IsAgvReady = false;
                    await _agvService.GoToGoalAsync(PutCupsGoal, false);
                    IsAgvReady = true;
                });
            }
        }

        public Command GoToIntermediateGoal
        {
            get
            {
                return new Command(async () =>
                {
                    IsAgvReady = false;
                    await _agvService.GoToGoalAsync(IntermediatePutCupsGoal, false);
                    IsAgvReady = true;
                });
            }
        }

        public Command GoToCustomGoal
        {
            get
            {
                return new Command(async () =>
                {
                    IsAgvReady = false;
                    await _agvService.GoToGoalAsync(CustomGoal, false);
                    IsAgvReady = true;
                });
            }
        }

        public Command PatrolRoute
        {
            get
            {
                return new Command(async () =>
                {
                    IsAgvReady = false;
                    if (!await _agvService.PatrolRouteAsync(RouteToPatrol, PatrolRouteForever)) 
                        await CoreMethods.DisplayAlert("Сообщение", "Тележка не приедет", "ОК");
                    else await CoreMethods.DisplayAlert("Сообщение", "Приехали", "ОК");
                    IsAgvReady = true;
                });
            }
        }

        public Command Say
        {
            get
            {
                return new Command(async () => { await _agvService.SayAsync(TextToSay); });
            }
        }

        public Command Update
        {
            get
            {
                return new Command(async () => { await GetData(); });
            }
        }

        public Command Reset
        {
            get
            {
                return new Command(async () =>
                {
                    await _agvService.ResetAsync();
                });
            }
        }

        public Command CancelSpill
        {
            get
            {
                return new Command(() =>
                {
                    _barmanService.ResetPutCupsToAgv();
                });
            }
        }

        public ObservableCollection<string> Routes { get; set; }
        
        public ObservableCollection<string> Goals { get; set; }

        public string PutCupsGoal
        {
            get => _agvService.PutCupsGoal;
            set
            {
                if (_agvService.PutCupsGoal != value)
                {
                    _agvService.PutCupsGoal = value;
                    RaisePropertyChanged(nameof(PutCupsGoal));
                }
            }
        }

        public string IntermediatePutCupsGoal
        {
            get => _agvService.IntermediatePutCupsGoal;
            set
            {
                if (_agvService.IntermediatePutCupsGoal != value)
                {
                    _agvService.IntermediatePutCupsGoal = value;
                    RaisePropertyChanged(nameof(IntermediatePutCupsGoal));
                }
            }
        }

        private string _customGoal;
        public string CustomGoal
        {
            get => _customGoal;
            set
            {
                if (_customGoal != value)
                {
                    _customGoal = value;
                    RaisePropertyChanged(nameof(CustomGoal));
                }
            }
        }

        public string RouteToPatrol
        {
            get => _agvService.RouteToPatrol;
            set
            {
                if (_agvService.RouteToPatrol != value)
                {
                    _agvService.RouteToPatrol = value;
                    RaisePropertyChanged(nameof(RouteToPatrol));
                }
            }
        }

        public bool PatrolRouteForever 
        {  
            get => _agvService.PatrolRouteForever;
            set
            {
                if (_agvService.PatrolRouteForever != value)
                {
                    _agvService.PatrolRouteForever = value;
                    RaisePropertyChanged(nameof(PatrolRouteForever));
                }
            } 
        }

        private Position _baseCupPosition;

        public double X
        {
            get => _baseCupPosition.Point.X * 1000;
            set
            {
                if (_baseCupPosition.Point.X != value)
                {
                    _barmanService.BaseCupPosition.Point.X = value / 1000;
                    _barmanService.BaseCupPosition = _baseCupPosition;
                    RaisePropertyChanged(nameof(X));
                }
            }
        }

        public double Y
        {
            get => _baseCupPosition.Point.Y * 1000;
            set
            {
                if ( _baseCupPosition.Point.Y != value)
                {
                    _barmanService.BaseCupPosition.Point.Y = value / 1000;
                    _barmanService.BaseCupPosition = _baseCupPosition;
                    RaisePropertyChanged(nameof(Y));
                }
            }
        }

        public double Z
        {
            get =>_baseCupPosition.Point.Z * 1000;
            set
            {
                if (_baseCupPosition.Point.Z != value)
                {
                    _barmanService.BaseCupPosition.Point.Z = value / 1000;
                    _barmanService.BaseCupPosition = _baseCupPosition;
                    RaisePropertyChanged(nameof(Z));
                }
            }
        }

        public bool IsAgvConnected => _agvService.IsConnected;

        private bool _isAgvReady = false;

        public bool IsAgvReady
        {
            get => _isAgvReady;
            set
            {
                _isAgvReady = value; 
                RaisePropertyChanged(nameof(IsAgvReady));
            }
        }


        public bool IsRobotConnected => _robotService.IsRobotConnected;

        public bool IsRobotReady => !_robotService.IsRobotBusy && _robotService.IsRobotConnected;
        
        
        protected override void ViewIsAppearing(object sender, EventArgs e)
        {
            RaisePropertyChanged(nameof(IsRobotReady));
            base.ViewIsAppearing(sender, e);
        }

        public override void Init(object initData)
        {
            _baseCupPosition = _barmanService.BaseCupPosition;
            RaisePropertyChanged(nameof(IsAgvReady));

            base.Init(initData);
        }

        private async Task GetData()
        {
            if (await _agvService.RefreshGoalsAsync())
            {
                Goals = new ObservableCollection<string>(_agvService.Goals);
                RaisePropertyChanged(nameof(Goals));
            }

            if (await _agvService.RefreshRoutesAsync())
            {
                Routes = new ObservableCollection<string>(_agvService.Routes);
                RaisePropertyChanged(nameof(Routes));
            }
        }
    }
}
