using System;
using System.Collections.Generic;
using System.Text;

namespace PokemonGame.Classes
{
    public static class GameData
    {
        public static Pokemon[] regularPokemon;
        public static Pokemon[] regionalVariantsPokemon;
        public static Pokemon[] megaPokemon;
        public static Pokemon[] gmaxPokemon;
        public static Pokemon[] majorVariantPokemon;
        public static Pokemon[] minorVariantPokemon;
        public static Pokemon[] majorTransformationPokemon;
        public static Pokemon[] minorTransformationPokemon;
        public static Pokemon[] reallyMinorVariantPokemon;

        public static Prompt[] prompts;

        public static void LoadAllData()
        {
            // Initialize and load regularPokemon
            regularPokemon = new Pokemon[TextData.regularPokemon.Length];
            for (int i = 0; i < regularPokemon.Length; ++i)
            {
                Pokemon pokemonToAdd = new Pokemon(TextData.regularPokemon[i]);
                regularPokemon[i] = pokemonToAdd;
            }

            // Initialize and load regionalVariantsPokemon
            regionalVariantsPokemon = new Pokemon[TextData.regionalVariantPokemon.Length];
            for (int i = 0; i < regionalVariantsPokemon.Length; ++i)
            {
                Pokemon pokemonToAdd = new Pokemon(TextData.regionalVariantPokemon[i]);
                regionalVariantsPokemon[i] = pokemonToAdd;
            }

            // Initialize and load megaPokemon
            megaPokemon = new Pokemon[TextData.megaPokemon.Length];
            for (int i = 0; i < megaPokemon.Length; ++i)
            {
                Pokemon pokemonToAdd = new Pokemon(TextData.megaPokemon[i]);
                megaPokemon[i] = pokemonToAdd;
            }

            // Initialize and load gmaxPokemon
            gmaxPokemon = new Pokemon[TextData.gmaxPokemon.Length];
            for (int i = 0; i < gmaxPokemon.Length; ++i)
            {
                Pokemon pokemonToAdd = new Pokemon(TextData.gmaxPokemon[i]);
                gmaxPokemon[i] = pokemonToAdd;
            }

            // Initialize and load majorVariantPokemon
            majorVariantPokemon = new Pokemon[TextData.majorVariantPokemon.Length];
            for (int i = 0; i < majorVariantPokemon.Length; ++i)
            {
                Pokemon pokemonToAdd = new Pokemon(TextData.majorVariantPokemon[i]);
                majorVariantPokemon[i] = pokemonToAdd;
            }

            // Initialize and load minorVariantPokemon
            minorVariantPokemon = new Pokemon[TextData.minorVariantPokemon.Length];
            for (int i = 0; i < minorVariantPokemon.Length; ++i)
            {
                Pokemon pokemonToAdd = new Pokemon(TextData.minorVariantPokemon[i]);
                minorVariantPokemon[i] = pokemonToAdd;
            }

            // Initialize and load majorTransformationPokemon
            majorTransformationPokemon = new Pokemon[TextData.majorTransformationPokemon.Length];
            for (int i = 0; i < majorTransformationPokemon.Length; ++i)
            {
                Pokemon pokemonToAdd = new Pokemon(TextData.majorTransformationPokemon[i]);
                majorTransformationPokemon[i] = pokemonToAdd;
            }

            // Initialize and load minorTransformationPokemon
            minorTransformationPokemon = new Pokemon[TextData.minorTransformationPokemon.Length];
            for (int i = 0; i < minorTransformationPokemon.Length; ++i)
            {
                Pokemon pokemonToAdd = new Pokemon(TextData.minorTransformationPokemon[i]);
                minorTransformationPokemon[i] = pokemonToAdd;
            }

            // Initialize and load reallyMinorVariantPokemon
            reallyMinorVariantPokemon = new Pokemon[TextData.reallyMinorVariantPokemon.Length];
            for (int i = 0; i < reallyMinorVariantPokemon.Length; ++i)
            {
                Pokemon pokemonToAdd = new Pokemon(TextData.reallyMinorVariantPokemon[i]);
                reallyMinorVariantPokemon[i] = pokemonToAdd;
            }

            prompts = new Prompt[TextData.prompts.Length];
            for (int i = 0; i < prompts.Length; ++i)
            {
                if (TextData.prompts[i].Split("|").Length >= 2)
                {
                    Prompt promptToAdd = new Prompt(TextData.prompts[i]);
                    prompts[i] = promptToAdd;
                }
            }
        }



    }
}
