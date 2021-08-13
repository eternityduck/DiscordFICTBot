using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using DS_Bot.Common;
using DS_Bot.Utilities;
using KpiDsLibrary;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace DS_Bot.Services
{
    //TODO Set the welcome message to be changed
    public class CommandHandler : InitializedService
    {
        private readonly IServiceProvider _provider;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _service;
        private readonly IConfiguration _config;
        private readonly Servers _servers;
        
        private readonly Images _images;
        public static List<Mute> Mutes = new List<Mute>();
        public static List<SocketRole> Roles = new List<SocketRole>();

        public CommandHandler(IServiceProvider provider, DiscordSocketClient client, CommandService service,
            IConfiguration config, Servers servers, Images images)
        {
            _provider = provider;
            _client = client;
            _service = service;
            _config = config;
            _servers = servers;
            _images = images;
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
            _client.ReactionRemoved += OnReactionRemoved;
            
            

            var newTask = new Task(async () => await MuteHandler());
            newTask.Start();
            await _service.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
        }


        private async Task OnMessageReceived(SocketMessage arg)
        {
            if (!(arg is SocketUserMessage {Source: MessageSource.User} message)) return;
            if (message.Content.Contains("https://discord.gg/"))
            {
                if (!((SocketGuildChannel) message.Channel).Guild.GetUser(message.Author.Id).GuildPermissions
                    .Administrator)
                {
                    await message.DeleteAsync();
                    await message.Channel.SendErrorAsync("No permission", "You cannot send invite links");
                    return;
                }
                
            }


            var argPos = 0;
            var prefix = await _servers.GetGuildPrefix(((SocketGuildChannel) message.Channel).Guild.Id) ?? "!";

            if (arg.Content == "Стреляю вверх") await arg.Channel.SendMessageAsync("Прямо в рай");
            if (arg.Content == "Если не сосешь мне") await arg.Channel.SendMessageAsync("Тогда живо умирай");
            if (arg.Content == "Доставай наличку") await arg.Channel.SendMessageAsync("Не убирай");
            if (arg.Content == "Ты знаешь я люблю") await arg.Channel.SendMessageAsync("Когда карманы жрут салат");
            if (!message.HasStringPrefix(prefix, ref argPos) &&
                !message.HasMentionPrefix(_client.CurrentUser, ref argPos)) return;

            var context = new SocketCommandContext(_client, message);
            await _service.ExecuteAsync(context, argPos, _provider);
        }

        private async Task OnCommandExecuted(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (command.IsSpecified && !result.IsSuccess)
                await (context.Channel as ISocketMessageChannel).SendErrorAsync("Something went wrong...",
                    $"{result.ErrorReason} ");
        }

        private async Task HandleUserJoined(SocketGuildUser arg)
        {
            // var roles = await _autoRolesHelper.GetAutoRolesAsync(socketGuildUser.Guild);
            // if(roles.Count < 1) return;
            // await socketGuildUser.AddRolesAsync(roles);
            var channelId = await _servers.GetWelcomeAsync(arg.Guild.Id);
            if (channelId == 0) return;
            var channel = arg.Guild.GetTextChannel(channelId);
            if (channel == null)
            {
                await _servers.ClearWelcomeAsync(arg.Guild.Id);
                return;
            }

            var background = await _servers.GetBackgroundAsync(arg.Guild.Id);

            string path = await _images.CreateImageAsync(arg, background);
            await channel.SendFileAsync(path, null);
            File.Delete(path);
            await arg.SendMessageAsync(
                $"Привет, мы ждали именно тебя.\nДобро пожаловать на **FІCT.TALKING & GAMING!**\nЗайди и получи роли в отдельном канале {arg.Guild.GetTextChannel(759794521243779084).Mention} и ознакомься с нашими правилами в  {arg.Guild.GetTextChannel(755005977148915754).Mention} :stuck_out_tongue_winking_eye:");

            await channel
                .SendMessageAsync(
                    $"Привет, мы ждали именно тебя, {arg.Mention}.\nДобро пожаловать на **FІCT.TALKING & GAMING!**\nЗайди и получи роли в отдельном канале {arg.Guild.GetTextChannel(759794521243779084).Mention} и ознакомься с нашими правилами в  {arg.Guild.GetTextChannel(755005977148915754).Mention} :stuck_out_tongue_winking_eye:");
        }

        private async Task UserJoined(SocketGuildUser arg)
        {
            var newTask = new Task(async () => await HandleUserJoined(arg));
            newTask.Start();
        }


        private async Task UserLeft(SocketGuildUser arg)
        {
            await arg.Guild.TextChannels.First(x => x.Name == "🆕гостевой")
                .SendMessageAsync($"Прощай, {arg.Mention}");
        }

        private async Task OnChannelCreated(SocketChannel arg)
        {
            if ((arg as ITextChannel) == null) return;
            var channel = arg as ITextChannel;

            await channel.SendMessageAsync("Thank u for creating the channel, I am watching you");
        }

        private async Task OnJoinedGuild(SocketGuild arg)
        {
            await arg.DefaultChannel.SendMessageAsync("Thank you for using KPI bot!!! Type !help to get all commands");
        }

        private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2,
            SocketReaction arg3)
        {
            if (arg1.Id != 871070199107973120) return;

            if (arg3.Emote.Name != "👍") return;

            var role = (arg2 as SocketGuildChannel)?.Guild.Roles.FirstOrDefault(x => x.Id == 871070668014379028);
            if (arg3.User.Value != null) await ((SocketGuildUser) arg3.User.Value)?.AddRoleAsync(role);
        }

        private Task OnReactionRemoved(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2,
            SocketReaction arg3)
        {
            throw new NotImplementedException();
        }

        private async Task MuteHandler()
        {
            List<Mute> removes = new List<Mute>();
            foreach (var mute in Mutes)
            {
                if (DateTime.Now < mute.End)
                {
                    continue;
                }

                var guild = _client.GetGuild(mute.Guild.Id);
                if (guild.GetRole(mute.Role.Id) == null)
                {
                    removes.Add(mute);
                    continue;
                }

                var role = guild.GetRole(mute.Role.Id);
                if (guild.GetUser(mute.User.Id) == null)
                {
                    removes.Add(mute);
                    continue;
                }

                var user = guild.GetUser(mute.User.Id);
                if (role.Position > guild.CurrentUser.Hierarchy)
                {
                    removes.Add(mute);
                    continue;
                }

                await user.RemoveRoleAsync(mute.Role);
                //if(Roles != null) await user.AddRolesAsync(Roles);
                removes.Add(mute);
            }

            Mutes = Mutes.Except(removes).ToList();
            await Task.Delay(1 * 60 * 1000);
            await MuteHandler();
        }
    }
}