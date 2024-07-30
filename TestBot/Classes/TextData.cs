using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGame.Classes
{
    public static class TextData
    {
        private static readonly HttpClient client = new HttpClient();
        public static string gitHubTextUrl = "https://raw.githubusercontent.com/Zephyrlily/MostLikelyToPokeImages/main/lists/";
        public static string gitHubImageUrl = "https://raw.githubusercontent.com/Zephyrlily/MostLikelyToPokeImages/main/images/";
        public static string localImageUrl = "C:\\Users\\vapor\\Documents\\MostLikelyToPokeImages\\images\\";
        public static string localFontUrl = "C:\\Users\\vapor\\Documents\\MostLikelyToPokeImages\\fonts\\";

        public static string[] happyEmojis = new string[0];
        public static string[] sadEmojis = new string[0];
        public static string[] confusedEmojis = new string[0];
        public static string[] angryEmojis = new string[0];
        public static string[] regularPokemon = new string[0];
        public static string[] megaPokemon = new string[0];
        public static string[] gmaxPokemon = new string[0];
        public static string[] regionalVariantPokemon = new string[0];
        public static string[] majorVariantPokemon = new string[0];
        public static string[] minorVariantPokemon = new string[0];
        public static string[] majorTransformationPokemon = new string[0];
        public static string[] minorTransformationPokemon = new string[0];
        public static string[] reallyMinorVariantPokemon = new string[0];
        public static string[] prompts = new string[0];

        public static async Task GetAllTextFiles()
        {
            happyEmojis = LoadTextFile("Emojis/Happy%20Emojis.txt").Result;
            sadEmojis = LoadTextFile("Emojis/Sad%20Emojis.txt").Result;
            confusedEmojis = LoadTextFile("Emojis/Confused%20Emojis.txt").Result;
            angryEmojis = LoadTextFile("Emojis/Angry%20Emojis.txt").Result;

            prompts = LoadTextFile("Prompts/Prompts.txt").Result;

            regularPokemon = LoadTextFile("Pokemon/Pokemon%20Game%20Data.txt").Result;
            regionalVariantPokemon = LoadTextFile("Pokemon/Regional%20Variants.txt").Result;
            megaPokemon = LoadTextFile("Pokemon/Megas.txt").Result;
            gmaxPokemon = LoadTextFile("Pokemon/Gigamax.txt").Result;
            majorVariantPokemon = LoadTextFile("Pokemon/Notable%20Variants.txt").Result;
            minorVariantPokemon = LoadTextFile("Pokemon/Minor%20Variants.txt").Result;
            majorTransformationPokemon = LoadTextFile("Pokemon/Notable%20Transformations.txt").Result;
            minorTransformationPokemon = LoadTextFile("Pokemon/Smaller%20Transformations.txt").Result;
            reallyMinorVariantPokemon = LoadTextFile("Pokemon/REALLY%20Minor%20Variants.txt").Result;

            Console.WriteLine(happyEmojis.Length);
            Console.WriteLine(sadEmojis.Length);
            Console.WriteLine(confusedEmojis.Length);
            Console.WriteLine(angryEmojis.Length);
            Console.WriteLine(prompts.Length);
            Console.WriteLine(regularPokemon.Length);
            Console.WriteLine(regionalVariantPokemon.Length);
            Console.WriteLine(megaPokemon.Length);
            Console.WriteLine(gmaxPokemon.Length);
            Console.WriteLine(majorVariantPokemon.Length);
            Console.WriteLine(minorVariantPokemon.Length);
            Console.WriteLine(majorTransformationPokemon.Length);
            Console.WriteLine(minorTransformationPokemon.Length);
            Console.WriteLine(reallyMinorVariantPokemon.Length);
            
        }

        private static async Task<string[]> LoadTextFile(string directory)
        {
            string[] strings = new string[0];
            try
            {
                // Download the file content
                string fileContent = await DownloadFileContent(gitHubTextUrl + directory);

                // Convert the file content to an array of strings
                strings = fileContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            return strings;
        }

        private static async Task<string> DownloadFileContent(string url)
        {
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }
}
