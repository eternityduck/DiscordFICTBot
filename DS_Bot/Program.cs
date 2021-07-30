using System;
using System.Threading.Channels;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
namespace DS_Bot
{
    public class Program
    {
        private DiscordSocketClient _client;
	
        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            _client = new DiscordSocketClient();
            _client.MessageReceived += CommandsHandler;
            _client.Log += Log;

            var token = "ODcwNTk5NjUxMDQzMTg0NzE0.YQPHGw.uXN4Dy2S2R9YI2KiObIghFX9afA";
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();
            
            await Task.Delay(-1);
            Console.ReadLine();
        }

        private Task CommandsHandler(SocketMessage arg)
        {
            if (!arg.Author.IsBot)
            {
                switch (arg.Content)
                {
                    case "!hi":
                        arg.Channel.SendMessageAsync($"Hi {arg.Author.Mention}");
                        break;

                }
            }

            return Task.CompletedTask;
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}