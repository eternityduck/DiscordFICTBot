using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace KpiDsLibrary
{
    public class Ranks
    {
        private readonly KpiBotContext _context;

        public Ranks(KpiBotContext context) => _context = context;

        public async Task<List<Rank>> GerRankAsync(ulong id)
        {
            var ranks = await _context.Ranks
                .Where(x => x.ServerId == id)
                .ToListAsync();
            return await Task.FromResult(ranks);
        }

        public async Task AddRankAsync(ulong id, ulong roleid)
        {
            var server = await _context.Servers
                .FindAsync(id);
            if (server == null)
            {
                _context.Add(new Server() {Id = id});
            }

            _context.Add(new Rank() {RoleId = roleid, ServerId = id});
            await _context.SaveChangesAsync();
        }

        public async Task RemoveRankAsync(ulong id, ulong roleId)
        {
            var role = await _context.Ranks
                .Where(x => x.RoleId == roleId)
                .FirstOrDefaultAsync();
            _context.Remove(role);
            await _context.SaveChangesAsync();
        }

        public async Task ClearRolesAsync(List<Rank> roles)
        {
            _context.RemoveRange(roles);
            await _context.SaveChangesAsync();
        }
    }
}