using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
            Positions = GetPositions();
            
            Poses = JsonConvert.DeserializeObject<Dictionary<string, Pose>>(
                LocalDataHandler.GetTextFromAssembly("Data.poses.json"));

            Tools = JsonConvert.DeserializeObject<Dictionary<string, Gripper>>(
                LocalDataHandler.GetTextFromAssembly("Data.tools.json"));

            Drinks = JsonConvert.DeserializeObject<List<DrinksPageItem>>(
                LocalDataHandler.GetTextFromAssembly("Data.drinks.json"));

            var availableDrinks = "";
            if ((availableDrinks = LocalDataHandler.GetTextData("availableDrinks.json")) == "")
                availableDrinks = LocalDataHandler.GetTextFromAssembly("Data.availableDrinks.json");
            AvailableDrinks = JsonConvert.DeserializeObject<List<DrinksPageItem>>(availableDrinks);                                
        }

        private List<DrinksPageItem> Drinks { get; }
        private List<DrinksPageItem> AvailableDrinks { get; }
        private Dictionary<string, Position> Positions { get; }        
        private Dictionary<string, Pose> Poses { get; }
        private Dictionary<string, Gripper> Tools { get; }

        public void SaveRobotData(RealRobot data)
        {
            LocalDataHandler.SaveTextData("robot.json", JsonConvert.SerializeObject(data, Formatting.Indented));
        }

        public Agv GetAgvData()
        {
            var textData = LocalDataHandler.GetTextData("agv.json");
            return string.IsNullOrEmpty(textData)
                ? new Agv()
                : JsonConvert.DeserializeObject<Agv>(textData);
        }    

        public void SaveSoundInfo(List<Sound> sounds)
        {
            LocalDataHandler.SaveTextData("sounds.json", JsonConvert.SerializeObject(sounds, Formatting.Indented));
        }

        public void SaveBaseCupPosition(Position position)
        {
            LocalDataHandler.SaveTextData("baseCupPosition.json", JsonConvert.SerializeObject(position, Formatting.Indented));
        }

        public Dictionary<string, Position> GetPositions()
        {
            var textData = LocalDataHandler.GetTextData("positions.json");
            if (string.IsNullOrEmpty(textData))
            {
                textData = LocalDataHandler.GetTextFromAssembly("Data.positions.json");
            }                  

            return JsonConvert.DeserializeObject<Dictionary<string, Position>>(textData);
        }

        public Position GetBaseCupPosition()
        {
            var textData = LocalDataHandler.GetTextData("baseCupPosition.json");
            return string.IsNullOrEmpty(textData)
                ? Positions["Base2Position"]
                : JsonConvert.DeserializeObject<Position>(textData);
        }


        public void ResetTakeCupPosition()
        {
            Positions["CupPosition"] = Positions["DefaultCupPosition"].Clone();
            Positions["DownCupPosition"] = Positions["DefaultDownCupPosition"].Clone();
            SavePositions();
        }

        public Position GetTakeCupPosition()
        {
            return Positions["CupPosition"];
        }
        
        public void SaveTakeCupPosition(Position position)
        {
            var cloned = position.Clone();
            Positions["CupPosition"] = cloned;
            Positions["DownCupPosition"].Point.X = cloned.Point.X;
            Positions["DownCupPosition"].Point.Y = cloned.Point.Y;
            SavePositions();
        }

        private void SavePositions()
        {
            LocalDataHandler.SaveTextData("positions.json", JsonConvert.SerializeObject(Positions, Formatting.Indented));
        }

        public List<Sound> GetSoundInfo()
        {
            var textData = LocalDataHandler.GetTextData("sounds.json");
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

        public RealRobot GetRobotData()
        {
            var textData = LocalDataHandler.GetTextData("robot.json");
            return string.IsNullOrEmpty(textData)
                ? new RealRobot()
                : JsonConvert.DeserializeObject<RealRobot>(textData);
        }

        public void SaveAgvData(Agv agv)
        {
            LocalDataHandler.SaveTextData("agv.json", JsonConvert.SerializeObject(agv, Formatting.Indented));
        }

        public List<DrinksPageItem> GetDrinks()
        {
            return Drinks;
        }

        public Position GetPosition(string name)
        {
            return Positions[name];
        }

        public Pose GetPose(string name)
        {
            return Poses[name];
        }

        public Gripper GetGripper(string name)
        {
            return Tools[name];
        }

        public DrinksPageItem GetDrink(string name)
        {
            return Drinks.Find(x => x.Title == name);
        }

        public void SetAvailableDrink(DrinksPageItem drinkToAdd, DrinkPosition position)
        {
            if (drinkToAdd.Clone() is DrinksPageItem newDrink)
            {
                newDrink.DrinkPosition = position;                
                AvailableDrinks.RemoveAt((int)position);
                AvailableDrinks.Insert((int) newDrink.DrinkPosition, newDrink);
            }

            LocalDataHandler.SaveTextData("availableDrinks.json",
                JsonConvert.SerializeObject(AvailableDrinks, Formatting.Indented));
        }

        public List<DrinksPageItem> GetAvailableDrinks()
        {
            return AvailableDrinks;
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