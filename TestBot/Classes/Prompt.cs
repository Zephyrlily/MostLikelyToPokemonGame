using System;
using System.Collections.Generic;
using System.Text;

namespace PokemonGame.Classes
{
    public class Prompt
    {
        public string promptText { get; set; }

        public int responsesNeeded { get; set; }
        public string url { get; set; }

        public Prompt(string promptText, int responsesNeeded) 
        {
            this.promptText = promptText;
            this.responsesNeeded = responsesNeeded;
        }

        public Prompt(string promptData)
        {
            string[] data = promptData.Split("|");
            promptText = data[0];
            responsesNeeded = int.Parse(data[1]);
        }


    }
}
