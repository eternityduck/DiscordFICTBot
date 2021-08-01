using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using DS_Bot.Utilities;
using KpiDsLibrary;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace DS_Bot.Services
{
    public class CommandHandler : InitializedService
    {
        private readonly IServiceProvider _provider;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _service;
        private readonly IConfiguration _config;
        private readonly Servers _servers;
        private readonly AutoRolesHelper _autoRolesHelper;

        public CommandHandler(IServiceProvider provider, DiscordSocketClient client, CommandService service,
            IConfiguration config, Servers servers, AutoRolesHelper autoRolesHelper)
        {
            _provider = provider;
            _client = client;
            _service = service;
            _config = config;
            _servers = servers;
            _autoRolesHelper = autoRolesHelper;
        }

        public override async Task InitializeAsync(CancellationToken cancellationToken)
        {
            _client.MessageReceived += OnMessageReceived;
            _service.CommandExecuted += OnCommandExecuted;
            _client.ChannelCreated += OnChannelCreated;
            _client.ReactionAdded += OnReactionAdded;
            _client.JoinedGuild += OnJoinedGuild;
            _client.UserJoined += UserJoined;
            _client.UserLeft += UserLeft;
            await _service.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
        }

        


        private async Task OnMessageReceived(SocketMessage arg)
        {
            if (!(arg is SocketUserMessage message)) return;
            if (message.Source != MessageSource.User) return;

            var argPos = 0;
            var prefix = await _servers.GetGuildPrefix(((SocketGuildChannel) message.Channel).Guild.Id) ?? "!";
            if (!message.HasStringPrefix(prefix, ref argPos) &&
                !message.HasMentionPrefix(_client.CurrentUser, ref argPos)) return;

            var context = new SocketCommandContext(_client, message);
            await _service.ExecuteAsync(context, argPos, _provider);
        }

        private async Task OnCommandExecuted(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (command.IsSpecified && !result.IsSuccess) await context.Channel.SendMessageAsync($"Error: {result}");
        }

        private async Task UserJoined(SocketGuildUser socketGuildUser)
        {
            // var roles = await _autoRolesHelper.GetAutoRolesAsync(socketGuildUser.Guild);
            // if(roles.Count < 1) return;
            // await socketGuildUser.AddRolesAsync(roles);
            
            await socketGuildUser.SendMessageAsync($"Hello {socketGuildUser.Mention}");
          
            await socketGuildUser.Guild.TextChannels.First(x => x.Name == "🆕гостевой")
                .SendMessageAsync($"Привет, мы ждали именно тебя, {socketGuildUser.Mention}.\nДобро пожаловать на **FІCT.TALKING & GAMING!**\nЗайди и получи роли в отдельном канале {socketGuildUser.Guild.GetTextChannel(759794521243779084).Mention} и ознакомься с нашими правилами в  {socketGuildUser.Guild.GetTextChannel(755005977148915754).Mention} :stuck_out_tongue_winking_eye:");
        }
        private async Task UserLeft(SocketGuildUser arg)
        {
            await arg.Guild.TextChannels.First(x => x.Name == "🆕гостевой")
                .SendMessageAsync($"Прощай, {arg.Mention}");
        }
        private async Task OnChannelCreated(SocketChannel arg)
        {
            if((arg as ITextChannel) == null) return;
            var channel = arg as ITextChannel;

            await channel.SendMessageAsync("The event was called");
        }
        private async Task OnJoinedGuild(SocketGuild arg)
        {
            await arg.DefaultChannel.SendMessageAsync("Thank you for using KPI bot!!! Type !help to get all commands");
        }
        private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            if(arg1.Id != 871070199107973120) return;
            
            if(arg3.Emote.Name != "👍") return;

            var role = (arg2 as SocketGuildChannel)?.Guild.Roles.FirstOrDefault(x => x.Id == 871070668014379028);
            if (arg3.User.Value != null) await ((SocketGuildUser) arg3.User.Value)?.AddRoleAsync(role);
        }
    }
}