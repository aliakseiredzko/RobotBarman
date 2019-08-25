using FreshMvvm;
using RobotBarman.Models;
using RobotBarman.Services;
using RobotBarman.Services.Interfaces;
using System.Diagnostics;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]

namespace RobotBarman
{
    public class App : Application
    {
        public App()
        {
            FreshIOC.Container.Register<IAgvService, AgvService>();
            FreshIOC.Container.Register<IJsonDatabaseService, JsonDatabaseService>();
            FreshIOC.Container.Register<ISoundService, SoundService>();
            FreshIOC.Container.Register<IRobotService, RobotService>();
            FreshIOC.Container.Register<IBarmanService, BarmanService>();                        

            var masterDetailNav = new MasterDetailNavigation();
            masterDetailNav.Initialize("Menu", "hamburger.png");
            //masterDetailNav.AddPageWithIcon<FirstPageModel>("ГЛАВНАЯ", "star.png");
            masterDetailNav.AddPageWithIcon<DrinksPageModel>("БАРМЕН", "cup.png");
            masterDetailNav.AddPageWithIcon<BarPageModel>("НАСТРОЙКА БАРА", "tree_cup.png");
            masterDetailNav.AddPageWithIcon<SettingsPageModel>("НАСТРОЙКА РОБОТА", "robot_industrial.png");
            masterDetailNav.AddPageWithIcon<AgvPageModel>("НАСТРОЙКА ТЕЛЕЖКИ", "tractor.png");
                       
            MainPage = masterDetailNav;
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}