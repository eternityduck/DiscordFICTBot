using System.Data;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace DS_Bot.Modules
{
    public class MessageModule : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<MessageModule> _logger;

        public MessageModule(ILogger<MessageModule> logger)
            => _logger = logger;

        [Command("ping")]
        //[Alias("p")] сокращение
        //[RequireUserPermission(GuildPermission.Administrator)]
        public async Task PingAsync()
        {
            await Context.Channel.TriggerTypingAsync();
            await Context.Channel.SendMessageAsync("Pon");
            await Context.User.SendMessageAsync("qq");
        }

        [Command("math")]
        public async Task MathAsync([Remainder] string math)
        {
            var dt = new DataTable();
            var result = dt.Compute(math, null);
            
            await ReplyAsync($"Result: {result}");
            _logger.LogInformation($"{Context.User.Username} executed the math command!");
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
        
       
    }
}