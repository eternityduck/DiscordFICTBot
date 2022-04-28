using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using DS_Bot.Utilities;
using KpiDsLibrary;

namespace DS_Bot.Modules
{
    public class Config : ModuleBase<SocketCommandContext>
    {
        private readonly Servers _servers;
        private readonly Ranks _ranks;
        private readonly AutoRoles _autoRoles;
        private readonly ServerHelper _serverHelper;

        public Config(Servers servers, Ranks ranks, AutoRoles autoRoles, ServerHelper serverHelper) =>
            ( _servers, _ranks, _autoRoles, _serverHelper) = ( servers, ranks, autoRoles, serverHelper);

        [Summary("Gets all ranks from server")]
        [Command("ranks", RunMode = RunMode.Async)]
        public async Task Ranks()
        {
            var ranks = await _serverHelper.GetRanksAsync(Context.Guild);
            if (ranks.Count == 0)
            {
                await ReplyAsync("This server doesn`t have any ranks!");
                return;
            }

            await Context.Channel.TriggerTypingAsync();
            string desc = "This message lists all available ranks";
            foreach (var rank in ranks)
            {
                desc += $"\n{rank.Mention} ({rank.Id})";
            }

            await ReplyAsync(desc);
        }

        
        [Command("addrank", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [Summary("Adds the rank")]
        public async Task AddRankAsync([Remainder] string name)
        {
            await Context.Channel.TriggerTypingAsync();
            var ranks = await _serverHelper.GetRanksAsync(Context.Guild);

            var role = Context.Guild.Roles.FirstOrDefault(x =>
                string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
            if (role is null)
            {
                await ReplyAsync("That role doesn`t exist");
                return;
            }

            if (role.Position > Context.Guild.CurrentUser.Hierarchy)
            {
                await ReplyAsync("That role has higher than the bot!");
                return;
            }

            if (ranks.Any(x => x.Id == role.Id))
            {
                await ReplyAsync("That role is already a rank!");
                return;
            }

            await _ranks.AddRankAsync(Context.Guild.Id, role.Id);
            await ReplyAsync($"Successfully added the {role.Mention}");
        }

        [Command("delrank", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [Summary("Deletes the rank")]
        public async Task DeleteRankAsync([Remainder] string name)
        {
            await Context.Channel.TriggerTypingAsync();
            var ranks = await _serverHelper.GetRanksAsync(Context.Guild);
            var role = Context.Guild.Roles.FirstOrDefault(x =>
                string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
            if (role is null)
            {
                await ReplyAsync("That role doesn`t exist");
                return;
            }
            if (role.Position > Context.Guild.CurrentUser.Hierarchy)
            {
                await ReplyAsync("That role has higher than the bot!");
                return;
            }
            if (ranks.Any(x => x.Id != role.Id))
            {
                await ReplyAsync("That role is not a rank!");
                return;
            }

            await _ranks.RemoveRankAsync(Context.Guild.Id, role.Id);
            await ReplyAsync($"The {role.Mention} has been removed");
        }
        [Command("autoroles", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("Gets autoroles from server")]
        public async Task AutoRoles()
        {
            var autoRoles = await _serverHelper.GetAutoRolesAsync(Context.Guild);
            if (autoRoles.Count == 0)
            {
                await ReplyAsync("This server doesn`t have any autoRoles!");
                return;
            }

            await Context.Channel.TriggerTypingAsync();
            string desc = "This message lists all available autoRoles";
            foreach (var autorole in autoRoles)
            {
                desc += $"\n{autorole.Mention} ({autorole.Id})";
            }

            await ReplyAsync(desc);
        }

        
        [Command("addautorole", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [Summary("Adds role to the autoroles")]
        public async Task AddAutoRoleAsync([Remainder] string name)
        {
            await Context.Channel.TriggerTypingAsync();
            var autoRoles = await _serverHelper.GetAutoRolesAsync(Context.Guild);

            var role = Context.Guild.Roles.FirstOrDefault(x =>
                string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
            if (role is null)
            {
                await ReplyAsync("That role doesn`t exist");
                return;
            }

            if (autoRoles.Any(x => x.Id == role.Id))
            {
                await ReplyAsync("That role is already a rank!");
                return;
            }

            await _autoRoles.AddAutoRoleAsync(Context.Guild.Id, role.Id);
            await ReplyAsync($"Successfully added the {role.Mention}");
        }
        [Command("delautorole", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [Summary("Deletes role from autoroles")]
        public async Task DeleteAutoRoleAsync([Remainder] string name)
        {
            await Context.Channel.TriggerTypingAsync();
            var autoRolesAsync = await _serverHelper.GetAutoRolesAsync(Context.Guild);
            var role = Context.Guild.Roles.FirstOrDefault(x =>
                string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
            if (role is null)
            {
                await ReplyAsync("That role doesn`t exist");
                return;
            }
            if (autoRolesAsync.Any(x => x.Id != role.Id))
            {
                await ReplyAsync("That role is not a autorole!");
                return;
            }

            await _autoRoles.RemoveAutoRoleAsync(Context.Guild.Id, role.Id);
            await ReplyAsync($"The {role.Mention} has been removed");
        }
        [Command("prefix", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("Changes the prefix on server")]
        public async Task PrefixAsync(string prefix = null)
        {
            if (prefix == null)
            {
                var guildPrefix = await _servers.GetGuildPrefix(Context.Guild.Id) ?? "!";
                await ReplyAsync($"Current prefix is `{guildPrefix}`");
                return;
            }

            if (prefix.Length > 5)
            {
                await ReplyAsync("The Length of new prefix is too long, type something shorter");
            }

            await _servers.ModifyPrefix(Context.Guild.Id, prefix);
            await ReplyAsync($"The prefix was adjusted to `{prefix}`");
            await _serverHelper.SendLogAsync(Context.Guild, "Prefix adjusted", $"Modify the prefix to {prefix}");
        }
        [Command("logs")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("Sets up the logs channel")]
        public async Task Logs(string value = null)
        {
            if (value is null)
            {
                var fetched = await _servers.GetLogsAsync(Context.Guild.Id);
                if (fetched == 0)
                {
                    await ReplyAsync("There is not a logs channel yet! Set it up!");
                    return;
                }

                var fetchedChannel = Context.Guild.GetTextChannel(fetched);
                if (fetchedChannel is null)
                {
                    await ReplyAsync("There is not a logs channel yet! Set it up!");
                    await _servers.ClearWelcomeAsync(Context.Guild.Id);
                }

                await ReplyAsync($"The channel for logs is set to {fetchedChannel?.Mention}");

                return;
            }

            if (value != "clear")
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

                await _servers.ModifyLogsAsync(Context.Guild.Id, parseId);
                await ReplyAsync($"Successfully modified the logs channel to {Context.Channel.Name}");
                return;
            }

            await _servers.ClearWelcomeAsync(Context.Guild.Id);
            await ReplyAsync("Successfully cleared the logs channel");
        }
    }
}