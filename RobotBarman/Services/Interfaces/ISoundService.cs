using System;
using System.Collections.Generic;
using System.Text;

namespace RobotBarman.Services.Interfaces
{
    public interface ISoundService
    {
        void PlayOpeningSound();
        void PlayBeforeSpillSound();
        void PlayAfterSpillSound();
        void PlayChangeBottleSound(DrinkPosition position);
        System.Threading.Tasks.Task InitDataAsync();
    }
}
