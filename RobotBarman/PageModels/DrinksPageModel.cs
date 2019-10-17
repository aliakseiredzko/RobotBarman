using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using FreshMvvm;
using RobotBarman.Services;
using RobotBarman.Services.Interfaces;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace RobotBarman
{
    internal class DrinksPageModel : FreshBasePageModel
    {
        private IBarmanService _barmanService;
        private IJsonDatabaseService _databaseService;
        private IRobotService _robotService;
        private ISoundService _soundService;

        private DrinksPageItem _selectedDrink;

        public DrinksPageModel()
        {
            AvailableDrinks = new ObservableCollection<DrinksPageItem>();
        }

        private int _numberOfCups = 1;
        public int NumberOfCups
        { 
            get => _numberOfCups;
            set
            {
                if (_numberOfCups != value)
                {
                    _numberOfCups = value;
                    RaisePropertyChanged(nameof(NumberOfCups));
                }
            }
        }

        public Command SpillDrink
        {
            get
            {
                return new Command(async () =>
                {
                    if (SelectedDrink == null)
                    {
                        await CoreMethods.DisplayAlert("Опаньки", "Сначала выбираем напиток, затем наливаем!",
                            "Точно!");
                    }
                    else
                    {
                        _barmanService.SelectedBottle = SelectedDrink;
                        _soundService.PlayBeforeSpillSound();

                        RaisePropertyChanged(nameof(IsRobotAvailable));

                        while (NumberOfCups >= 1)
                        {
                            await _barmanService.SpillAsync();
                            if (_barmanService.IsCanceledByTimer)
                            {
                                await CoreMethods.DisplayAlert("",
                                    $"Похоже, что йогурт закончился. Пожалуйста, замените бутылку № {(int)SelectedDrink.DrinkPosition+1}!",
                                    "Будет!");
                            }

                            NumberOfCups -= 1;
                        }

                        NumberOfCups = 1;

                        RaisePropertyChanged(nameof(IsRobotAvailable));

                        _soundService.PlayAfterSpillSound();                        
                        SelectedDrink = null;
                    }
                });
            }
        }

        public bool IsRobotAvailable => _robotService.IsRobotConnected
                                        && !_barmanService.IsSpillRunning;

        public ObservableCollection<DrinksPageItem> AvailableDrinks { get; set; }

        public DrinksPageItem SelectedDrink
        {
            get => _selectedDrink;
            set
            {
                _selectedDrink = value;
                RaisePropertyChanged(nameof(SelectedDrink));
            }
        }
       
        protected override void ViewIsAppearing(object sender, EventArgs e)
        {
            AvailableDrinks = new ObservableCollection<DrinksPageItem>(_databaseService.GetAvailableDrinks());
            RaisePropertyChanged(nameof(AvailableDrinks));
            RaisePropertyChanged(nameof(IsRobotAvailable));
            base.ViewIsAppearing(sender, e);
        }

        public override void Init(object initData)
        {
            _soundService = FreshIOC.Container.Resolve<ISoundService>();
            _barmanService = FreshIOC.Container.Resolve<IBarmanService>();
            _robotService = FreshIOC.Container.Resolve<IRobotService>();
            _databaseService = FreshIOC.Container.Resolve<IJsonDatabaseService>();
            
            AvailableDrinks = new ObservableCollection<DrinksPageItem>();
        }
    }
}