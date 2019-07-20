using FreshMvvm;
using RozumConnectionLib;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace RobotBarman.Services
{
    public class BarmanService : IBarmanService
    {
        private readonly IRobotService _robotService;
        private readonly IJsonDatabaseService _databaseService;
        private Position _baseCupPosition;

        private CancellationTokenSource _bottleTokenSource;
        private CancellationTokenSource _agvTokenSource;

        public BarmanService()
        {
            _robotService = FreshIOC.Container.Resolve<IRobotService>();
            _databaseService = FreshIOC.Container.Resolve<IJsonDatabaseService>();                      
            _bottleTokenSource = new CancellationTokenSource();
            _agvTokenSource = new CancellationTokenSource();

            Speed = 60;
            SelectedBottle = new DrinksPageItem
            {
                DrinkPosition = DrinkPosition.First
            };

            BaseCupPosition = _databaseService.GetBaseCupPosition();
        }

        public int Speed { get; set; }
        public DrinksPageItem SelectedBottle { get; set; }
        public bool IsSpillRunning { get; set; }
        public bool IsCanceledByTimer { get; set; }
        public Position BaseCupPosition
        {
            get => _baseCupPosition;
            set
            {
                if (_baseCupPosition != value)
                {
                    _baseCupPosition = value;
                    _databaseService.SaveBaseCupPosition(value);
                }
            }
        }

        public void ResetSpill()
        {
            _bottleTokenSource.Cancel();
            
        }

        public void ResetPutCupsToAgv()
        {
            _agvTokenSource.Cancel();
            IsSpillRunning = false;
        }

        public async Task SpillAsync()
        {
            IsSpillRunning = true;

            if (_robotService.IsRobotConnected)
            {                    
                if (!_bottleTokenSource.IsCancellationRequested)
                    await _robotService.WaitForInputSignalAsync(1, false);
                else
                {
                    _bottleTokenSource = new CancellationTokenSource();
                    return;
                }

                if(!_bottleTokenSource.IsCancellationRequested)
                    await PutCupAsync();
                else
                {
                    _bottleTokenSource = new CancellationTokenSource();
                    return;
                }
                
                if (!_bottleTokenSource.IsCancellationRequested)
                    await TakeBottle();
                else
                {
                    _bottleTokenSource = new CancellationTokenSource();
                    return;
                }

                if (!_bottleTokenSource.IsCancellationRequested)
                    await Spill();
                else
                {
                    _bottleTokenSource = new CancellationTokenSource();
                    return;
                }

                if (!_bottleTokenSource.IsCancellationRequested)
                    await PutBottle();
                else
                {
                    _bottleTokenSource = new CancellationTokenSource();
                    return;
                }              
            }
            
            IsSpillRunning = false;
        }

        public async Task PutCupAsync()
        {
            await _robotService
                .RunPositionsAsync(true, "Base1Position", "DownCupPosition", "CupPosition");
            await _robotService.CloseGripperAsync(1500);
            await _robotService.RunPositionsAsync(false, "DownCupPosition");
            await _robotService
                .RunPositionsAsync(false, "DropCupPosition");
            await _robotService.OpenGripperAsync(2000);           
        }

        public async Task PutCupsToAgv()
        {
            var xOffset = 0.12;
            var yOffset = 0.1;

            if (_agvTokenSource.IsCancellationRequested)
            {
                _agvTokenSource = new CancellationTokenSource();
                return;
            }
            
            for (int i = 0; i < 2; i++)
            {
                var preUpPosition = _databaseService.GetPosition("Base2PreUpPosition").Clone();
                var upPosition = _databaseService.GetPosition("Base2UpPosition").Clone();
                var downPosition = BaseCupPosition.Clone();

                preUpPosition.Point.Y -= i * yOffset;
                upPosition.Point.Y -= i * yOffset;
                downPosition.Point.Y -= i * yOffset;                

                for (int j = 0; j < 3; j++)
                {
                    if (_agvTokenSource.IsCancellationRequested)
                    {
                        _agvTokenSource = new CancellationTokenSource();
                        return;
                    }   

                    SelectedBottle = new DrinksPageItem {DrinkPosition = (DrinkPosition) j};

                    await SpillAsync();

                    await _robotService.RunPositionsAsync(true, "ReverseUpDropCupPosition", "ReverseDropCupPosition");

                    await _robotService.CloseGripperAsync(1500);

                    preUpPosition.Point.X -= xOffset;
                    upPosition.Point.X -= xOffset;
                    downPosition.Point.X -= xOffset;

                    await _robotService
                        .RunPositionsAsync(false, "ReverseUpDropCupPosition", "GoToAgvPosition");

                    await _robotService.RunPositionsAsync(false, preUpPosition, upPosition, downPosition);

                    await _robotService.OpenGripperAsync(2000);

                    await _robotService
                        .RunPositionsAsync(true, upPosition, preUpPosition);

                    await _robotService
                        .RunPositionsAsync(true, "GoToAgvPosition", "ReverseUpDropCupPosition",
                            "PreBase1UpPosition", "Base1UpPosition");
                }
            }
        }

        private async Task TakeBottle()
        {
            await _robotService.OpenGripperAsync();
            switch (SelectedBottle.DrinkPosition)
            {
                case DrinkPosition.First:
                    await _robotService.RunPositionsAsync(true, "UpDropCupPosition",
                        "Base1UpPosition", "PreUpFirstDrink", "UpFirstDrink", "FirstDrink");
                    await _robotService.CloseGripperAsync(1500);
                    await _robotService.RunPositionsAsync(false, "UpFirstDrink");
                    break;
                case DrinkPosition.Second:
                    await _robotService.RunPositionsAsync(true, "UpDropCupPosition",
                        "Base1UpPosition", "PreUpSecondDrink", "UpSecondDrink", "SecondDrink");
                    await _robotService.CloseGripperAsync(1500);
                    await _robotService.RunPositionsAsync(false, "UpSecondDrink");
                    break;
                case DrinkPosition.Third:
                    await _robotService.RunPositionsAsync(true, "UpDropCupPosition",
                        "Base1UpPosition", "PreUpThirdDrink", "UpThirdDrink", "ThirdDrink");
                    await _robotService.CloseGripperAsync(1500);
                    await _robotService.RunPositionsAsync(false, "UpThirdDrink");
                    break;
            }
        }

        private async Task PutBottle()
        {            
            switch (SelectedBottle.DrinkPosition)
            {
                case DrinkPosition.First:
                    await _robotService.RunPositionsAsync(true, "SpillPosition",
                        "Base1UpPosition", "PreUpFirstDrink", "UpFirstDrink");
                    await _robotService.RunPositionsAsync(false, "FirstDrink");
                    await _robotService.OpenGripperAsync(2000);
                    await _robotService.RunPositionsAsync(true, "UpFirstDrink", "PreUpFirstDrink", "Base1UpPosition");
                    break;
                case DrinkPosition.Second:
                    await _robotService.RunPositionsAsync(true, "SpillPosition",
                        "Base1UpPosition", "PreUpSecondDrink", "UpSecondDrink");
                    await _robotService.RunPositionsAsync(false, "SecondDrink");
                    await _robotService.OpenGripperAsync(2000);
                    await _robotService.RunPositionsAsync(true, "UpSecondDrink", "PreUpSecondDrink", "Base1UpPosition");
                    break;
                case DrinkPosition.Third:
                    await _robotService.RunPositionsAsync(true, "SpillPosition",
                        "PreUpThirdDrink", "UpThirdDrink");
                    await _robotService.RunPositionsAsync(false, "ThirdDrink");
                    await _robotService.OpenGripperAsync(2000);
                    await _robotService.RunPositionsAsync(true, "UpThirdDrink", "PreUpThirdDrink", "Base1UpPosition");
                    break;
            }          
        }

        private async Task Spill()
        {            
            switch (SelectedBottle.DrinkPosition)
            {
                case DrinkPosition.First:
                    await _robotService.RunPositionsAsync(true,
                        "PreUpFirstDrink", "Base1UpPosition", "SpillPosition");
                    break;
                case DrinkPosition.Second:
                    await _robotService.RunPositionsAsync(true,
                        "PreUpSecondDrink", "Base1UpPosition", "SpillPosition");
                    break;
                case DrinkPosition.Third:
                    await _robotService.RunPositionsAsync(true,
                        "PreUpThirdDrink", "Base1UpPosition", "SpillPosition");
                    break;
            }

            await _robotService.RunPositionsAsync(false,
                "SpillDownDrinkPosition");

            if (SelectedBottle.DrinkType == DrinkType.Third)
            {
                await _robotService.SetToolAsync("SpillThirdDrinkTool");
                await _robotService.RunPositionsAsync(false, "SpillThirdDrinkPosition");
            }
            else
            {
                await _robotService.SetToolAsync("SpillDrinkTool");
                await _robotService.RunPositionsAsync(false, "SpillDrinkPosition");
            }

            var tokenSource = new CancellationTokenSource();

            await Task.WhenAny(SpillAsync(tokenSource, 5, SelectedBottle.SpillsLeft),
                WaitForLiquid(tokenSource));

            Debug.WriteLine($"{SelectedBottle.Title}: {SelectedBottle.SpillsLeft}");

            await _robotService.Robot.SetModeAsync(RobotMode.Freeze);

            await _robotService.SetToolAsync("default");
            await _robotService.RunPositionsAsync(false, "PreSpillDrinkPosition");
        }

        private async Task SpillAsync(CancellationTokenSource tokenSource, int secondsToWait, int cycle = 1)
        {          
            await _robotService.RunPositionAsync(false, 1, 1f, "AfterSpillDrinkPosition");

            await Task.Delay(TimeSpan.FromSeconds(secondsToWait));
            if (!tokenSource.IsCancellationRequested)
            {
                tokenSource.Cancel();
                Debug.WriteLine("Task cancelled from SpillAsync");
                IsCanceledByTimer = true;
            }
        }

        private async Task WaitForLiquid(CancellationTokenSource tokenSource)
        {
            var signal = false;
            while (!tokenSource.Token.IsCancellationRequested && !signal)
            {
                signal = await _robotService.GetInputSignalAsync(1);
                await Task.Delay(50);
            }

            if (!tokenSource.IsCancellationRequested)
            {
                tokenSource.Cancel();
                Debug.WriteLine("Task cancelled from WaitForLiquid");
                IsCanceledByTimer = false;
            }
        }
    }
}