using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using FreshMvvm;

using Plugin.SimpleAudioPlayer;
using RobotBarman.Models;
using RobotBarman.Services;
using RobotBarman.Services.Interfaces;
using Xamarin.Forms;

namespace RobotBarman
{
    public class FirstPageModel : FreshBasePageModel
    {
        private ISoundService _soundService;
        public Command OpenDrinksPage
        {
            get
            {
                return new Command(async () =>
                {                      
                    _soundService.PlayOpeningSound();
                    await CoreMethods.PushPageModel<DrinksPageModel>();
                });
            }
        }

        public override void Init(object initData)
        {                    
            _soundService = FreshIOC.Container.Resolve<ISoundService>();
            base.Init(initData);            
        }
    }
}