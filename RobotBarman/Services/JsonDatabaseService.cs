using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RobotBarman.Models;
using RozumConnectionLib;
using Xamarin.Forms.Internals;

namespace RobotBarman.Services
{
    public class JsonDatabaseService : IJsonDatabaseService
    {
        public JsonDatabaseService()
        {

        }

        public async Task InitDataAsync()
        {
            if (Positions == null)
                Positions = await Deserialize<Dictionary<string, Position>>("Data.positions.json");

            if (Poses == null)
                Poses = await Deserialize<Dictionary<string, Pose>>("Data.poses.json");

            if (Tools == null)
                Tools = await Deserialize<Dictionary<string, Gripper>>("Data.tools.json");

            if (Drinks == null)
                Drinks = await Deserialize<List<DrinksPageItem>>("Data.drinks.json");

            var availableDrinks = await LocalDataHandler.GetTextDataAsync("availableDrinks.json");
            if (availableDrinks == "")
                AvailableDrinks = await Deserialize<List<DrinksPageItem>>("Data.availableDrinks.json");
        }

        private async Task<T> Deserialize<T>(string path)
        {
            var text = await LocalDataHandler.GetTextFromAssemblyAsync(path);
            return await Task.FromResult(JsonConvert.DeserializeObject<T>(text));
        }

        private List<DrinksPageItem> Drinks { get; set; }
        private List<DrinksPageItem> AvailableDrinks { get; set; }
        private Dictionary<string, Position> Positions { get; set; }
        private Dictionary<string, Pose> Poses { get; set; }
        private Dictionary<string, Gripper> Tools { get; set; }

        public async Task SaveRobotDataAsync(RealRobot data)
        {
            await LocalDataHandler.SaveTextDataAsync("robot.json",
                JsonConvert.SerializeObject(data, Formatting.Indented));
        }

        public async Task<Agv> GetAgvDataAsync()
        {
            var textData = await LocalDataHandler.GetTextDataAsync("agv.json");
            return string.IsNullOrEmpty(textData)
                ? new Agv()
                : JsonConvert.DeserializeObject<Agv>(textData);
        }

        public async Task SaveSoundInfoAsync(List<Sound> sounds)
        {
            await Serialize(sounds, "sounds.json");
        }

        public async Task SaveBaseCupPositionAsync(Position position)
        {
            await Serialize(position, "baseCupPosition.json");
        }

        private static async Task Serialize(object data, string fileName)
        {
            await LocalDataHandler.SaveTextDataAsync(fileName,
                  JsonConvert.SerializeObject(data, Formatting.Indented));
        }

        public async Task<Position> GetBaseCupPositionAsync()
        {
            var textData = await LocalDataHandler.GetTextDataAsync("baseCupPosition.json");

            return string.IsNullOrEmpty(textData)
                ? Positions["Base2Position"]
                : JsonConvert.DeserializeObject<Position>(textData);
        }

        public async Task<List<Sound>> GetSoundInfoAsync()
        {
            var textData = await LocalDataHandler.GetTextDataAsync("sounds.json");
            if (string.IsNullOrEmpty(textData))
            {
                var sounds = GetSounds();
                //SaveSoundInfo(sounds);
                return sounds;
            }

            return JsonConvert.DeserializeObject<List<Sound>>(textData);
        }

        private List<Sound> GetSounds()
        {
            var sounds = new List<Sound>();
            var assembly = typeof(App).GetTypeInfo().Assembly;
            var soundsList = assembly.GetManifestResourceNames().Where(x => x.Contains("Sounds")).ToList();

            foreach (var soundPath in soundsList)
            {
                sounds.Add(new Sound
                {
                    Path = soundPath
                });
            }
            return sounds;
        }

        public async Task<RealRobot> GetRobotDataAsync()
        {
            var textData = await LocalDataHandler.GetTextDataAsync("robot.json");
            return string.IsNullOrEmpty(textData)
                ? new RealRobot()
                : JsonConvert.DeserializeObject<RealRobot>(textData);
        }

        public async Task SaveAgvDataAsync(Agv agv)
        {
            await LocalDataHandler.SaveTextDataAsync("agv.json",
                JsonConvert.SerializeObject(agv, Formatting.Indented));
        }

        public async Task<List<DrinksPageItem>> GetDrinksAsync()
        {
            return await Task.FromResult(Drinks);
        }

        public async Task<Position> GetPositionAsync(string name)
        {
            return await Task.FromResult(Positions[name]);
        }

        public async Task<Pose> GetPoseAsync(string name)
        {
            return await Task.FromResult(Poses[name]);
        }

        public async Task<Gripper> GetGripperAsync(string name)
        {
            return await Task.FromResult(Tools[name]);
        }

        public async Task<DrinksPageItem> GetDrinkAsync(string name)
        {
            return await Task.FromResult(Drinks.Find(x => x.Title == name));
        }

        public async Task SetAvailableDrinkAsync(DrinksPageItem drinkToAdd, DrinkPosition position)
        {
            if (drinkToAdd.Clone() is DrinksPageItem newDrink)
            {
                newDrink.DrinkPosition = position;
                AvailableDrinks.RemoveAt((int)position);
                AvailableDrinks.Insert((int)newDrink.DrinkPosition, newDrink);
            }

            await LocalDataHandler.SaveTextDataAsync("availableDrinks.json",
                JsonConvert.SerializeObject(AvailableDrinks, Formatting.Indented));
        }

        public async Task<List<DrinksPageItem>> GetAvailableDrinksAsync()
        {
            return await Task.FromResult(AvailableDrinks);
        }
    }

    public static class PositionExtension
    {
        public static Position Clone(this Position position)
        {
            string serialized = JsonConvert.SerializeObject(position);
            return JsonConvert.DeserializeObject<Position>(serialized);
        }
    }
}