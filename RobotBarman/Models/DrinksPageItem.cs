using System;
using System.Reflection;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace RobotBarman
{
    public class DrinksPageItem: ICloneable
    {
        public string Title { get; set; }
        public string IconSource { get; set; }

        [JsonIgnore]
        public ImageSource ImageSource =>
            ImageSource.FromResource(IconSource, typeof(DrinksPageItem).GetTypeInfo().Assembly);

        public DrinkPosition DrinkPosition { get; set; }        
        public int SpillsLeft { get; set; }

        public DrinkType DrinkType { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public override string ToString()
        {
            return Title;
        }
    }

    public enum DrinkType
    {
        First,
        Second,
        Third,
        Fourth
    }

    public enum DrinkPosition
    {
        First,
        Second,
        Third,
        NotAvailable
    }
}