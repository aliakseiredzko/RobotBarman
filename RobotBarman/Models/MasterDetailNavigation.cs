using System;
using System.Collections.Generic;
using FreshMvvm;
using Xamarin.Forms;

namespace RobotBarman.Models
{
    public class MasterDetailNavigation : FreshMasterDetailNavigationContainer
    {
        private readonly List<MenuItem> pageIcons = new List<MenuItem>();

        public void Initialize(string menuTitle, string menuIcon = null)
        {
            MasterBehavior = MasterBehavior.Popover;
            CreateMenuPage(menuTitle, menuIcon);
            RegisterNavigation();
        }

        public void AddPageWithIcon<T>(string title, string icon = "", object data = null) where T : FreshBasePageModel
        {
            base.AddPage<T>(title, data);
            pageIcons.Add(new MenuItem
            {
                Title = title,
                IconSource = icon
            });
        }

        protected override void CreateMenuPage(string menuPageTitle, string menuIcon = null)
        {
            var listview = new ListView();
            var menuPage = new ContentPage();
            menuPage.Title = menuPageTitle;
            //_menuPage.BackgroundColor = Color.FromHex("#c8c8c8");

            listview.ItemsSource = pageIcons;

            var cell = new DataTemplate(typeof(ImageCell));
            cell.SetValue(TextCell.TextColorProperty, Color.Black);
            cell.SetBinding(TextCell.TextProperty, "Title");
            cell.SetBinding(ImageCell.ImageSourceProperty, "IconSource");


            listview.ItemTemplate = cell;
            listview.ItemSelected += (sender, args) =>
            {
                if (Pages.ContainsKey(((MenuItem) args.SelectedItem).Title))
                    Detail = Pages[((MenuItem) args.SelectedItem).Title];
                IsPresented = false;
            };

            menuPage.Content = listview;

            var navPage = new NavigationPage(menuPage) {Title = "Menu"};

            if (!string.IsNullOrEmpty(menuIcon))
                navPage.Icon = menuIcon;

            Master = navPage;
        }

        protected override Page CreateContainerPage(Page page)
        {
            var navigation = new NavigationPage(page);
            //navigation.BarTextColor = Color.White;

            return navigation;
        }
    }

    public class MenuItem
    {
        public string Title { get; set; }
        public string IconSource { get; set; }
    }
}