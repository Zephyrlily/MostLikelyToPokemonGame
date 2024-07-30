using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGame.Classes
{
    public class PokeGame
    {
        public List<Player> Players = new List<Player>();
        public List<Pokemon> pokemonDeck { get; set; } = new List<Pokemon>();
        public List<Prompt> prompts = new List<Prompt>();
        public ISocketMessageChannel Channel { get; set; }

        // Button settings
        public bool RegionalVariants { get; set; } = true;
        public bool Gmax { get; set; } = false;
        public bool Megas { get; set; } = true;
        public bool NotableVariants { get; set; } = true;
        public bool MinorVariants { get; set; } = false;
        public bool MajorTransformations { get; set; } = true;
        public bool MinorTransformations { get; set; } = false;
        public bool ReallyMinorVariants { get; set; } = false;
        public bool PokemonStack { get; set; } = false;
        public bool SingleHost { get; set; } = false;
        public Player SingleHostUser { get; set; }
        private int playerCards = 6; // Default value
        public int PlayerCards
        {
            get => playerCards;
            set
            {
                if (value >= 1 && value <= 10)
                    playerCards = value;
            }
        }

        private int customPromptChance = 5; // Default percentage
        public int CustomPromptChance
        {
            get => customPromptChance;
            set
            {
                if (value >= 0 && value <= 100)
                    customPromptChance = value;
            }
        }

        private int pointsToWin = 4; // Default value
        public int PointsToWin
        {
            get => pointsToWin;
            set
            {
                if (value >= 1)
                    pointsToWin = value;
            }
        }
        public GamePhase Phase { get; set; } = GamePhase.INITIALIZATION;

        private RestUserMessage initializationMessage, gamePromptMessage, winnerSelectMessage;
        private IUserMessage pokemonDisplayMessage;

        private ImageCreator imageCreator = new ImageCreator();

        private static Random rng = new Random();
        public int nextPlayerToPresent;
        public int playerHost;
        public bool ReadyForNextPresentation = false;
        public bool WinnerSelected = false;
        public Player CurrentSelectedWinner;

        public async Task InitializeGame(ISocketMessageChannel channel, SocketUser host)
        {
            Channel = channel;
            AddPlayer(host);

            await InitializeMessage(true); //Send the initial message
        }

        #region Initialize Game

        public async Task InitializeMessage(bool isInitializing)
        {
            var embed = InitializeGameEmbed().Build();
            var components = InitializeComponents().Build();

            if (isInitializing)
            {
                initializationMessage = await Channel.SendMessageAsync(embed: embed, components: components);
            }
            else
            {
                await initializationMessage.ModifyAsync(msg =>
                {
                    msg.Embed = embed;
                    msg.Components = components;
                });
            }
        }

        private EmbedBuilder InitializeGameEmbed()
        {
            var builder = new EmbedBuilder();

            builder.WithTitle("Game created!");
            builder.WithDescription("Welcome to the game! Use the buttons below to configure your game settings.")
                   .AddField("Regional Variants", RegionalVariants ? "Enabled" : "Disabled")
                   .AddField("GMax", Gmax ? "Enabled" : "Disabled")
                   .AddField("Megas", Megas ? "Enabled" : "Disabled")
                   .AddField("Notable Variants", NotableVariants ? "Enabled" : "Disabled")
                   .AddField("Minor Variants", MinorVariants ? "Enabled" : "Disabled")
                   .AddField("Really Minor Variants", ReallyMinorVariants ? "Enabled" : "Disabled")
                   .AddField("Major Transformations", MajorTransformations ? "Enabled" : "Disabled")
                   .AddField("Minor Transformations", MinorTransformations ? "Enabled" : "Disabled")
                   .AddField("Pokemon Stack", PokemonStack ? "Enabled" : "Disabled")
                   .AddField("Player Hand Size", $"**{PlayerCards}** cards (max 10)")
                   .AddField("Custom Prompt Chance", $"**{CustomPromptChance}%**")
                   .AddField("Points Needed to Win", $"**{PointsToWin}** points");

            builder.WithImageUrl("https://i.giphy.com/media/v1.Y2lkPTc5MGI3NjExYXR5ZWdsaTFscHZlaG9iOW9xY2s4c2phbXFhbmpkdWkwZDduZ2V1MCZlcD12MV9pbnRlcm5hbF9naWZfYnlfaWQmY3Q9Zw/td02jbtsXIxpBv45rJ/giphy.gif");

            return builder;
        }

        private ComponentBuilder InitializeComponents()
        {
            var componentBuilder = new ComponentBuilder();

            componentBuilder.WithButton("Regional Variants", "regional_variants", ButtonStyle.Primary);
            componentBuilder.WithButton("GMax", "gmax", ButtonStyle.Primary);
            componentBuilder.WithButton("Megas", "megas", ButtonStyle.Primary);
            componentBuilder.WithButton("Notable Variants", "notable_variants", ButtonStyle.Primary);
            componentBuilder.WithButton("Minor Variants", "minor_variants", ButtonStyle.Primary);
            componentBuilder.WithButton("Really Minor Variants", "really_minor_variants", ButtonStyle.Primary);
            componentBuilder.WithButton("Major Transformations", "major_transformations", ButtonStyle.Primary);
            componentBuilder.WithButton("Minor Transformations", "minor_transformations", ButtonStyle.Primary);

            componentBuilder.WithButton("Toggle Pokemon Stack", "toggle_pokemon_stack", ButtonStyle.Secondary, row: 2);
            componentBuilder.WithButton("Set Player Cards", "set_player_cards", ButtonStyle.Secondary, row: 2);
            componentBuilder.WithButton("Set Custom Prompt Chance", "set_custom_prompt_chance", ButtonStyle.Secondary, row: 2);
            componentBuilder.WithButton("Set Points to Win", "set_points_to_win", ButtonStyle.Secondary, row: 2);

            componentBuilder.WithButton("Open Room", "join_screen", ButtonStyle.Success, row: 3);

            return componentBuilder;
        }
        #endregion

        #region Join Screen

        public async Task JoinScreenMessage()
        {
            Phase = GamePhase.JOIN_SCREEN;

            var embed = JoinScreenEmbed().Build();
            var components = JoinScreenComponents().Build();

            await initializationMessage.ModifyAsync(msg =>
                {
                    msg.Embed = embed;
                    msg.Components = components;
                });
        }

        private EmbedBuilder JoinScreenEmbed()
        {
            var builder = new EmbedBuilder();

            builder.WithTitle("Welcome!");

            if (SingleHostUser != null)
            {
                builder.WithDescription("It's almost time to begin! Use the buttons below to join in on the fun!")
                   .AddField("Single Host", SingleHostUser.User.Mention);
            } else
            {
                builder.WithDescription("It's almost time to begin! Use the buttons below to join in on the fun!")
                   .AddField("Single Host", SingleHost ? "Enabled" : "Disabled");
            }

            if (Players.Count > 0)
            {
                string playerList = "";
                foreach (Player p in Players)
                {
                    if (p.host)
                    {
                        playerList += p.User.Mention + " (:crown:)\n";
                    }
                    else
                    {
                        playerList += p.User.Mention + "\n";
                    }
                }
                builder.AddField("Players (" + Players.Count + "):", playerList);
            }

            builder.WithImageUrl("https://i.giphy.com/media/v1.Y2lkPTc5MGI3NjExczAwZXRyZDFlbmRuaHB0eHhlb2h1OTRxcnF4MTU2YjkwYTA5NGE3MiZlcD12MV9pbnRlcm5hbF9naWZfYnlfaWQmY3Q9Zw/8UGGp7rQvfhe63HrFq/giphy.gif");
            return builder;
        }

        private ComponentBuilder JoinScreenComponents()
        {
            var componentBuilder = new ComponentBuilder();

            componentBuilder.WithButton("Join", "join_game", ButtonStyle.Primary);
            componentBuilder.WithButton("Leave", "leave_game", ButtonStyle.Danger);
            componentBuilder.WithButton("Single Host", "single_host", ButtonStyle.Secondary);
            componentBuilder.WithButton("Start", "start_game", ButtonStyle.Success);

            if (SingleHost && Players.Count > 0)
            {
                // Ensure the SelectMenuBuilder is initialized with a custom ID
                var select = new SelectMenuBuilder()
                    .WithCustomId("select_host")
                    .WithPlaceholder("Choose a host");

                foreach (Player p in Players)
                {
                    // Create and add select menu options correctly
                    select.AddOption(new SelectMenuOptionBuilder()
                        .WithLabel(p.User.Username)
                        .WithValue(p.User.Id.ToString()));
                }

                // Add the select menu to the component builder
                componentBuilder.WithSelectMenu(select);
            }

            return componentBuilder;
        }

        public void AddPlayer(SocketUser userToAdd)
        {
            Player playerToAdd = new Player(userToAdd, this);
            if(Players.Count == 0)
            {
                playerToAdd.host = true;
            }
            Players.Add(playerToAdd);
        }

        public void RemovePlayer(SocketUser userToRemove)
        {
            Player playerToRemove = Players.Where(p => p.User.Id == userToRemove.Id).First();
            Players.RemoveAll(p => p.User.Id == userToRemove.Id);
            if(Players.Count == 0)
            {
                // TODO: Terminate games
            }
            else if (playerToRemove.host == true)
            {
                Players[0].host = true;
            }
            
        }

        #endregion

        #region Room Screen

        public async Task RoomScreenMessage()
        {
            // Generate the image and get the stream
            using (var imageStream = await imageCreator.GenerateRoomImage(Players))
            {
                // Upload the image as an attachment to a Discord message
                await Channel.SendFileAsync(imageStream, "room.png");
            }
        }

        public async Task PromptMessage()
        {
            // Generate the image and get the stream
            using (var imageStream = await imageCreator.GeneratePromptImage(prompts[0]))
            {
                // Upload the image as an attachment to a Discord message
                gamePromptMessage = await Channel.SendFileAsync(imageStream, "room.png",
                    $"Host: {Players[playerHost].User.Mention}",
                    components:PreviewPromptComponents().Build());
                prompts[0].url = gamePromptMessage.Attachments.First().Url;
            }
        }

        private ComponentBuilder RoomScreenComponents()
        {
            var componentBuilder = new ComponentBuilder();

            componentBuilder.WithButton("Join", "join_game", ButtonStyle.Primary);
            componentBuilder.WithButton("Leave", "leave_game", ButtonStyle.Danger);
            componentBuilder.WithButton("Single Host", "single_host", ButtonStyle.Secondary);
            componentBuilder.WithButton("Start", "start_game", ButtonStyle.Success);

            if (SingleHost && Players.Count > 0)
            {
                // Ensure the SelectMenuBuilder is initialized with a custom ID
                var select = new SelectMenuBuilder()
                    .WithCustomId("select_host")
                    .WithPlaceholder("Choose a host");

                foreach (Player p in Players)
                {
                    // Create and add select menu options correctly
                    select.AddOption(new SelectMenuOptionBuilder()
                        .WithLabel(p.User.Username)
                        .WithValue(p.User.Id.ToString()));
                }

                // Add the select menu to the component builder
                componentBuilder.WithSelectMenu(select);
            }

            return componentBuilder;
        }



        public async Task SetUpGame()
        {

            try
            {
                await initializationMessage.DeleteAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            LoadInAllPokemon();
            LoadInAllPrompts();

            playerHost = rng.Next(0, Players.Count);
            nextPlayerToPresent = NextPlayer(playerHost);

            await PreviewPrompt();
        }

        public async Task PreviewPrompt()
        {
            Phase = GamePhase.PROMPT_SELECT;



            await RoomScreenMessage();
            await Task.Delay(4000); // TODO: Enable this

            if (Players.Any(p => p.Wins == PointsToWin))
            {
                await Channel.SendMessageAsync(embed: GrandVictorEmbed().Build());
            }
            else
            {
                await PromptMessage();
            }
        }

        private ComponentBuilder PreviewPromptComponents()
        {
            var componentBuilder = new ComponentBuilder();

            componentBuilder.WithButton("Approve", "approve_prompt", ButtonStyle.Primary);
            componentBuilder.WithButton("Deny", "deny_prompt", ButtonStyle.Danger);

            return componentBuilder;
        }

        public async Task AcceptPrompt()
        {
            if (gamePromptMessage != null)
            {
                await gamePromptMessage.ModifyAsync(m => 
                m.Components = new ComponentBuilder().Build());
            }
            await StartNextRound();
        }

        public async Task DenyPrompt()
        {
            if (gamePromptMessage != null)
            {
                await gamePromptMessage.DeleteAsync();
            }

            prompts.RemoveAt(0);
            await PromptMessage();
        }

        public async Task StartNextRound()
        {
            Phase = GamePhase.GAMEPLAY;
            // Create a list of tasks to run in parallel
            List<Task> playerTasks = new List<Task>();
            ReadyForNextPresentation = true;
            WinnerSelected = false;

            foreach (Player p in Players)
            {
                if (Players[playerHost] != p)
                {
                    p.DrawCards();
                    playerTasks.Add(p.DisplayHand());
                    p.hasPresented = false;
                    p.ReadyToPresent = false;
                }
            }

            // Wait for all DisplayHand tasks to complete
            await Task.WhenAll(playerTasks);
        }

        public void LoadInAllPokemon()
        {
            pokemonDeck.AddRange(GameData.regularPokemon);
            if (RegionalVariants)
            {
                pokemonDeck.AddRange(GameData.regionalVariantsPokemon);
            }
            if (Megas)
            {
                pokemonDeck.AddRange(GameData.megaPokemon);
            }
            if (Gmax)
            {
                pokemonDeck.AddRange(GameData.gmaxPokemon);
            }
            if (MajorTransformations)
            {
                pokemonDeck.AddRange(GameData.majorTransformationPokemon);
            }
            if (MinorTransformations)
            {
                pokemonDeck.AddRange(GameData.minorTransformationPokemon);
            }
            if (NotableVariants)
            {
                pokemonDeck.AddRange(GameData.majorVariantPokemon);
            }
            if (MinorVariants)
            {
                pokemonDeck.AddRange(GameData.minorVariantPokemon);
            }
            if (ReallyMinorVariants)
            {
                pokemonDeck.AddRange(GameData.reallyMinorVariantPokemon);
            }
            pokemonDeck.Shuffle();


            if (PokemonStack)
            {
                var seenSpecies = new HashSet<string>();
                var filteredDeck = new List<Pokemon>();

                foreach (var pokemon in pokemonDeck)
                {
                    if (seenSpecies.Add(pokemon.SpeciesName))
                    {
                        filteredDeck.Add(pokemon);
                    }
                }

                pokemonDeck = filteredDeck;
            }

        }

        public void LoadInAllPrompts()
        {
            prompts = GameData.prompts.ToList();
            prompts.Shuffle();
        }

        public int NextPlayer(int player)
        {
            int i = player + 1;
            if(i >= Players.Count)
            {
                return 0;
            } else
            {
                return i;
            }
        }

        #endregion

        #region Pokemon Display

        public async Task DisplayPokemonMessage()
        {
            ReadyForNextPresentation = false;
            
            if(pokemonDisplayMessage != null)
            {
                await pokemonDisplayMessage.ModifyAsync(m => 
                    m.Components = new ComponentBuilder().Build());
            }
            var messageEmbed = DisplayPokemonEmbed().Build();
            var messageComponents = DisplayPokemonComponents().Build();

            var attachments = new List<FileAttachment>();

            var player = Players[nextPlayerToPresent];


            foreach (var no in player.SubmittedResponses)
            {
                var pokemon = player.Hand[no];
                string filePath = pokemon.GetCardURL();
                if (File.Exists(filePath))
                {
                    var imageStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                    var attachment = new FileAttachment(imageStream, $"{pokemon.Name}.png");
                    attachments.Add(attachment);
                }
                else
                {
                    Console.WriteLine($"File not found: {filePath}");
                }
            }

            /*List<string> urls = new List<string>();
            foreach (var no in player.SubmittedResponses)
            {
                var pokemon = player.Hand[no];
                string filePath = pokemon.GetCardURL();
                urls.Add(filePath);
                if (File.Exists(filePath))
                {
                    urls.Add(filePath);
                }
                else
                {
                    Console.WriteLine($"File not found: {filePath}");
                }
            }*/

            /*// Generate the image and get the stream
            using (var imageStream = await imageCreator.GenerateResponseImage(urls))
            {
                // Upload the image as an attachment to a Discord message
                await Channel.SendFileAsync(imageStream, "response.png");
            }*/

            // Send files asynchronously
            pokemonDisplayMessage = await Channel.SendFilesAsync(attachments, embed: messageEmbed,
                components:messageComponents);

            // Dispose of the streams after sending
            foreach (var attachment in attachments)
            {
                attachment.Dispose(); // This will dispose of the FileStream used in the FileAttachment
            }


        }

        public async Task<int> CheckNextPlayer()
        {
            int nextPlayer = NextPlayer(nextPlayerToPresent);
            bool wasReady = ReadyForNextPresentation;

            ReadyForNextPresentation = true;

            if (!wasReady)
            {
                nextPlayerToPresent = nextPlayer;
                if (nextPlayer == playerHost)
                {
                    // Display winner stuff
                    Phase = GamePhase.WINNER_SELECT;
                    await DisplayWinnerChoiceMessage();
                    return 2;
                }
                else if (Players[nextPlayer].ReadyToPresent)
                {
                    await Players[nextPlayer].ShowSubmission();
                    return 1;
                }
                else
                {
                    return 0;

                }
            }
            else
            {
                return 3;
            }
        }

        private EmbedBuilder DisplayPokemonEmbed()
        {
            var embedBuilder = new EmbedBuilder();

            embedBuilder.WithDescription("# " + Players[nextPlayerToPresent].User.Mention + "'s submission:");

            return embedBuilder;
        }

        private ComponentBuilder DisplayPokemonComponents()
        {
            var componentBuilder = new ComponentBuilder();

            componentBuilder.WithButton("Next", "check_next_submission");

            return componentBuilder;
        }

        #endregion

        #region Winner Select

        public async Task DisplayWinnerChoiceMessage(bool winnerDeclared = false)
        {
            var embed = DisplayWinnerChoiceEmbed(winnerDeclared).Build();
            if (!winnerDeclared)
            {
                var components = DisplayWinnerChoiceComponents().Build();

                if (winnerSelectMessage == null)
                {
                    winnerSelectMessage = await Channel.SendMessageAsync(embed: embed, components: components);
                }
                else
                {
                    await winnerSelectMessage.ModifyAsync(m =>
                    {
                        m.Embed = embed;
                        m.Components = components;
                    });
                }
            }
            else
            {
                await winnerSelectMessage.ModifyAsync(m =>
                {
                    m.Embed = embed;
                    m.Components = new ComponentBuilder().Build();
                });
            }
           
        }

        private EmbedBuilder DisplayWinnerChoiceEmbed(bool winnerDeclared = false)
        {
            var embedBuilder = new EmbedBuilder();

            string desc = "# Prompt: " + prompts[0].promptText + "\n\n";

            if (!winnerDeclared)
            {

                if (!WinnerSelected)
                {
                    foreach (Player p in Players)
                    {
                        if (p != Players[playerHost])
                        {

                            desc += p.User.Mention + ": " + CapitalizeFirst(p.Hand[p.SubmittedResponses[0]].Name);
                            for (int i = 1; i < p.SubmittedResponses.Count; i++)
                            {
                                desc += " / " + CapitalizeFirst(p.Hand[p.SubmittedResponses[i]].Name);
                            }
                            desc += "\n";
                        }
                    }
                }
                else
                {
                    Player p = CurrentSelectedWinner;

                    desc += p.User.Mention + ": " + CapitalizeFirst(p.Hand[p.SubmittedResponses[0]].Name);
                    for (int i = 1; i < p.SubmittedResponses.Count; i++)
                    {
                        desc += " / " + CapitalizeFirst(p.Hand[p.SubmittedResponses[i]].Name);
                    }
                    desc += "\n";
                }
                embedBuilder.WithDescription(desc);
                embedBuilder.WithImageUrl("https://media1.tenor.com/m/UMrSFD-f1PAAAAAC/riolu-cute.gif");
            }
            else
            {
                Player p = CurrentSelectedWinner;

                desc += "# " + p.User.Mention + " has won the round with " + p.Hand[p.SubmittedResponses[0]].Name;
                for (int i = 1; i < p.SubmittedResponses.Count; i++)
                {
                    desc += ", " + p.Hand[p.SubmittedResponses[i]].Name;
                }
                desc += "\n";

                embedBuilder.WithDescription(desc);
                embedBuilder.WithImageUrl("https://media1.tenor.com/m/7XmW1k6F5XsAAAAC/plusle-minun.gif");
            }

            return embedBuilder;
        }

        private ComponentBuilder DisplayWinnerChoiceComponents()
        {
            var componentBuilder = new ComponentBuilder();

            if (!WinnerSelected)
            {
                if (Players.Count > 0)
                {
                    // Ensure the SelectMenuBuilder is initialized with a custom ID
                    var select = new SelectMenuBuilder()
                        .WithCustomId("select_winner")
                        .WithPlaceholder("Select the winner");

                    foreach (Player p in Players)
                    {
                        if (p != Players[playerHost])
                        {
                            // Create and add select menu options correctly
                            select.AddOption(new SelectMenuOptionBuilder()
                                .WithLabel(p.User.Username)
                                .WithValue(p.User.Id.ToString()));
                        }
                    }

                    // Add the select menu to the component builder
                    componentBuilder.WithSelectMenu(select);
                }
            } else
            {
                componentBuilder.WithButton("Confirm", "confirm_winner", ButtonStyle.Success);
                componentBuilder.WithButton("Cancel", "cancel_winner", ButtonStyle.Danger);
            }

            return componentBuilder;
        }

        public async Task WinnerChoiceSelected()
        {
            WinnerSelected = true;
            await DisplayWinnerChoiceMessage();
        }

        public async Task WinnerCancelled()
        {
            WinnerSelected = false;
            await DisplayWinnerChoiceMessage();
        }

        public async Task WinnerChosen()
        {
            // Display some message indicating the winner has been chosen
            await DisplayWinnerChoiceMessage(true);
            winnerSelectMessage = null;

            // Find the index of the currently selected winner in the Players list
            int winnerIndex = Players.IndexOf(CurrentSelectedWinner);

            // Increment the Wins property of the selected winner
            if (winnerIndex >= 0)
            {
                Players[winnerIndex].Wins += 1;

                prompts.RemoveAt(0);
                foreach(Player p in Players)
                {
                    List<int> pokemonPlayed = new List<int>(p.SubmittedResponses.OrderByDescending(x => x));
                    foreach(int x in pokemonPlayed)
                    {
                        p.Hand.RemoveAt(x);
                    }
                    p.SubmittedResponses.Clear();
                    p.DrawCards();
                }

                playerHost = NextPlayer(playerHost);
                nextPlayerToPresent = NextPlayer(playerHost);
                WinnerSelected = false;
                

                await Task.Delay(4000);

                await PreviewPrompt();
            }
            else
            {
                // Handle the case where the winner is not found in the Players list
                // This could be an error condition that needs to be logged or addressed
                Console.WriteLine("Error: Selected winner not found in the players list.");
            }
        }

        private EmbedBuilder GrandVictorEmbed()
        {
            var embedBuilder = new EmbedBuilder();

            string desc = "";

            Player p = CurrentSelectedWinner;

            desc = "# CONGRATULATIONS TO " + p.User.Mention + ", THE WINNER!!! ";

            embedBuilder.WithDescription(desc);
            embedBuilder.WithImageUrl("https://media1.tenor.com/m/qObvHG4rT28AAAAC/pikachu-pokemon.gif");
            embedBuilder.WithColor(Color.Gold);


            return embedBuilder;
        }

        #endregion

        public enum GamePhase
        {
            INITIALIZATION, JOIN_SCREEN, GAMEPLAY, WINNER_SELECT, PROMPT_SELECT
        }

        private string CapitalizeFirst(string s)
        {
            string capitalS = s[0].ToString().ToUpper();
            return capitalS + s.Remove(0, 1);
        }
    }
}
