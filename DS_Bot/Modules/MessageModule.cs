using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DS_Bot.Utilities;
using KpiDsLibrary;
using Microsoft.Extensions.Logging;

namespace DS_Bot.Modules
{
    public class MessageModule : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<MessageModule> _logger;
        private readonly RanksHelper _ranksHelper;
        private readonly Servers _servers;
        public MessageModule(ILogger<MessageModule> logger, RanksHelper ranksHelper, Servers servers)
            => (_logger, _ranksHelper, _servers) = (logger, ranksHelper, servers);

        [Command("ping")]
        //[Alias("p")] сокращение
        public async Task PingAsync()
        {
            await Context.Channel.TriggerTypingAsync();
            await Context.Channel.SendMessageAsync("Pon");
            await Context.User.SendMessageAsync("qq");
        }

        [Command("test")]
        public async Task TestAsync()
        {
            await Context.Channel.SendMessageAsync("**FІCT. TALKING & GAMING!**");
        }
        
        [Command("delete")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task DeleteAmountOfMsgAsync(int amount)
        {
            IEnumerable<IMessage> messages = await Context.Channel.GetMessagesAsync(amount + 1).FlattenAsync();
            await ((ITextChannel) Context.Channel).DeleteMessagesAsync(messages);
            const int delay = 3000;
            IUserMessage m = await ReplyAsync($"I have deleted {amount} messages for ya. :)");
            await Task.Delay(delay);
            await m.DeleteAsync();
        }
        

        [Command("cry")]
        public async Task CryAsync()
        {
            await ReplyAsync($"{Context.User.Username} is crying");
        }

        [Command("rank", RunMode = RunMode.Async)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task RankAsync([Remainder] string name)
        {
            await Context.Channel.TriggerTypingAsync();
            var ranks = await _ranksHelper.GetRanksAsync(Context.Guild);

            IRole role;

            if (ulong.TryParse(name, out ulong roleId))
            {
                var roleById = Context.Guild.Roles.FirstOrDefault(x => x.Id == roleId);
                if (roleById is null)
                {
                    await ReplyAsync("That role doesn`t exist");
                    return;
                }

                role = roleById;
            }


            else
            {
                var roleByName = Context.Guild.Roles.FirstOrDefault(x =>
                    string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
                if (roleByName is null)
                {
                    await ReplyAsync("That role doesn`t exist");
                    return;
                }

                role = roleByName;
            }

            if (ranks.Any(x => x.Id != role.Id))
            {
                await ReplyAsync("That role doesn`t exist");
                return;
            }

            if (((SocketGuildUser) Context.User).Roles.Any(x => x.Id == role.Id))
            {
                await ((SocketGuildUser) Context.User).RemoveRoleAsync(role);
                await ReplyAsync($"Successfully removed the rank {role.Mention} ");
                return;
            }
            await ((SocketGuildUser) Context.User).AddRoleAsync(role);
            await ReplyAsync($"Successfully added the rank {role.Mention} ");
        }
        
        [Command("welcome")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Welcome(string option = null, string value = null)
        {
            if (option is null && value is null)
            {
                var fetched = await _servers.GetWelcomeAsync(Context.Guild.Id);
                if (fetched == 0)
                {
                    await ReplyAsync("There is not a welcome channel yet! Set it up!");
                    return;
                }

                var fetchedChannel = Context.Guild.GetTextChannel(fetched);
                if (fetchedChannel is null)
                {
                    await ReplyAsync("There is not a welcome channel yet! Set it up!");
                    await _servers.ClearWelcomeAsync(Context.Guild.Id);
                }
                var fetchedBack = await _servers.GetBackgroundAsync(Context.Guild.Id);
                if (fetchedBack != null)
                {
                    await ReplyAsync($"The channel used for welcome module is {fetchedChannel.Mention}.\n The background is {fetchedBack}.");
               
                }
                else
                {
                    await ReplyAsync($"The channel used for welcome module is {fetchedChannel?.Mention}");
                }
                return;
            }

            if (option == "channel" && value != null)
            {
                if (!MentionUtils.TryParseChannel(value, out ulong parseId))
                {
                    await ReplyAsync("Please pass a valid channel");
                    return;
                }

                var parsedChannel = Context.Guild.GetTextChannel(parseId);
                if (parsedChannel is null)
                {
                    await ReplyAsync("Please pass a valid channel");
                    return;
                }
                await _servers.ModifyWelcomeAsync(Context.Guild.Id, parseId);
                await ReplyAsync($"Successfully modified");
                return;
            }
            if (option == "background" && value != null)
            {
                if (value == "clear")
                {
                    await _servers.ClearBackgroundAsync(Context.Guild.Id);
                    await ReplyAsync("Successfully cleared the background");
                    return;
                }
                await _servers.ModifyBackgroundAsync(Context.Guild.Id, value);
                await ReplyAsync($"Successfully modified");
                return;
            }

            if (option == "clear" && value == null)
            {
                await _servers.ClearWelcomeAsync(Context.Guild.Id);
                await ReplyAsync("Successfully cleared the welcome channel");
                return;
            }
            
        }
        
    }
}