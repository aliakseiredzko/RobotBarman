using System.Collections.Generic;
using RobotBarman.Models;
using RozumConnectionLib;

namespace RobotBarman.Services
{
    public interface IJsonDatabaseService
    {
        void SaveRobotData(RealRobot data);
        Agv GetAgvData();
        void SaveSoundInfo(List<Sound> sounds);
        void SaveBaseCupPosition(Position position);
        Position GetBaseCupPosition();
        List<Sound> GetSoundInfo();
        RealRobot GetRobotData();
        void SaveAgvData(Agv agv);
        List<DrinksPageItem> GetDrinks();
        Position GetPosition(string name);
        Pose GetPose(string name);
        Gripper GetGripper(string name);
        DrinksPageItem GetDrink(string name);
        void SetAvailableDrink(DrinksPageItem drinkToAdd, DrinkPosition position);
        List<DrinksPageItem> GetAvailableDrinks();
        Position GetTakeCupPosition();
        void SaveTakeCupPosition(Position position);
        void ResetTakeCupPosition();
    }
}