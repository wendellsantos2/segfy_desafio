using System;
using System.Threading;
using System.Threading.Tasks;
using Sinistros.Domain.SeedWork;

namespace Sinistros.Domain.Clientes
{
    public interface IClienteRepository : IRepository<Cliente>
    {
        Task<Cliente?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task AdicionarAsync(Cliente cliente, CancellationToken cancellationToken = default);
    }
}
