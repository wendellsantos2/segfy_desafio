using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sinistros.Domain.Sinistros;

namespace Sinistros.Infrastructure.Persistence.Repositories
{
    public class SinistroRepository : ISinistroRepository
    {
        private readonly AppDbContext _context;

        public SinistroRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Sinistro?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Sinistros
                .Include(s => s.HistoricoSinistros)
                .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        }

        public async Task AdicionarAsync(Sinistro sinistro, CancellationToken cancellationToken = default)
        {
            await _context.Sinistros.AddAsync(sinistro, cancellationToken);
        }
    }
}
