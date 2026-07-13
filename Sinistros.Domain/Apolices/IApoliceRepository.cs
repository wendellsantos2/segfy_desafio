using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sinistros.Domain.SeedWork;

namespace Sinistros.Domain.Apolices
{
    public interface IApoliceRepository : IRepository<Apolice>
    {
        Task<Apolice?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Apolice>> ListarAsync(CancellationToken cancellationToken = default);
        Task AdicionarAsync(Apolice apolice, CancellationToken cancellationToken = default);
    }
}
