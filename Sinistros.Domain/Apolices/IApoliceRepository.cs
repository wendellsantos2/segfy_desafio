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
        Task<(IEnumerable<Apolice> Itens, int Total)> ListarAsync(string? status, int page, int pageSize, CancellationToken cancellationToken = default);
        Task AdicionarAsync(Apolice apolice, CancellationToken cancellationToken = default);
    }
}
