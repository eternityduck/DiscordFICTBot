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

        public MessageModule(ILogger<MessageModule> logger, RanksHelper ranksHelper)
            => (_logger, _ranksHelper) = (logger, ranksHelper);

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

        [Command("math")]
        public async Task MathAsync([Remainder] string math)
        {
            var dt = new DataTable();
            var result = dt.Compute(math, null);

            await ReplyAsync($"Result: {result}");
            _logger.LogInformation($"{Context.User.Username} executed the math command!");
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

        [Command("info")]
        public async Task InfoAsync(SocketGuildUser socketGuildUser = null)
        {
            if (socketGuildUser is null)
            {
                socketGuildUser = Context.User as SocketGuildUser;
            }

            await ReplyAsync($"ID: {socketGuildUser.Id}\n" + $"Name: {socketGuildUser.Username}");
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
    }
}