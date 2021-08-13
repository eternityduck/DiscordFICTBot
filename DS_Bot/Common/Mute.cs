using System;
using Discord;
using Discord.WebSocket;

namespace DS_Bot.Common
{
    public class Mute
    {
        public SocketGuild Guild;
        public SocketGuildUser User;
        public IRole Role;
        public DateTime End;
    }
}