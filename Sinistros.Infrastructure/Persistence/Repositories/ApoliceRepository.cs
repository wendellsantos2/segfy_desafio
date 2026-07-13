using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sinistros.Domain.Apolices;

namespace Sinistros.Infrastructure.Persistence.Repositories
{
    public class ApoliceRepository : IApoliceRepository
    {
        private readonly AppDbContext _context;

        public ApoliceRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Apolice?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Apolices.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<Apolice>> ListarAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Apolices.ToListAsync(cancellationToken);
        }

        public async Task AdicionarAsync(Apolice apolice, CancellationToken cancellationToken = default)
        {
            await _context.Apolices.AddAsync(apolice, cancellationToken);
        }
    }
}
