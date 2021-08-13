using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DS_Bot.Common;
using DS_Bot.Services;
using DS_Bot.Utilities;
using KpiDsLibrary;
using Microsoft.Extensions.Logging;

namespace DS_Bot.Modules
{
   
    //TODO Use the extenstion methods
    public class MessageModule : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<MessageModule> _logger;
        private readonly ServerHelper _serverHelper;
        private readonly Servers _servers;

        public MessageModule(ILogger<MessageModule> logger, ServerHelper ranksHelper, Servers servers)
            => (_logger, _serverHelper, _servers) = (logger, ranksHelper, servers);
        
        //[Alias("p")] сокращение
        [Command("test")]
        public async Task TestAsync()
        {
            await Context.Channel.SendSuccessAsync("Check", "the result is tested method");
        }

        [Command("delete")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [Summary("Pass the amount of messages to be deleted")]
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
        [Summary("Adds or removes the rank")]
        public async Task RankAsync([Remainder] string name)
        {
            await Context.Channel.TriggerTypingAsync();
            var ranks = await _serverHelper.GetRanksAsync(Context.Guild);

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
        [Summary("Setting up the welcome background and message, pass like 'option(channel, background)'")]
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
                    await ReplyAsync(
                        $"The channel used for welcome module is {fetchedChannel.Mention}.\n The background is {fetchedBack}.");
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

        [Command("mute")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [Summary("Mutes the person")]
        public async Task Mute(SocketGuildUser user, int minutes, [Remainder] string reason = null)
        {
            
            if (user.Hierarchy > Context.Guild.CurrentUser.Hierarchy)
            {
                await Context.Channel.SendErrorAsync("Invalid user", "This user has a higher permission than bot");
                return;
            }
       
            var role = (Context.Guild as IGuild).Roles.FirstOrDefault(fi => fi.Name == "Muted");
            if (role == null)
                role = await Context.Guild.CreateRoleAsync("Muted", new GuildPermissions(sendMessages: false), null,
                    false, null);
            if (role.Position > Context.Guild.CurrentUser.Hierarchy)
            {
                await Context.Channel.SendErrorAsync("Invalid user", "This user has a higher permission than bot");
                return;
            }

            if (user.Roles.Contains(role))
            {
                await Context.Channel.SendErrorAsync("Invalid", "Already muted");
                return;
            }
            // CommandHandler.Roles.AddRange(user.Roles);
            // var rolesToRemove = user.Roles.Where(x => x.Id != 753345298184667206).ToList();
            // await user.RemoveRolesAsync(rolesToRemove);
            await role.ModifyAsync(x => x.Position = Context.Guild.CurrentUser.Hierarchy);

            foreach (var ch in Context.Guild.TextChannels)
            {
                if (!ch.GetPermissionOverwrite(role).HasValue ||
                    ch.GetPermissionOverwrite(role).Value.SendMessages == PermValue.Allow)
                {
                    await ch.AddPermissionOverwriteAsync(role, new OverwritePermissions(sendMessages: PermValue.Deny));
                }
            }
            CommandHandler.Mutes.Add(new Mute(){Guild = Context.Guild, User = user, End = DateTime.Now +TimeSpan.FromMinutes(minutes), Role = role});
            
            await user.AddRoleAsync(role);
            await Context.Channel.SendSuccessAsync("Muted", $"{user.Mention} were muted by reason: {reason ?? "None"}");
        }

        [Command("unmute")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [Summary("Unmutes the person")]
        public async Task UnMute(SocketGuildUser user)
        {
            var role = (Context.Guild as IGuild).Roles.FirstOrDefault(fi => fi.Name == "Muted");
            if (role == null)
            {
                await Context.Channel.SendErrorAsync("Not muted", "This user has not been muted");
                return;
            }
                
            if (role.Position > Context.Guild.CurrentUser.Hierarchy)
            {
                await Context.Channel.SendErrorAsync("Invalid user", "This user has a higher permission than bot");
                return;
            }

            if (user.Roles.Contains(role))
            {
                await Context.Channel.SendErrorAsync("Invalid", "Not muted");
                return;
            }
            
            await user.RemoveRoleAsync(role);
            await Context.Channel.SendSuccessAsync($"UnMuted {user.Mention}", $"{user.Mention} were unmuted");
        }

        [Command("slowmode")]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        [Summary("Slowmodes the channel")]
        public async Task SlowMode(int interval = 0)
        {
            await ((SocketTextChannel) Context.Channel).ModifyAsync(x => x.SlowModeInterval = interval);
            await ReplyAsync($"Now channel has the slowmode in {interval} seconds");
            await _serverHelper.SendLogAsync(Context.Guild, $"Slowmoded", $"Slowmode in {Context.Channel} for {interval} seconds");
        }
        
        
    }
}