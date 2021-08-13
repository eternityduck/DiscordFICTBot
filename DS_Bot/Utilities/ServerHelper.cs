using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using DS_Bot.Common;
using KpiDsLibrary;

namespace DS_Bot.Utilities
{
    public class ServerHelper
    {
        private readonly Servers _servers;
        private readonly Ranks _ranks;
        private readonly AutoRoles _roles;
        public ServerHelper(Servers servers, Ranks ranks, AutoRoles roles) => (_servers, _ranks, _roles) = (servers, ranks, roles);
        

        public async Task SendLogAsync(IGuild guild, string title, string desc)
        {
            var channelId = await _servers.GetLogsAsync(guild.Id);
            if(channelId == 0) return;
            var fetchedChannel = await guild.GetTextChannelAsync(channelId);
            if (fetchedChannel == null)
            {
                await _servers.ClearLogsAsync(guild.Id);
                return;
            }

            await fetchedChannel.SendLogAsync(title, desc);
        }
        public async Task<List<IRole>> GetRanksAsync(IGuild guild)
        {
            var roles = new List<IRole>();
            var invalid = new List<Rank>();

            var ranks = await _ranks.GerRankAsync(guild.Id);

            foreach (var rank in ranks)
            {
                var role = guild.Roles.FirstOrDefault(x => x.Id == rank.RoleId);
                if(role is null) invalid.Add(rank);
                else
                {
                    var currentUser = await guild.GetCurrentUserAsync();
                    var levels = (currentUser as SocketGuildUser)?.Hierarchy;
                    if (role.Position > levels)
                    {
                        invalid.Add(rank);
                    }
                    else roles.Add(role);
                }
            }

            if (invalid.Count > 0) await _ranks.ClearRolesAsync(invalid);
            return roles;
        }
        public async Task<List<IRole>> GetAutoRolesAsync(IGuild guild)
        {
            var roles = new List<IRole>();
            var invalid = new List<AutoRole>();

            var autoRoles = await _roles.GetAutoRoleAsync(guild.Id);

            foreach (var autoRole in autoRoles)
            {
                var role = guild.Roles.FirstOrDefault(x => x.Id == autoRole.RoleId);
                if(role is null) invalid.Add(autoRole);
                else
                {
                    var currentUser = await guild.GetCurrentUserAsync();
                    var levels = (currentUser as SocketGuildUser)?.Hierarchy;
                    if (role.Position > levels)
                    {
                        invalid.Add(autoRole);
                    }
                    else roles.Add(role);
                }
            }

            if (invalid.Count > 0) await _roles.ClearRolesAsync(invalid);
            return roles;
        }
    }
}