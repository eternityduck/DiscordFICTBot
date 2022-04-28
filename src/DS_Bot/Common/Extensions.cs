using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace DS_Bot.Common
{
    public static class Extensions
    {
        public static async Task<IMessage> SendSuccessAsync(this ISocketMessageChannel channel, string title, string desc)
        {
            var embed = new EmbedBuilder()
                .WithColor(new Color(137, 250, 242))
                .WithTitle(title)
                .WithDescription(desc)
                .WithAuthor(author =>
                {
                    author.WithIconUrl(
                            "https://icons-for-free.com/iconfiles/png/512/complete+done+green+success+valid+icon-1320183462969251652.png")
                        .WithName(title);
                })
                .Build();
            var msg = await channel.SendMessageAsync(embed: embed);
            return msg;
        }
        public static async Task<IMessage> SendErrorAsync(this ISocketMessageChannel channel, string title, string desc)
        {
            var embed = new EmbedBuilder()
                .WithColor(new Color(240, 32, 17))
                .WithTitle(title)
                .WithDescription(desc)
                .WithAuthor(author =>
                {
                    author.WithIconUrl(
                            "https://icon-library.com/images/failed-icon/failed-icon-7.jpg")
                        .WithName(title);
                })
                .Build();
            var msg = await channel.SendMessageAsync(embed: embed);
            return msg;
        }
        public static async Task<IMessage> SendLogAsync(this ITextChannel channel, string title, string description)
        {
            var embed = new EmbedBuilder()
                .WithColor(new Color(26, 155, 226))
                .WithDescription(description)
                .WithAuthor(author =>
                {
                    author
                        .WithIconUrl("https://i.imgur.com/gLR4k7d.png")
                        .WithName(title);
                })
                .Build();
            
            var message = await channel.SendMessageAsync(embed: embed);
            return message;
        }
    }
}