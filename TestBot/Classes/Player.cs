using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGame.Classes
{
    public class Player
    {
        public SocketUser User { get; set; }
        public PokeGame Game { get; set; }
        public List<Pokemon> Hand {  get; set; }
        public int Wins { get; set; } = 0;
        public bool host { get; set; } = false;
        public string AvatarURL { get; set; }
        private HttpClient httpClient = StaticHTTPClient.httpClient;
        public List<int> ResponseNumbers { get; set; } = new List<int>();
        public bool hasPresented { get; set; } = true;
        private IUserMessage handMessage = null;
        public List<int> SubmittedResponses { get; set; } = new List<int>();
        public bool ReadyToPresent { get; set; } = false;

        public Player(SocketUser user, PokeGame game) 
        { 
            User = user;
            Hand = new List<Pokemon>();
            Wins = 0;
            AvatarURL = user.GetDisplayAvatarUrl();
            Game = game;
        }

        public Player(SocketUser user, bool host, PokeGame game)
        {
            User = user;
            Hand = new List<Pokemon>();
            Wins = 0;
            this.host = host;
            AvatarURL = user.GetDisplayAvatarUrl();
            Game = game;
        }

        public void DrawCards()
        {
            for(int i = Hand.Count; i < Game.PlayerCards; i++)
            {
                Hand.Add(Game.pokemonDeck[0]);
                Game.pokemonDeck.RemoveAt(0);
            }
        }

        public async Task DisplayHand()
        {
            var attachments = new List<FileAttachment>();

            ResponseNumbers = new List<int>();
            SubmittedResponses = new List<int>();

            foreach (var pokemon in Hand)
            {
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

            var embed = new EmbedBuilder()
                .WithTitle($"{User.Username}'s Hand")
                .WithDescription("Select your picks!")
                .WithImageUrl(Game.prompts[0].url)
                .Build();

            if(handMessage != null)
            {
                await handMessage.DeleteAsync();
            }
            // Send files asynchronously
            handMessage = await User.SendFilesAsync(attachments, embed: embed, 
                components:GetPokemonButtons().Build());

            // Dispose of the streams after sending
            foreach (var attachment in attachments)
            {
                attachment.Dispose(); // This will dispose of the FileStream used in the FileAttachment
            }
        }

        public async Task UpdateHandMessage()
        {
            if(handMessage != null)
            {
                await handMessage.ModifyAsync(m => 
                m.Components = GetPokemonButtons().Build());
            }
        }

        private ComponentBuilder GetPokemonButtons()
        {
            var buttons = new ComponentBuilder();

            if (ResponseNumbers.Count < Game.prompts[0].responsesNeeded)
            {
                for(int i = 0; i < Hand.Count; i++)
                {
                    if (!ResponseNumbers.Contains(i))
                    {
                        buttons.WithButton(Hand[i].Name, "pokemon_select_" + i);
                    }
                    else
                    {
                        buttons.WithButton(Hand[i].Name, "pokemon_select_" + i, disabled:true);
                    }
                }
                buttons.WithButton("Undo", "undo_select", ButtonStyle.Secondary);
                buttons.WithButton("Reset", "reset_select", ButtonStyle.Danger);
            } else
            {
                foreach (int responseNumber in ResponseNumbers)
                {
                    buttons.WithButton(Hand[responseNumber].Name, "pokemon_select_" + 
                        responseNumber, disabled:true);
                }
                buttons.WithButton("Submit", "submit_select", ButtonStyle.Success);
                buttons.WithButton("Undo", "undo_select", ButtonStyle.Secondary);
                buttons.WithButton("Reset", "reset_select", ButtonStyle.Danger);
            }

            return buttons;
        }

        public async Task SubmitSelection()
        {
            SubmittedResponses = new List<int>(ResponseNumbers);
            ReadyToPresent = true;
            if (Game.Players[Game.nextPlayerToPresent] == this && Game.ReadyForNextPresentation)
            {
                await ShowSubmission();
            }
        }

        public async Task ShowSubmission()
        {
            hasPresented = true;
            await Game.DisplayPokemonMessage();
        }

        public void ResetSelection()
        {
            ResponseNumbers = new List<int>();
        }

        public void UndoSelection()
        {
            ResponseNumbers.RemoveAt(ResponseNumbers.Count - 1);
        }

        public void AddResponse(int responseNo)
        {
            ResponseNumbers.Add(responseNo);
        }
        

    }
}
