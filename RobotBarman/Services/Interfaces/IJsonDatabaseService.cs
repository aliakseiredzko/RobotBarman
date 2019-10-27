using System.Collections.Generic;
using System.Threading.Tasks;
using RobotBarman.Models;
using RozumConnectionLib;

namespace RobotBarman.Services
{
    public interface IJsonDatabaseService
    {
        Task<Agv> GetAgvDataAsync();
        Task<List<DrinksPageItem>> GetAvailableDrinksAsync();
        Task<Position> GetBaseCupPositionAsync();
        Task<DrinksPageItem> GetDrinkAsync(string name);
        Task<List<DrinksPageItem>> GetDrinksAsync();
        Task<Gripper> GetGripperAsync(string name);
        Task<Pose> GetPoseAsync(string name);
        Task<Position> GetPositionAsync(string name);
        Task<RealRobot> GetRobotDataAsync();
        Task<List<Sound>> GetSoundInfoAsync();
        Task InitDataAsync();
        Task SaveAgvDataAsync(Agv agv);
        Task SaveBaseCupPositionAsync(Position position);
        Task SaveRobotDataAsync(RealRobot data);
        Task SaveSoundInfoAsync(List<Sound> sounds);
        Task SetAvailableDrinkAsync(DrinksPageItem drinkToAdd, DrinkPosition position);
    }
}