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

        public async Task<(IEnumerable<Apolice> Itens, int Total)> ListarAsync(string? status, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            var query = _context.Apolices.AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (Enum.TryParse<Sinistros.Domain.Enums.StatusApolice>(status, true, out var statusEnum))
                {
                    query = query.Where(a => a.Status == statusEnum);
                }
                else
                {
                    return (new List<Apolice>(), 0);
                }
            }

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderBy(a => a.Numero.Valor)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (itens, total);
        }

        public async Task AdicionarAsync(Apolice apolice, CancellationToken cancellationToken = default)
        {
            await _context.Apolices.AddAsync(apolice, cancellationToken);
        }
    }
}
