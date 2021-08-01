using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace KpiDsLibrary
{
    public class Servers
    {
        private readonly KpiBotContext _context;

        public Servers(KpiBotContext context) => _context = context;

        public async Task ModifyPrefix(ulong id, string prefix)
        {
            var server = await _context.Servers
                .FindAsync(id);

            if (server is null) _context.Add(new Server() {Id = id, Prefix = prefix});
            else
            {
                server.Prefix = prefix;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<string> GetGuildPrefix(ulong id)
        {
            var prefix = await _context.Servers
                .Where(x => x.Id == id)
                .Select(x => x.Prefix)
                .FirstOrDefaultAsync();
            return await Task.FromResult(prefix);
        }
    }
}