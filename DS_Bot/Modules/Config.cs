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
        private readonly RanksHelper _ranksHelper;
        private readonly AutoRolesHelper _rolesHelper;
        private readonly Servers _servers;
        private readonly Ranks _ranks;
        private readonly AutoRoles _autoRoles;

        public Config(RanksHelper ranksHelper, AutoRolesHelper autoRolesHelper, Servers servers, Ranks ranks, AutoRoles autoRoles) =>
            (_ranksHelper, _rolesHelper, _servers, _ranks, _autoRoles) = (ranksHelper, autoRolesHelper, servers, ranks, autoRoles);

        [Command("ranks", RunMode = RunMode.Async)]
        public async Task Ranks()
        {
            var ranks = await _ranksHelper.GetRanksAsync(Context.Guild);
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
        public async Task AddRankAsync([Remainder] string name)
        {
            await Context.Channel.TriggerTypingAsync();
            var ranks = await _ranksHelper.GetRanksAsync(Context.Guild);

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
        public async Task DeleteRankAsync([Remainder] string name)
        {
            await Context.Channel.TriggerTypingAsync();
            var ranks = await _ranksHelper.GetRanksAsync(Context.Guild);
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
        public async Task AutoRoles()
        {
            var autoRoles = await _rolesHelper.GetAutoRolesAsync(Context.Guild);
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
        public async Task AddAutoRoleAsync([Remainder] string name)
        {
            await Context.Channel.TriggerTypingAsync();
            var autoRoles = await _rolesHelper.GetAutoRolesAsync(Context.Guild);

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
        public async Task DeleteAutoRoleAsync([Remainder] string name)
        {
            await Context.Channel.TriggerTypingAsync();
            var autoRolesAsync = await _rolesHelper.GetAutoRolesAsync(Context.Guild);
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
        }
    }
}