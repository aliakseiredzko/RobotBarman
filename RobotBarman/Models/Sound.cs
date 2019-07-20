using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using RobotBarman.Services;

namespace RobotBarman.Models
{
    public class Sound
    {
        public string Path { get; set; }
        public Stream SoundStream => LocalDataHandler.GetSoundStream(Path);       
    }    
}
