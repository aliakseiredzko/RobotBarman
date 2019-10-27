using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using FreshMvvm;
using RobotBarman.Services;

namespace RobotBarman
{
    public class BarPageModel : FreshBasePageModel
    {
        private IJsonDatabaseService _databaseService;
        private DrinksPageItem _firstSelectedDrink;
        private DrinksPageItem _secondSelectedDrink;
        private DrinksPageItem _thirdSelectedDrink;
        public ObservableCollection<DrinksPageItem> Drinks { get; set; }

        public DrinksPageItem FirstSelectedDrink
        {
            get => _firstSelectedDrink;
            set
            {
                if (_firstSelectedDrink.Title != value.Title)
                {
                    _firstSelectedDrink = value;
                    _databaseService.SetAvailableDrinkAsync(_firstSelectedDrink, DrinkPosition.First);
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
                    _databaseService.SetAvailableDrinkAsync(_secondSelectedDrink, DrinkPosition.Second);
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
                    _databaseService.SetAvailableDrinkAsync(_thirdSelectedDrink, DrinkPosition.Third);
                    RaisePropertyChanged(nameof(ThirdSelectedDrink));
                }
            }
        }

        protected override void ViewIsAppearing(object sender, EventArgs e)
        {
            base.ViewIsAppearing(sender, e);
        }

        public override async void Init(object initData)
        {
            _databaseService = FreshIOC.Container.Resolve<IJsonDatabaseService>();
            await _databaseService.InitDataAsync();
            Drinks = new ObservableCollection<DrinksPageItem>(await _databaseService.GetDrinksAsync());

            _firstSelectedDrink = (await _databaseService.GetAvailableDrinksAsync())[0];
            _secondSelectedDrink = (await _databaseService.GetAvailableDrinksAsync())[1];
            _thirdSelectedDrink = (await _databaseService.GetAvailableDrinksAsync())[2];

            base.Init(initData);
        }
    }
}