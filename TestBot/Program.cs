using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DuoVia.FuzzyStrings;
using static TestBot.Program;
using System.Linq.Expressions;
using System.Net.Cache;
using System.Transactions;
using PokemonGame.Classes;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Numerics;
using DotNetEnv;
using dotenv.net;

namespace TestBot
{
    class Program
    {
        static void Main(string[] args) => new Program().RunBotAsync().GetAwaiter().GetResult();

        

        string prefix = "p!"; //Prefix to use for bot commands, eg j!initialize, %initialize, etc
        string token = "[placeholder]"; //Get this fron the Discord Developers Portal, don't share it
        
        
        // Don't mess with anything below this line unless you know what you're doing

        static Random rdm = new Random();

        private DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _services;

        private List<PokeGame> games = new List<PokeGame>();
        
        

        
        public async Task RunBotAsync()
        {
            DotEnv.Load();

            Console.WriteLine($"Current Directory: {Directory.GetCurrentDirectory()}");
            Console.WriteLine($"DISCORD_BOT_TOKEN: {Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN")}");


            

            // Get the Discord bot token
            token = Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN");

            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("Error: Token is null or empty.");
                return;
            }

            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.All,
                UseInteractionSnowflakeDate = false
            });
            _commands = new CommandService();
            _client.Log += _client_Log;
            _client.Ready += () =>
            {
                return Task.CompletedTask;

            };

            await RegisterCommandsAsync();
            await _client.LoginAsync(TokenType.Bot, token);
            await TextData.GetAllTextFiles();
            GameData.LoadAllData();
            await _client.StartAsync();
            await Task.Delay(-1);

            

        }

        

        private Task _client_Log(LogMessage arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }

        public async Task RegisterCommandsAsync()
        {
            _client.MessageReceived += HandleCommandAsync;
            _client.ButtonExecuted += MyButtonHandler;
            _client.SelectMenuExecuted += MyMenuHandler;
            _client.ModalSubmitted += async modal =>
            {
                await HandleModalAsync(modal);
            };
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        public int GetGameNo(ISocketMessageChannel channel)
        {
            return games.FindIndex(g => g.Channel.Id == channel.Id);
        }


        private async Task HandleCommandAsync(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            if (_client != null && message != null)
            {
                var context = new SocketCommandContext(_client, message);
                int argPos = 0;
                string messageContext = context.Message.Content.ToLower();
                if (!context.User.IsBot)
                {
                    if(messageContext.Equals(prefix + "create"))
                    {
                        if (!games.Any(g => g.Channel.Id == context.Channel.Id)
                            && !AnyGameHasPlayer(context.User))
                        {
                            PokeGame game = new PokeGame();
                            await game.InitializeGame(context.Channel, context.User);
                            games.Add(game);
                        }
                    }
                }
            }
        }

        public bool AnyGameHasPlayer(SocketUser user)
        {
            return games.Any(g => g.Players.Any(p => p.User.Id == user.Id));
        }

        public async Task MyButtonHandler(SocketMessageComponent component)
        {
            SocketInteractionContext ctx = new SocketInteractionContext(_client, component);

            int gameNumber = GetGameNo(ctx.Channel);
            bool isPrivate = ctx.Channel is IPrivateChannel;




            if (gameNumber >= 0)
            {
                PokeGame game = games[gameNumber];
                Console.WriteLine(game.Phase);

                if (game.Players.Any(p => p.User.Id == ctx.User.Id && p.host == true
                    && game.Phase == PokeGame.GamePhase.INITIALIZATION))
                {
                    switch (component.Data.CustomId)
                    {
                        case "regional_variants":
                            game.RegionalVariants = !game.RegionalVariants;
                            await component.RespondAsync($"Regional Variants: {(game.RegionalVariants ? "Enabled" : "Disabled")} " + GetHappyEmoji(), ephemeral: true);
                            break;
                        case "gmax":
                            game.Gmax = !game.Gmax;
                            await component.RespondAsync($"GMax: {(game.Gmax ? "Enabled" : "Disabled")} " + GetHappyEmoji(), ephemeral: true);
                            break;
                        case "megas":
                            game.Megas = !game.Megas;
                            await component.RespondAsync($"Megas: {(game.Megas ? "Enabled" : "Disabled")} " + GetHappyEmoji(), ephemeral: true);
                            break;
                        case "notable_variants":
                            game.NotableVariants = !game.NotableVariants;
                            await component.RespondAsync($"Notable Variants: {(game.NotableVariants ? "Enabled" : "Disabled")} " + GetHappyEmoji(), ephemeral: true);
                            break;
                        case "minor_variants":
                            game.MinorVariants = !game.MinorVariants;
                            await component.RespondAsync($"Minor Variants: {(game.MinorVariants ? "Enabled" : "Disabled")} " + GetHappyEmoji(), ephemeral: true);
                            break;
                        case "really_minor_variants":
                            game.ReallyMinorVariants = !game.ReallyMinorVariants;
                            await component.RespondAsync($"Really Minor Variants: {(game.ReallyMinorVariants ? "Enabled" : "Disabled")} " + GetHappyEmoji(), ephemeral: true);
                            break;
                        case "major_transformations":
                            game.MajorTransformations = !game.MajorTransformations;
                            await component.RespondAsync($"Major Transformations: {(game.MajorTransformations ? "Enabled" : "Disabled")} " + GetHappyEmoji(), ephemeral: true);
                            break;
                        case "minor_transformations":
                            game.MinorTransformations = !game.MinorTransformations;
                            await component.RespondAsync($"Minor Transformations: {(game.MinorTransformations ? "Enabled" : "Disabled")} " + GetHappyEmoji(), ephemeral: true);
                            break;
                        case "toggle_pokemon_stack":
                            game.PokemonStack = !game.PokemonStack;
                            await component.RespondAsync($"Pokemon Stack: {(game.PokemonStack ? "Enabled" : "Disabled")} " + GetHappyEmoji(), ephemeral: true);
                            break;
                        case "set_player_cards":
                            await ShowModal("player_cards", "Enter the number of cards (1-10):", "Hand Size:", component);
                            break;
                        case "set_custom_prompt_chance":
                            await ShowModal("custom_prompt_chance", "Enter the custom prompt chance (0-100):", "Custom Prompt Chance:", component);
                            break;
                        case "set_points_to_win":
                            await ShowModal("points_to_win", "Enter the points needed to win:", "Points Required:", component);
                            break;
                        case "join_screen":
                            await game.JoinScreenMessage();
                            await component.DeferAsync();
                            break;

                    }
                    if (game.Phase == PokeGame.GamePhase.INITIALIZATION)
                    {
                        await game.InitializeMessage(false);
                    }
                }
                else if (game.Phase == PokeGame.GamePhase.JOIN_SCREEN)
                {
                    if (game.Players.Any(p => p.User.Id == ctx.User.Id && p.host == true))
                    {
                        if (component.Data.CustomId.Equals("single_host"))
                        {
                            game.SingleHost = !game.SingleHost;
                            await component.RespondAsync($"Single Host: {(game.SingleHost ? "Enabled" : "Disabled")} " + GetHappyEmoji(), ephemeral: true);
                        }
                        else if (component.Data.CustomId.Equals("start_game"))
                        {
                            Console.WriteLine("Neat!");
                            game.Phase = PokeGame.GamePhase.GAMEPLAY; // extra button press failsafe
                            await component.DeferAsync();
                            await game.SetUpGame();
                        }
                    }
                    switch (component.Data.CustomId)
                    {
                        case "join_game":
                            if (!AnyGameHasPlayer(ctx.User))
                            {
                                game.AddPlayer(ctx.User);
                                await component.RespondAsync($"Welcome to the game {ctx.User.Mention} " + GetHappyEmoji());
                            }
                            else
                            {
                                await component.RespondAsync($"You already joined " + GetConfusedEmoji(), ephemeral: true);
                            }
                            break;
                        case "leave_game":
                            if (game.Players.Any(p => p.User.Id == ctx.User.Id))
                            {
                                game.RemovePlayer(ctx.User);
                                await component.RespondAsync($"{ctx.User.Mention} left the game " + GetSadEmoji());
                            }
                            else
                            {
                                await component.RespondAsync($"You're not even playing " + GetConfusedEmoji(), ephemeral: true);
                            }
                            break;

                    }

                    if (game.Phase == PokeGame.GamePhase.JOIN_SCREEN)
                    {
                        await game.JoinScreenMessage();
                    }
                }
                else if (game.Phase == PokeGame.GamePhase.PROMPT_SELECT)
                {
                    // Check if the user is already in the game
                    bool isHost = (
                        game.Players[game.playerHost].User.Id == ctx.User.Id
                        || game.Players[0].User.Id == ctx.User.Id);
                    if (isHost)
                    {
                        if (component.Data.CustomId.Equals("approve_prompt"))
                        {
                            await component.RespondAsync($"Prompt has been accepted " + GetHappyEmoji(), ephemeral: true);
                            await game.AcceptPrompt();
                        }
                        else
                        {
                            await game.DenyPrompt();
                            await component.RespondAsync($"Prompt has been denied " + GetHappyEmoji(), ephemeral: true);
                        }
                    }
                    else
                    {

                    }
                }
                else if (game.Phase == PokeGame.GamePhase.GAMEPLAY)
                {
                    Console.WriteLine("Hello");
                    bool isHost = (
                        game.Players[game.playerHost].User.Id == ctx.User.Id
                        || game.Players[0].User.Id == ctx.User.Id);
                    if (isHost)
                    {
                        if (component.Data.CustomId.Equals("check_next_submission"))
                        {
                            Console.WriteLine("Hello");
                            int res = await game.CheckNextPlayer();
                            switch (res)
                            {
                                case 0:
                                    await component.RespondAsync("Waiting for a submission... " + GetHappyEmoji(), ephemeral: true);
                                    break;
                                case 1:
                                    await component.DeferAsync();
                                    break;
                                case 2:
                                    await component.DeferAsync();
                                    break;
                                case 3:
                                    await component.RespondAsync("They sure are taking a while... " + GetAngryEmoji(), ephemeral: true);
                                    break;
                            }
                        }
                    }
                }
                else if (game.Phase == PokeGame.GamePhase.WINNER_SELECT)
                {
                    bool isHost = (
                        game.Players[game.playerHost].User.Id == ctx.User.Id);
                    if (isHost)
                    {
                        if (component.Data.CustomId.Equals("confirm_winner"))
                        {
                            await component.DeferAsync();
                            await game.WinnerChosen();
                        } else if (component.Data.CustomId.Equals("cancel_winner"))
                        {
                            await component.DeferAsync();
                            await game.WinnerCancelled();
                        }
                    }
                }
            }
            else
            {

                PokeGame game = games.Where(g => g.Players.Any(p => p.User.Id == ctx.User.Id))
                    .FirstOrDefault();

                if (game != null)
                {
                    var player = game.Players.FirstOrDefault(p => p.User.Id == ctx.User.Id);



                    if (game.Phase == PokeGame.GamePhase.GAMEPLAY && !player.hasPresented)
                    {
                        if (player != null)
                        {
                            if (!component.Data.CustomId.Equals("submit_select"))
                            {
                                player.ReadyToPresent = false;
                            }

                            if (component.Data.CustomId.StartsWith("pokemon_select_")
                                && !player.hasPresented)
                            {
                                int res;
                                bool parsed =
                                    int.TryParse(component.Data.CustomId.Substring(15), out res);

                                if (parsed)
                                {
                                    if (!player.ResponseNumbers.Contains(res) && res < player.Hand.Count()
                                        && player.ResponseNumbers.Count + 1 <= player.Game.prompts[0].responsesNeeded)
                                    {
                                        string selectedPokemon = player.Hand[res].SpeciesName;
                                        player.AddResponse(res);
                                        await component.RespondAsync($"{selectedPokemon} was selected " + GetHappyEmoji(), ephemeral: true);
                                    }
                                    else
                                    {
                                        await component.RespondAsync("What are you doing " + GetAngryEmoji());
                                    }
                                }
                                else
                                {
                                    await component.RespondAsync("Something that shouldn't have happened happened " + GetAngryEmoji());
                                }
                            }
                            else
                            {
                                switch (component.Data.CustomId)
                                {
                                    case "submit_select":
                                        await player.SubmitSelection();
                                        await component.RespondAsync($"Submitted! " + GetHappyEmoji(), ephemeral: true);
                                        break;
                                    case "undo_select":
                                        player.UndoSelection();
                                        await component.DeferAsync();
                                        break;
                                    case "reset_select":
                                        player.ResetSelection();
                                        await component.DeferAsync();
                                        break;
                                }
                            }
                            if (game.Phase == PokeGame.GamePhase.GAMEPLAY)
                            {
                                await player.UpdateHandMessage();
                            }
                        }
                        else
                        {
                            Console.WriteLine("Player not found");
                        }
                    }
                }
            }
        }

        public async Task MyMenuHandler(SocketMessageComponent component)
        {
            // Ensure proper context is obtained
            SocketInteractionContext ctx = new SocketInteractionContext(_client, component);
            int gameNumber = GetGameNo(ctx.Channel);

            if (gameNumber >= 0)
            {
                PokeGame game = games[gameNumber];
                string selectedValue = component.Data.Values.FirstOrDefault();

                // Ensure the selected value is not null or empty
                if (string.IsNullOrEmpty(selectedValue))
                {
                    await component.RespondAsync("No value selected.", ephemeral: true);
                    return;
                }

                // Handle single player menu selection
                if (component.Data.CustomId == "single_player_menu")
                {
                    var selectedPlayer = game.Players.FirstOrDefault(p => p.User.Id.ToString() == selectedValue);

                    if (selectedPlayer != null)
                    {
                        game.SingleHostUser = selectedPlayer;
                        await component.RespondAsync($"{selectedValue} has been set as the single host {GetHappyEmoji()}", ephemeral: true);
                        await game.JoinScreenMessage();
                    }
                    else
                    {
                        await component.RespondAsync("Player not found.", ephemeral: true);
                    }
                }

                // Handle winner selection
                else if (component.Data.CustomId == "select_winner" && game.Phase == PokeGame.GamePhase.WINNER_SELECT)
                {
                    bool isHost = (game.Players[game.playerHost].User.Id == ctx.User.Id);

                    if (isHost)
                    {
                        var selectedWinner = game.Players.FirstOrDefault(p => p.User.Id.ToString() == selectedValue);

                        if (selectedWinner != null)
                        {
                            game.CurrentSelectedWinner = selectedWinner;
                            await component.RespondAsync($"Confirm as the winner? {GetConfusedEmoji()}", ephemeral: true);
                            await game.WinnerChoiceSelected();
                        }
                        else
                        {
                            await component.RespondAsync("Winner not found.", ephemeral: true);
                        }
                    }
                    else
                    {
                        await component.RespondAsync("Only the host can select a winner.", ephemeral: true);
                    }
                }
            }
            else
            {
                await component.RespondAsync("Game not found.", ephemeral: true);
            }
        }


        private async Task HandleModalAsync(SocketModal modal)
        {
            // Extract the custom ID and input value from the modal
            var customId = modal.Data.CustomId;
            var inputValue = modal.Data.Components.First().Value;

            var game = games[GetGameNo(modal.Channel)];
            // Process the modal based on its custom ID
            switch (customId)
            {
                case "player_cards":
                    if (int.TryParse(inputValue, out int playerCards) && playerCards >= 1 && playerCards <= 10)
                    {
                        game.PlayerCards = playerCards;
                        await modal.RespondAsync($"Player cards set to **{playerCards}** " + GetHappyEmoji(), ephemeral:true);
                    }
                    else
                    {
                        await modal.RespondAsync("Invalid number of cards. Please enter a value between 1 and 10.", ephemeral: true);
                    }
                    break;

                case "custom_prompt_chance":
                    if (int.TryParse(inputValue, out int customPromptChance) && customPromptChance >= 0 && customPromptChance <= 100)
                    {
                        game.CustomPromptChance = customPromptChance;
                        await modal.RespondAsync($"Custom prompt chance set to **{customPromptChance}%** " + GetHappyEmoji(), ephemeral: true);
                    }
                    else
                    {
                        await modal.RespondAsync("Invalid prompt chance. Please enter a value between 0 and 100.", ephemeral: true);
                    }
                    break;

                case "points_to_win":
                    if (int.TryParse(inputValue, out int pointsToWin) && pointsToWin >= 1)
                    {
                        game.PointsToWin = pointsToWin;
                        await modal.RespondAsync($"Points needed to win set to **{pointsToWin}** " + GetHappyEmoji(), ephemeral: true);
                    }
                    else
                    {
                        await modal.RespondAsync("Invalid points value. Please enter a value greater than 0.", ephemeral: true);
                    }
                    break;

                default:
                    await modal.RespondAsync("Unknown modal submission.", ephemeral: true);
                    break;
            }

            if(game.Phase == PokeGame.GamePhase.INITIALIZATION)
            {
                await game.InitializeMessage(false);
            }
        }

        public async Task ShowModal(string customId, string title, string placeholder, SocketMessageComponent component)
        {
            var modalBuilder = new ModalBuilder()
                .WithTitle(title)
                .WithCustomId(customId)
                .AddTextInput(placeholder, "input", TextInputStyle.Short, minLength: 1, maxLength: 100);

            var modal = modalBuilder.Build();

            await component.RespondWithModalAsync(modal);
        }

        

        private string GetHappyEmoji()
        {
            return TextData.happyEmojis[rdm.Next(0, TextData.happyEmojis.Length)];
        }

        private string GetConfusedEmoji()
        {
            return TextData.confusedEmojis[rdm.Next(0, TextData.confusedEmojis.Length)];
        }

        private string GetSadEmoji()
        {
            return TextData.sadEmojis[rdm.Next(0, TextData.sadEmojis.Length)];
        }

        private string GetAngryEmoji()
        {
            return TextData.angryEmojis[rdm.Next(0, TextData.angryEmojis.Length)];
        }

    }
}