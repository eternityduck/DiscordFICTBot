
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;

using DS_Bot.Common;
using KpiDsLibrary;

namespace DS_Bot.Modules
{
    public class InteractiveModule : InteractiveBase
    {
        private readonly CommandService _service;
        private readonly Servers _servers;
        public InteractiveModule(CommandService service, Servers servers) => (_service, _servers) = (service, servers);
        
        
        [Command("next", RunMode = RunMode.Async)]
        public async Task Test_NextMessageAsync()
        {
            await ReplyAsync("What is 2+2?");
            var response = await NextMessageAsync();
            if (response != null)
                await ReplyAsync($"You replied: {response.Content}");
            else
                await ReplyAsync("You did not reply before the timeout");
        }

        
        [Command("paginator")]
        [Summary("This is will create a paginator")]
        public async Task Test_Paginator()
        {
            List<string> pages = new List<string> {"**Help command**`!help-get all commands`"};
            PaginatedMessage paginatedMessage = new PaginatedMessage()
            {
                Pages = pages,
                Options = new PaginatedAppearanceOptions()
                {
                    InformationText = "This is a test",
                    //Info = new Emoji("😎"),
                },
                Color = Color.Green,
                Title = "Testing something"
            };
            
            await PagedReplyAsync(paginatedMessage);
        }
        [Summary("Gets all commands")]
        [Command("help")]
        public async Task Help()
        {
            List<string> pages = new List<string>();
            string prefix = await _servers.GetGuildPrefix(Context.Guild.Id);
            foreach (var ser in _service.Modules)
            {
                string page = $"**{ser.Name}**\n\n";
                foreach (var command in ser.Commands)
                {
                    page += $"{prefix}`{command.Name}` - {command.Summary ?? "No description of command"}\n";
                }
                pages.Add(page);
            }
            PaginatedMessage paginatedMessage = new PaginatedMessage()
            {
                Pages = pages,
                Color = Color.Green,
            };
            await PagedReplyAsync(paginatedMessage);
        }
   
        [Summary("Use this command like 'title and choices', remember that title must be in quotes")]
        [Command("poll")]
        public async Task PollAsync( string title, params string[] choices)
        {
            
            if (choices == null)
            {
                await Context.Channel.SendErrorAsync("Null", "The number of options can not be empty");
                return;
            }

            if (choices.Length > 9)
            {
                await Context.Channel.SendErrorAsync("Error", "The number of options can not be more than 10");
                return;
            }
            List<IEmote> emojis = new List<IEmote>()
            {
                new Emoji("1️⃣"), new Emoji("2️⃣"), new Emoji("3️⃣"), new Emoji("4️⃣"),
                new Emoji("5️⃣"), new Emoji("6️⃣"),new Emoji("7️⃣"), new Emoji("8️⃣"), new Emoji("9️⃣")
            };
           
            var embed = new EmbedBuilder()
                .WithTitle($"**{title}**")
                .WithColor(Color.Magenta);
            string description = "";
            for (int j = 0; j < choices.Length; j++)
            {
                description += $"{emojis[j]} {choices[j]}\n ";
            }
            
            var msg = await this.Context.Channel.SendMessageAsync(string.Empty, false, embed.WithDescription(description).Build());
            await msg.AddReactionsAsync(emojis.TakeWhile((x, y)=> y != choices.Length).ToArray());
            
        }

        [Command("rolegive")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task RoleGiveAsync()
        {
            
        }
        [Command("createcommand")]
        public async Task CreateCategory(string commandName)
        {
            
        }
    }
}