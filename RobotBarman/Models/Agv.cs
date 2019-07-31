using System;
using System.Collections.Generic;
using System.Text;

namespace RobotBarman
{
    public class Agv
    {
        public string Ip { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 7171;
        public string Password { get; set; } = "Omron4you";
    }
}
