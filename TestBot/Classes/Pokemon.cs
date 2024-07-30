using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PokemonGame.Classes
{
    public class Pokemon
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string SpeciesName { get; set; }
        public bool Shiny { get; set; } = false;
        public string ImageURL { get; set; }

        public Pokemon(string data)
        {
            string[] dataParts = data.Split("|");
            ID = int.Parse(dataParts[0]);
            Name = dataParts[1];
            SpeciesName = dataParts[2];
            if (new Random().Next(0, 100) == 0)
            {
                Shiny = true;
                ImageURL = dataParts[7];
            }
            else
            {
                ImageURL = dataParts[6];
            }
        }

        public string GetCardURL()
        {
            // Web fetch version
            /*if (!Shiny)
            {
                return TextData.gitHubImageUrl + SpeciesName + ".png";
            }
            else
            {
                return TextData.gitHubImageUrl + SpeciesName + "-shiny.png";
            }*/

            string basePath = TextData.localImageUrl; // Change this to your local path

            if (!Shiny)
            {
                return Path.Combine(basePath, Name + ".png");
            }
            else
            {
                return Path.Combine(basePath, Name + "-shiny.png");
            }

        }
    }
}