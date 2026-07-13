using System;
using System.Threading;
using System.Threading.Tasks;
using Sinistros.Domain.SeedWork;

namespace Sinistros.Domain.Sinistros
{
    public interface ISinistroRepository : IRepository<Sinistro>
    {
        Task<Sinistro?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task AdicionarAsync(Sinistro sinistro, CancellationToken cancellationToken = default);
    }
}
