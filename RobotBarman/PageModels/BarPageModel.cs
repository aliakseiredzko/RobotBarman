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

        protected override void ViewIsAppearing(object sender, EventArgs e)
        {
            base.ViewIsAppearing(sender, e);
        }

        public override void Init(object initData)
        {
            _databaseService = FreshIOC.Container.Resolve<IJsonDatabaseService>();
            Drinks = new ObservableCollection<DrinksPageItem>(_databaseService.GetDrinks());

            _firstSelectedDrink = _databaseService.GetAvailableDrinks()[0];
            _secondSelectedDrink = _databaseService.GetAvailableDrinks()[1];
            _thirdSelectedDrink = _databaseService.GetAvailableDrinks()[2];

            base.Init(initData);
        }
    }
}