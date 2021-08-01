using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using KpiDsLibrary;

namespace DS_Bot.Utilities
{
    public class AutoRolesHelper
    {
        private readonly AutoRoles _roles;

        public AutoRolesHelper(AutoRoles roles)
        {
            _roles = roles;
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