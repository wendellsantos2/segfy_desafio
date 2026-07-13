using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sinistros.Domain.Clientes;

namespace Sinistros.Infrastructure.Persistence.Repositories
{
    public class ClienteRepository : IClienteRepository
    {
        private readonly AppDbContext _context;

        public ClienteRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Cliente?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Clientes.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        public async Task AdicionarAsync(Cliente cliente, CancellationToken cancellationToken = default)
        {
            await _context.Clientes.AddAsync(cliente, cancellationToken);
        }
    }
}
