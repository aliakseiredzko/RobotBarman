using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using FreshMvvm;
using RobotBarman.Services;
using RozumConnectionLib;
using Xamarin.Forms;

namespace RobotBarman
{
    public class BarPageModel : FreshBasePageModel
    {
        private IRobotService _robotService;
        private IBarmanService _barmanService;
        private IJsonDatabaseService _databaseService;
        private DrinksPageItem _firstSelectedDrink;
        private DrinksPageItem _secondSelectedDrink;
        private DrinksPageItem _thirdSelectedDrink;
        private Position _takeCupPosition;
        public ObservableCollection<DrinksPageItem> Drinks { get; set; }

        private bool _canTest = true;

        public bool CanTest
        {
            get => _canTest;
            set
            {
                _canTest = value;
                RaisePropertyChanged("CanTest");
            }
        }


        public DrinksPageItem FirstSelectedDrink
        {
            get => _firstSelectedDrink;
            set
            {
                if (_firstSelectedDrink.Title != value.Title)
                {
                    _firstSelectedDrink = value;
                    _databaseService.SetAvailableDrink(_firstSelectedDrink, DrinkPosition.First);
                    RaisePropertyChanged(nameof(FirstSelectedDrink));
                }
            }
        }

        public DrinksPageItem SecondSelectedDrink
        {
            get => _secondSelectedDrink;
            set
            {
                if (_secondSelectedDrink.Title != value.Title)
                {
                    _secondSelectedDrink = value;
                    _databaseService.SetAvailableDrink(_secondSelectedDrink, DrinkPosition.Second);
                    RaisePropertyChanged(nameof(SecondSelectedDrink));
                }
            }
        }

        public DrinksPageItem ThirdSelectedDrink
        {
            get =>_thirdSelectedDrink;
            set
            {
                if (_thirdSelectedDrink.Title != value.Title)
                {
                    _thirdSelectedDrink = value;
                    _databaseService.SetAvailableDrink(_thirdSelectedDrink, DrinkPosition.Third);
                    RaisePropertyChanged(nameof(ThirdSelectedDrink));
                }
            }
        }

        public double CupX
        {
            get => _takeCupPosition.Point.X * 1000;
            set
            {
                _takeCupPosition.Point.X = value / 1000;
            }
        }

        public double CupY
        {
            get => _takeCupPosition.Point.Y * 1000;
            set
            {
                _takeCupPosition.Point.Y = value / 1000;
            }
        }

        public double CupZ
        {
            get => _takeCupPosition.Point.Z * 1000;
            set
            {
                _takeCupPosition.Point.Z = value / 1000;
            }
        }

        public Command TestTakeCup
        {
            get
            {
                return new Command(async () =>
                {
                    CanTest = false;
                    await _barmanService.PutCupAsync();
                    await _robotService
                        .RunPositionsAsync(false, _robotService.Speed, "UpDropCupPosition");
                    await _robotService.GoToBasePosition();
                    await CoreMethods.DisplayAlert("", $"Готово", "OK");
                    CanTest = true;
                });
            }
        }

        public Command SetTakeCupPosition
        {
            get
            {
                return new Command(async () =>
                {
                    _databaseService.SaveTakeCupPosition(_takeCupPosition);
                    _takeCupPosition = _databaseService.GetTakeCupPosition().Clone();
                    RaisePropertyChanged("CupX");
                    RaisePropertyChanged("CupY");
                    RaisePropertyChanged("CupZ");

                    await CoreMethods.DisplayAlert("", $"Установлено", "OK");
                });
            }
        }

        public Command ResetTakeCupPosition
        {
            get
            {
                return new Command(async () =>
                {
                    _databaseService.ResetTakeCupPosition();
                    _takeCupPosition = _databaseService.GetTakeCupPosition().Clone();
                    RaisePropertyChanged("CupX");
                    RaisePropertyChanged("CupY");
                    RaisePropertyChanged("CupZ");
                    await CoreMethods.DisplayAlert("", $"Сброшено по умолчанию", "OK");
                });
            }
        }

        protected override void ViewIsAppearing(object sender, EventArgs e)
        {
            base.ViewIsAppearing(sender, e);
        }

        public override void Init(object initData)
        {
            _robotService = FreshIOC.Container.Resolve<IRobotService>();
            _databaseService = FreshIOC.Container.Resolve<IJsonDatabaseService>();
            _barmanService = FreshIOC.Container.Resolve<IBarmanService>();
            Drinks = new ObservableCollection<DrinksPageItem>(_databaseService.GetDrinks());

            _firstSelectedDrink = _databaseService.GetAvailableDrinks()[0];
            _secondSelectedDrink = _databaseService.GetAvailableDrinks()[1];
            _thirdSelectedDrink = _databaseService.GetAvailableDrinks()[2];
            _takeCupPosition = _databaseService.GetTakeCupPosition().Clone();
            base.Init(initData);
        }
    }
}